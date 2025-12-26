# ASP.NET Core 8 Minimal API - AWS ECS Fargate Deployment Guide

This guide provides comprehensive instructions for containerizing and deploying your ASP.NET Core 8 Minimal API application to AWS ECS Fargate.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Project Overview](#project-overview)
- [Local Development](#local-development)
- [Docker Deployment](#docker-deployment)
- [AWS ECS Fargate Deployment](#aws-ecs-fargate-deployment)
- [Configuration Management](#configuration-management)
- [Monitoring and Logging](#monitoring-and-logging)
- [Security Considerations](#security-considerations)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for local development and testing)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS CLI](https://aws.amazon.com/cli/) (configured with appropriate permissions)
- [AWS Account](https://aws.amazon.com/) with permissions to create the following resources:
  - Amazon ECR repositories
  - ECS clusters, task definitions, and services
  - IAM roles and policies
  - VPC, subnets, security groups
  - Application Load Balancers (optional)
  - CloudWatch Log Groups

### AWS Resources Required

- VPC with at least two subnets across different availability zones
- Security group that allows inbound HTTP/HTTPS traffic
- ECS task execution role (`ecsTaskExecutionRole`)

## Project Overview

This project is an ASP.NET Core 8 Minimal API application with the following features:

- Student management API with CRUD operations
- Health check endpoint at `/health`
- Swagger/OpenAPI support (in development environment)
- HTTPS redirection

## Local Development

### Running Locally with Docker

1. Build and run the Docker container:

   ```bash
   docker build -t minimalapi-app .
   docker run -p 8080:80 minimalapi-app
   ```

2. Access the application:
   - API: http://localhost:8080/api/students
   - Health check: http://localhost:8080/health
   - Swagger UI (development only): http://localhost:8080/swagger

### Using Docker Compose

1. Start the application using Docker Compose:

   ```bash
   docker-compose up --build
   ```

2. Access the application at the same endpoints as above.

## Docker Deployment

### Building and Pushing Docker Image

Use the provided scripts to build and push the Docker image to your registry of choice:

#### For Linux/macOS:

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### For Windows:

```cmd
scripts\build-push.bat
```

The script will prompt you for:
1. Project name (defaults to minimalapi-app)
2. Registry type (AWS ECR or Docker Hub)
3. Registry-specific details (region, credentials)
4. Image tag (defaults to latest)

## AWS ECS Fargate Deployment

### Prerequisites

1. Create an IAM role named `ecsTaskExecutionRole` with the following policies:
   - `AmazonECR-FullAccess`
   - `AmazonECS-FullAccess`
   - `CloudWatchLogsFullAccess`

2. Ensure your AWS CLI is configured with permissions to create ECS resources.

### Network Infrastructure

Your AWS account should have:

1. A VPC with at least two subnets in different availability zones
2. A security group that allows inbound HTTP traffic (port 80)

### Deploying to ECS Fargate

#### For Linux/macOS:

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### For Windows:

```cmd
scripts\deploy-image.bat
```

The deployment script will:

1. Prompt for AWS region, ECS cluster name, subnet IDs, security group ID, and Docker image URI
2. Create an ECS cluster if it doesn't exist
3. Optionally create an Application Load Balancer and target group
4. Register an ECS task definition
5. Create or update an ECS service
6. Wait for the service to stabilize
7. Display the service status and access information

### ECS Task Definition Explained

The task definition configures:

- Fargate launch type with 0.5 vCPU (512 CPU units) and 1GB memory
- Container configuration:
  - Port mapping: 80
  - Environment variables: `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_URLS=http://+:80`
  - CloudWatch logging

### ECS Service Configuration

The service configuration includes:

- Fargate launch type
- Desired count of 2 tasks
- Network configuration with VPC subnets and security groups
- Optional load balancer configuration
- Deployment configuration with maximum 200% and minimum 50% healthy percent

## Configuration Management

### Environment Variables

The application uses the following environment variables:

- `ASPNETCORE_ENVIRONMENT`: Sets the environment (Development, Staging, Production)
- `ASPNETCORE_URLS`: Configures the URLs and ports the application listens on

### Using Different Environments

To run the container with a specific environment:

```bash
docker run -e ASPNETCORE_ENVIRONMENT=Development -p 8080:80 minimalapi-app
```

## Monitoring and Logging

### CloudWatch Logs

The application logs are sent to CloudWatch Logs in the following group:

```
/ecs/minimalapi-app
```

To view logs in AWS Console:
1. Navigate to CloudWatch > Log Groups
2. Select the `/ecs/minimalapi-app` log group
3. Browse log streams for each container instance

### Health Checks

The application exposes a health check endpoint at `/health`. The ECS task definition and load balancer target group are configured to use this endpoint for health monitoring.

## Security Considerations

### Network Security

- The application runs with a non-root user in the container
- Only necessary ports (80) are exposed
- Consider using AWS WAF with your load balancer for additional security

### HTTPS

To enable HTTPS in production:

1. Obtain an SSL certificate (using AWS Certificate Manager)
2. Configure the load balancer with an HTTPS listener
3. Update the container's environment variables to include HTTPS configuration

## Troubleshooting

### Common Issues

#### Container Fails to Start

Check CloudWatch Logs for application errors:

```bash
aws logs get-log-events --log-group-name /ecs/minimalapi-app --log-stream-name <log-stream> --region <region>
```

#### Service Fails to Stabilize

1. Check if the task is being killed due to memory/CPU constraints:
   ```bash
   aws ecs describe-tasks --cluster <cluster-name> --tasks <task-id> --region <region>
   ```

2. Verify health check is passing:
   - Ensure `/health` endpoint is accessible
   - Check target group health in EC2 > Target Groups

#### Permission Issues

Verify that the `ecsTaskExecutionRole` has the necessary permissions:
- ECR access for pulling images
- CloudWatch Logs permissions for logging

#### Networking Issues

1. Verify the security group allows inbound traffic on port 80
2. Check that the subnets have internet access (through an Internet Gateway or NAT Gateway)
3. If using `assignPublicIp: DISABLED`, ensure NAT Gateway is configured

### Viewing Deployment Status

```bash
aws ecs describe-services --cluster <cluster-name> --services minimalapi-service --region <region>
```

## ASP.NET Core 8 Optimization Recommendations

1. **ReadyToRun (R2R) Compilation**: For improved startup performance:
   ```xml
   <PropertyGroup>
     <PublishReadyToRun>true</PublishReadyToRun>
   </PropertyGroup>
   ```

2. **Enable Trimming**: For smaller deployment size:
   ```xml
   <PropertyGroup>
     <PublishTrimmed>true</PublishTrimmed>
   </PropertyGroup>
   ```

3. **Use Self-Contained Deployment**: For complete package including runtime:
   ```bash
   dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
   ```

4. **Implement Structured Logging**: Consider adding Serilog for improved logging:
   ```csharp
   builder.Host.UseSerilog((context, services, configuration) => configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext());
   ```

5. **Add Application Insights**: For comprehensive monitoring in Azure:
   ```csharp
   builder.Services.AddApplicationInsightsTelemetry();
   ```