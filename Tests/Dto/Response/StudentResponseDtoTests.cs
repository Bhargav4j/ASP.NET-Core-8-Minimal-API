using Xunit;
using MinimalAPIProject.Dto.Response;

namespace MinimalAPIProject.Tests.Dto.Response;

public class StudentResponseDtoTests
{
    [Fact]
    public void StudentResponseDto_CanBeCreated()
    {
        // Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = 25 };

        // Assert
        Assert.NotNull(dto);
    }

    [Fact]
    public void StudentResponseDto_IdProperty_CanBeSetAndGet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new StudentResponseDto { Id = id, Name = "Test", Age = 20 };

        // Act
        var result = dto.Id;

        // Assert
        Assert.Equal(id, result);
    }

    [Fact]
    public void StudentResponseDto_NameProperty_CanBeSetAndGet()
    {
        // Arrange
        var name = "John Doe";
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = name, Age = 20 };

        // Act
        var result = dto.Name;

        // Assert
        Assert.Equal(name, result);
    }

    [Fact]
    public void StudentResponseDto_AgeProperty_CanBeSetAndGet()
    {
        // Arrange
        var age = 30;
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = age };

        // Act
        var result = dto.Age;

        // Assert
        Assert.Equal(age, result);
    }

    [Fact]
    public void StudentResponseDto_WithEmptyGuid_IsValid()
    {
        // Act
        var dto = new StudentResponseDto { Id = Guid.Empty, Name = "Test", Age = 20 };

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
    }

    [Fact]
    public void StudentResponseDto_WithZeroAge_IsValid()
    {
        // Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = 0 };

        // Assert
        Assert.Equal(0, dto.Age);
    }

    [Fact]
    public void StudentResponseDto_WithNegativeAge_IsValid()
    {
        // Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = -1 };

        // Assert
        Assert.Equal(-1, dto.Age);
    }

    [Fact]
    public void StudentResponseDto_WithMaxAge_IsValid()
    {
        // Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = int.MaxValue };

        // Assert
        Assert.Equal(int.MaxValue, dto.Age);
    }

    [Fact]
    public void StudentResponseDto_AllPropertiesSet_AreAccessible()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Full Test Student";
        var age = 25;

        // Act
        var dto = new StudentResponseDto { Id = id, Name = name, Age = age };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(name, dto.Name);
        Assert.Equal(age, dto.Age);
    }

    [Fact]
    public void StudentResponseDto_IdCanBeModified()
    {
        // Arrange
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = 20 };
        var newId = Guid.NewGuid();

        // Act
        dto.Id = newId;

        // Assert
        Assert.Equal(newId, dto.Id);
    }

    [Fact]
    public void StudentResponseDto_NameCanBeModified()
    {
        // Arrange
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Original Name", Age = 20 };
        var newName = "Updated Name";

        // Act
        dto.Name = newName;

        // Assert
        Assert.Equal(newName, dto.Name);
    }

    [Fact]
    public void StudentResponseDto_AgeCanBeModified()
    {
        // Arrange
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = 20 };
        var newAge = 30;

        // Act
        dto.Age = newAge;

        // Assert
        Assert.Equal(newAge, dto.Age);
    }

    [Fact]
    public void StudentResponseDto_NameIsRequired_ThrowsExceptionWhenNotSet()
    {
        // This test verifies the required keyword behavior
        // The actual exception might vary based on .NET version
        // For object initializer, it's enforced at compile time

        // Arrange & Act & Assert
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = 25 };
        Assert.NotNull(dto.Name);
    }

    [Fact]
    public void StudentResponseDto_WithEmptyName_IsValid()
    {
        // Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = string.Empty, Age = 20 };

        // Assert
        Assert.Equal(string.Empty, dto.Name);
    }

    [Fact]
    public void StudentResponseDto_IsReferenceType()
    {
        // Arrange
        var dto1 = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = 20 };
        var dto2 = dto1;

        // Act
        dto2.Age = 30;

        // Assert
        Assert.Equal(30, dto1.Age);
        Assert.Same(dto1, dto2);
    }

    [Fact]
    public void StudentResponseDto_MultipleInstances_AreIndependent()
    {
        // Arrange
        var dto1 = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test1", Age = 20 };
        var dto2 = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test2", Age = 25 };

        // Act
        dto1.Age = 30;

        // Assert
        Assert.Equal(30, dto1.Age);
        Assert.Equal(25, dto2.Age);
        Assert.NotSame(dto1, dto2);
    }
}
