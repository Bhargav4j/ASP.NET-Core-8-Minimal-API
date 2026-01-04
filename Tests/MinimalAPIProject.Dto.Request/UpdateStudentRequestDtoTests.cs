using Xunit;
using MinimalAPIProject.Dto.Request;

namespace MinimalAPIProject.Dto.Request.Tests;

public class UpdateStudentRequestDtoTests
{
    [Fact]
    public void UpdateStudentRequestDto_CanSetAndGetName()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto { Name = "John Doe", Age = 25 };

        // Act
        var name = dto.Name;

        // Assert
        Assert.Equal("John Doe", name);
    }

    [Fact]
    public void UpdateStudentRequestDto_CanSetAndGetAge()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto { Name = "John Doe", Age = 25 };

        // Act
        var age = dto.Age;

        // Assert
        Assert.Equal(25, age);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto { Name = "Jane Smith", Age = 30 };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Jane Smith", dto.Name);
        Assert.Equal(30, dto.Age);
    }

    [Theory]
    [InlineData("Alice", 18)]
    [InlineData("Bob", 60)]
    [InlineData("Charlie", 45)]
    public void UpdateStudentRequestDto_WithVariousValidInputs_CreatesInstance(string name, int age)
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto { Name = name, Age = age };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(name, dto.Name);
        Assert.Equal(age, dto.Age);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithZeroAge_CreatesInstance()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = 0 };

        // Assert
        Assert.Equal(0, dto.Age);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithNegativeAge_CreatesInstance()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = -5 };

        // Assert
        Assert.Equal(-5, dto.Age);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithEmptyName_CreatesInstance()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto { Name = "", Age = 25 };

        // Assert
        Assert.Equal("", dto.Name);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithWhitespaceName_CreatesInstance()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto { Name = "   ", Age = 25 };

        // Assert
        Assert.Equal("   ", dto.Name);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithLongName_CreatesInstance()
    {
        // Arrange
        var longName = new string('A', 1000);

        // Act
        var dto = new UpdateStudentRequestDto { Name = longName, Age = 25 };

        // Assert
        Assert.Equal(longName, dto.Name);
    }

    [Fact]
    public void UpdateStudentRequestDto_WithSpecialCharactersInName_CreatesInstance()
    {
        // Arrange
        var name = "John@#$%^&*()_+{}|:\"<>?";

        // Act
        var dto = new UpdateStudentRequestDto { Name = name, Age = 25 };

        // Assert
        Assert.Equal(name, dto.Name);
    }

    [Fact]
    public void UpdateStudentRequestDto_ModifyName_ReflectsChange()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto { Name = "Initial", Age = 25 };

        // Act
        dto.Name = "Modified";

        // Assert
        Assert.Equal("Modified", dto.Name);
    }

    [Fact]
    public void UpdateStudentRequestDto_ModifyAge_ReflectsChange()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = 25 };

        // Act
        dto.Age = 35;

        // Assert
        Assert.Equal(35, dto.Age);
    }
}
