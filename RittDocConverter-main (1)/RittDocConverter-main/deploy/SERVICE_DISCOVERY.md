# RittDoc Service Configuration
# ==============================
# This file defines how services discover and communicate with each other.
# Copy the appropriate environment file to .env in your deployment.

# =============================================================================
# SERVICE REGISTRY
# =============================================================================
# The service registry is the single source of truth for service URLs.
# All services and the UI should fetch their configuration from here.

# Option 1: Static Configuration (Simple)
# ----------------------------------------
# Define URLs directly in environment variables.
# Best for: Simple deployments, single-environment setups

# Option 2: API Gateway (Recommended)
# -----------------------------------
# All services are accessed through a single gateway URL.
# The gateway handles routing, load balancing, and SSL termination.
# Best for: Production deployments, multi-service architectures

# Option 3: Service Discovery (Advanced)
# --------------------------------------
# Services register themselves with a discovery service (Consul, etcd, K8s).
# Clients query the discovery service to find other services.
# Best for: Dynamic environments, Kubernetes, auto-scaling

# =============================================================================
# CONFIGURATION ENDPOINT
# =============================================================================
# Each service exposes a /api/v1/service-info endpoint that returns:
# - Service name and version
# - Available endpoints
# - Health status
# - Dependencies and their URLs
#
# The UI can call this endpoint to discover what's available.

# =============================================================================
# ENVIRONMENT DETECTION
# =============================================================================
# Services detect their environment from the ENVIRONMENT variable.
# This affects logging levels, feature flags, and default behaviors.

ENVIRONMENT=development  # development | staging | production

# =============================================================================
# URL PATTERNS BY ENVIRONMENT
# =============================================================================

# Local Development:
#   EPUB: http://localhost:5001
#   UI:   http://localhost:3000

# Docker Compose (internal network):
#   EPUB: http://epub-service:5001
#   UI:   http://ui:3000

# Staging:
#   Gateway: https://api-staging.rittdoc.example.com
#   EPUB:    https://api-staging.rittdoc.example.com/epub
#   UI:      https://staging.rittdoc.example.com

# Production:
#   Gateway: https://api.rittdoc.example.com
#   EPUB:    https://api.rittdoc.example.com/epub
#   UI:      https://app.rittdoc.example.com

# Customer AWS (Dedicated):
#   Gateway: https://api.{customer}.rittdoc.example.com
#   or
#   Gateway: https://rittdoc-api.{customer-domain}.com
