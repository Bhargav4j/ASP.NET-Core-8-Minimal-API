using Xunit;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPIProject.Endpoint;
using MinimalAPIProject.Repository;
using MinimalAPIProject.Service;

namespace MinimalAPIProject.Endpoint.Tests;

public class StudentApiEndpointTests
{
    [Fact]
    public void AddStudentApi_ShouldRegisterTimeProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();
        var timeProvider = serviceProvider.GetService<TimeProvider>();

        // Assert
        Assert.NotNull(timeProvider);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterIStudentRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetService<IStudentRepository>();

        // Assert
        Assert.NotNull(repository);
        Assert.IsType<StudentRepository>(repository);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterIStudentService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IStudentService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<StudentService>(service);
    }

    [Fact]
    public void AddStudentApi_ShouldReturnServiceCollection()
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
    public void AddStudentApi_ShouldRegisterRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var repo1 = scope1.ServiceProvider.GetService<IStudentRepository>();
        var repo2 = scope2.ServiceProvider.GetService<IStudentRepository>();

        // Assert
        Assert.NotNull(repo1);
        Assert.NotNull(repo2);
        Assert.NotSame(repo1, repo2);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterServiceAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var service1 = scope1.ServiceProvider.GetService<IStudentService>();
        var service2 = scope2.ServiceProvider.GetService<IStudentService>();

        // Assert
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterTimeProviderAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        var timeProvider1 = serviceProvider.GetService<TimeProvider>();
        var timeProvider2 = serviceProvider.GetService<TimeProvider>();

        // Assert
        Assert.NotNull(timeProvider1);
        Assert.NotNull(timeProvider2);
        Assert.Same(timeProvider1, timeProvider2);
    }

    [Fact]
    public void AddStudentApi_MultipleCallsShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Record.Exception(() =>
        {
            services.AddStudentApi();
            services.AddStudentApi();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void AddStudentApi_ShouldAllowServiceResolution()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
        var service = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        Assert.NotNull(repository);
        Assert.NotNull(service);
        Assert.NotNull(timeProvider);
    }

    [Fact]
    public void AddStudentApi_ServiceShouldReceiveRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IStudentService>();
        Assert.NotNull(service);
        Assert.IsType<StudentService>(service);
    }

    [Fact]
    public void AddStudentApi_ShouldRegisterSystemTimeProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStudentApi();
        var serviceProvider = services.BuildServiceProvider();
        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

        // Assert
        Assert.NotNull(timeProvider);
        Assert.Equal(TimeProvider.System, timeProvider);
    }
}
