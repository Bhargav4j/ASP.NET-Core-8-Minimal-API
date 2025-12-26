#!/bin/bash

# Exit on error
set -e

echo "=========================================="
echo "    Docker Build and Push Script"       
echo "=========================================="

# Get project name, will be sanitized later
read -p "Enter project name (default: minimalapi-app): " PROJECT_NAME
PROJECT_NAME=${PROJECT_NAME:-minimalapi-app}

# Sanitize project name for Docker
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo "Using sanitized image name: $IMAGE_NAME"

# Select registry type
echo "Select registry type:"
echo "1. AWS ECR"
echo "2. Docker Hub"
read -p "Enter selection (1-2): " REGISTRY_TYPE

if [ "$REGISTRY_TYPE" == "1" ]; then
  # AWS ECR
  echo "\nAWS ECR selected\n"
  
  # Get AWS region
  read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
  
  # Login to ECR
  echo "Logging in to AWS ECR in region ${AWS_REGION}..."
  aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $(aws sts get-caller-identity --query Account --output text).dkr.ecr.$AWS_REGION.amazonaws.com
  
  # Create ECR repository if it doesn't exist
  ECR_REPO=$IMAGE_NAME
  echo "Checking if ECR repository $ECR_REPO exists..."
  aws ecr describe-repositories --repository-names $ECR_REPO --region $AWS_REGION > /dev/null 2>&1 || \
    (echo "Creating ECR repository $ECR_REPO..." && \
     aws ecr create-repository --repository-name $ECR_REPO --region $AWS_REGION)
  
  # Get full registry URL
  REGISTRY_URL=$(aws sts get-caller-identity --query Account --output text).dkr.ecr.$AWS_REGION.amazonaws.com
  
  # Set full image name with registry
  read -p "Enter image tag (default: latest): " IMAGE_TAG
  IMAGE_TAG=${IMAGE_TAG:-latest}
  IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')
  
  FULL_IMAGE_NAME=$REGISTRY_URL/$ECR_REPO:$IMAGE_TAG
  
else
  # Docker Hub
  echo "\nDocker Hub selected\n"
  
  # Get Docker Hub username
  read -p "Enter Docker Hub username: " DOCKER_USERNAME
  read -s -p "Enter Docker Hub password: " DOCKER_PASSWORD
  echo ""
  
  # Login to Docker Hub
  echo "Logging in to Docker Hub..."
  echo $DOCKER_PASSWORD | docker login --username $DOCKER_USERNAME --password-stdin
  
  # Set full image name with registry
  read -p "Enter image tag (default: latest): " IMAGE_TAG
  IMAGE_TAG=${IMAGE_TAG:-latest}
  IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')
  
  FULL_IMAGE_NAME=$DOCKER_USERNAME/$IMAGE_NAME:$IMAGE_TAG
fi

echo "\nBuilding Docker image: $FULL_IMAGE_NAME\n"

# Build the Docker image
docker build -t $FULL_IMAGE_NAME .

echo "\nPushing Docker image to registry: $FULL_IMAGE_NAME\n"

# Push the image to the registry
docker push $FULL_IMAGE_NAME

echo "\n=========================================="
echo "    Build and Push Completed"           
echo "=========================================="
echo "Image: $FULL_IMAGE_NAME"
echo "=========================================="