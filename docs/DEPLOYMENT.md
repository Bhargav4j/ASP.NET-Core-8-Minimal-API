# ASP.NET Core 8.0 Application - AWS ECS Fargate Deployment Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Project Overview](#project-overview)
3. [Local Development Setup](#local-development-setup)
4. [Docker Deployment](#docker-deployment)
5. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
6. [ECS Fargate Setup](#ecs-fargate-setup)
7. [Building and Pushing Docker Images](#building-and-pushing-docker-images)
8. [ECS Task Definition Explained](#ecs-task-definition-explained)
9. [ECS Service Configuration](#ecs-service-configuration)
10. [ECS Fargate Deployment Walkthrough](#ecs-fargate-deployment-walkthrough)
11. [Monitoring and Logging](#monitoring-and-logging)
12. [Troubleshooting](#troubleshooting)
13. [Scaling and Management](#scaling-and-management)
14. [Security Considerations](#security-considerations)
15. [Performance Optimization](#performance-optimization)

---

## Prerequisites

### Required Tools
- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **AWS CLI v2** - [Installation Guide](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html)
- **AWS Account** with appropriate permissions
- **Git** (for version control)

### AWS Permissions Required
Your AWS IAM user/role needs the following permissions:
- `ecs:*` (ECS full access)
- `ecr:*` (ECR repository management)
- `iam:PassRole` (for ECS task execution role)
- `elasticloadbalancing:*` (for ALB/Target Groups)
- `logs:*` (for CloudWatch Logs)
- `ec2:DescribeVpcs`, `ec2:DescribeSubnets`, `ec2:DescribeSecurityGroups`

---

## Project Overview

**Application**: ASP.NET Core 8.0 Minimal API  
**Framework**: .NET 8.0  
**Application Type**: Web API  
**Default Port**: 8080  
**Health Endpoint**: `/health`  
**Deployment Target**: AWS ECS Fargate  

### Key Features
- Minimal API architecture
- Production-ready containerization
- AWS ECS Fargate deployment
- CloudWatch logging integration
- Application Load Balancer support
- Health check monitoring

---

## Local Development Setup

### 1. Clone and Build the Project

```bash
# Navigate to project directory
cd /modernize-data/studio-data/TNT1001/APP2550/transformed-code/725/studio-workspace/ASPNETCore8MinimalNoDBLLM

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run the application
dotnet run
```

### 2. Test the Application Locally

```bash
# Test the health endpoint
curl http://localhost:8080/health

# Test application endpoints
curl http://localhost:8080/api/your-endpoint
```

### 3. Development with Hot Reload

```bash
# Run with hot reload (Development mode)
dotnet watch run
```

---

## Docker Deployment

### 1. Build Docker Image Locally

```bash
# Build the Docker image
docker build -f Dockerfile -t aspnetcore8minimalnodbllm:latest .

# Verify the image
docker images | grep aspnetcore8minimalnodbllm
```

### 2. Run Container Locally

```bash
# Run the container
docker run -d \
  --name aspnetcore-app \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  aspnetcore8minimalnodbllm:latest

# Check container logs
docker logs aspnetcore-app

# Test the application
curl http://localhost:8080/health
```

### 3. Using Docker Compose

```bash
# Start application with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f

# Stop application
docker-compose down
```

---

## AWS ECS Fargate Prerequisites

### 1. Configure AWS CLI

```bash
# Configure AWS credentials
aws configure

# Enter your:
# - AWS Access Key ID
# - AWS Secret Access Key
# - Default region (e.g., us-east-1)
# - Default output format (json)

# Verify configuration
aws sts get-caller-identity
```

### 2. VPC and Network Setup

You'll need the following network resources:

#### Option A: Use Default VPC
```bash
# Get default VPC ID
aws ec2 describe-vpcs --filters "Name=isDefault,Values=true" --query "Vpcs[0].VpcId" --output text

# Get default subnets
aws ec2 describe-subnets --filters "Name=default-for-az,Values=true" --query "Subnets[*].SubnetId" --output text
```

#### Option B: Create New VPC (Recommended for Production)
```bash
# Create VPC
VPC_ID=$(aws ec2 create-vpc --cidr-block 10.0.0.0/16 --query 'Vpc.VpcId' --output text)

# Create subnets in different AZs
SUBNET_1=$(aws ec2 create-subnet --vpc-id $VPC_ID --cidr-block 10.0.1.0/24 --availability-zone us-east-1a --query 'Subnet.SubnetId' --output text)
SUBNET_2=$(aws ec2 create-subnet --vpc-id $VPC_ID --cidr-block 10.0.2.0/24 --availability-zone us-east-1b --query 'Subnet.SubnetId' --output text)

# Create Internet Gateway
IGW_ID=$(aws ec2 create-internet-gateway --query 'InternetGateway.InternetGatewayId' --output text)
aws ec2 attach-internet-gateway --vpc-id $VPC_ID --internet-gateway-id $IGW_ID

# Create route table and associate
RTB_ID=$(aws ec2 create-route-table --vpc-id $VPC_ID --query 'RouteTable.RouteTableId' --output text)
aws ec2 create-route --route-table-id $RTB_ID --destination-cidr-block 0.0.0.0/0 --gateway-id $IGW_ID
aws ec2 associate-route-table --subnet-id $SUBNET_1 --route-table-id $RTB_ID
aws ec2 associate-route-table --subnet-id $SUBNET_2 --route-table-id $RTB_ID
```

### 3. Create Security Group

```bash
# Create security group
SG_ID=$(aws ec2 create-security-group \
  --group-name aspnetcore-ecs-sg \
  --description "Security group for ASP.NET Core ECS tasks" \
  --vpc-id $VPC_ID \
  --query 'GroupId' \
  --output text)

# Allow inbound traffic on port 8080 (application)
aws ec2 authorize-security-group-ingress \
  --group-id $SG_ID \
  --protocol tcp \
  --port 8080 \
  --cidr 0.0.0.0/0

# Allow inbound traffic on port 80 (ALB)
aws ec2 authorize-security-group-ingress \
  --group-id $SG_ID \
  --protocol tcp \
  --port 80 \
  --cidr 0.0.0.0/0
```

### 4. Create IAM Roles

#### ECS Task Execution Role
```bash
# Create trust policy
cat > trust-policy.json <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ecs-tasks.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF

# Create role
aws iam create-role \
  --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://trust-policy.json

# Attach managed policy
aws iam attach-role-policy \
  --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

#### ECS Task Role (Optional - for application permissions)
```bash
# Create task role
aws iam create-role \
  --role-name ecsTaskRole \
  --assume-role-policy-document file://trust-policy.json

# Attach policies as needed (e.g., S3, DynamoDB access)
aws iam attach-role-policy \
  --role-name ecsTaskRole \
  --policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess
```

### 5. Create ECR Repository

```bash
# Create ECR repository
aws ecr create-repository \
  --repository-name aspnetcore8minimalnodbllm \
  --region us-east-1

# Get repository URI
REPO_URI=$(aws ecr describe-repositories \
  --repository-names aspnetcore8minimalnodbllm \
  --query 'repositories[0].repositoryUri' \
  --output text)

echo "ECR Repository URI: $REPO_URI"
```

---

## ECS Fargate Setup

### 1. Create ECS Cluster

```bash
# Create ECS cluster
aws ecs create-cluster \
  --cluster-name aspnetcore-cluster \
  --region us-east-1

# Verify cluster
aws ecs describe-clusters \
  --clusters aspnetcore-cluster \
  --region us-east-1
```

### 2. Create CloudWatch Log Group

```bash
# Create log group
aws logs create-log-group \
  --log-group-name /ecs/aspnetcore8minimalnodbllm \
  --region us-east-1

# Set retention policy (optional)
aws logs put-retention-policy \
  --log-group-name /ecs/aspnetcore8minimalnodbllm \
  --retention-in-days 7
```

---

## Building and Pushing Docker Images

### Linux/macOS

```bash
# Make script executable
chmod +x scripts/build-push.sh

# Run build and push script
./scripts/build-push.sh

# Follow prompts:
# 1. Select registry (AWS ECR or Docker Hub)
# 2. Enter registry details
# 3. Enter image tag (default: latest)
```

### Windows

```cmd
# Run build and push script
scripts\build-push.bat

# Follow prompts:
# 1. Select registry (AWS ECR or Docker Hub)
# 2. Enter registry details
# 3. Enter image tag (default: latest)
```

### Manual Build and Push to ECR

```bash
# Set variables
AWS_REGION="us-east-1"
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
REPO_NAME="aspnetcore8minimalnodbllm"
IMAGE_TAG="latest"

# Authenticate Docker to ECR
aws ecr get-login-password --region $AWS_REGION | \
  docker login --username AWS --password-stdin \
  $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Build image
docker build -f Dockerfile -t $REPO_NAME:$IMAGE_TAG .

# Tag image
docker tag $REPO_NAME:$IMAGE_TAG \
  $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPO_NAME:$IMAGE_TAG

# Push image
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPO_NAME:$IMAGE_TAG
```

---

## ECS Task Definition Explained

### Key Components

#### 1. Fargate Configuration
```json
{
  "requiresCompatibilities": ["FARGATE"],
  "networkMode": "awsvpc",
  "cpu": "512",
  "memory": "1024"
}
```

**Valid Fargate CPU/Memory Combinations:**
- CPU: 256 (.25 vCPU) → Memory: 512, 1024, 2048 MB
- CPU: 512 (.5 vCPU) → Memory: 1024, 2048, 3072, 4096 MB
- CPU: 1024 (1 vCPU) → Memory: 2048-8192 MB (1024 MB increments)
- CPU: 2048 (2 vCPU) → Memory: 4096-16384 MB (1024 MB increments)
- CPU: 4096 (4 vCPU) → Memory: 8192-30720 MB (1024 MB increments)

#### 2. Container Definition
```json
{
  "name": "aspnetcore8minimalnodbllm",
  "image": "{{IMAGE_URI}}",
  "essential": true,
  "portMappings": [
    {
      "containerPort": 8080,
      "protocol": "tcp"
    }
  ]
}
```

#### 3. Environment Variables
```json
"environment": [
  {
    "name": "ASPNETCORE_ENVIRONMENT",
    "value": "Production"
  },
  {
    "name": "ASPNETCORE_URLS",
    "value": "http://+:8080"
  }
]
```

#### 4. Logging Configuration
```json
"logConfiguration": {
  "logDriver": "awslogs",
  "options": {
    "awslogs-group": "/ecs/aspnetcore8minimalnodbllm",
    "awslogs-region": "us-east-1",
    "awslogs-stream-prefix": "ecs",
    "awslogs-create-group": "true"
  }
}
```

---

## ECS Service Configuration

### Key Service Settings

#### 1. Launch Type and Networking
```json
{
  "launchType": "FARGATE",
  "networkConfiguration": {
    "awsvpcConfiguration": {
      "subnets": ["subnet-xxx", "subnet-yyy"],
      "securityGroups": ["sg-xxx"],
      "assignPublicIp": "ENABLED"
    }
  }
}
```

#### 2. Load Balancer Integration
```json
{
  "loadBalancers": [
    {
      "targetGroupArn": "arn:aws:elasticloadbalancing:...",
      "containerName": "aspnetcore8minimalnodbllm",
      "containerPort": 8080
    }
  ],
  "healthCheckGracePeriodSeconds": 300
}
```

#### 3. Deployment Configuration
```json
{
  "deploymentConfiguration": {
    "maximumPercent": 200,
    "minimumHealthyPercent": 50,
    "deploymentCircuitBreaker": {
      "enable": true,
      "rollback": true
    }
  }
}
```

---

## ECS Fargate Deployment Walkthrough

### Automated Deployment

#### Linux/macOS
```bash
# Make script executable
chmod +x scripts/deploy-image.sh

# Run deployment script
./scripts/deploy-image.sh
```

#### Windows
```cmd
# Run deployment script
scripts\deploy-image.bat
```

### Script Prompts:
1. **AWS Region**: e.g., `us-east-1`
2. **ECS Cluster Name**: e.g., `aspnetcore-cluster`
3. **VPC ID**: e.g., `vpc-0abc123def456`
4. **Subnet IDs**: e.g., `subnet-0abc123,subnet-0def456`
5. **Security Group ID**: e.g., `sg-0abc123def`
6. **Docker Image URI**: Full ECR URI from build-push script
7. **Load Balancer**: y/n (script will auto-create ALB and Target Group)

### Manual Deployment Steps

#### 1. Register Task Definition
```bash
# Update task definition with your image URI
sed -i 's|{{IMAGE_URI}}|YOUR_IMAGE_URI|g' ecs/task-definition.json
sed -i 's|{{AWS_REGION}}|us-east-1|g' ecs/task-definition.json
sed -i 's|{{ACCOUNT_ID}}|YOUR_ACCOUNT_ID|g' ecs/task-definition.json

# Register task definition
aws ecs register-task-definition \
  --cli-input-json file://ecs/task-definition.json \
  --region us-east-1
```

#### 2. Create or Update Service
```bash
# Update service definition
sed -i 's|{{CLUSTER_NAME}}|aspnetcore-cluster|g' ecs/service-definition.json
sed -i 's|{{SUBNET_1}}|subnet-xxx|g' ecs/service-definition.json
sed -i 's|{{SUBNET_2}}|subnet-yyy|g' ecs/service-definition.json
sed -i 's|{{SECURITY_GROUP}}|sg-xxx|g' ecs/service-definition.json
sed -i 's|{{TARGET_GROUP_ARN}}|arn:aws:...|g' ecs/service-definition.json

# Create service
aws ecs create-service \
  --cli-input-json file://ecs/service-definition.json \
  --region us-east-1

# OR update existing service
aws ecs update-service \
  --cluster aspnetcore-cluster \
  --service aspnetcore8minimalnodbllm-service \
  --task-definition aspnetcore8minimalnodbllm-task \
  --force-new-deployment \
  --region us-east-1
```

#### 3. Wait for Service Stability
```bash
aws ecs wait services-stable \
  --cluster aspnetcore-cluster \
  --services aspnetcore8minimalnodbllm-service \
  --region us-east-1
```

---

## Monitoring and Logging

### CloudWatch Logs

```bash
# View real-time logs
aws logs tail /ecs/aspnetcore8minimalnodbllm --follow --region us-east-1

# View logs from specific time
aws logs tail /ecs/aspnetcore8minimalnodbllm \
  --since 1h \
  --format short \
  --region us-east-1

# Search logs
aws logs filter-log-events \
  --log-group-name /ecs/aspnetcore8minimalnodbllm \
  --filter-pattern "ERROR" \
  --region us-east-1
```

### Service Monitoring

```bash
# Check service status
aws ecs describe-services \
  --cluster aspnetcore-cluster \
  --services aspnetcore8minimalnodbllm-service \
  --region us-east-1

# List running tasks
aws ecs list-tasks \
  --cluster aspnetcore-cluster \
  --service-name aspnetcore8minimalnodbllm-service \
  --region us-east-1

# Describe specific task
aws ecs describe-tasks \
  --cluster aspnetcore-cluster \
  --tasks TASK_ARN \
  --region us-east-1
```

### CloudWatch Metrics

Key metrics to monitor:
- **CPUUtilization**: CPU usage percentage
- **MemoryUtilization**: Memory usage percentage
- **RunningTaskCount**: Number of running tasks
- **DesiredTaskCount**: Desired number of tasks

---

## Troubleshooting

### Common Issues and Solutions

#### 1. Task Fails to Start

**Symptoms**: Tasks immediately transition to STOPPED state

**Solutions**:
```bash
# Check stopped task reason
aws ecs describe-tasks \
  --cluster aspnetcore-cluster \
  --tasks TASK_ARN \
  --query 'tasks[0].stoppedReason' \
  --region us-east-1

# Common reasons:
# - Invalid CPU/memory combination
# - Image pull errors (check ECR permissions)
# - Application errors (check CloudWatch logs)
```

#### 2. Cannot Pull Image from ECR

**Solution**:
```bash
# Verify task execution role has ECR permissions
aws iam get-role-policy \
  --role-name ecsTaskExecutionRole \
  --policy-name AmazonECSTaskExecutionRolePolicy

# Verify image exists in ECR
aws ecr describe-images \
  --repository-name aspnetcore8minimalnodbllm \
  --region us-east-1
```

#### 3. Health Check Failures

**Solution**:
```bash
# Verify health endpoint responds
curl http://ALB_DNS_NAME/health

# Check target group health
aws elbv2 describe-target-health \
  --target-group-arn TARGET_GROUP_ARN \
  --region us-east-1

# Increase health check grace period
aws ecs update-service \
  --cluster aspnetcore-cluster \
  --service aspnetcore8minimalnodbllm-service \
  --health-check-grace-period-seconds 300 \
  --region us-east-1
```

#### 4. Service Not Accessible via ALB

**Solution**:
```bash
# Verify security group allows traffic
aws ec2 describe-security-groups \
  --group-ids SECURITY_GROUP_ID \
  --region us-east-1

# Check ALB listener configuration
aws elbv2 describe-listeners \
  --load-balancer-arn ALB_ARN \
  --region us-east-1

# Verify target group has healthy targets
aws elbv2 describe-target-health \
  --target-group-arn TARGET_GROUP_ARN \
  --region us-east-1
```

#### 5. High Memory or CPU Usage

**Solution**:
```bash
# Update task definition with more resources
# Edit ecs/task-definition.json:
# "cpu": "1024",
# "memory": "2048"

# Register new task definition and update service
aws ecs register-task-definition --cli-input-json file://ecs/task-definition.json
aws ecs update-service \
  --cluster aspnetcore-cluster \
  --service aspnetcore8minimalnodbllm-service \
  --task-definition aspnetcore8minimalnodbllm-task:NEW_REVISION \
  --force-new-deployment
```

---

## Scaling and Management

### Manual Scaling

```bash
# Scale to 5 tasks
aws ecs update-service \
  --cluster aspnetcore-cluster \
  --service aspnetcore8minimalnodbllm-service \
  --desired-count 5 \
  --region us-east-1
```

### Auto Scaling

```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/aspnetcore-cluster/aspnetcore8minimalnodbllm-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10 \
  --region us-east-1

# Create scaling policy (target tracking)
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/aspnetcore-cluster/aspnetcore8minimalnodbllm-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name cpu-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json \
  --region us-east-1
```

**scaling-policy.json**:
```json
{
  "TargetValue": 70.0,
  "PredefinedMetricSpecification": {
    "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
  },
  "ScaleInCooldown": 300,
  "ScaleOutCooldown": 60
}
```

### Rolling Updates

```bash
# Deploy new version with rolling update
aws ecs update-service \
  --cluster aspnetcore-cluster \
  --service aspnetcore8minimalnodbllm-service \
  --task-definition aspnetcore8minimalnodbllm-task:NEW_REVISION \
  --deployment-configuration "maximumPercent=200,minimumHealthyPercent=100" \
  --force-new-deployment \
  --region us-east-1
```

### Blue/Green Deployments

For blue/green deployments, use AWS CodeDeploy integration:

```bash
# Create CodeDeploy application
aws deploy create-application \
  --application-name aspnetcore-app \
  --compute-platform ECS

# Create deployment group (configure with ECS cluster, service, ALB)
aws deploy create-deployment-group \
  --application-name aspnetcore-app \
  --deployment-group-name aspnetcore-dg \
  --deployment-config-name CodeDeployDefault.ECSAllAtOnce \
  --ecs-services clusterName=aspnetcore-cluster,serviceName=aspnetcore8minimalnodbllm-service \
  --load-balancer-info targetGroupPairInfoList=[...]
```

---

## Security Considerations

### 1. Use Secrets Manager for Sensitive Data

```bash
# Store secret
aws secretsmanager create-secret \
  --name aspnetcore/production/appsettings \
  --secret-string '{"ConnectionString":"...","ApiKey":"..."}' \
  --region us-east-1

# Update task definition to use secrets
# Add to containerDefinitions:
"secrets": [
  {
    "name": "ConnectionString",
    "valueFrom": "arn:aws:secretsmanager:REGION:ACCOUNT:secret:aspnetcore/production/appsettings:ConnectionString::"
  }
]
```

### 2. Enable Container Insights

```bash
# Enable Container Insights for cluster
aws ecs update-cluster-settings \
  --cluster aspnetcore-cluster \
  --settings name=containerInsights,value=enabled \
  --region us-east-1
```

### 3. Implement Network Security

- Use private subnets for tasks (with NAT Gateway)
- Restrict security group ingress to ALB only
- Enable VPC Flow Logs for network monitoring

### 4. Implement IAM Best Practices

- Use separate task role for application permissions
- Follow principle of least privilege
- Rotate credentials regularly
- Enable MFA for AWS console access

---

## Performance Optimization

### 1. .NET Application Optimizations

**appsettings.Production.json**:
```json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100,
      "MaxRequestBodySize": 10485760,
      "KeepAliveTimeout": "00:02:00"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 2. Container Optimizations

- Use ReadyToRun (R2R) compilation for faster startup
- Enable tiered compilation
- Configure garbage collection appropriately

**Dockerfile additions**:
```dockerfile
# Use ReadyToRun
RUN dotnet publish -c Release -o /app/publish -r linux-x64 --self-contained false /p:PublishReadyToRun=true

# Set GC mode
ENV DOTNET_gcServer=1
ENV DOTNET_GCConserveMemory=5
```

### 3. ECS Task Placement

```bash
# Use task placement strategies for better distribution
aws ecs create-service \
  --placement-strategy type=spread,field=attribute:ecs.availability-zone \
  --placement-strategy type=spread,field=instanceId
```

### 4. Enable ALB Connection Draining

```bash
# Configure deregistration delay
aws elbv2 modify-target-group-attributes \
  --target-group-arn TARGET_GROUP_ARN \
  --attributes Key=deregistration_delay.timeout_seconds,Value=30
```

---

## Additional Resources

### AWS Documentation
- [ECS Fargate Documentation](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html)
- [ECS Task Definitions](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task_definitions.html)
- [ECS Service Auto Scaling](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/service-auto-scaling.html)

### .NET Documentation
- [ASP.NET Core Performance Best Practices](https://docs.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)
- [.NET Container Images](https://hub.docker.com/_/microsoft-dotnet-aspnet)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

### Monitoring and Observability
- [CloudWatch Container Insights](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/ContainerInsights.html)
- [AWS X-Ray Integration](https://docs.aws.amazon.com/xray/latest/devguide/xray-services-ecs.html)

---

## Support and Maintenance

For issues or questions:
1. Check CloudWatch logs first
2. Review ECS service events
3. Verify task definition and service configuration
4. Consult AWS documentation
5. Contact AWS Support if needed

---

**Last Updated**: December 26, 2025  
**Version**: 1.0  
**Target Platform**: AWS ECS Fargate  
**Application Framework**: ASP.NET Core 8.0