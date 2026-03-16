# EC2 Deployment Guide

This guide covers deploying RittDocConverter to AWS EC2.

## Prerequisites

- AWS EC2 instance (t3.medium or larger recommended)
- MongoDB Atlas account with connection string
- Domain name (optional, for SSL)

## Quick Start

### Option 1: Fresh EC2 Instance (User Data)

When launching a new EC2 instance, paste the contents of `ec2-bootstrap.sh` into the User Data field. This will:

1. Install Docker and Docker Compose
2. Clone the repository
3. Create configuration directories

After launch, SSH into the instance and configure:

```bash
# Configure MongoDB connection
sudo nano /opt/rittdoc/config/app.env

# Deploy
cd /opt/rittdoc/app
./deploy/ec2/deploy.sh production
```

### Option 2: Existing EC2 Instance

```bash
# Run bootstrap script
curl -sSL https://raw.githubusercontent.com/akashh-dotcom/RittDocConverter/main/deploy/ec2/ec2-bootstrap.sh | sudo bash

# Configure MongoDB
sudo nano /opt/rittdoc/config/app.env

# Deploy
cd /opt/rittdoc/app
./deploy/ec2/deploy.sh production
```

## Configuration

### Environment Variables

Edit `/opt/rittdoc/config/app.env`:

```bash
# Required - MongoDB Atlas connection
MONGODB_URI=mongodb+srv://user:password@cluster.mongodb.net/database
MONGODB_DATABASE=RittenhouseXMLConverter

# Ports
EPUB_API_PORT=5001
EDITOR_PORT=5000

# Performance
MAX_CONCURRENT_JOBS=4
JOB_TIMEOUT_SECONDS=3600
LOG_LEVEL=WARNING
```

### Security Groups

Configure your EC2 security group:

| Port | Protocol | Source | Description |
|------|----------|--------|-------------|
| 22 | TCP | Your IP | SSH |
| 80 | TCP | 0.0.0.0/0 | HTTP (redirect to HTTPS) |
| 443 | TCP | 0.0.0.0/0 | HTTPS |
| 5001 | TCP | Your IP/VPC | EPUB API (if not using proxy) |
| 5000 | TCP | Your IP/VPC | Editor (if not using proxy) |

## Management

### Systemd Service

```bash
# Start services
sudo systemctl start rittdoc

# Stop services
sudo systemctl stop rittdoc

# Restart services
sudo systemctl restart rittdoc

# View status
sudo systemctl status rittdoc

# Enable auto-start on boot
sudo systemctl enable rittdoc
```

### Docker Commands

```bash
cd /opt/rittdoc/app

# View running containers
docker compose -f deploy/ec2/docker-compose.prod.yml ps

# View logs
docker compose -f deploy/ec2/docker-compose.prod.yml logs -f

# View specific service logs
docker compose -f deploy/ec2/docker-compose.prod.yml logs -f epub-service

# Restart a service
docker compose -f deploy/ec2/docker-compose.prod.yml restart epub-service

# Stop all services
docker compose -f deploy/ec2/docker-compose.prod.yml down
```

## SSL/TLS Setup

### Option 1: Let's Encrypt (Recommended)

```bash
# Install certbot
sudo yum install -y certbot  # Amazon Linux
sudo apt install -y certbot  # Ubuntu

# Generate certificate
sudo certbot certonly --standalone -d your-domain.com

# Copy certificates
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /opt/rittdoc/ssl/server.crt
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /opt/rittdoc/ssl/server.key

# Start with nginx proxy
docker compose -f deploy/ec2/docker-compose.prod.yml --profile with-proxy up -d
```

### Option 2: Self-Signed (Development)

```bash
cd /opt/rittdoc/ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout server.key -out server.crt \
    -subj "/CN=localhost"
```

## Monitoring

### Health Checks

```bash
# Check EPUB service
curl http://localhost:5001/health

# Check Editor service
curl http://localhost:5000/health
```

### Logs

```bash
# Application logs
docker compose logs -f

# System logs
sudo journalctl -u rittdoc -f

# Nginx logs (if using proxy)
docker compose logs -f nginx
```

### Resource Usage

```bash
# Container stats
docker stats

# System resources
htop
```

## Troubleshooting

### Services won't start

```bash
# Check Docker
sudo systemctl status docker

# Check container logs
docker compose logs --tail=100

# Check disk space
df -h

# Check memory
free -m
```

### MongoDB connection issues

```bash
# Test connection
docker exec rittdoc-epub python -c "
from pymongo import MongoClient
import os
client = MongoClient(os.environ['MONGODB_URI'])
print(client.server_info())
"
```

### Port conflicts

```bash
# Check what's using a port
sudo netstat -tlnp | grep 5001
sudo lsof -i :5001
```

## Updates

```bash
cd /opt/rittdoc/app

# Pull latest changes
git pull origin main

# Rebuild and restart
docker compose -f deploy/ec2/docker-compose.prod.yml build
docker compose -f deploy/ec2/docker-compose.prod.yml up -d
```

## Backup

### Data Volumes

```bash
# Backup volumes
docker run --rm -v rittdoc_epub-data:/data -v $(pwd):/backup alpine \
    tar czf /backup/epub-data-backup.tar.gz /data

# Restore volumes
docker run --rm -v rittdoc_epub-data:/data -v $(pwd):/backup alpine \
    tar xzf /backup/epub-data-backup.tar.gz -C /
```

## Architecture

```
                    ┌─────────────────────┐
                    │   Load Balancer     │
                    │   (ALB/NLB/Nginx)   │
                    └──────────┬──────────┘
                               │
                    ┌──────────┴──────────┐
                    │      EC2 Instance    │
                    │                      │
                    │  ┌────────────────┐  │
                    │  │ Docker Compose │  │
                    │  │                │  │
                    │  │ ┌────────────┐ │  │
                    │  │ │epub-service│ │  │
                    │  │ │   :5001    │ │  │
                    │  │ └────────────┘ │  │
                    │  │                │  │
                    │  │ ┌────────────┐ │  │
                    │  │ │  editor    │ │  │
                    │  │ │   :5000    │ │  │
                    │  │ └────────────┘ │  │
                    │  │                │  │
                    │  └────────────────┘  │
                    │                      │
                    └──────────┬───────────┘
                               │
                    ┌──────────┴──────────┐
                    │   MongoDB Atlas      │
                    │   (External)         │
                    └─────────────────────┘
```
