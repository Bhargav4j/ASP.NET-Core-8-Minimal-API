@echo off
setlocal enabledelayedexpansion

echo =====================================
echo Docker Build and Push Script
echo =====================================
echo.

set PROJECT_NAME=ASPNETCore8Minimal23

for /f "delims=" %%i in ('powershell -Command "'%PROJECT_NAME%'.ToLower() -replace '[^a-z0-9]+','-' -replace '^-+','' -replace '-+$',''"') do set IMAGE_NAME=%%i
echo Sanitized image name: !IMAGE_NAME!
echo.

set /p IMAGE_TAG="Enter image tag (default: latest): "
if "!IMAGE_TAG!"==" " set IMAGE_TAG=latest
for /f "delims=" %%i in ('powershell -Command "'!IMAGE_TAG!'.ToLower() -replace '[^a-z0-9.-]+','-' -replace '^-+','' -replace '-+$',''"') do set IMAGE_TAG=%%i
echo Using tag: !IMAGE_TAG!
echo.

echo Select container registry:
echo 1. AWS ECR
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter choice (1 or 2): "
echo.

if "!REGISTRY_CHOICE!"=="1" (
    echo Selected: AWS ECR
    echo.
    
    set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
    set /p AWS_ACCOUNT_ID="Enter AWS Account ID: "
    set /p ECR_REPO="Enter ECR Repository Name (default: !IMAGE_NAME!): "
    if "!ECR_REPO!"=="" set ECR_REPO=!IMAGE_NAME!
    
    set REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com
    set FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!
    
    echo.
    echo Logging into AWS ECR...
    for /f "delims=" %%p in ('aws ecr get-login-password --region !AWS_REGION!') do set ECR_PASSWORD=%%p
    echo !ECR_PASSWORD! | docker login --username AWS --password-stdin !REGISTRY_URL!
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to login to AWS ECR
        exit /b 1
    )
    
    echo Successfully logged into AWS ECR
    echo.
    
    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Repository does not exist. Creating ECR repository...
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
        echo ECR repository created successfully
    )
    echo.
    
) else if "!REGISTRY_CHOICE!"=="2" (
    echo Selected: Docker Hub
    echo.
    
    set /p DOCKER_USERNAME="Enter Docker Hub Username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub Password or Access Token: "
    
    set FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!
    
    echo.
    echo Logging into Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    
    if !ERRORLEVEL! neq 0 (
        echo Failed to login to Docker Hub
        exit /b 1
    )
    
    echo Successfully logged into Docker Hub
    echo.
) else (
    echo Invalid choice. Exiting.
    exit /b 1
)

echo Building Docker image: !FULL_IMAGE_NAME!
echo.
docker build -f Dockerfile -t !FULL_IMAGE_NAME! .

if !ERRORLEVEL! neq 0 (
    echo Docker build failed
    exit /b 1
)

echo.
echo Docker image built successfully
echo.

echo Pushing Docker image to registry...
docker push !FULL_IMAGE_NAME!

if !ERRORLEVEL! neq 0 (
    echo Docker push failed
    exit /b 1
)

echo.
echo =====================================
echo Build and Push Completed Successfully
echo =====================================
echo Image: !FULL_IMAGE_NAME!
echo.

endlocal