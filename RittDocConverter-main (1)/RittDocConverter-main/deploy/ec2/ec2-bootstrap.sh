#!/bin/bash
# =============================================================================
# EC2 Bootstrap Script (User Data)
# =============================================================================
# This script can be used as EC2 User Data to bootstrap a new instance
# or run manually on an existing EC2 instance.
#
# Usage: curl -sSL <raw-url> | sudo bash
# Or in EC2 User Data (paste entire script)
# =============================================================================

set -euo pipefail

LOG_FILE="/var/log/rittdoc-bootstrap.log"
exec > >(tee -a "$LOG_FILE") 2>&1

echo "=========================================="
echo "RittDocConverter EC2 Bootstrap"
echo "Started: $(date)"
echo "=========================================="

# Configuration
DEPLOY_USER="ec2-user"  # Change to 'ubuntu' for Ubuntu AMIs
REPO_URL="https://github.com/akashh-dotcom/RittDocConverter.git"
BRANCH="main"
DEPLOY_DIR="/opt/rittdoc"

# Detect OS
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$ID
else
    OS="unknown"
fi

echo "[INFO] Detected OS: $OS"

# Install Docker
install_docker() {
    echo "[INFO] Installing Docker..."

    case $OS in
        amzn)
            # Amazon Linux 2
            yum update -y
            yum install -y docker git
            systemctl enable docker
            systemctl start docker
            usermod -aG docker $DEPLOY_USER
            # Install docker-compose
            curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
            chmod +x /usr/local/bin/docker-compose
            ln -sf /usr/local/bin/docker-compose /usr/bin/docker-compose
            ;;
        ubuntu|debian)
            # Ubuntu/Debian
            apt-get update
            apt-get install -y ca-certificates curl gnupg
            install -m 0755 -d /etc/apt/keyrings
            curl -fsSL https://download.docker.com/linux/$OS/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
            chmod a+r /etc/apt/keyrings/docker.gpg
            echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/$OS $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
            apt-get update
            apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin git
            systemctl enable docker
            systemctl start docker
            DEPLOY_USER="ubuntu"
            usermod -aG docker $DEPLOY_USER
            ;;
        *)
            echo "[ERROR] Unsupported OS: $OS"
            exit 1
            ;;
    esac

    echo "[INFO] Docker installed successfully"
}

# Install additional tools
install_tools() {
    echo "[INFO] Installing additional tools..."

    case $OS in
        amzn)
            yum install -y jq htop curl wget
            ;;
        ubuntu|debian)
            apt-get install -y jq htop curl wget
            ;;
    esac
}

# Clone repository
clone_repo() {
    echo "[INFO] Cloning repository..."

    mkdir -p "$DEPLOY_DIR"
    if [ -d "$DEPLOY_DIR/app/.git" ]; then
        cd "$DEPLOY_DIR/app"
        git fetch origin
        git checkout "$BRANCH"
        git pull origin "$BRANCH"
    else
        git clone -b "$BRANCH" "$REPO_URL" "$DEPLOY_DIR/app"
    fi

    chown -R "$DEPLOY_USER:$DEPLOY_USER" "$DEPLOY_DIR"
}

# Setup directories
setup_directories() {
    echo "[INFO] Setting up directories..."

    mkdir -p "$DEPLOY_DIR"/{data,logs,config,ssl}
    mkdir -p /tmp/rittdoc

    chown -R "$DEPLOY_USER:$DEPLOY_USER" "$DEPLOY_DIR"
    chmod 755 "$DEPLOY_DIR"
}

# Create placeholder env file
create_env_template() {
    echo "[INFO] Creating environment template..."

    if [ ! -f "$DEPLOY_DIR/config/app.env" ]; then
        cat > "$DEPLOY_DIR/config/app.env" << 'EOF'
# RittDocConverter Environment Configuration
# ===========================================
# IMPORTANT: Update these values before starting services!

# MongoDB Atlas Connection (REQUIRED)
MONGODB_URI=mongodb+srv://user:password@cluster.mongodb.net/database

# Database
MONGODB_DATABASE=RittenhouseXMLConverter
MONGODB_COLLECTION=conversions

# API Configuration
EPUB_API_PORT=5001
EDITOR_PORT=5000
API_DEBUG=false
LOG_LEVEL=WARNING

# Processing
MAX_CONCURRENT_JOBS=4
JOB_TIMEOUT_SECONDS=3600
EOF
        chmod 600 "$DEPLOY_DIR/config/app.env"
        chown "$DEPLOY_USER:$DEPLOY_USER" "$DEPLOY_DIR/config/app.env"
    fi
}

# Print next steps
print_next_steps() {
    echo ""
    echo "=========================================="
    echo "Bootstrap Complete!"
    echo "=========================================="
    echo ""
    echo "Next steps:"
    echo ""
    echo "1. Configure MongoDB connection:"
    echo "   sudo nano $DEPLOY_DIR/config/app.env"
    echo ""
    echo "2. Deploy the application:"
    echo "   cd $DEPLOY_DIR/app"
    echo "   ./deploy/ec2/deploy.sh production"
    echo ""
    echo "3. Or use systemd service:"
    echo "   sudo systemctl start rittdoc"
    echo "   sudo systemctl status rittdoc"
    echo ""
    echo "4. View logs:"
    echo "   docker compose logs -f"
    echo ""
    echo "=========================================="
}

# Main
main() {
    install_docker
    install_tools
    setup_directories
    clone_repo
    create_env_template
    print_next_steps
}

main "$@"

echo "Finished: $(date)"
