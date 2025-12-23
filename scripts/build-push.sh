#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}Docker Build and Push Script${NC}"
echo -e "${GREEN}=====================================${NC}"
echo ""

# Project name
PROJECT_NAME="ASPNETCore8Minimal23"

# Sanitize project name for Docker
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')
echo -e "${YELLOW}Sanitized image name: ${IMAGE_NAME}${NC}"
echo ""

# Prompt for image tag
echo -e "${YELLOW}Enter image tag (default: latest):${NC}"
read -p "Tag: " IMAGE_TAG
IMAGE_TAG=${IMAGE_TAG:-latest}
IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
echo -e "${GREEN}Using tag: ${IMAGE_TAG}${NC}"
echo ""

# Registry selection
echo -e "${YELLOW}Select container registry:${NC}"
echo "1. AWS ECR"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " REGISTRY_CHOICE
echo ""

if [ "$REGISTRY_CHOICE" = "1" ]; then
    echo -e "${GREEN}Selected: AWS ECR${NC}"
    echo ""
    
    # AWS ECR Configuration
    read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    read -p "Enter ECR Repository Name (default: ${IMAGE_NAME}): " ECR_REPO
    ECR_REPO=${ECR_REPO:-$IMAGE_NAME}
    
    REGISTRY_URL="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    FULL_IMAGE_NAME="${REGISTRY_URL}/${ECR_REPO}:${IMAGE_TAG}"
    
    echo ""
    echo -e "${YELLOW}Logging into AWS ECR...${NC}"
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to login to AWS ECR${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Successfully logged into AWS ECR${NC}"
    echo ""
    
    # Check if repository exists, create if not
    echo -e "${YELLOW}Checking if ECR repository exists...${NC}"
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || {
        echo -e "${YELLOW}Repository does not exist. Creating ECR repository...${NC}"
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
        echo -e "${GREEN}ECR repository created successfully${NC}"
    }
    echo ""
    
elif [ "$REGISTRY_CHOICE" = "2" ]; then
    echo -e "${GREEN}Selected: Docker Hub${NC}"
    echo ""
    
    # Docker Hub Configuration
    read -p "Enter Docker Hub Username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub Password or Access Token: " DOCKER_PASSWORD
    echo ""
    
    FULL_IMAGE_NAME="${DOCKER_USERNAME}/${IMAGE_NAME}:${IMAGE_TAG}"
    
    echo ""
    echo -e "${YELLOW}Logging into Docker Hub...${NC}"
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Failed to login to Docker Hub${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}Successfully logged into Docker Hub${NC}"
    echo ""
else
    echo -e "${RED}Invalid choice. Exiting.${NC}"
    exit 1
fi

# Build Docker image
echo -e "${YELLOW}Building Docker image: ${FULL_IMAGE_NAME}${NC}"
echo ""
docker build -f Dockerfile -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker build failed${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Docker image built successfully${NC}"
echo ""

# Push Docker image
echo -e "${YELLOW}Pushing Docker image to registry...${NC}"
docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker push failed${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}Build and Push Completed Successfully${NC}"
echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}Image: ${FULL_IMAGE_NAME}${NC}"
echo ""