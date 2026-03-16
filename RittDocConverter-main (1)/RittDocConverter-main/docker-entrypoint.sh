#!/bin/bash
set -e

# RittDocConverter Docker Entrypoint
# ===================================

echo "=============================================="
echo "  RittDocConverter EPUB Processing Service"
echo "=============================================="
echo ""

# Display configuration
echo "Configuration:"
echo "  API_HOST:        ${API_HOST:-0.0.0.0}"
echo "  API_PORT:        ${API_PORT:-5001}"
echo "  LOG_LEVEL:       ${LOG_LEVEL:-INFO}"
echo "  MONGODB_URI:     ${MONGODB_URI:-mongodb://mongodb:27017}"
echo "  MAX_CONCURRENT:  ${MAX_CONCURRENT_JOBS:-4}"
echo ""

# Wait for MongoDB if configured
if [ -n "$MONGODB_URI" ] && [ "$WAIT_FOR_MONGODB" = "true" ]; then
    echo "Waiting for MongoDB..."

    # Extract host and port from MongoDB URI
    MONGO_HOST=$(echo $MONGODB_URI | sed -e 's|mongodb://||' -e 's|/.*||' -e 's|:.*||')
    MONGO_PORT=$(echo $MONGODB_URI | sed -e 's|mongodb://||' -e 's|/.*||' -e 's|.*:||')
    MONGO_PORT=${MONGO_PORT:-27017}

    # Wait up to 30 seconds for MongoDB
    for i in $(seq 1 30); do
        if python -c "import socket; s=socket.socket(); s.settimeout(1); s.connect(('$MONGO_HOST', $MONGO_PORT)); s.close()" 2>/dev/null; then
            echo "MongoDB is available!"
            break
        fi
        echo "Waiting for MongoDB... ($i/30)"
        sleep 1
    done
fi

# Handle different commands
case "$1" in
    api)
        echo "Starting API server..."
        exec python -m api.server \
            --host "${API_HOST:-0.0.0.0}" \
            --port "${API_PORT:-5001}" \
            ${API_DEBUG:+--debug}
        ;;

    worker)
        echo "Starting batch worker..."
        exec python batch_processor.py \
            --mode automated \
            --config /app/config.yaml
        ;;

    convert)
        shift
        echo "Running single conversion..."
        exec python epub_pipeline.py "$@"
        ;;

    shell)
        echo "Starting interactive shell..."
        exec /bin/bash
        ;;

    test)
        echo "Running tests..."
        exec python -m pytest tests/ -v
        ;;

    *)
        # If command doesn't match, pass through to exec
        exec "$@"
        ;;
esac
