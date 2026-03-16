"""
Thread-Safe Context Management for RittDocConverter

Provides thread-local storage for ID Authority and Reference Mapper
to enable safe concurrent conversions in the API server.

Usage:
    # In API request handler:
    with ConversionContext() as ctx:
        # ctx.authority and ctx.mapper are thread-local instances
        result = convert_epub(epub_path, authority=ctx.authority, mapper=ctx.mapper)

    # Or use the context manager directly:
    with thread_safe_conversion() as (authority, mapper):
        result = convert_epub(epub_path, authority=authority, mapper=mapper)
"""

import logging
import threading
from contextlib import contextmanager
from dataclasses import dataclass, field
from typing import Any, Dict, Generator, Optional, Tuple, TYPE_CHECKING

if TYPE_CHECKING:
    from id_authority import IDAuthority
    from reference_mapper import ReferenceMapper

logger = logging.getLogger(__name__)


# =============================================================================
# Thread-Local Storage
# =============================================================================

class ThreadLocalStorage:
    """
    Thread-local storage with lazy initialization.

    Each thread gets its own instance of stored objects.
    """

    def __init__(self):
        self._local = threading.local()
        self._factory_funcs: Dict[str, callable] = {}
        self._lock = threading.Lock()

    def register_factory(self, key: str, factory: callable) -> None:
        """Register a factory function for creating instances."""
        with self._lock:
            self._factory_funcs[key] = factory

    def get(self, key: str) -> Any:
        """Get thread-local instance, creating if necessary."""
        if not hasattr(self._local, key) or getattr(self._local, key) is None:
            with self._lock:
                if key not in self._factory_funcs:
                    raise KeyError(f"No factory registered for '{key}'")
                factory = self._factory_funcs[key]

            # Create instance outside lock
            instance = factory()
            setattr(self._local, key, instance)

        return getattr(self._local, key)

    def set(self, key: str, value: Any) -> None:
        """Set thread-local instance directly."""
        setattr(self._local, key, value)

    def clear(self, key: str) -> None:
        """Clear thread-local instance."""
        if hasattr(self._local, key):
            setattr(self._local, key, None)

    def clear_all(self) -> None:
        """Clear all thread-local instances for current thread."""
        for key in self._factory_funcs.keys():
            self.clear(key)

    def has(self, key: str) -> bool:
        """Check if instance exists for current thread."""
        return hasattr(self._local, key) and getattr(self._local, key) is not None


# Global thread-local storage
_thread_storage = ThreadLocalStorage()


def get_thread_storage() -> ThreadLocalStorage:
    """Get the global thread-local storage."""
    return _thread_storage


# =============================================================================
# Conversion Context
# =============================================================================

@dataclass
class ConversionContext:
    """
    Thread-safe context for a single conversion operation.

    Provides isolated instances of ID Authority and Reference Mapper
    for each conversion, preventing cross-contamination in concurrent
    API requests.

    Usage:
        with ConversionContext() as ctx:
            # Access thread-local instances
            ctx.authority.register_chapter(...)
            ctx.mapper.add_resource(...)
    """

    # Optional pre-created instances (for testing)
    _authority: Optional['IDAuthority'] = None
    _mapper: Optional['ReferenceMapper'] = None

    # Conversion metadata
    conversion_id: str = ""
    isbn: str = ""
    publisher: str = ""

    # Thread tracking
    _thread_id: int = field(default_factory=lambda: threading.get_ident())
    _entered: bool = False

    def __post_init__(self):
        """Initialize thread ID."""
        self._thread_id = threading.get_ident()

    def __enter__(self) -> 'ConversionContext':
        """Enter context - set up thread-local instances."""
        if self._entered:
            raise RuntimeError("ConversionContext already entered")

        self._entered = True
        storage = get_thread_storage()

        # Create fresh instances for this context
        if self._authority is None:
            from id_authority import IDAuthority
            self._authority = IDAuthority()

        if self._mapper is None:
            from reference_mapper import ReferenceMapper
            self._mapper = ReferenceMapper()

        # Store in thread-local storage
        storage.set('authority', self._authority)
        storage.set('mapper', self._mapper)
        storage.set('context', self)

        logger.debug(
            f"ConversionContext entered: thread={self._thread_id}, "
            f"conversion_id={self.conversion_id}"
        )

        return self

    def __exit__(self, exc_type, exc_val, exc_tb) -> bool:
        """Exit context - clean up thread-local instances."""
        if not self._entered:
            return False

        storage = get_thread_storage()

        # Clear thread-local storage
        storage.clear('authority')
        storage.clear('mapper')
        storage.clear('context')

        self._entered = False

        logger.debug(
            f"ConversionContext exited: thread={self._thread_id}, "
            f"conversion_id={self.conversion_id}, "
            f"exception={exc_type.__name__ if exc_type else None}"
        )

        # Don't suppress exceptions
        return False

    @property
    def authority(self) -> 'IDAuthority':
        """Get the ID Authority instance."""
        if not self._entered:
            raise RuntimeError("ConversionContext not entered")
        return self._authority

    @property
    def mapper(self) -> 'ReferenceMapper':
        """Get the Reference Mapper instance."""
        if not self._entered:
            raise RuntimeError("ConversionContext not entered")
        return self._mapper


# =============================================================================
# Context Helpers
# =============================================================================

@contextmanager
def thread_safe_conversion(
    conversion_id: str = "",
    isbn: str = "",
    publisher: str = ""
) -> Generator[Tuple['IDAuthority', 'ReferenceMapper'], None, None]:
    """
    Context manager for thread-safe conversion.

    Yields:
        Tuple of (IDAuthority, ReferenceMapper) instances

    Usage:
        with thread_safe_conversion(conversion_id="job-123") as (authority, mapper):
            # Use authority and mapper
            pass
    """
    ctx = ConversionContext(
        conversion_id=conversion_id,
        isbn=isbn,
        publisher=publisher
    )

    with ctx:
        yield (ctx.authority, ctx.mapper)


def get_current_authority() -> Optional['IDAuthority']:
    """
    Get the ID Authority for the current thread.

    Returns None if not in a ConversionContext.
    """
    storage = get_thread_storage()
    if storage.has('authority'):
        return storage.get('authority')
    return None


def get_current_mapper() -> Optional['ReferenceMapper']:
    """
    Get the Reference Mapper for the current thread.

    Returns None if not in a ConversionContext.
    """
    storage = get_thread_storage()
    if storage.has('mapper'):
        return storage.get('mapper')
    return None


def get_current_context() -> Optional[ConversionContext]:
    """
    Get the ConversionContext for the current thread.

    Returns None if not in a ConversionContext.
    """
    storage = get_thread_storage()
    if storage.has('context'):
        return storage.get('context')
    return None


# =============================================================================
# Fallback to Global Instances (Backward Compatibility)
# =============================================================================

def get_authority_or_global() -> 'IDAuthority':
    """
    Get thread-local authority, falling back to global singleton.

    For backward compatibility with code that doesn't use ConversionContext.
    """
    authority = get_current_authority()
    if authority is not None:
        return authority

    # Fall back to global singleton
    from id_authority import get_authority
    return get_authority()


def get_mapper_or_global() -> 'ReferenceMapper':
    """
    Get thread-local mapper, falling back to global singleton.

    For backward compatibility with code that doesn't use ConversionContext.
    """
    mapper = get_current_mapper()
    if mapper is not None:
        return mapper

    # Fall back to global singleton
    from reference_mapper import get_mapper
    return get_mapper()


# =============================================================================
# Thread-Safe Singleton Wrapper
# =============================================================================

class ThreadSafeSingleton:
    """
    Wrapper that makes a singleton class thread-safe.

    Can be used for both read-only and read-write access patterns.

    Usage:
        # Wrap existing class
        SafeAuthority = ThreadSafeSingleton(IDAuthority)

        # Use with locking
        with SafeAuthority.lock:
            authority = SafeAuthority.get_instance()
            authority.map_id(...)
    """

    def __init__(self, cls: type):
        self._cls = cls
        self._instance: Optional[Any] = None
        self._lock = threading.RLock()

    @property
    def lock(self) -> threading.RLock:
        """Get the lock for thread-safe access."""
        return self._lock

    def get_instance(self) -> Any:
        """Get or create the singleton instance."""
        if self._instance is None:
            with self._lock:
                if self._instance is None:
                    self._instance = self._cls()
        return self._instance

    def reset(self) -> None:
        """Reset the singleton instance."""
        with self._lock:
            self._instance = None

    def __enter__(self) -> Any:
        """Context manager entry - acquire lock and return instance."""
        self._lock.acquire()
        return self.get_instance()

    def __exit__(self, exc_type, exc_val, exc_tb) -> bool:
        """Context manager exit - release lock."""
        self._lock.release()
        return False


# =============================================================================
# Parallel Processing Support
# =============================================================================

@dataclass
class ParallelConversionResult:
    """Result from a parallel chapter conversion."""
    chapter_id: str
    success: bool
    output_path: Optional[str] = None
    error: Optional[str] = None
    execution_time_ms: float = 0.0


def parallel_chapter_worker(
    chapter_data: Dict[str, Any],
    conversion_func: callable,
    shared_config: Dict[str, Any]
) -> ParallelConversionResult:
    """
    Worker function for parallel chapter processing.

    Each worker gets its own thread-local context.

    Args:
        chapter_data: Chapter-specific data (path, id, etc.)
        conversion_func: Function to call for conversion
        shared_config: Shared configuration (read-only)

    Returns:
        ParallelConversionResult with success/failure status
    """
    import time
    start_time = time.time()

    chapter_id = chapter_data.get('chapter_id', 'unknown')

    try:
        # Create thread-local context for this worker
        with ConversionContext(conversion_id=f"parallel-{chapter_id}") as ctx:
            # Execute conversion
            output_path = conversion_func(
                chapter_data=chapter_data,
                authority=ctx.authority,
                mapper=ctx.mapper,
                config=shared_config
            )

            return ParallelConversionResult(
                chapter_id=chapter_id,
                success=True,
                output_path=output_path,
                execution_time_ms=(time.time() - start_time) * 1000
            )

    except Exception as e:
        logger.error(f"Parallel worker failed for {chapter_id}: {e}", exc_info=True)
        return ParallelConversionResult(
            chapter_id=chapter_id,
            success=False,
            error=str(e),
            execution_time_ms=(time.time() - start_time) * 1000
        )
