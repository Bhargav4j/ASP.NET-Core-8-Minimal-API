using Xunit;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPIProject.Endpoint;
using MinimalAPIProject.Repository;
using MinimalAPIProject.Service;

namespace MinimalAPIProject.Endpoint.Tests;

public class StudentApiEndpointTests
{
    [Fact]
    public void AddStudentApi_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddStudentApi();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(services, s => s.ServiceType == typeof(TimeProvider));
        Assert.Contains(services, s => s.ServiceType == typeof(IStudentRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IStudentService));
    }

    [Fact]
    public void AddStudentApi_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddStudentApi();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterTimeProviderAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TimeProvider));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterStudentRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IStudentRepository));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(StudentRepository), descriptor.ImplementationType);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterStudentServiceAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IStudentService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(StudentService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterCorrectImplementations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var provider = services.BuildServiceProvider();

        // Assert
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetService<IStudentRepository>();
        var service = scope.ServiceProvider.GetService<IStudentService>();

        Assert.NotNull(repository);
        Assert.IsType<StudentRepository>(repository);
        Assert.NotNull(service);
        Assert.IsType<StudentService>(service);
    }

    [Fact]
    public void AddStudentApi_ShouldAllowMultipleCalls()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        services.AddStudentApi();

        // Assert
        var repositoryDescriptors = services.Where(s => s.ServiceType == typeof(IStudentRepository)).ToList();
        Assert.True(repositoryDescriptors.Count >= 1);
    }

    [Fact]
    public void AddStudentApi_ShouldWorkWithEmptyServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddStudentApi();

        // Assert
        Assert.NotEmpty(services);
        Assert.True(services.Count >= 3);
    }

    [Fact]
    public void AddStudentApi_ShouldWorkWithExistingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<string>("test");

        // Act
        services.AddStudentApi();

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(string));
        Assert.Contains(services, s => s.ServiceType == typeof(IStudentService));
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterSystemTimeProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var provider = services.BuildServiceProvider();

        // Assert
        var timeProvider = provider.GetService<TimeProvider>();
        Assert.NotNull(timeProvider);
        Assert.Same(TimeProvider.System, timeProvider);
    }
}
