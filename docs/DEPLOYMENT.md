# ASP.NET Core 8.0 Minimal API - AWS ECS Fargate Deployment Guide

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development Setup](#local-development-setup)
3. [Docker Deployment](#docker-deployment)
4. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
5. [ECS Fargate Setup](#ecs-fargate-setup)
6. [ECS Task Definition Explained](#ecs-task-definition-explained)
7. [ECS Service Configuration](#ecs-service-configuration)
8. [Deployment Walkthrough](#deployment-walkthrough)
9. [Configuration Management](#configuration-management)
10. [Monitoring and Logging](#monitoring-and-logging)
11. [Troubleshooting](#troubleshooting)
12. [Security Considerations](#security-considerations)
13. [Scaling and Performance](#scaling-and-performance)

---

## Prerequisites

### Required Software

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **AWS CLI v2** - [Installation Guide](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- **Git** (optional) - For version control

### AWS Account Requirements

- Active AWS account with appropriate permissions
- AWS CLI configured with credentials (`aws configure`)
- Permissions for:
  - ECS (create clusters, services, task definitions)
  - ECR (create repositories, push images)
  - IAM (create/manage roles)
  - VPC (manage subnets, security groups)
  - CloudWatch (logs and monitoring)
  - Elastic Load Balancing (ALB and target groups)

---

## Local Development Setup

### Running the Application Locally

1. **Clone or navigate to the project directory:**

   ```bash
   cd /modernize-data/studio-data/TNT1001/APP2470/transformed-code/707/studio-workspace/ASPNETCore8Minimal23
   ```

2. **Restore dependencies:**

   ```bash
   dotnet restore
   ```

3. **Build the application:**

   ```bash
   dotnet build -c Release
   ```

4. **Run the application:**

   ```bash
   dotnet run
   ```

   The application will start on `http://localhost:5000` or `https://localhost:5001` by default.

5. **Test the health endpoint:**

   ```bash
   curl http://localhost:5000/health
   ```

### Configuration Files

- **appsettings.json** - Base configuration
- **appsettings.Development.json** - Development environment settings
- **appsettings.Production.json** - Production environment settings

---

## Docker Deployment

### Building the Docker Image Locally

1. **Build the Docker image:**

   ```bash
   docker build -t aspnetcore8minimal23:latest .
   ```

2. **Run the container locally:**

   ```bash
   docker run -d -p 8080:8080 --name aspnetcore8minimal23 aspnetcore8minimal23:latest
   ```

3. **Test the containerized application:**

   ```bash
   curl http://localhost:8080/health
   ```

4. **View logs:**

   ```bash
   docker logs aspnetcore8minimal23
   ```

5. **Stop and remove the container:**

   ```bash
   docker stop aspnetcore8minimal23
   docker rm aspnetcore8minimal23
   ```

### Using Docker Compose

1. **Start the application:**

   ```bash
   docker-compose up -d
   ```

2. **View logs:**

   ```bash
   docker-compose logs -f
   ```

3. **Stop the application:**

   ```bash
   docker-compose down
   ```

---

## AWS ECS Fargate Prerequisites

### 1. IAM Roles Setup

#### ECS Task Execution Role

This role allows ECS to pull container images and write logs.

**Create the role using AWS CLI:**

```bash
aws iam create-role \
  --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://trust-policy.json
```

**trust-policy.json:**

```json
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
```

**Attach the AWS managed policy:**

```bash
aws iam attach-role-policy \
  --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

#### ECS Task Role (Optional)

This role grants permissions to your application (e.g., access to S3, DynamoDB).

```bash
aws iam create-role \
  --role-name ecsTaskRole \
  --assume-role-policy-document file://trust-policy.json

# Attach policies as needed for your application
aws iam attach-role-policy \
  --role-name ecsTaskRole \
  --policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess
```

### 2. VPC and Networking Setup

#### Create VPC (if needed)

```bash
aws ec2 create-vpc --cidr-block 10.0.0.0/16
```

#### Create Subnets

Create at least two subnets in different availability zones:

```bash
# Subnet 1 (AZ 1)
aws ec2 create-subnet \
  --vpc-id vpc-xxxxxxxxx \
  --cidr-block 10.0.1.0/24 \
  --availability-zone us-east-1a

# Subnet 2 (AZ 2)
aws ec2 create-subnet \
  --vpc-id vpc-xxxxxxxxx \
  --cidr-block 10.0.2.0/24 \
  --availability-zone us-east-1b
```

#### Create Internet Gateway

```bash
aws ec2 create-internet-gateway
aws ec2 attach-internet-gateway \
  --vpc-id vpc-xxxxxxxxx \
  --internet-gateway-id igw-xxxxxxxxx
```

#### Create Route Table

```bash
aws ec2 create-route-table --vpc-id vpc-xxxxxxxxx
aws ec2 create-route \
  --route-table-id rtb-xxxxxxxxx \
  --destination-cidr-block 0.0.0.0/0 \
  --gateway-id igw-xxxxxxxxx
```

#### Create Security Group

```bash
aws ec2 create-security-group \
  --group-name aspnetcore8minimal23-sg \
  --description "Security group for ASP.NET Core application" \
  --vpc-id vpc-xxxxxxxxx

# Allow inbound traffic on port 8080
aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxxxxxx \
  --protocol tcp \
  --port 8080 \
  --cidr 0.0.0.0/0

# Allow inbound traffic on port 80 (for ALB)
aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxxxxxx \
  --protocol tcp \
  --port 80 \
  --cidr 0.0.0.0/0
```

### 3. CloudWatch Log Group

Create a log group for your application:

```bash
aws logs create-log-group --log-group-name /ecs/aspnetcore8minimal23
```

---

## ECS Fargate Setup

### Understanding ECS Components

- **Cluster** - Logical grouping of tasks or services
- **Task Definition** - Blueprint for your application (CPU, memory, container settings)
- **Service** - Maintains desired count of tasks and handles load balancing
- **Task** - Running instance of a task definition

### Valid Fargate CPU/Memory Combinations

| CPU (vCPU) | Memory (MB) |
|------------|-------------|
| 256 (.25)  | 512, 1024, 2048 |
| 512 (.5)   | 1024, 2048, 3072, 4096 |
| 1024 (1)   | 2048, 3072, 4096, 5120, 6144, 7168, 8192 |
| 2048 (2)   | 4096-16384 (increments of 1024) |
| 4096 (4)   | 8192-30720 (increments of 1024) |

**Default recommended values:**
- CPU: `512` (.5 vCPU)
- Memory: `1024` MB

---

## ECS Task Definition Explained

### Key Configuration Elements

```json
{
  "family": "aspnetcore8minimal23-task",
  "requiresCompatibilities": ["FARGATE"],
  "networkMode": "awsvpc",
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::ACCOUNT_ID:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::ACCOUNT_ID:role/ecsTaskRole",
  "containerDefinitions": [
    {
      "name": "aspnetcore8minimal23",
      "image": "IMAGE_URI",
      "essential": true,
      "portMappings": [{"containerPort": 8080, "protocol": "tcp"}],
      "environment": [
        {"name": "ASPNETCORE_ENVIRONMENT", "value": "Production"},
        {"name": "ASPNETCORE_URLS", "value": "http://+:8080"}
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/aspnetcore8minimal23",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ]
}
```

### Important Notes

- **networkMode**: Must be `awsvpc` for Fargate
- **requiresCompatibilities**: Must include `FARGATE`
- **executionRoleArn**: Required for pulling images and writing logs
- **taskRoleArn**: Optional, for application permissions
- **portMappings**: Only specify `containerPort` (no `hostPort` for Fargate)

---

## ECS Service Configuration

### Service Definition Overview

```json
{
  "serviceName": "aspnetcore8minimal23-service",
  "cluster": "my-ecs-cluster",
  "taskDefinition": "aspnetcore8minimal23-task",
  "desiredCount": 2,
  "launchType": "FARGATE",
  "networkConfiguration": {
    "awsvpcConfiguration": {
      "subnets": ["subnet-1", "subnet-2"],
      "securityGroups": ["sg-xxx"],
      "assignPublicIp": "ENABLED"
    }
  },
  "loadBalancers": [
    {
      "targetGroupArn": "arn:aws:elasticloadbalancing:...",
      "containerName": "aspnetcore8minimal23",
      "containerPort": 8080
    }
  ],
  "deploymentConfiguration": {
    "maximumPercent": 200,
    "minimumHealthyPercent": 50
  }
}
```

### Key Parameters

- **desiredCount**: Number of task instances to run
- **launchType**: Must be `FARGATE`
- **assignPublicIp**: Set to `ENABLED` if using public subnets
- **loadBalancers**: Optional, for ALB integration
- **deploymentConfiguration**: Controls rolling update behavior

---

## Deployment Walkthrough

### Step 1: Build and Push Docker Image

#### Option A: Using AWS ECR

**Linux/macOS:**

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

**Windows:**

```cmd
scripts\build-push.bat
```

Follow the prompts:
1. Select registry type: `1` (AWS ECR)
2. Enter AWS region (e.g., `us-east-1`)
3. Enter AWS account ID
4. Enter ECR repository name (default: `aspnetcore8minimal23`)
5. Enter image tag (default: `latest`)

#### Option B: Using Docker Hub

Follow the same script but select `2` for Docker Hub and provide your credentials.

### Step 2: Deploy to ECS Fargate

**Linux/macOS:**

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

**Windows:**

```cmd
scripts\deploy-image.bat
```

Follow the prompts:
1. Enter AWS region
2. Enter ECS cluster name (will be created if it doesn't exist)
3. Enter VPC ID
4. Enter subnet IDs (comma-separated)
5. Enter security group ID
6. Enter Docker image URI from Step 1
7. Choose whether to create a load balancer (y/n)

The script will:
- Create/verify ECS cluster
- Create ALB and target group (if selected)
- Register task definition
- Create or update ECS service
- Wait for service to become stable
- Display deployment status and URLs

### Step 3: Verify Deployment

#### Check Service Status

```bash
aws ecs describe-services \
  --cluster my-ecs-cluster \
  --services aspnetcore8minimal23-service \
  --query 'services[0].{Status:status,Running:runningCount,Desired:desiredCount}'
```

#### List Running Tasks

```bash
aws ecs list-tasks \
  --cluster my-ecs-cluster \
  --service-name aspnetcore8minimal23-service
```

#### Test Application

If using a load balancer:

```bash
curl http://YOUR-ALB-DNS-NAME/health
```

---

## Configuration Management

### Environment Variables

Set environment variables in the task definition:

```json
"environment": [
  {"name": "ASPNETCORE_ENVIRONMENT", "value": "Production"},
  {"name": "ASPNETCORE_URLS", "value": "http://+:8080"},
  {"name": "ConnectionStrings__DefaultConnection", "value": "..."}
]
```

### Using AWS Systems Manager Parameter Store

Store secrets securely:

```bash
aws ssm put-parameter \
  --name "/aspnetcore8minimal23/database/password" \
  --value "my-secret-password" \
  --type "SecureString"
```

Reference in task definition:

```json
"secrets": [
  {
    "name": "DATABASE_PASSWORD",
    "valueFrom": "/aspnetcore8minimal23/database/password"
  }
]
```

### Using AWS Secrets Manager

Store and rotate secrets:

```bash
aws secretsmanager create-secret \
  --name aspnetcore8minimal23/db-credentials \
  --secret-string '{"username":"admin","password":"secret"}'
```

Reference in task definition:

```json
"secrets": [
  {
    "name": "DB_CREDENTIALS",
    "valueFrom": "arn:aws:secretsmanager:region:account:secret:aspnetcore8minimal23/db-credentials"
  }
]
```

---

## Monitoring and Logging

### CloudWatch Logs

#### View Logs

```bash
aws logs tail /ecs/aspnetcore8minimal23 --follow
```

#### Filter Logs

```bash
aws logs filter-log-events \
  --log-group-name /ecs/aspnetcore8minimal23 \
  --filter-pattern "ERROR"
```

### CloudWatch Metrics

ECS automatically publishes metrics:
- CPUUtilization
- MemoryUtilization
- NetworkReceiveBytes
- NetworkTransmitBytes

#### View Metrics

```bash
aws cloudwatch get-metric-statistics \
  --namespace AWS/ECS \
  --metric-name CPUUtilization \
  --dimensions Name=ServiceName,Value=aspnetcore8minimal23-service Name=ClusterName,Value=my-ecs-cluster \
  --start-time 2024-01-01T00:00:00Z \
  --end-time 2024-01-01T23:59:59Z \
  --period 3600 \
  --statistics Average
```

### Application Insights (Recommended)

For ASP.NET Core applications, integrate Application Insights:

1. Install NuGet package:

   ```bash
   dotnet add package Microsoft.ApplicationInsights.AspNetCore
   ```

2. Configure in `Program.cs`:

   ```csharp
   builder.Services.AddApplicationInsightsTelemetry();
   ```

3. Set instrumentation key via environment variable:

   ```json
   {"name": "APPLICATIONINSIGHTS_CONNECTION_STRING", "value": "InstrumentationKey=..."}
   ```

---

## Troubleshooting

### Common Issues

#### 1. Task Fails to Start

**Symptom:** Tasks are constantly stopping and starting.

**Check:**
- Task definition CPU/memory combination is valid
- Execution role has correct permissions
- Image URI is correct and accessible
- Security group allows necessary traffic

**Debug:**

```bash
aws ecs describe-tasks \
  --cluster my-ecs-cluster \
  --tasks TASK_ARN \
  --query 'tasks[0].stoppedReason'
```

#### 2. Cannot Pull Image from ECR

**Symptom:** `CannotPullContainerError`

**Solution:**
- Verify execution role has `AmazonECSTaskExecutionRolePolicy`
- Ensure image exists in ECR:

  ```bash
  aws ecr describe-images --repository-name aspnetcore8minimal23
  ```

#### 3. Health Check Failures

**Symptom:** Tasks fail health checks and are replaced.

**Solution:**
- Verify health endpoint is accessible: `/health`
- Increase `startPeriod` in health check configuration
- Check application logs for startup errors
- Ensure container has `curl` installed (or modify health check)

#### 4. Service Not Accessible via Load Balancer

**Symptom:** ALB returns 502/504 errors.

**Solution:**
- Verify security group allows traffic from ALB to tasks
- Check target group health check settings
- Ensure tasks are registered as healthy targets:

  ```bash
  aws elbv2 describe-target-health --target-group-arn TARGET_GROUP_ARN
  ```

#### 5. Invalid CPU/Memory Combination

**Symptom:** `Invalid CPU or memory value specified`

**Solution:**
- Use valid Fargate combinations (see [Valid Fargate CPU/Memory Combinations](#valid-fargate-cpumemory-combinations))
- Default safe values: CPU `512`, Memory `1024`

### Useful Commands

#### Describe Task

```bash
aws ecs describe-tasks \
  --cluster my-ecs-cluster \
  --tasks TASK_ARN
```

#### View Task Logs

```bash
aws logs get-log-events \
  --log-group-name /ecs/aspnetcore8minimal23 \
  --log-stream-name ecs/aspnetcore8minimal23/TASK_ID
```

#### Force New Deployment

```bash
aws ecs update-service \
  --cluster my-ecs-cluster \
  --service aspnetcore8minimal23-service \
  --force-new-deployment
```

#### Stop Task

```bash
aws ecs stop-task \
  --cluster my-ecs-cluster \
  --task TASK_ARN \
  --reason "Manual restart"
```

---

## Security Considerations

### Container Security

1. **Non-root User**
   - Dockerfile creates and uses non-root user (`appuser`)
   - Limits potential damage from container compromise

2. **Read-only Root Filesystem** (Optional)
   - Add to container definition:

     ```json
     "readonlyRootFilesystem": true
     ```

3. **Security Group Rules**
   - Only open necessary ports
   - Restrict source IPs when possible
   - Use separate security groups for ALB and tasks

### IAM Best Practices

1. **Least Privilege**
   - Grant only necessary permissions to task and execution roles
   - Use separate roles for different applications

2. **Execution Role vs Task Role**
   - **Execution Role**: ECS agent permissions (pull images, write logs)
   - **Task Role**: Application permissions (access AWS services)

### Secrets Management

1. **Never hardcode secrets** in task definitions
2. Use AWS Secrets Manager or Parameter Store
3. Enable automatic secret rotation
4. Use IAM roles for AWS service authentication

### Network Security

1. **Private Subnets** (Recommended for production)
   - Place tasks in private subnets
   - Use NAT Gateway for outbound internet access
   - Only ALB in public subnets

2. **VPC Endpoints**
   - Use VPC endpoints for ECR, CloudWatch, Secrets Manager
   - Reduces data transfer costs
   - Improves security (no internet gateway needed)

---

## Scaling and Performance

### Auto Scaling

#### Application Auto Scaling

**Register scalable target:**

```bash
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/my-ecs-cluster/aspnetcore8minimal23-service \
  --min-capacity 2 \
  --max-capacity 10
```

**Create scaling policy (CPU-based):**

```bash
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/my-ecs-cluster/aspnetcore8minimal23-service \
  --policy-name cpu-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json
```

**scaling-policy.json:**

```json
{
  "TargetValue": 70.0,
  "PredefinedMetricSpecification": {
    "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
  },
  "ScaleOutCooldown": 60,
  "ScaleInCooldown": 300
}
```

### Performance Optimization

#### .NET Runtime Optimizations

1. **Server Garbage Collection**
   - Automatically enabled in containers
   - Optimized for throughput

2. **ReadyToRun (R2R) Images**
   - Pre-compiled ahead-of-time
   - Faster startup, larger image size

   ```bash
   dotnet publish -c Release -r linux-x64 --self-contained false /p:PublishReadyToRun=true
   ```

3. **Tiered Compilation**
   - Enabled by default in .NET 6+
   - Balances startup time and throughput

#### Fargate Platform Version

- Use `LATEST` platform version for latest performance improvements
- Fargate 1.4.0+ includes improved task launch times

### Cost Optimization

1. **Right-size CPU/Memory**
   - Monitor actual usage via CloudWatch
   - Start with smaller sizes and scale up if needed

2. **Use Fargate Spot** (for non-critical workloads)
   - Up to 70% cost savings
   - May be interrupted with 2-minute warning

3. **Reserved Capacity** (Savings Plans)
   - Commit to consistent usage for 1-3 years
   - Up to 50% savings

---

## Additional Resources

### AWS Documentation

- [Amazon ECS Developer Guide](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/)
- [AWS Fargate User Guide](https://docs.aws.amazon.com/AmazonECS/latest/userguide/)
- [Amazon ECR User Guide](https://docs.aws.amazon.com/AmazonECR/latest/userguide/)

### .NET on AWS

- [.NET on AWS](https://aws.amazon.com/developer/language/net/)
- [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/)
- [Deploying .NET Applications to AWS](https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/)

### Best Practices

- [ECS Best Practices Guide](https://docs.aws.amazon.com/AmazonECS/latest/bestpracticesguide/intro.html)
- [ASP.NET Core Performance Best Practices](https://docs.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)

---

## Support and Feedback

For issues or questions:

1. Check AWS Service Health Dashboard
2. Review CloudWatch logs and metrics
3. Consult AWS Support (if applicable)
4. Review .NET and ASP.NET Core documentation

---

**Last Updated:** 2024-12-23

**Version:** 1.0.0
