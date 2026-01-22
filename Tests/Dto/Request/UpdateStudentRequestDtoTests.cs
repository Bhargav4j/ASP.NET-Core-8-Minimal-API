using Xunit;
using MinimalAPIProject.Dto.Request;

namespace MinimalAPIProject.Tests.Dto.Request;

public class UpdateStudentRequestDtoTests
{
    [Fact]
    public void UpdateStudentRequestDto_CanBeCreated()
    {
        // Act
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = 25 };

        // Assert
        Assert.NotNull(dto);
    }

    [Fact]
    public void UpdateStudentRequestDto_NameProperty_CanBeSetAndGet()
    {
        // Arrange
        var name = "John Doe";
        var dto = new UpdateStudentRequestDto { Name = name, Age = 20 };

        // Act
        var result = dto.Name;

        // Assert
        Assert.Equal(name, result);
    }

    [Fact]
    public void UpdateStudentRequestDto_AgeProperty_CanBeSetAndGet()
    {
        // Arrange
        var age = 30;
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = age };

        // Act
        var result = dto.Age;

        // Assert
        Assert.Equal(age, result);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithZeroAge_IsValid()
    {
        // Act
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = 0 };

        // Assert
        Assert.Equal(0, dto.Age);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithNegativeAge_IsValid()
    {
        // Act
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = -1 };

        // Assert
        Assert.Equal(-1, dto.Age);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithMaxAge_IsValid()
    {
        // Act
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = int.MaxValue };

        // Assert
        Assert.Equal(int.MaxValue, dto.Age);
    }

    [Fact]
    public void UpdateStudentRequestDto_AllPropertiesSet_AreAccessible()
    {
        // Arrange
        var name = "Full Test Student";
        var age = 25;

        // Act
        var dto = new UpdateStudentRequestDto { Name = name, Age = age };

        // Assert
        Assert.Equal(name, dto.Name);
        Assert.Equal(age, dto.Age);
    }

    [Fact]
    public void UpdateStudentRequestDto_NameCanBeModified()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto { Name = "Original Name", Age = 20 };
        var newName = "Updated Name";

        // Act
        dto.Name = newName;

        // Assert
        Assert.Equal(newName, dto.Name);
    }

    [Fact]
    public void UpdateStudentRequestDto_AgeCanBeModified()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = 20 };
        var newAge = 30;

        // Act
        dto.Age = newAge;

        // Assert
        Assert.Equal(newAge, dto.Age);
    }

    [Fact]
    public void UpdateStudentRequestDto_NameIsRequired_IsNotNull()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = 25 };

        // Assert
        Assert.NotNull(dto.Name);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithEmptyName_IsValid()
    {
        // Act
        var dto = new UpdateStudentRequestDto { Name = "", Age = 25 };

        // Assert
        Assert.Equal("", dto.Name);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithLongName_IsValid()
    {
        // Arrange
        var longName = new string('a', 1000);

        // Act
        var dto = new UpdateStudentRequestDto { Name = longName, Age = 25 };

        // Assert
        Assert.Equal(longName, dto.Name);
        Assert.Equal(1000, dto.Name.Length);
    }

    [Fact]
    public void UpdateStudentRequestDto_IsReferenceType()
    {
        // Arrange
        var dto1 = new UpdateStudentRequestDto { Name = "Test", Age = 20 };
        var dto2 = dto1;

        // Act
        dto2.Age = 30;

        // Assert
        Assert.Equal(30, dto1.Age);
        Assert.Same(dto1, dto2);
    }

    [Fact]
    public void UpdateStudentRequestDto_MultipleInstances_AreIndependent()
    {
        // Arrange
        var dto1 = new UpdateStudentRequestDto { Name = "Test1", Age = 20 };
        var dto2 = new UpdateStudentRequestDto { Name = "Test2", Age = 25 };

        // Act
        dto1.Age = 30;

        // Assert
        Assert.Equal(30, dto1.Age);
        Assert.Equal(25, dto2.Age);
        Assert.NotSame(dto1, dto2);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithMinAge_IsValid()
    {
        // Act
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = int.MinValue };

        // Assert
        Assert.Equal(int.MinValue, dto.Age);
    }
}
