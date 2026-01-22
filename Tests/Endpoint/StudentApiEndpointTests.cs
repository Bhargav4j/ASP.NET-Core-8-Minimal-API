using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MinimalAPIProject.Endpoint;
using MinimalAPIProject.Repository;
using MinimalAPIProject.Service;

namespace MinimalAPIProject.Tests.Endpoint;

public class StudentApiEndpointTests
{
    [Fact]
    public void AddStudentApi_RegistersServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        StudentApiEndpoint.AddStudentApi(services);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(TimeProvider));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IStudentRepository));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IStudentService));
    }

    [Fact]
    public void AddStudentApi_RegistersTimeProviderAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        StudentApiEndpoint.AddStudentApi(services);

        // Assert
        var timeProviderDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TimeProvider));
        Assert.NotNull(timeProviderDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, timeProviderDescriptor.Lifetime);
    }

    [Fact]
    public void AddStudentApi_RegistersStudentRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        StudentApiEndpoint.AddStudentApi(services);

        // Assert
        var repositoryDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IStudentRepository));
        Assert.NotNull(repositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, repositoryDescriptor.Lifetime);
        Assert.Equal(typeof(StudentRepository), repositoryDescriptor.ImplementationType);
    }

    [Fact]
    public void AddStudentApi_RegistersStudentServiceAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        StudentApiEndpoint.AddStudentApi(services);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IStudentService));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
        Assert.Equal(typeof(StudentService), serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void AddStudentApi_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = StudentApiEndpoint.AddStudentApi(services);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddStudentApi_WithEmptyServices_CanAddServices()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        var result = StudentApiEndpoint.AddStudentApi(services);

        // Assert
        Assert.NotNull(result);
        Assert.True(services.Count >= 3);
    }

    [Fact]
    public void MapStudentApiRoutes_ReturnsEndpointRouteBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddStudentApi();
        var app = builder.Build();

        // Act
        var result = StudentApiEndpoint.MapStudentApiRoutes(app);

        // Assert
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    [Fact]
    public void AddStudentApi_MultipleCallsToAddStudentApi_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        StudentApiEndpoint.AddStudentApi(services);
        StudentApiEndpoint.AddStudentApi(services);

        // Assert
        Assert.True(services.Count >= 3);
    }

    [Fact]
    public void AddStudentApi_ServicesCanBeBuilt()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        StudentApiEndpoint.AddStudentApi(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider);
    }
}
