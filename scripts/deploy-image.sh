#!/bin/bash
set -e
set -o pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}AWS ECS Fargate Deployment Script${NC}"
echo -e "${GREEN}=====================================${NC}"
echo ""

# Project configuration
PROJECT_NAME="aspnetcore8minimal23"
TASK_FAMILY="${PROJECT_NAME}-task"
SERVICE_NAME="${PROJECT_NAME}-service"
CONTAINER_NAME="${PROJECT_NAME}"

# Prompt for AWS configuration
read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
export AWS_DEFAULT_REGION="$AWS_REGION"

read -p "Enter ECS Cluster Name (e.g., my-ecs-cluster): " CLUSTER_NAME

read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID

read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNET_IDS
IFS=',' read -ra SUBNET_ARRAY <<< "$SUBNET_IDS"
SUBNET_1="${SUBNET_ARRAY[0]}"
SUBNET_2="${SUBNET_ARRAY[1]:-$SUBNET_1}"

read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP

read -p "Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/app:latest): " IMAGE_URI

echo ""
echo -e "${YELLOW}Getting AWS Account ID...${NC}"
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo -e "${GREEN}Account ID: ${ACCOUNT_ID}${NC}"
echo ""

# Check/create ECS cluster
echo -e "${YELLOW}Checking ECS cluster...${NC}"
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo -e "${YELLOW}Cluster does not exist. Creating ECS cluster...${NC}"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
    echo -e "${GREEN}ECS cluster created successfully${NC}"
}
echo ""

# Load balancer configuration
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB

if [[ "$NEED_LB" =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Creating Application Load Balancer and Target Group...${NC}"
    
    # Create ALB
    ALB_NAME="${PROJECT_NAME}-alb"
    echo -e "${YELLOW}Creating Application Load Balancer: ${ALB_NAME}${NC}"
    ALB_ARN=$(aws elbv2 create-load-balancer \
        --name "$ALB_NAME" \
        --subnets "$SUBNET_1" "$SUBNET_2" \
        --security-groups "$SECURITY_GROUP" \
        --scheme internet-facing \
        --type application \
        --ip-address-type ipv4 \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text)
    
    echo -e "${GREEN}ALB Created: ${ALB_ARN}${NC}"
    
    # Create Target Group with target-type ip (required for Fargate awsvpc mode)
    TG_NAME="${PROJECT_NAME}-tg"
    echo -e "${YELLOW}Creating Target Group: ${TG_NAME}${NC}"
    TARGET_GROUP_ARN=$(aws elbv2 create-target-group \
        --name "$TG_NAME" \
        --protocol HTTP \
        --port 8080 \
        --vpc-id "$VPC_ID" \
        --target-type ip \
        --health-check-enabled \
        --health-check-protocol HTTP \
        --health-check-path "/health" \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text)
    
    echo -e "${GREEN}Target Group Created: ${TARGET_GROUP_ARN}${NC}"
    
    # Create Listener
    echo -e "${YELLOW}Creating ALB Listener...${NC}"
    LISTENER_ARN=$(aws elbv2 create-listener \
        --load-balancer-arn "$ALB_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" \
        --query 'Listeners[0].ListenerArn' \
        --output text)
    
    echo -e "${GREEN}Listener Created: ${LISTENER_ARN}${NC}"
    echo ""
    
    # Get ALB DNS name
    ALB_DNS=$(aws elbv2 describe-load-balancers \
        --load-balancer-arns "$ALB_ARN" \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].DNSName' \
        --output text)
else
    TARGET_GROUP_ARN=""
fi

# Prepare task definition
echo -e "${YELLOW}Preparing ECS task definition...${NC}"
TASK_DEF_FILE="ecs/task-definition.json"

cp "$TASK_DEF_FILE" "${TASK_DEF_FILE}.tmp"

sed -i "s|{{IMAGE_URI}}|${IMAGE_URI}|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{AWS_REGION}}|${AWS_REGION}|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{ACCOUNT_ID}}|${ACCOUNT_ID}|g" "${TASK_DEF_FILE}.tmp"

echo -e "${GREEN}Task definition prepared${NC}"
echo ""

# Register task definition
echo -e "${YELLOW}Registering ECS task definition...${NC}"
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://${TASK_DEF_FILE}.tmp \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

echo -e "${GREEN}Task definition registered: ${TASK_DEF_ARN}${NC}"
echo ""

# Clean up temp file
rm "${TASK_DEF_FILE}.tmp"

# Prepare service definition
echo -e "${YELLOW}Preparing ECS service definition...${NC}"
SERVICE_DEF_FILE="ecs/service-definition.json"

cp "$SERVICE_DEF_FILE" "${SERVICE_DEF_FILE}.tmp"

sed -i "s|{{CLUSTER_NAME}}|${CLUSTER_NAME}|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SUBNET_1}}|${SUBNET_1}|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SUBNET_2}}|${SUBNET_2}|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SECURITY_GROUP}}|${SECURITY_GROUP}|g" "${SERVICE_DEF_FILE}.tmp"

if [[ -n "$TARGET_GROUP_ARN" ]]; then
    sed -i "s|{{TARGET_GROUP_ARN}}|${TARGET_GROUP_ARN}|g" "${SERVICE_DEF_FILE}.tmp"
else
    # Remove loadBalancers section if no load balancer
    sed -i '/"loadBalancers"/,/],/d' "${SERVICE_DEF_FILE}.tmp"
    sed -i '/"healthCheckGracePeriodSeconds"/d' "${SERVICE_DEF_FILE}.tmp"
fi

echo -e "${GREEN}Service definition prepared${NC}"
echo ""

# Check if service exists
echo -e "${YELLOW}Checking if ECS service exists...${NC}"
SERVICE_EXISTS=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].serviceName' \
    --output text 2>/dev/null || echo "None")

if [[ "$SERVICE_EXISTS" == "None" ]] || [[ "$SERVICE_EXISTS" == "" ]]; then
    echo -e "${YELLOW}Service does not exist. Creating new ECS service...${NC}"
    aws ecs create-service \
        --cli-input-json file://${SERVICE_DEF_FILE}.tmp \
        --region "$AWS_REGION"
    echo -e "${GREEN}ECS service created successfully${NC}"
else
    echo -e "${YELLOW}Service exists. Updating ECS service...${NC}"
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --region "$AWS_REGION"
    echo -e "${GREEN}ECS service updated successfully${NC}"
fi

echo ""

# Clean up temp file
rm "${SERVICE_DEF_FILE}.tmp"

# Wait for service stability
echo -e "${YELLOW}Waiting for service to become stable (this may take a few minutes)...${NC}"
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

echo -e "${GREEN}Service is stable${NC}"
echo ""

# Verify deployment
echo -e "${YELLOW}Verifying deployment...${NC}"
RUNNING_TASKS=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].runningCount' \
    --output text)

echo -e "${GREEN}Running tasks: ${RUNNING_TASKS}${NC}"
echo ""

echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}Deployment Completed Successfully${NC}"
echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}Cluster: ${CLUSTER_NAME}${NC}"
echo -e "${GREEN}Service: ${SERVICE_NAME}${NC}"
echo -e "${GREEN}Task Definition: ${TASK_DEF_ARN}${NC}"
echo -e "${GREEN}Running Tasks: ${RUNNING_TASKS}${NC}"

if [[ -n "$ALB_DNS" ]]; then
    echo -e "${GREEN}Application URL: http://${ALB_DNS}${NC}"
    echo -e "${GREEN}Health Check: http://${ALB_DNS}/health${NC}"
fi

echo -e "${GREEN}CloudWatch Logs: /ecs/${PROJECT_NAME}${NC}"
echo ""

echo -e "${YELLOW}Troubleshooting:${NC}"
echo "  - View logs: aws logs tail /ecs/${PROJECT_NAME} --follow"
echo "  - List tasks: aws ecs list-tasks --cluster ${CLUSTER_NAME} --service-name ${SERVICE_NAME}"
echo "  - Describe service: aws ecs describe-services --cluster ${CLUSTER_NAME} --services ${SERVICE_NAME}"
echo ""