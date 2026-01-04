using Xunit;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPIProject.Endpoint;
using MinimalAPIProject.Service;
using MinimalAPIProject.Repository;

namespace MinimalAPIProject.Endpoint.Tests;

public class StudentApiEndpointTests
{
    [Fact]
    public void AddStudentApi_RegistersTimeProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var timeProvider = serviceProvider.GetService<TimeProvider>();
        Assert.NotNull(timeProvider);
    }

    [Fact]
    public void AddStudentApi_RegistersIStudentRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<IStudentRepository>();
        Assert.NotNull(repository);
    }

    [Fact]
    public void AddStudentApi_RegistersStudentRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<IStudentRepository>();
        Assert.IsType<StudentRepository>(repository);
    }

    [Fact]
    public void AddStudentApi_RegistersIStudentService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = serviceProvider.GetService<IStudentService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddStudentApi_RegistersStudentService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = serviceProvider.GetService<IStudentService>();
        Assert.IsType<StudentService>(service);
    }

    [Fact]
    public void AddStudentApi_RegistersRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStudentRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddStudentApi_RegistersServiceAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStudentService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddStudentApi_RegistersTimeProviderAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TimeProvider));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddStudentApi_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddStudentApi();

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddStudentApi_CanBeCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = serviceProvider.GetService<IStudentService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddStudentApi_TimeProviderIsSystemTimeProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();
        var timeProvider = serviceProvider.GetService<TimeProvider>();

        // Assert
        Assert.NotNull(timeProvider);
        Assert.Same(TimeProvider.System, timeProvider);
    }
}
