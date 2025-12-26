FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["MinimalAPIProject.csproj", "./"]
RUN dotnet restore "MinimalAPIProject.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "MinimalAPIProject.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MinimalAPIProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published files from publish stage
COPY --from=publish /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "MinimalAPIProject.dll"]