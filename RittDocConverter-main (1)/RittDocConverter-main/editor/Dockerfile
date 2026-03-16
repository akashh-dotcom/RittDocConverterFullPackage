# RittDoc Editor Service
# ======================
# Web-based DocBook XML editor with synchronized EPUB/PDF viewing

FROM python:3.11-slim

LABEL maintainer="RittDoc Team"
LABEL description="DocBook XML editor with Package Mode support"
LABEL version="1.0.0"

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    libxml2 \
    libxslt1.1 \
    libxml2-dev \
    libxslt1-dev \
    build-essential \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean

# Copy requirements and install dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy parent directory modules for reprocessing support
# Note: In production, mount or copy the parent repo for full functionality
COPY . /app/editor/

# Set working directory to editor folder
WORKDIR /app/editor

# Environment variables
ENV PYTHONUNBUFFERED=1
ENV FLASK_APP=server.py
ENV EDITOR_HOST=0.0.0.0
ENV EDITOR_PORT=5000

# Expose editor port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import urllib.request; urllib.request.urlopen('http://localhost:5000/')" || exit 1

# Default command
CMD ["python", "server.py", "--host", "0.0.0.0", "--port", "5000"]
