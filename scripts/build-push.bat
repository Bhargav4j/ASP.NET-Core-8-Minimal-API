@echo off
setlocal enabledelayedexpansion

echo ==========================================
echo     Docker Build and Push Script
echo ==========================================

:: Get project name, will be sanitized later
set /p PROJECT_NAME="Enter project name (default: minimalapi-app): "
if "!PROJECT_NAME!" == "" set PROJECT_NAME=minimalapi-app

:: Sanitize project name for Docker (using PowerShell)
for /f "delims=" %%i in ('powershell -command "'!PROJECT_NAME!' -replace '[^a-zA-Z0-9]', '-' -replace '-+', '-' -replace '^-|-$', '' | ForEach-Object { $_.ToLower() }"') do set IMAGE_NAME=%%i

echo Using sanitized image name: !IMAGE_NAME!

:: Select registry type
echo Select registry type:
echo 1. AWS ECR
echo 2. Docker Hub
set /p REGISTRY_TYPE="Enter selection (1-2): "

if "!REGISTRY_TYPE!" == "1" (
  :: AWS ECR
  echo.
  echo AWS ECR selected
  echo.
  
  :: Get AWS region
  set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
  
  :: Login to ECR
  echo Logging in to AWS ECR in region !AWS_REGION!...
  for /f "tokens=*" %%a in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%a
  set REGISTRY_URL=!ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com
  
  aws ecr get-login-password --region !AWS_REGION! | docker login --username AWS --password-stdin !REGISTRY_URL!
  if !ERRORLEVEL! neq 0 (
    echo ECR login failed
    exit /b 1
  )
  
  :: Create ECR repository if it doesn't exist
  set ECR_REPO=!IMAGE_NAME!
  echo Checking if ECR repository !ECR_REPO! exists...
  aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
  if !ERRORLEVEL! neq 0 (
    echo Creating ECR repository !ECR_REPO!...
    aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    if !ERRORLEVEL! neq 0 (
      echo Failed to create ECR repository
      exit /b 1
    )
  )
  
  :: Set full image name with registry
  set /p IMAGE_TAG="Enter image tag (default: latest): "
  if "!IMAGE_TAG!" == "" set IMAGE_TAG=latest
  for /f "delims=" %%i in ('powershell -command "'!IMAGE_TAG!' -replace '[^a-zA-Z0-9]', '-' -replace '-+', '-' -replace '^-|-$', '' | ForEach-Object { $_.ToLower() }"') do set IMAGE_TAG=%%i
  
  set FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!
  
) else (
  :: Docker Hub
  echo.
  echo Docker Hub selected
  echo.
  
  :: Get Docker Hub username
  set /p DOCKER_USERNAME="Enter Docker Hub username: "
  set /p DOCKER_PASSWORD="Enter Docker Hub password: "
  
  :: Login to Docker Hub
  echo Logging in to Docker Hub...
  echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
  if !ERRORLEVEL! neq 0 (
    echo Docker Hub login failed
    exit /b 1
  )
  
  :: Set full image name with registry
  set /p IMAGE_TAG="Enter image tag (default: latest): "
  if "!IMAGE_TAG!" == "" set IMAGE_TAG=latest
  for /f "delims=" %%i in ('powershell -command "'!IMAGE_TAG!' -replace '[^a-zA-Z0-9]', '-' -replace '-+', '-' -replace '^-|-$', '' | ForEach-Object { $_.ToLower() }"') do set IMAGE_TAG=%%i
  
  set FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!
)

echo.
echo Building Docker image: !FULL_IMAGE_NAME!
echo.

:: Build the Docker image
docker build -t !FULL_IMAGE_NAME! .
if !ERRORLEVEL! neq 0 (
  echo Docker build failed
  exit /b 1
)

echo.
echo Pushing Docker image to registry: !FULL_IMAGE_NAME!
echo.

:: Push the image to the registry
docker push !FULL_IMAGE_NAME!
if !ERRORLEVEL! neq 0 (
  echo Docker push failed
  exit /b 1
)

echo.
echo ==========================================
echo     Build and Push Completed
echo ==========================================
echo Image: !FULL_IMAGE_NAME!
echo ==========================================

endlocal