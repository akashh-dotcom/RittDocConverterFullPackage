"""
Logging utilities for RittDocConverter.
"""

import logging
import sys
from pathlib import Path
from typing import Optional


def configure_third_party_loggers():
    """
    Configure third-party library loggers to reduce noise.
    Sets verbose libraries like pymongo to WARNING level.
    """
    # Suppress verbose pymongo heartbeat/topology logs
    logging.getLogger('pymongo').setLevel(logging.WARNING)
    logging.getLogger('pymongo.topology').setLevel(logging.WARNING)
    logging.getLogger('pymongo.connection').setLevel(logging.WARNING)
    logging.getLogger('pymongo.serverSelection').setLevel(logging.WARNING)

    # Suppress verbose boto3/botocore logs (for S3)
    logging.getLogger('boto3').setLevel(logging.WARNING)
    logging.getLogger('botocore').setLevel(logging.WARNING)
    logging.getLogger('urllib3').setLevel(logging.WARNING)

    # Suppress verbose werkzeug access logs (optional)
    # logging.getLogger('werkzeug').setLevel(logging.WARNING)


def setup_logger(
    name: str,
    log_file: Optional[Path] = None,
    level: int = logging.INFO,
    console: bool = True
) -> logging.Logger:
    """
    Set up a logger with file and console handlers.

    Args:
        name: Logger name
        log_file: Optional path to log file
        level: Logging level (default: INFO)
        console: Whether to log to console (default: True)

    Returns:
        Configured logger instance
    """
    # Configure third-party loggers first
    configure_third_party_loggers()

    logger = logging.getLogger(name)
    logger.setLevel(level)
    logger.propagate = False

    # Clear existing handlers
    logger.handlers.clear()

    # Create formatter
    formatter = logging.Formatter(
        '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )

    # Console handler
    if console:
        console_handler = logging.StreamHandler(sys.stdout)
        console_handler.setLevel(level)
        console_handler.setFormatter(formatter)
        logger.addHandler(console_handler)

    # File handler
    if log_file:
        log_file.parent.mkdir(parents=True, exist_ok=True)
        file_handler = logging.FileHandler(log_file, encoding='utf-8')
        file_handler.setLevel(level)
        file_handler.setFormatter(formatter)
        logger.addHandler(file_handler)

    return logger
