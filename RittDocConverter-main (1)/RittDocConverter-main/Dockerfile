# RittDocConverter EPUB Processing Service
# ==========================================
# Multi-stage build for optimized production image

# Stage 1: Builder
FROM python:3.11-slim as builder

WORKDIR /app

# Install build dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    libxml2-dev \
    libxslt1-dev \
    libffi-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements first for layer caching
COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir --user -r requirements.txt

# Stage 2: Runtime
FROM python:3.11-slim

LABEL maintainer="RittDoc Team"
LABEL description="EPUB to DocBook XML conversion service"
LABEL version="2.0.0"

WORKDIR /app

# Install runtime dependencies (essential for lxml and cairosvg)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libxml2 \
    libxslt1.1 \
    libcairo2 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libgdk-pixbuf-2.0-0 \
    shared-mime-info \
    && rm -rf /var/lib/apt/lists/*

# Copy Python packages from builder
COPY --from=builder /root/.local /root/.local

# Ensure scripts in .local are usable
ENV PATH=/root/.local/bin:$PATH

# Copy application code
COPY . .

# Create directories for data persistence
RUN mkdir -p /app/Output /app/logs /app/data /app/uploads /app/storage

# Environment variables with defaults
ENV PYTHONUNBUFFERED=1
ENV PYTHONDONTWRITEBYTECODE=1

# API Server configuration
ENV API_HOST=0.0.0.0
ENV API_PORT=5001
ENV API_DEBUG=false

# MongoDB configuration
ENV MONGODB_URI=mongodb://mongodb:27017
ENV MONGODB_DATABASE=RittenhouseXMLConverter
ENV MONGODB_COLLECTION=conversion_dashboard

# Processing configuration
ENV MAX_CONCURRENT_JOBS=4
ENV JOB_TIMEOUT_SECONDS=3600
ENV LOG_LEVEL=INFO

# Storage configuration
ENV STORAGE_BACKEND=gridfs
ENV LOCAL_STORAGE_PATH=/app/storage

# S3 configuration (optional)
ENV S3_ENABLED=false
ENV S3_BUCKET=
ENV S3_REGION=us-east-1
ENV AWS_ACCESS_KEY_ID=
ENV AWS_SECRET_ACCESS_KEY=

# Expose API port
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import urllib.request; urllib.request.urlopen('http://localhost:5001/api/v1/health')" || exit 1

# Volumes for persistent data
VOLUME ["/app/Output", "/app/logs", "/app/data", "/app/uploads", "/app/storage"]

# Copy and set entrypoint (fix line endings in case of Windows clone)
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN sed -i 's/\r$//' /docker-entrypoint.sh && chmod +x /docker-entrypoint.sh

ENTRYPOINT ["/docker-entrypoint.sh"]

# Default command: run API server
CMD ["api"]
