"""
API Authentication Module

Provides API key and JWT-based authentication for the REST API.

Authentication Methods:
1. API Key: Pass via X-API-Key header or api_key query parameter
2. JWT Token: Pass via Authorization: Bearer <token> header

Configuration via environment variables:
- API_KEY: Required API key (can be comma-separated for multiple keys)
- JWT_SECRET: Secret for JWT validation (optional, enables JWT auth)
- AUTH_ENABLED: Set to 'false' to disable authentication (development only)

Usage:
    from api.auth import require_auth, require_admin

    @app.route('/protected')
    @require_auth
    def protected_endpoint():
        return jsonify({'message': 'authenticated'})

    @app.route('/admin')
    @require_admin
    def admin_endpoint():
        return jsonify({'message': 'admin access'})
"""

import hashlib
import hmac
import logging
import os
import secrets
import time
from dataclasses import dataclass, field
from datetime import datetime, timedelta
from functools import wraps
from typing import Callable, Dict, List, Optional, Set, Tuple

from flask import current_app, g, jsonify, request

logger = logging.getLogger(__name__)


# =============================================================================
# Configuration
# =============================================================================

@dataclass
class AuthConfig:
    """Authentication configuration."""
    enabled: bool = True
    api_keys: Set[str] = field(default_factory=set)
    admin_keys: Set[str] = field(default_factory=set)
    jwt_secret: Optional[str] = None
    jwt_algorithm: str = "HS256"
    jwt_expiry_hours: int = 24
    rate_limit_enabled: bool = True
    rate_limit_requests: int = 100  # requests per window
    rate_limit_window: int = 60  # seconds

    @classmethod
    def from_environment(cls) -> 'AuthConfig':
        """Load configuration from environment variables."""
        config = cls()

        # Check if auth is disabled (development only)
        auth_enabled = os.getenv('AUTH_ENABLED', 'true').lower()
        config.enabled = auth_enabled not in ('false', '0', 'no', 'disabled')

        # Load API keys
        api_keys_str = os.getenv('API_KEY', os.getenv('API_KEYS', ''))
        if api_keys_str:
            config.api_keys = {k.strip() for k in api_keys_str.split(',') if k.strip()}

        # Load admin keys (separate from regular API keys)
        admin_keys_str = os.getenv('ADMIN_API_KEY', os.getenv('ADMIN_API_KEYS', ''))
        if admin_keys_str:
            config.admin_keys = {k.strip() for k in admin_keys_str.split(',') if k.strip()}

        # JWT configuration
        config.jwt_secret = os.getenv('JWT_SECRET')
        config.jwt_algorithm = os.getenv('JWT_ALGORITHM', 'HS256')
        config.jwt_expiry_hours = int(os.getenv('JWT_EXPIRY_HOURS', '24'))

        # Rate limiting
        config.rate_limit_enabled = os.getenv('RATE_LIMIT_ENABLED', 'true').lower() == 'true'
        config.rate_limit_requests = int(os.getenv('RATE_LIMIT_REQUESTS', '100'))
        config.rate_limit_window = int(os.getenv('RATE_LIMIT_WINDOW', '60'))

        return config


# Global configuration (loaded lazily)
_auth_config: Optional[AuthConfig] = None


def get_auth_config() -> AuthConfig:
    """Get authentication configuration (lazy loaded)."""
    global _auth_config
    if _auth_config is None:
        _auth_config = AuthConfig.from_environment()
    return _auth_config


def reload_auth_config() -> AuthConfig:
    """Reload authentication configuration from environment."""
    global _auth_config
    _auth_config = AuthConfig.from_environment()
    return _auth_config


# =============================================================================
# Token Management
# =============================================================================

@dataclass
class AuthToken:
    """Represents an authenticated token/key."""
    token_type: str  # 'api_key' or 'jwt'
    identifier: str  # Key ID or JWT subject
    is_admin: bool = False
    expires_at: Optional[datetime] = None
    scopes: List[str] = field(default_factory=list)
    metadata: Dict[str, str] = field(default_factory=dict)

    @property
    def is_expired(self) -> bool:
        """Check if token is expired."""
        if self.expires_at is None:
            return False
        return datetime.utcnow() > self.expires_at


def generate_api_key(prefix: str = "rdc") -> str:
    """
    Generate a secure API key.

    Format: {prefix}_{random_32_chars}
    Example: rdc_a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
    """
    random_part = secrets.token_hex(16)
    return f"{prefix}_{random_part}"


def hash_api_key(api_key: str) -> str:
    """
    Hash an API key for secure storage.

    Uses SHA-256 for consistent hashing.
    """
    return hashlib.sha256(api_key.encode()).hexdigest()


# =============================================================================
# JWT Support (Optional)
# =============================================================================

def create_jwt_token(
    subject: str,
    scopes: Optional[List[str]] = None,
    is_admin: bool = False,
    expiry_hours: Optional[int] = None,
    additional_claims: Optional[Dict] = None
) -> Optional[str]:
    """
    Create a JWT token.

    Returns None if JWT is not configured.
    """
    config = get_auth_config()
    if not config.jwt_secret:
        logger.warning("JWT_SECRET not configured, cannot create JWT token")
        return None

    try:
        import jwt
    except ImportError:
        logger.warning("PyJWT not installed, cannot create JWT token")
        return None

    now = datetime.utcnow()
    expiry = expiry_hours or config.jwt_expiry_hours

    payload = {
        'sub': subject,
        'iat': now,
        'exp': now + timedelta(hours=expiry),
        'scopes': scopes or [],
        'admin': is_admin,
    }

    if additional_claims:
        payload.update(additional_claims)

    return jwt.encode(payload, config.jwt_secret, algorithm=config.jwt_algorithm)


def decode_jwt_token(token: str) -> Optional[AuthToken]:
    """
    Decode and validate a JWT token.

    Returns None if invalid or JWT not configured.
    """
    config = get_auth_config()
    if not config.jwt_secret:
        return None

    try:
        import jwt
    except ImportError:
        return None

    try:
        payload = jwt.decode(
            token,
            config.jwt_secret,
            algorithms=[config.jwt_algorithm]
        )

        return AuthToken(
            token_type='jwt',
            identifier=payload.get('sub', 'unknown'),
            is_admin=payload.get('admin', False),
            expires_at=datetime.utcfromtimestamp(payload['exp']),
            scopes=payload.get('scopes', []),
            metadata={'jwt_payload': str(payload)}
        )
    except jwt.ExpiredSignatureError:
        logger.debug("JWT token expired")
        return None
    except jwt.InvalidTokenError as e:
        logger.debug(f"Invalid JWT token: {e}")
        return None


# =============================================================================
# Rate Limiting
# =============================================================================

class RateLimiter:
    """Simple in-memory rate limiter using sliding window."""

    def __init__(self):
        self._requests: Dict[str, List[float]] = {}

    def is_allowed(self, identifier: str, max_requests: int, window_seconds: int) -> Tuple[bool, int]:
        """
        Check if request is allowed under rate limit.

        Returns:
            Tuple of (is_allowed, remaining_requests)
        """
        now = time.time()
        window_start = now - window_seconds

        # Get or create request list for this identifier
        if identifier not in self._requests:
            self._requests[identifier] = []

        # Clean old requests outside window
        self._requests[identifier] = [
            t for t in self._requests[identifier]
            if t > window_start
        ]

        # Check limit
        current_count = len(self._requests[identifier])
        if current_count >= max_requests:
            return False, 0

        # Record this request
        self._requests[identifier].append(now)

        return True, max_requests - current_count - 1

    def cleanup(self, max_age_seconds: int = 3600) -> int:
        """Remove old entries to prevent memory growth."""
        now = time.time()
        cutoff = now - max_age_seconds
        cleaned = 0

        keys_to_remove = []
        for identifier, timestamps in self._requests.items():
            self._requests[identifier] = [t for t in timestamps if t > cutoff]
            if not self._requests[identifier]:
                keys_to_remove.append(identifier)
                cleaned += 1

        for key in keys_to_remove:
            del self._requests[key]

        return cleaned


# Global rate limiter
_rate_limiter = RateLimiter()


def get_rate_limiter() -> RateLimiter:
    """Get the global rate limiter."""
    return _rate_limiter


# =============================================================================
# Authentication Helpers
# =============================================================================

def extract_auth_token() -> Optional[AuthToken]:
    """
    Extract authentication token from request.

    Checks in order:
    1. Authorization header (Bearer token for JWT, or API key)
    2. X-API-Key header
    3. api_key query parameter
    """
    config = get_auth_config()

    # Check Authorization header
    auth_header = request.headers.get('Authorization', '')
    if auth_header:
        if auth_header.startswith('Bearer '):
            # Try JWT first
            token = auth_header[7:]
            jwt_token = decode_jwt_token(token)
            if jwt_token:
                return jwt_token

            # Fall back to treating it as API key
            if token in config.api_keys:
                return AuthToken(
                    token_type='api_key',
                    identifier=hash_api_key(token)[:8],
                    is_admin=token in config.admin_keys
                )
        elif auth_header.startswith('ApiKey '):
            api_key = auth_header[7:]
            if api_key in config.api_keys:
                return AuthToken(
                    token_type='api_key',
                    identifier=hash_api_key(api_key)[:8],
                    is_admin=api_key in config.admin_keys
                )

    # Check X-API-Key header
    api_key = request.headers.get('X-API-Key')
    if api_key and api_key in config.api_keys:
        return AuthToken(
            token_type='api_key',
            identifier=hash_api_key(api_key)[:8],
            is_admin=api_key in config.admin_keys
        )

    # Check query parameter (least preferred)
    api_key = request.args.get('api_key')
    if api_key and api_key in config.api_keys:
        return AuthToken(
            token_type='api_key',
            identifier=hash_api_key(api_key)[:8],
            is_admin=api_key in config.admin_keys
        )

    return None


def get_client_identifier() -> str:
    """Get a unique identifier for the client (for rate limiting)."""
    # Check for authenticated user first
    if hasattr(g, 'auth_token') and g.auth_token:
        return f"auth:{g.auth_token.identifier}"

    # Fall back to IP + User-Agent hash
    ip = request.remote_addr or 'unknown'
    ua = request.headers.get('User-Agent', '')[:50]
    return f"ip:{ip}:{hash_api_key(ua)[:8]}"


# =============================================================================
# Decorators
# =============================================================================

def require_auth(f: Callable) -> Callable:
    """
    Decorator to require authentication for an endpoint.

    Sets g.auth_token with the authenticated token on success.

    Returns 401 if:
    - No valid authentication provided
    - Token is expired

    Returns 429 if rate limit exceeded.
    """
    @wraps(f)
    def decorated(*args, **kwargs):
        config = get_auth_config()

        # Skip auth if disabled (development only)
        if not config.enabled:
            g.auth_token = AuthToken(
                token_type='bypass',
                identifier='auth_disabled',
                is_admin=True
            )
            return f(*args, **kwargs)

        # Check if any API keys are configured
        if not config.api_keys and not config.jwt_secret:
            # No auth configured - warn but allow (misconfiguration)
            logger.warning("No API keys or JWT secret configured - authentication bypassed")
            g.auth_token = AuthToken(
                token_type='bypass',
                identifier='no_auth_configured',
                is_admin=True
            )
            return f(*args, **kwargs)

        # Extract and validate token
        token = extract_auth_token()
        if token is None:
            return jsonify({
                'error': 'Authentication required',
                'message': 'Provide API key via X-API-Key header or Authorization header'
            }), 401

        if token.is_expired:
            return jsonify({
                'error': 'Token expired',
                'message': 'Please obtain a new token'
            }), 401

        # Check rate limit
        if config.rate_limit_enabled:
            identifier = f"auth:{token.identifier}"
            allowed, remaining = _rate_limiter.is_allowed(
                identifier,
                config.rate_limit_requests,
                config.rate_limit_window
            )

            if not allowed:
                return jsonify({
                    'error': 'Rate limit exceeded',
                    'message': f'Maximum {config.rate_limit_requests} requests per {config.rate_limit_window} seconds',
                    'retry_after': config.rate_limit_window
                }), 429

        # Store token in request context
        g.auth_token = token

        return f(*args, **kwargs)

    return decorated


def require_admin(f: Callable) -> Callable:
    """
    Decorator to require admin authentication.

    Must be used after @require_auth or combined with it.
    """
    @wraps(f)
    @require_auth
    def decorated(*args, **kwargs):
        if not hasattr(g, 'auth_token') or not g.auth_token.is_admin:
            return jsonify({
                'error': 'Admin access required',
                'message': 'This endpoint requires admin privileges'
            }), 403

        return f(*args, **kwargs)

    return decorated


def require_scope(*scopes: str) -> Callable:
    """
    Decorator factory to require specific scopes.

    Usage:
        @require_scope('read', 'write')
        def endpoint():
            ...
    """
    def decorator(f: Callable) -> Callable:
        @wraps(f)
        @require_auth
        def decorated(*args, **kwargs):
            if not hasattr(g, 'auth_token'):
                return jsonify({'error': 'Authentication required'}), 401

            token = g.auth_token

            # Admin has all scopes
            if token.is_admin:
                return f(*args, **kwargs)

            # Check required scopes
            missing_scopes = set(scopes) - set(token.scopes)
            if missing_scopes:
                return jsonify({
                    'error': 'Insufficient permissions',
                    'message': f'Required scopes: {", ".join(scopes)}',
                    'missing_scopes': list(missing_scopes)
                }), 403

            return f(*args, **kwargs)

        return decorated

    return decorator


def optional_auth(f: Callable) -> Callable:
    """
    Decorator for endpoints that work with or without authentication.

    Sets g.auth_token if valid auth is provided, but doesn't require it.
    """
    @wraps(f)
    def decorated(*args, **kwargs):
        config = get_auth_config()

        # Try to extract token but don't require it
        token = extract_auth_token()
        if token and not token.is_expired:
            g.auth_token = token
        else:
            g.auth_token = None

        return f(*args, **kwargs)

    return decorated


# =============================================================================
# API Key Management Endpoints
# =============================================================================

def create_auth_management_blueprint():
    """
    Create a Blueprint for API key management endpoints.

    These endpoints require admin authentication.
    """
    from flask import Blueprint

    auth_bp = Blueprint('auth', __name__, url_prefix='/api/v1/auth')

    @auth_bp.route('/token', methods=['POST'])
    @require_admin
    def create_token():
        """
        Create a new JWT token.

        Request body:
            {
                "subject": "user_identifier",
                "scopes": ["read", "write"],
                "expiry_hours": 24,
                "admin": false
            }
        """
        config = get_auth_config()
        if not config.jwt_secret:
            return jsonify({
                'error': 'JWT not configured',
                'message': 'Set JWT_SECRET environment variable to enable JWT tokens'
            }), 503

        data = request.get_json() or {}
        subject = data.get('subject', 'api_user')
        scopes = data.get('scopes', [])
        expiry_hours = data.get('expiry_hours', config.jwt_expiry_hours)
        is_admin = data.get('admin', False)

        # Only admins can create admin tokens
        if is_admin and not g.auth_token.is_admin:
            return jsonify({'error': 'Cannot create admin tokens'}), 403

        token = create_jwt_token(
            subject=subject,
            scopes=scopes,
            is_admin=is_admin,
            expiry_hours=expiry_hours
        )

        if token is None:
            return jsonify({'error': 'Failed to create token'}), 500

        return jsonify({
            'token': token,
            'token_type': 'Bearer',
            'expires_in': expiry_hours * 3600,
            'subject': subject,
            'scopes': scopes
        })

    @auth_bp.route('/generate-key', methods=['POST'])
    @require_admin
    def generate_key():
        """
        Generate a new API key.

        Note: The key is returned ONCE. Store it securely.
        """
        data = request.get_json() or {}
        prefix = data.get('prefix', 'rdc')

        new_key = generate_api_key(prefix)

        return jsonify({
            'api_key': new_key,
            'key_hash': hash_api_key(new_key)[:16],
            'message': 'Store this key securely - it will not be shown again'
        })

    @auth_bp.route('/verify', methods=['GET'])
    @require_auth
    def verify_token():
        """
        Verify current authentication and return token info.
        """
        token = g.auth_token
        return jsonify({
            'valid': True,
            'token_type': token.token_type,
            'identifier': token.identifier,
            'is_admin': token.is_admin,
            'scopes': token.scopes,
            'expires_at': token.expires_at.isoformat() if token.expires_at else None
        })

    @auth_bp.route('/rate-limit-status', methods=['GET'])
    @require_auth
    def rate_limit_status():
        """
        Get current rate limit status.
        """
        config = get_auth_config()
        identifier = f"auth:{g.auth_token.identifier}"

        # Check without recording
        requests = _rate_limiter._requests.get(identifier, [])
        window_start = time.time() - config.rate_limit_window
        current_requests = len([t for t in requests if t > window_start])

        return jsonify({
            'rate_limit_enabled': config.rate_limit_enabled,
            'limit': config.rate_limit_requests,
            'window_seconds': config.rate_limit_window,
            'current_requests': current_requests,
            'remaining': max(0, config.rate_limit_requests - current_requests)
        })

    return auth_bp
