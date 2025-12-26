#!/bin/bash

# Enable strict error handling
set -e
set -o pipefail

echo "=========================================="
echo "    AWS ECS Fargate Deployment Script"   
echo "=========================================="

# Collect deployment information
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter ECS cluster name (e.g., minimalapi-cluster): " CLUSTER_NAME
read -p "Enter comma-separated subnet IDs (e.g., subnet-abc123,subnet-def456): " SUBNETS_INPUT
read -p "Enter security group ID (e.g., sg-abc123): " SECURITY_GROUP
read -p "Enter Docker image URI (e.g., 123456789012.dkr.ecr.us-east-1.amazonaws.com/minimalapi-app:latest): " IMAGE_URI

# Get AWS account ID
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)

# Check if ECS cluster exists, create if not
echo "\nChecking if ECS cluster exists..."
aws ecs describe-clusters --clusters $CLUSTER_NAME --region $AWS_REGION >/dev/null 2>&1 || \
  (echo "Creating ECS cluster $CLUSTER_NAME..." && \
   aws ecs create-cluster --cluster-name $CLUSTER_NAME --region $AWS_REGION)

# Convert comma-separated subnets into array
IFS=',' read -ra SUBNET_ARRAY <<< "$SUBNETS_INPUT"
SUBNET_1=${SUBNET_ARRAY[0]}
SUBNET_2=${SUBNET_ARRAY[1]:-$SUBNET_1}  # Use first subnet as second if only one provided

# Ask if load balancer is needed
read -p "\nDo you need a load balancer for this service? (y/n): " LOAD_BALANCER_NEEDED

if [[ "$LOAD_BALANCER_NEEDED" =~ ^[Yy] ]]; then
  echo "\nSetting up Application Load Balancer..."
  
  # Create load balancer
  read -p "Enter load balancer name (default: minimalapi-lb): " LB_NAME
  LB_NAME=${LB_NAME:-minimalapi-lb}
  
  # Create load balancer
  echo "Creating load balancer: $LB_NAME"
  LB_ARN=$(aws elbv2 create-load-balancer \
    --name $LB_NAME \
    --subnets $SUBNET_1 $SUBNET_2 \
    --security-groups $SECURITY_GROUP \
    --region $AWS_REGION \
    --query 'LoadBalancers[0].LoadBalancerArn' \
    --output text)
  
  # Create target group
  echo "Creating target group: minimalapi-tg"
  TG_ARN=$(aws elbv2 create-target-group \
    --name minimalapi-tg \
    --protocol HTTP \
    --port 80 \
    --vpc-id $(aws ec2 describe-subnets \
      --subnet-ids $SUBNET_1 \
      --query 'Subnets[0].VpcId' \
      --output text \
      --region $AWS_REGION) \
    --target-type ip \
    --health-check-path /health \
    --health-check-interval-seconds 30 \
    --health-check-timeout-seconds 5 \
    --healthy-threshold-count 3 \
    --unhealthy-threshold-count 3 \
    --region $AWS_REGION \
    --query 'TargetGroups[0].TargetGroupArn' \
    --output text)
  
  # Create listener
  echo "Creating listener on port 80"
  aws elbv2 create-listener \
    --load-balancer-arn $LB_ARN \
    --protocol HTTP \
    --port 80 \
    --default-actions Type=forward,TargetGroupArn=$TG_ARN \
    --region $AWS_REGION
  
  TARGET_GROUP_ARN=$TG_ARN
  
  # Use service definition with load balancer
  cp ecs/service-definition.json ecs/service-definition-deploy.json
else
  echo "\nSkipping load balancer setup. Service will not be publicly accessible."
  
  # Remove loadBalancers section from service definition
  cp ecs/service-definition.json ecs/service-definition-deploy.json
  sed -i '"loadBalancers": \[/,/\],/d' ecs/service-definition-deploy.json
  sed -i '"healthCheckGracePeriodSeconds": 300,/d' ecs/service-definition-deploy.json
  
  TARGET_GROUP_ARN=""
fi

# Create directories if they don't exist
mkdir -p ecs

# Copy task definition template
cp ecs/task-definition.json ecs/task-definition-deploy.json

# Replace placeholders in task definition
echo "\nConfiguring task definition..."
sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" ecs/task-definition-deploy.json
sed -i "s|{{AWS_REGION}}|$AWS_REGION|g" ecs/task-definition-deploy.json
sed -i "s|{{ACCOUNT_ID}}|$ACCOUNT_ID|g" ecs/task-definition-deploy.json

# Replace placeholders in service definition
echo "Configuring service definition..."
sed -i "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" ecs/service-definition-deploy.json
sed -i "s|{{TARGET_GROUP_ARN}}|$TARGET_GROUP_ARN|g" ecs/service-definition-deploy.json
sed -i "s|{{SUBNET_1}}|$SUBNET_1|g" ecs/service-definition-deploy.json
sed -i "s|{{SUBNET_2}}|$SUBNET_2|g" ecs/service-definition-deploy.json
sed -i "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" ecs/service-definition-deploy.json

# Register task definition
echo "\nRegistering task definition..."
TASK_DEF_ARN=$(aws ecs register-task-definition \
  --cli-input-json file://ecs/task-definition-deploy.json \
  --region $AWS_REGION \
  --query 'taskDefinition.taskDefinitionArn' \
  --output text)

# Check if service exists
SERVICE_EXISTS=$(aws ecs describe-services \
  --cluster $CLUSTER_NAME \
  --services minimalapi-service \
  --region $AWS_REGION \
  --query 'services[?length(serviceName)>0]' \
  --output text)

if [ -z "$SERVICE_EXISTS" ]; then
  # Create service
  echo "\nCreating new ECS service..."
  aws ecs create-service \
    --cli-input-json file://ecs/service-definition-deploy.json \
    --region $AWS_REGION
else
  # Update service
  echo "\nUpdating existing ECS service with new task definition..."
  aws ecs update-service \
    --cluster $CLUSTER_NAME \
    --service minimalapi-service \
    --task-definition $TASK_DEF_ARN \
    --force-new-deployment \
    --region $AWS_REGION
fi

# Wait for service to stabilize
echo "\nWaiting for service to stabilize..."
aws ecs wait services-stable \
  --cluster $CLUSTER_NAME \
  --services minimalapi-service \
  --region $AWS_REGION

# Describe service to verify deployment
echo "\nService deployment complete. Verifying details..."
aws ecs describe-services \
  --cluster $CLUSTER_NAME \
  --services minimalapi-service \
  --region $AWS_REGION \
  --query 'services[0].{Status:status,RunningCount:runningCount,DesiredCount:desiredCount}'

# Display load balancer DNS if created
if [[ "$LOAD_BALANCER_NEEDED" =~ ^[Yy] ]]; then
  LB_DNS=$(aws elbv2 describe-load-balancers \
    --load-balancer-arns $LB_ARN \
    --region $AWS_REGION \
    --query 'LoadBalancers[0].DNSName' \
    --output text)
  
  echo "\n=========================================="
  echo "Application deployed successfully!"
  echo "=========================================="
  echo "Access your application at: http://$LB_DNS"
  echo "Health check endpoint: http://$LB_DNS/health"
else
  echo "\n=========================================="
  echo "Application deployed successfully!"
  echo "=========================================="
  echo "Service is running but not publicly accessible."
  echo "Use AWS Console to access the service directly."
fi

echo "\nCloudWatch Logs: /ecs/minimalapi-app"
echo "=========================================="