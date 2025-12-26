@echo off
setlocal enabledelayedexpansion

echo ==========================================
echo   AWS ECS Fargate Deployment Script
echo ==========================================
echo.

rem Project configuration
set "PROJECT_NAME=aspnetcore8minimalnodbllm"
set "TASK_FAMILY=!PROJECT_NAME!-task"
set "SERVICE_NAME=!PROJECT_NAME!-service"
set "CONTAINER_NAME=!PROJECT_NAME!"
set "CONTAINER_PORT=8080"

rem Prompt for AWS configuration
echo --- AWS Configuration ---
set /p "AWS_REGION=Enter AWS Region (e.g., us-east-1): "
set /p "CLUSTER_NAME=Enter ECS Cluster Name (e.g., my-ecs-cluster): "

rem Get AWS Account ID
echo.
echo Retrieving AWS Account ID...
for /f "delims=" %%i in ('aws sts get-caller-identity --query Account --output text') do set "ACCOUNT_ID=%%i"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to retrieve AWS Account ID. Check your AWS credentials.
    exit /b 1
)
echo AWS Account ID: !ACCOUNT_ID!

rem Check if cluster exists
echo.
echo Checking if ECS cluster exists...
aws ecs describe-clusters --clusters !CLUSTER_NAME! --region !AWS_REGION! >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Cluster does not exist. Creating ECS cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name !CLUSTER_NAME! --region !AWS_REGION!
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Failed to create ECS cluster
        exit /b 1
    )
)

rem Prompt for network configuration
echo.
echo --- Network Configuration ---
set /p "VPC_ID=Enter VPC ID (e.g., vpc-0abc123def456): "
set /p "SUBNET_INPUT=Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p "SECURITY_GROUP=Enter Security Group ID (e.g., sg-0abc123def): "

rem Parse subnets
for /f "tokens=1,2 delims=," %%a in ("!SUBNET_INPUT!") do (
    set "SUBNET_1=%%a"
    set "SUBNET_2=%%b"
)
if "!SUBNET_2!"=="" set "SUBNET_2=!SUBNET_1!"

rem Prompt for Docker image URI
echo.
echo --- Docker Image Configuration ---
set /p "IMAGE_URI=Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/app:latest): "

rem Load balancer configuration
echo.
set /p "NEED_LB=Do you need a load balancer for this service? (y/n): "

if /i "!NEED_LB!"=="y" (
    echo.
    echo Creating Application Load Balancer and Target Group...
    
    set "ALB_NAME=!PROJECT_NAME!-alb"
    echo Creating ALB: !ALB_NAME!
    for /f "delims=" %%i in ('aws elbv2 create-load-balancer --name !ALB_NAME! --subnets !SUBNET_1! !SUBNET_2! --security-groups !SECURITY_GROUP! --scheme internet-facing --type application --ip-address-type ipv4 --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set "ALB_ARN=%%i"
    
    if "!ALB_ARN!"=="" (
        echo WARNING: Could not create new ALB. Please provide existing ALB ARN:
        set /p "ALB_ARN=Enter ALB ARN: "
    )
    
    set "TG_NAME=!PROJECT_NAME!-tg"
    echo Creating Target Group: !TG_NAME!
    for /f "delims=" %%i in ('aws elbv2 create-target-group --name !TG_NAME! --protocol HTTP --port !CONTAINER_PORT! --vpc-id !VPC_ID! --target-type ip --health-check-path "/health" --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set "TARGET_GROUP_ARN=%%i"
    
    if "!TARGET_GROUP_ARN!"=="" (
        echo WARNING: Could not create new Target Group. Please provide existing Target Group ARN:
        set /p "TARGET_GROUP_ARN=Enter Target Group ARN: "
    )
    
    echo Creating ALB Listener...
    aws elbv2 create-listener --load-balancer-arn !ALB_ARN! --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=!TARGET_GROUP_ARN! --region !AWS_REGION! >nul 2>&1
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    set "USE_LOAD_BALANCER=true"
) else (
    set "USE_LOAD_BALANCER=false"
)

rem Prepare task definition
echo.
echo Preparing ECS Task Definition...
set "TASK_DEF_FILE=ecs\task-definition.json"
set "TASK_DEF_TEMP=%TEMP%\task-definition-temp.json"

copy /y "!TASK_DEF_FILE!" "!TASK_DEF_TEMP!" >nul

rem Replace placeholders using PowerShell
powershell -Command "(Get-Content '!TASK_DEF_TEMP!') -replace '{{IMAGE_URI}}','!IMAGE_URI!' -replace '{{AWS_REGION}}','!AWS_REGION!' -replace '{{ACCOUNT_ID}}','!ACCOUNT_ID!' | Set-Content '!TASK_DEF_TEMP!'"

rem Register task definition
echo Registering ECS Task Definition...
for /f "delims=" %%i in ('aws ecs register-task-definition --cli-input-json file://!TASK_DEF_TEMP! --region !AWS_REGION! --query "taskDefinition.taskDefinitionArn" --output text') do set "TASK_DEF_ARN=%%i"

if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to register task definition
    exit /b 1
)

echo Task Definition ARN: !TASK_DEF_ARN!

rem Prepare service definition
echo.
echo Preparing ECS Service Definition...
set "SERVICE_DEF_FILE=ecs\service-definition.json"
set "SERVICE_DEF_TEMP=%TEMP%\service-definition-temp.json"

copy /y "!SERVICE_DEF_FILE!" "!SERVICE_DEF_TEMP!" >nul

rem Replace placeholders
powershell -Command "(Get-Content '!SERVICE_DEF_TEMP!') -replace '{{CLUSTER_NAME}}','!CLUSTER_NAME!' -replace '{{SUBNET_1}}','!SUBNET_1!' -replace '{{SUBNET_2}}','!SUBNET_2!' -replace '{{SECURITY_GROUP}}','!SECURITY_GROUP!' -replace '{{TARGET_GROUP_ARN}}','!TARGET_GROUP_ARN!' | Set-Content '!SERVICE_DEF_TEMP!'"

rem Remove loadBalancers if not needed
if "!USE_LOAD_BALANCER!"=="false" (
    echo Removing load balancer configuration...
    powershell -Command "$json = Get-Content '!SERVICE_DEF_TEMP!' | ConvertFrom-Json; $json.PSObject.Properties.Remove('loadBalancers'); $json.PSObject.Properties.Remove('healthCheckGracePeriodSeconds'); $json | ConvertTo-Json -Depth 10 | Set-Content '!SERVICE_DEF_TEMP!'"
)

rem Check if service exists
echo.
echo Checking if ECS service exists...
for /f "delims=" %%i in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].serviceName" --output text 2^>nul') do set "EXISTING_SERVICE=%%i"

if "!EXISTING_SERVICE!"=="!SERVICE_NAME!" (
    echo Service exists. Updating ECS service...
    aws ecs update-service --cluster !CLUSTER_NAME! --service !SERVICE_NAME! --task-definition !TASK_DEF_ARN! --force-new-deployment --region !AWS_REGION! >nul
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Failed to update ECS service
        exit /b 1
    )
) else (
    echo Service does not exist. Creating ECS service...
    aws ecs create-service --cli-input-json file://!SERVICE_DEF_TEMP! --region !AWS_REGION! >nul
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Failed to create ECS service
        exit /b 1
    )
)

rem Wait for service stability
echo.
echo Waiting for service to become stable...
aws ecs wait services-stable --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!

if !ERRORLEVEL! neq 0 (
    echo WARNING: Service did not stabilize within expected time
) else (
    echo Service is stable
)

rem Verify deployment
echo.
echo Verifying deployment...
for /f "delims=" %%i in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].runningCount" --output text') do set "RUNNING_COUNT=%%i"

echo Running tasks: !RUNNING_COUNT!

rem Display service information
echo.
echo ==========================================
echo Deployment Completed Successfully!
echo ==========================================
echo Cluster: !CLUSTER_NAME!
echo Service: !SERVICE_NAME!
echo Task Definition: !TASK_DEF_ARN!
echo Running Tasks: !RUNNING_COUNT!
echo.

if "!USE_LOAD_BALANCER!"=="true" (
    for /f "delims=" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns !ALB_ARN! --region !AWS_REGION! --query "LoadBalancers[0].DNSName" --output text 2^>nul') do set "ALB_DNS=%%i"
    if not "!ALB_DNS!"=="" (
        echo Application URL: http://!ALB_DNS!
        echo Health Check: http://!ALB_DNS!/health
        echo.
    )
)

echo CloudWatch Logs: /ecs/!PROJECT_NAME!
echo.
echo To view service details:
echo aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!
echo.

endlocal