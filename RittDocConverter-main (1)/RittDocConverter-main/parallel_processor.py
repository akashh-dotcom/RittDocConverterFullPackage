"""
Parallel Chapter Processing for RittDocConverter

Provides parallel processing capabilities for converting multiple chapters
concurrently, improving performance for large EPUB files.

Architecture:
- Uses ThreadPoolExecutor for I/O-bound operations (XML parsing/writing)
- Uses ProcessPoolExecutor for CPU-bound operations (transformation)
- Thread-safe context isolation for each worker
- Configurable worker count and batch sizes

Usage:
    from parallel_processor import ParallelChapterProcessor

    processor = ParallelChapterProcessor(max_workers=4)
    results = processor.process_chapters(chapter_list, conversion_func)

    # Or use the higher-level API
    from parallel_processor import parallel_convert_chapters
    results = parallel_convert_chapters(chapters, output_dir)
"""

import logging
import os
import time
import threading
from concurrent.futures import ThreadPoolExecutor, ProcessPoolExecutor, as_completed, Future
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Callable, Dict, List, Optional, Tuple, TypeVar, Generic
from queue import Queue

logger = logging.getLogger(__name__)


# =============================================================================
# Configuration
# =============================================================================

@dataclass
class ParallelConfig:
    """Configuration for parallel processing."""
    # Worker counts
    max_workers: int = 4
    use_processes: bool = False  # True = ProcessPoolExecutor, False = ThreadPoolExecutor

    # Batching
    batch_size: int = 10  # Process chapters in batches
    batch_timeout: float = 300.0  # Timeout per batch in seconds

    # Resource management
    max_memory_mb: int = 2048  # Memory limit per worker
    throttle_on_memory: bool = True  # Slow down if memory usage high

    # Error handling
    fail_fast: bool = False  # Stop on first error vs continue
    max_retries: int = 1  # Retry failed chapters
    retry_delay: float = 1.0  # Delay between retries

    # Progress tracking
    report_interval: float = 5.0  # Progress report interval in seconds

    @classmethod
    def from_environment(cls) -> 'ParallelConfig':
        """Load configuration from environment variables."""
        return cls(
            max_workers=int(os.getenv('PARALLEL_MAX_WORKERS', '4')),
            use_processes=os.getenv('PARALLEL_USE_PROCESSES', 'false').lower() == 'true',
            batch_size=int(os.getenv('PARALLEL_BATCH_SIZE', '10')),
            batch_timeout=float(os.getenv('PARALLEL_BATCH_TIMEOUT', '300')),
            fail_fast=os.getenv('PARALLEL_FAIL_FAST', 'false').lower() == 'true',
            max_retries=int(os.getenv('PARALLEL_MAX_RETRIES', '1')),
        )

    @classmethod
    def for_cpu_bound(cls) -> 'ParallelConfig':
        """Configuration optimized for CPU-bound tasks."""
        import multiprocessing
        return cls(
            max_workers=max(1, multiprocessing.cpu_count() - 1),
            use_processes=True,
            batch_size=5,
        )

    @classmethod
    def for_io_bound(cls) -> 'ParallelConfig':
        """Configuration optimized for I/O-bound tasks."""
        return cls(
            max_workers=8,
            use_processes=False,
            batch_size=20,
        )


# =============================================================================
# Result Types
# =============================================================================

T = TypeVar('T')


@dataclass
class ChapterResult:
    """Result from processing a single chapter."""
    chapter_id: str
    chapter_file: str
    success: bool
    output_path: Optional[str] = None
    error: Optional[str] = None
    error_type: Optional[str] = None
    execution_time_ms: float = 0.0
    retries: int = 0
    worker_id: Optional[str] = None

    def __str__(self) -> str:
        status = "OK" if self.success else f"FAILED: {self.error}"
        return f"{self.chapter_id}: {status} ({self.execution_time_ms:.0f}ms)"


@dataclass
class BatchResult:
    """Result from processing a batch of chapters."""
    batch_id: int
    results: List[ChapterResult] = field(default_factory=list)
    total_time_ms: float = 0.0
    successful: int = 0
    failed: int = 0

    @property
    def success_rate(self) -> float:
        total = self.successful + self.failed
        return self.successful / total if total > 0 else 0.0

    def add_result(self, result: ChapterResult) -> None:
        self.results.append(result)
        if result.success:
            self.successful += 1
        else:
            self.failed += 1


@dataclass
class ProcessingReport:
    """Complete report from parallel processing."""
    total_chapters: int = 0
    successful_chapters: int = 0
    failed_chapters: int = 0
    total_time_ms: float = 0.0
    avg_time_per_chapter_ms: float = 0.0
    max_concurrent_workers: int = 0
    batch_results: List[BatchResult] = field(default_factory=list)
    failed_chapters_detail: List[ChapterResult] = field(default_factory=list)

    @property
    def success_rate(self) -> float:
        return self.successful_chapters / self.total_chapters if self.total_chapters > 0 else 0.0

    def add_batch(self, batch: BatchResult) -> None:
        self.batch_results.append(batch)
        self.successful_chapters += batch.successful
        self.failed_chapters += batch.failed
        for result in batch.results:
            if not result.success:
                self.failed_chapters_detail.append(result)


# =============================================================================
# Progress Tracking
# =============================================================================

@dataclass
class ProgressState:
    """Thread-safe progress state."""
    total: int = 0
    completed: int = 0
    failed: int = 0
    current_batch: int = 0
    total_batches: int = 0
    start_time: float = 0.0
    _lock: threading.Lock = field(default_factory=threading.Lock)

    def update(self, success: bool) -> None:
        with self._lock:
            self.completed += 1
            if not success:
                self.failed += 1

    def set_batch(self, batch_num: int, total_batches: int) -> None:
        with self._lock:
            self.current_batch = batch_num
            self.total_batches = total_batches

    @property
    def percent_complete(self) -> float:
        return (self.completed / self.total * 100) if self.total > 0 else 0

    @property
    def elapsed_seconds(self) -> float:
        return time.time() - self.start_time if self.start_time else 0

    @property
    def eta_seconds(self) -> float:
        if self.completed == 0:
            return 0
        rate = self.completed / self.elapsed_seconds
        remaining = self.total - self.completed
        return remaining / rate if rate > 0 else 0


class ProgressReporter:
    """Reports progress at intervals."""

    def __init__(self, state: ProgressState, interval: float = 5.0):
        self.state = state
        self.interval = interval
        self._stop_event = threading.Event()
        self._thread: Optional[threading.Thread] = None

    def start(self) -> None:
        self._stop_event.clear()
        self._thread = threading.Thread(target=self._report_loop, daemon=True)
        self._thread.start()

    def stop(self) -> None:
        self._stop_event.set()
        if self._thread:
            self._thread.join(timeout=2.0)

    def _report_loop(self) -> None:
        while not self._stop_event.wait(self.interval):
            self._report()

    def _report(self) -> None:
        s = self.state
        logger.info(
            f"Progress: {s.completed}/{s.total} ({s.percent_complete:.1f}%) | "
            f"Batch {s.current_batch}/{s.total_batches} | "
            f"Failed: {s.failed} | "
            f"ETA: {s.eta_seconds:.0f}s"
        )


# =============================================================================
# Worker Functions
# =============================================================================

# Global lock for thread-unsafe fallback mode (used only when thread_safe_context unavailable)
_GLOBAL_FALLBACK_LOCK = threading.Lock()

# Flag to track if we've warned about thread-unsafe fallback
_THREAD_SAFETY_WARNING_SHOWN = False


def _create_worker_context():
    """Create thread-local context for a worker."""
    try:
        from thread_safe_context import ConversionContext
        return ConversionContext(conversion_id=f"worker-{threading.get_ident()}")
    except ImportError:
        return None


class ThreadSafetyError(RuntimeError):
    """Raised when parallel processing cannot be performed safely."""
    pass


def _process_chapter_worker(
    chapter_data: Dict[str, Any],
    conversion_func: Callable,
    config: Dict[str, Any],
    worker_id: str
) -> ChapterResult:
    """
    Worker function for processing a single chapter.

    Args:
        chapter_data: Chapter information (path, id, etc.)
        conversion_func: Function to call for conversion
        config: Shared configuration
        worker_id: Unique worker identifier

    Returns:
        ChapterResult with success/failure status
    """
    global _THREAD_SAFETY_WARNING_SHOWN

    chapter_id = chapter_data.get('chapter_id', 'unknown')
    chapter_file = chapter_data.get('file_path', '')
    start_time = time.time()

    try:
        # Create thread-local context
        ctx = _create_worker_context()

        if ctx:
            # Thread-safe mode: use isolated context
            with ctx:
                output_path = conversion_func(
                    chapter_data=chapter_data,
                    authority=ctx.authority,
                    mapper=ctx.mapper,
                    config=config
                )
        else:
            # Thread-unsafe fallback mode
            # Check if strict mode is enabled (fail fast)
            strict_mode = config.get('parallel_strict_thread_safety', False)
            if strict_mode:
                raise ThreadSafetyError(
                    "Parallel processing requires thread_safe_context module. "
                    "Install it or disable parallel processing with --no-parallel."
                )

            # Warn about thread-unsafe fallback (once per process)
            if not _THREAD_SAFETY_WARNING_SHOWN:
                logger.warning(
                    "Thread-safe context not available - using global lock fallback. "
                    "This may reduce parallel performance. Install thread_safe_context "
                    "for optimal parallel processing."
                )
                _THREAD_SAFETY_WARNING_SHOWN = True

            # Use global lock to serialize access to shared resources
            # This is slower but prevents race conditions
            with _GLOBAL_FALLBACK_LOCK:
                logger.debug(f"Worker {worker_id} acquired global lock for {chapter_id}")
                output_path = conversion_func(
                    chapter_data=chapter_data,
                    config=config
                )

        return ChapterResult(
            chapter_id=chapter_id,
            chapter_file=chapter_file,
            success=True,
            output_path=str(output_path) if output_path else None,
            execution_time_ms=(time.time() - start_time) * 1000,
            worker_id=worker_id
        )

    except ThreadSafetyError:
        # Re-raise thread safety errors (don't swallow them)
        raise

    except Exception as e:
        logger.error(f"Worker {worker_id} failed on {chapter_id}: {e}", exc_info=True)
        return ChapterResult(
            chapter_id=chapter_id,
            chapter_file=chapter_file,
            success=False,
            error=str(e),
            error_type=type(e).__name__,
            execution_time_ms=(time.time() - start_time) * 1000,
            worker_id=worker_id
        )


# =============================================================================
# Main Processor Class
# =============================================================================

class ParallelChapterProcessor:
    """
    Processes chapters in parallel using thread or process pools.

    Usage:
        processor = ParallelChapterProcessor(max_workers=4)

        # Define conversion function
        def convert_chapter(chapter_data, authority, mapper, config):
            # conversion logic
            return output_path

        # Process all chapters
        report = processor.process_chapters(chapters, convert_chapter)
    """

    def __init__(self, config: Optional[ParallelConfig] = None):
        self.config = config or ParallelConfig()
        self._executor: Optional[ThreadPoolExecutor] = None
        self._progress: Optional[ProgressState] = None
        self._reporter: Optional[ProgressReporter] = None

    def process_chapters(
        self,
        chapters: List[Dict[str, Any]],
        conversion_func: Callable,
        shared_config: Optional[Dict[str, Any]] = None,
        progress_callback: Optional[Callable[[int, int, bool], None]] = None
    ) -> ProcessingReport:
        """
        Process multiple chapters in parallel.

        Args:
            chapters: List of chapter data dictionaries
            conversion_func: Function to process each chapter
            shared_config: Configuration shared across all workers
            progress_callback: Called after each chapter (completed, total, success)

        Returns:
            ProcessingReport with complete results
        """
        if not chapters:
            return ProcessingReport()

        report = ProcessingReport(total_chapters=len(chapters))
        shared_config = shared_config or {}

        # Initialize progress tracking
        self._progress = ProgressState(
            total=len(chapters),
            start_time=time.time()
        )

        # Start progress reporter
        self._reporter = ProgressReporter(
            self._progress,
            self.config.report_interval
        )
        self._reporter.start()

        try:
            # Create batches
            batches = self._create_batches(chapters)
            self._progress.total_batches = len(batches)
            report.max_concurrent_workers = self.config.max_workers

            # Process batches
            for batch_idx, batch in enumerate(batches, 1):
                self._progress.set_batch(batch_idx, len(batches))

                batch_result = self._process_batch(
                    batch_idx,
                    batch,
                    conversion_func,
                    shared_config,
                    progress_callback
                )

                report.add_batch(batch_result)

                # Check for fail-fast
                if self.config.fail_fast and batch_result.failed > 0:
                    logger.warning("Fail-fast triggered, stopping processing")
                    break

            # Calculate final stats
            report.total_time_ms = (time.time() - self._progress.start_time) * 1000
            if report.total_chapters > 0:
                report.avg_time_per_chapter_ms = report.total_time_ms / report.total_chapters

        finally:
            # Stop progress reporter
            if self._reporter:
                self._reporter.stop()

        return report

    def _create_batches(self, chapters: List[Dict]) -> List[List[Dict]]:
        """Split chapters into batches."""
        batch_size = self.config.batch_size
        return [
            chapters[i:i + batch_size]
            for i in range(0, len(chapters), batch_size)
        ]

    def _process_batch(
        self,
        batch_id: int,
        chapters: List[Dict],
        conversion_func: Callable,
        shared_config: Dict[str, Any],
        progress_callback: Optional[Callable]
    ) -> BatchResult:
        """Process a single batch of chapters."""
        batch_result = BatchResult(batch_id=batch_id)
        batch_start = time.time()

        # Choose executor type
        ExecutorClass = (
            ProcessPoolExecutor if self.config.use_processes
            else ThreadPoolExecutor
        )

        with ExecutorClass(max_workers=self.config.max_workers) as executor:
            # Submit all chapters in batch
            futures: Dict[Future, Dict] = {}
            for idx, chapter in enumerate(chapters):
                worker_id = f"batch{batch_id}-worker{idx}"
                future = executor.submit(
                    _process_chapter_worker,
                    chapter,
                    conversion_func,
                    shared_config,
                    worker_id
                )
                futures[future] = chapter

            # Collect results
            for future in as_completed(futures, timeout=self.config.batch_timeout):
                chapter = futures[future]
                try:
                    result = future.result()

                    # Handle retries
                    if not result.success and self.config.max_retries > 0:
                        result = self._retry_chapter(
                            chapter,
                            conversion_func,
                            shared_config,
                            result
                        )

                    batch_result.add_result(result)

                    # Update progress
                    if self._progress:
                        self._progress.update(result.success)

                    # Call progress callback
                    if progress_callback:
                        progress_callback(
                            self._progress.completed if self._progress else 0,
                            self._progress.total if self._progress else 0,
                            result.success
                        )

                except Exception as e:
                    logger.error(f"Future failed: {e}")
                    batch_result.add_result(ChapterResult(
                        chapter_id=chapter.get('chapter_id', 'unknown'),
                        chapter_file=chapter.get('file_path', ''),
                        success=False,
                        error=str(e),
                        error_type=type(e).__name__
                    ))

        batch_result.total_time_ms = (time.time() - batch_start) * 1000
        return batch_result

    def _retry_chapter(
        self,
        chapter: Dict,
        conversion_func: Callable,
        shared_config: Dict[str, Any],
        previous_result: ChapterResult
    ) -> ChapterResult:
        """Retry a failed chapter."""
        chapter_id = chapter.get('chapter_id', 'unknown')

        for retry in range(1, self.config.max_retries + 1):
            logger.info(f"Retrying {chapter_id} (attempt {retry}/{self.config.max_retries})")
            time.sleep(self.config.retry_delay)

            result = _process_chapter_worker(
                chapter,
                conversion_func,
                shared_config,
                f"retry-{retry}"
            )
            result.retries = retry

            if result.success:
                return result

        # All retries failed, return last result
        previous_result.retries = self.config.max_retries
        return previous_result


# =============================================================================
# High-Level API
# =============================================================================

def parallel_convert_chapters(
    chapters: List[Dict[str, Any]],
    conversion_func: Callable,
    max_workers: int = 4,
    shared_config: Optional[Dict[str, Any]] = None,
    progress_callback: Optional[Callable] = None
) -> ProcessingReport:
    """
    High-level function to convert chapters in parallel.

    Args:
        chapters: List of chapter data dictionaries
        conversion_func: Function to process each chapter
        max_workers: Maximum number of concurrent workers
        shared_config: Configuration shared across workers
        progress_callback: Called after each chapter

    Returns:
        ProcessingReport with complete results
    """
    config = ParallelConfig(max_workers=max_workers)
    processor = ParallelChapterProcessor(config)
    return processor.process_chapters(
        chapters,
        conversion_func,
        shared_config,
        progress_callback
    )


def estimate_optimal_workers(
    chapter_count: int,
    avg_chapter_size_kb: float = 100,
    available_memory_mb: int = 4096
) -> int:
    """
    Estimate optimal number of workers based on workload.

    Args:
        chapter_count: Number of chapters to process
        avg_chapter_size_kb: Average chapter size in KB
        available_memory_mb: Available memory in MB

    Returns:
        Recommended number of workers
    """
    import multiprocessing
    cpu_count = multiprocessing.cpu_count()

    # Memory-based limit (rough estimate: 50MB per worker)
    memory_workers = max(1, available_memory_mb // 50)

    # CPU-based limit
    cpu_workers = max(1, cpu_count - 1)

    # Workload-based (no point having more workers than chapters)
    workload_workers = min(chapter_count, 16)

    # Take minimum of all constraints
    optimal = min(memory_workers, cpu_workers, workload_workers)

    logger.debug(
        f"Worker estimation: cpu={cpu_workers}, memory={memory_workers}, "
        f"workload={workload_workers}, optimal={optimal}"
    )

    return optimal


# =============================================================================
# Integration with Pipeline
# =============================================================================

def create_parallel_chapter_data(
    xhtml_files: List[Path],
    chapter_map: Dict[str, str],
    temp_dir: Path
) -> List[Dict[str, Any]]:
    """
    Create chapter data dictionaries for parallel processing.

    Args:
        xhtml_files: List of XHTML file paths
        chapter_map: Mapping of filename to chapter ID
        temp_dir: Temporary directory for output

    Returns:
        List of chapter data dictionaries
    """
    chapters = []
    for idx, xhtml_path in enumerate(xhtml_files):
        chapter_id = chapter_map.get(xhtml_path.name, f"ch{idx+1:04d}")
        chapters.append({
            'chapter_id': chapter_id,
            'file_path': str(xhtml_path),
            'filename': xhtml_path.name,
            'index': idx,
            'temp_dir': str(temp_dir),
            'output_path': str(temp_dir / f"{chapter_id}.xml"),
        })
    return chapters
