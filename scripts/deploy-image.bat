@echo off
setlocal enabledelayedexpansion

echo ===========================================
echo     AWS ECS Fargate Deployment Script
echo ===========================================

:: Collect deployment information
set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS cluster name (e.g., minimalapi-cluster): "
set /p SUBNETS_INPUT="Enter comma-separated subnet IDs (e.g., subnet-abc123,subnet-def456): "
set /p SECURITY_GROUP="Enter security group ID (e.g., sg-abc123): "
set /p IMAGE_URI="Enter Docker image URI (e.g., 123456789012.dkr.ecr.us-east-1.amazonaws.com/minimalapi-app:latest): "

:: Get AWS account ID
for /f "tokens=*" %%a in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%a

:: Check if ECS cluster exists, create if not
echo.
echo Checking if ECS cluster exists...
aws ecs describe-clusters --clusters !CLUSTER_NAME! --region !AWS_REGION! >nul 2>&1
if !ERRORLEVEL! neq 0 (
  echo Creating ECS cluster !CLUSTER_NAME!...
  aws ecs create-cluster --cluster-name !CLUSTER_NAME! --region !AWS_REGION!
)

:: Parse subnets
set SUBNET_ARRAY=!SUBNETS_INPUT:,= !
set i=0
for %%a in (!SUBNET_ARRAY!) do (
  if !i!==0 set SUBNET_1=%%a
  if !i!==1 set SUBNET_2=%%a
  set /a i+=1
)

:: If only one subnet provided, use it for both
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

:: Ask if load balancer is needed
set /p LOAD_BALANCER_NEEDED="Do you need a load balancer for this service? (y/n): "

if /i "!LOAD_BALANCER_NEEDED!"=="y" (
  echo.
  echo Setting up Application Load Balancer...
  
  :: Create load balancer
  set /p LB_NAME="Enter load balancer name (default: minimalapi-lb): "
  if "!LB_NAME!"=="" set LB_NAME=minimalapi-lb
  
  echo Creating load balancer: !LB_NAME!
  
  :: Get VPC ID from subnet
  for /f "tokens=*" %%v in ('aws ec2 describe-subnets --subnet-ids !SUBNET_1! --query "Subnets[0].VpcId" --output text --region !AWS_REGION!') do set VPC_ID=%%v
  
  :: Create load balancer
  for /f "tokens=*" %%a in ('aws elbv2 create-load-balancer --name !LB_NAME! --subnets !SUBNET_1! !SUBNET_2! --security-groups !SECURITY_GROUP! --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text') do set LB_ARN=%%a
  
  :: Create target group
  echo Creating target group: minimalapi-tg
  for /f "tokens=*" %%a in ('aws elbv2 create-target-group --name minimalapi-tg --protocol HTTP --port 80 --vpc-id !VPC_ID! --target-type ip --health-check-path /health --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 3 --unhealthy-threshold-count 3 --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text') do set TG_ARN=%%a
  
  :: Create listener
  echo Creating listener on port 80
  aws elbv2 create-listener --load-balancer-arn !LB_ARN! --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=!TG_ARN! --region !AWS_REGION!
  
  set TARGET_GROUP_ARN=!TG_ARN!
  
  :: Use service definition with load balancer
  copy ecs\service-definition.json ecs\service-definition-deploy.json
) else (
  echo.
  echo Skipping load balancer setup. Service will not be publicly accessible.
  
  :: Use PowerShell to remove loadBalancers section from service definition
  copy ecs\service-definition.json ecs\service-definition-deploy.json
  powershell -Command "(Get-Content ecs\service-definition-deploy.json) | Select-String -NotMatch '""loadBalancers"":|\[|\{|\}|\]|""targetGroupArn"":|""containerName"":|""containerPort"":' | Select-String -NotMatch '""healthCheckGracePeriodSeconds"":' | Set-Content ecs\service-definition-deploy.json"
  
  set TARGET_GROUP_ARN=""
)

:: Create directories if they don't exist
if not exist ecs mkdir ecs

:: Copy task definition template
copy ecs\task-definition.json ecs\task-definition-deploy.json

:: Replace placeholders in task definition
echo.
echo Configuring task definition...
powershell -Command "(Get-Content ecs\task-definition-deploy.json).replace('{{IMAGE_URI}}', '!IMAGE_URI!').replace('{{AWS_REGION}}', '!AWS_REGION!').replace('{{ACCOUNT_ID}}', '!ACCOUNT_ID!') | Set-Content ecs\task-definition-deploy.json"

:: Replace placeholders in service definition
echo Configuring service definition...
powershell -Command "(Get-Content ecs\service-definition-deploy.json).replace('{{CLUSTER_NAME}}', '!CLUSTER_NAME!').replace('{{TARGET_GROUP_ARN}}', '!TARGET_GROUP_ARN!').replace('{{SUBNET_1}}', '!SUBNET_1!').replace('{{SUBNET_2}}', '!SUBNET_2!').replace('{{SECURITY_GROUP}}', '!SECURITY_GROUP!') | Set-Content ecs\service-definition-deploy.json"

:: Register task definition
echo.
echo Registering task definition...
for /f "tokens=*" %%a in ('aws ecs register-task-definition --cli-input-json file://ecs/task-definition-deploy.json --region !AWS_REGION! --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%a

:: Check if service exists
for /f "tokens=*" %%a in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services minimalapi-service --region !AWS_REGION! --query "length(services[?length(serviceName)>0])" --output text') do set SERVICE_EXISTS=%%a

if "!SERVICE_EXISTS!"=="0" (
  :: Create service
  echo.
  echo Creating new ECS service...
  aws ecs create-service --cli-input-json file://ecs/service-definition-deploy.json --region !AWS_REGION!
) else (
  :: Update service
  echo.
  echo Updating existing ECS service with new task definition...
  aws ecs update-service --cluster !CLUSTER_NAME! --service minimalapi-service --task-definition !TASK_DEF_ARN! --force-new-deployment --region !AWS_REGION!
)

:: Wait for service to stabilize
echo.
echo Waiting for service to stabilize...
aws ecs wait services-stable --cluster !CLUSTER_NAME! --services minimalapi-service --region !AWS_REGION!

:: Describe service to verify deployment
echo.
echo Service deployment complete. Verifying details...
aws ecs describe-services --cluster !CLUSTER_NAME! --services minimalapi-service --region !AWS_REGION! --query "services[0].{Status:status,RunningCount:runningCount,DesiredCount:desiredCount}"

:: Display load balancer DNS if created
if /i "!LOAD_BALANCER_NEEDED!"=="y" (
  for /f "tokens=*" %%a in ('aws elbv2 describe-load-balancers --load-balancer-arns !LB_ARN! --region !AWS_REGION! --query "LoadBalancers[0].DNSName" --output text') do set LB_DNS=%%a
  
  echo.
  echo ===========================================
  echo Application deployed successfully!
  echo ===========================================
  echo Access your application at: http://!LB_DNS!
  echo Health check endpoint: http://!LB_DNS!/health
) else (
  echo.
  echo ===========================================
  echo Application deployed successfully!
  echo ===========================================
  echo Service is running but not publicly accessible.
  echo Use AWS Console to access the service directly.
)

echo.
echo CloudWatch Logs: /ecs/minimalapi-app
echo ===========================================

endlocal