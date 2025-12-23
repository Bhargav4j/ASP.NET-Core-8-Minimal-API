@echo off
setlocal enabledelayedexpansion

echo =====================================
echo AWS ECS Fargate Deployment Script
echo =====================================
echo.

set PROJECT_NAME=aspnetcore8minimal23
set TASK_FAMILY=!PROJECT_NAME!-task
set SERVICE_NAME=!PROJECT_NAME!-service
set CONTAINER_NAME=!PROJECT_NAME!

set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
set AWS_DEFAULT_REGION=!AWS_REGION!

set /p CLUSTER_NAME="Enter ECS Cluster Name (e.g., my-ecs-cluster): "

set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "

set /p SUBNET_IDS="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
for /f "tokens=1,2 delims=," %%a in ("!SUBNET_IDS!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "

set /p IMAGE_URI="Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/app:latest): "

echo.
echo Getting AWS Account ID...
for /f "delims=" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
echo Account ID: !ACCOUNT_ID!
echo.

echo Checking ECS cluster...
aws ecs describe-clusters --clusters !CLUSTER_NAME! --region !AWS_REGION! >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Cluster does not exist. Creating ECS cluster...
    aws ecs create-cluster --cluster-name !CLUSTER_NAME! --region !AWS_REGION!
    echo ECS cluster created successfully
)
echo.

set /p NEED_LB="Do you need a load balancer for this service? (y/n): "

if /i "!NEED_LB!"=="y" (
    echo Creating Application Load Balancer and Target Group...
    
    set ALB_NAME=!PROJECT_NAME!-alb
    echo Creating Application Load Balancer: !ALB_NAME!
    for /f "delims=" %%i in ('aws elbv2 create-load-balancer --name !ALB_NAME! --subnets !SUBNET_1! !SUBNET_2! --security-groups !SECURITY_GROUP! --scheme internet-facing --type application --ip-address-type ipv4 --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text') do set ALB_ARN=%%i
    
    echo ALB Created: !ALB_ARN!
    
    set TG_NAME=!PROJECT_NAME!-tg
    echo Creating Target Group: !TG_NAME!
    for /f "delims=" %%i in ('aws elbv2 create-target-group --name !TG_NAME! --protocol HTTP --port 8080 --vpc-id !VPC_ID! --target-type ip --health-check-enabled --health-check-protocol HTTP --health-check-path "/health" --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text') do set TARGET_GROUP_ARN=%%i
    
    echo Target Group Created: !TARGET_GROUP_ARN!
    
    echo Creating ALB Listener...
    for /f "delims=" %%i in ('aws elbv2 create-listener --load-balancer-arn !ALB_ARN! --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=!TARGET_GROUP_ARN! --region !AWS_REGION! --query "Listeners[0].ListenerArn" --output text') do set LISTENER_ARN=%%i
    
    echo Listener Created: !LISTENER_ARN!
    echo.
    
    for /f "delims=" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns !ALB_ARN! --region !AWS_REGION! --query "LoadBalancers[0].DNSName" --output text') do set ALB_DNS=%%i
) else (
    set TARGET_GROUP_ARN=
)

echo Preparing ECS task definition...
set TASK_DEF_FILE=ecs\task-definition.json

copy "!TASK_DEF_FILE!" "!TASK_DEF_FILE!.tmp" >nul

powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content '!TASK_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{AWS_REGION}}', '!AWS_REGION!' | Set-Content '!TASK_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!TASK_DEF_FILE!.tmp') -replace '{{ACCOUNT_ID}}', '!ACCOUNT_ID!' | Set-Content '!TASK_DEF_FILE!.tmp'"

echo Task definition prepared
echo.

echo Registering ECS task definition...
for /f "delims=" %%i in ('aws ecs register-task-definition --cli-input-json file://!TASK_DEF_FILE!.tmp --region !AWS_REGION! --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

echo Task definition registered: !TASK_DEF_ARN!
echo.

del "!TASK_DEF_FILE!.tmp"

echo Preparing ECS service definition...
set SERVICE_DEF_FILE=ecs\service-definition.json

copy "!SERVICE_DEF_FILE!" "!SERVICE_DEF_FILE!.tmp" >nul

powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{SUBNET_1}}', '!SUBNET_1!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{SUBNET_2}}', '!SUBNET_2!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"
powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"

if not "!TARGET_GROUP_ARN!"=="" (
    powershell -Command "(Get-Content '!SERVICE_DEF_FILE!.tmp') -replace '{{TARGET_GROUP_ARN}}', '!TARGET_GROUP_ARN!' | Set-Content '!SERVICE_DEF_FILE!.tmp'"
) else (
    powershell -Command "$content = Get-Content '!SERVICE_DEF_FILE!.tmp' -Raw; $content = $content -replace '(?s)\"loadBalancers\".*?\],', ''; $content = $content -replace '\"healthCheckGracePeriodSeconds\": 300,', ''; Set-Content '!SERVICE_DEF_FILE!.tmp' -Value $content"
)

echo Service definition prepared
echo.

echo Checking if ECS service exists...
for /f "delims=" %%i in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].serviceName" --output text 2^>nul') do set SERVICE_EXISTS=%%i

if "!SERVICE_EXISTS!"=="None" (
    echo Service does not exist. Creating new ECS service...
    aws ecs create-service --cli-input-json file://!SERVICE_DEF_FILE!.tmp --region !AWS_REGION!
    echo ECS service created successfully
) else (
    echo Service exists. Updating ECS service...
    aws ecs update-service --cluster !CLUSTER_NAME! --service !SERVICE_NAME! --task-definition !TASK_DEF_ARN! --region !AWS_REGION!
    echo ECS service updated successfully
)

echo.

del "!SERVICE_DEF_FILE!.tmp"

echo Waiting for service to become stable (this may take a few minutes)...
aws ecs wait services-stable --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!

echo Service is stable
echo.

echo Verifying deployment...
for /f "delims=" %%i in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].runningCount" --output text') do set RUNNING_TASKS=%%i

echo Running tasks: !RUNNING_TASKS!
echo.

echo =====================================
echo Deployment Completed Successfully
echo =====================================
echo Cluster: !CLUSTER_NAME!
echo Service: !SERVICE_NAME!
echo Task Definition: !TASK_DEF_ARN!
echo Running Tasks: !RUNNING_TASKS!

if not "!ALB_DNS!"=="" (
    echo Application URL: http://!ALB_DNS!
    echo Health Check: http://!ALB_DNS!/health
)

echo CloudWatch Logs: /ecs/!PROJECT_NAME!
echo.

echo Troubleshooting:
echo   - View logs: aws logs tail /ecs/!PROJECT_NAME! --follow
echo   - List tasks: aws ecs list-tasks --cluster !CLUSTER_NAME! --service-name !SERVICE_NAME!
echo   - Describe service: aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME!
echo.

endlocal