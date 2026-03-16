#!/bin/bash
# =============================================================================
# RittDocConverter EC2 Deployment Script
# =============================================================================
# Usage: ./deploy.sh [environment]
# Example: ./deploy.sh production
# =============================================================================

set -euo pipefail

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
DEPLOY_DIR="/opt/rittdoc"
SERVICE_NAME="rittdoc"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Parse arguments
ENVIRONMENT="${1:-production}"
ENV_FILE="$PROJECT_ROOT/deploy/environments/${ENVIRONMENT}.env"

if [[ ! -f "$ENV_FILE" ]]; then
    log_error "Environment file not found: $ENV_FILE"
    log_info "Available environments:"
    ls -1 "$PROJECT_ROOT/deploy/environments/"
    exit 1
fi

log_info "Deploying RittDocConverter ($ENVIRONMENT)"

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."

    local missing=()
    command -v docker &>/dev/null || missing+=("docker")
    command -v docker-compose &>/dev/null || command -v "docker compose" &>/dev/null || missing+=("docker-compose")

    if [[ ${#missing[@]} -gt 0 ]]; then
        log_error "Missing required tools: ${missing[*]}"
        log_info "Run: sudo ./ec2-bootstrap.sh to install dependencies"
        exit 1
    fi

    if ! docker info &>/dev/null; then
        log_error "Docker daemon not running or permission denied"
        log_info "Try: sudo systemctl start docker && sudo usermod -aG docker $USER"
        exit 1
    fi

    log_info "Prerequisites OK"
}

# Setup deployment directory
setup_deploy_dir() {
    log_info "Setting up deployment directory: $DEPLOY_DIR"

    sudo mkdir -p "$DEPLOY_DIR"/{data,logs,config}
    sudo chown -R "$USER:$USER" "$DEPLOY_DIR"

    # Copy project files
    rsync -av --exclude='.git' --exclude='venv' --exclude='__pycache__' \
        --exclude='*.pyc' --exclude='Output' --exclude='Input' \
        "$PROJECT_ROOT/" "$DEPLOY_DIR/app/"

    # Copy environment file
    cp "$ENV_FILE" "$DEPLOY_DIR/config/app.env"

    # Merge with local .env if exists (for secrets like MONGODB_URI)
    if [[ -f "$PROJECT_ROOT/.env" ]]; then
        log_info "Merging local .env secrets..."
        cat "$PROJECT_ROOT/.env" >> "$DEPLOY_DIR/config/app.env"
    fi
}

# Build and start services
deploy_services() {
    log_info "Building and starting services..."

    cd "$DEPLOY_DIR/app"

    # Use production compose file if exists, otherwise default
    COMPOSE_FILE="docker-compose.yml"
    if [[ -f "deploy/ec2/docker-compose.prod.yml" ]]; then
        COMPOSE_FILE="deploy/ec2/docker-compose.prod.yml"
    fi

    # Pull latest images or build
    docker compose -f "$COMPOSE_FILE" --env-file "$DEPLOY_DIR/config/app.env" build

    # Stop existing services gracefully
    docker compose -f "$COMPOSE_FILE" down --remove-orphans || true

    # Start services
    docker compose -f "$COMPOSE_FILE" --env-file "$DEPLOY_DIR/config/app.env" up -d

    log_info "Services started"
}

# Health check
health_check() {
    log_info "Running health checks..."

    local max_attempts=30
    local attempt=1
    local api_port="${API_PORT:-5001}"

    while [[ $attempt -le $max_attempts ]]; do
        if curl -sf "http://localhost:$api_port/health" &>/dev/null; then
            log_info "Health check passed!"
            return 0
        fi
        log_info "Waiting for services... (attempt $attempt/$max_attempts)"
        sleep 2
        ((attempt++))
    done

    log_error "Health check failed after $max_attempts attempts"
    docker compose logs --tail=50
    return 1
}

# Setup systemd service
setup_systemd() {
    log_info "Setting up systemd service..."

    sudo cp "$SCRIPT_DIR/rittdoc.service" /etc/systemd/system/
    sudo systemctl daemon-reload
    sudo systemctl enable rittdoc

    log_info "Systemd service configured (rittdoc.service)"
}

# Print status
print_status() {
    echo ""
    log_info "=========================================="
    log_info "Deployment Complete!"
    log_info "=========================================="
    echo ""
    docker compose ps
    echo ""
    log_info "Useful commands:"
    echo "  View logs:     docker compose logs -f"
    echo "  Restart:       sudo systemctl restart rittdoc"
    echo "  Status:        docker compose ps"
    echo "  Stop:          docker compose down"
    echo ""
}

# Main
main() {
    check_prerequisites
    setup_deploy_dir
    deploy_services
    health_check
    setup_systemd
    print_status
}

main "$@"
