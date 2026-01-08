using Xunit;
using MinimalAPIProject.Dto.Request;

namespace MinimalAPIProject.Dto.Request.Tests;

public class CreateStudentRequestDtoTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = "John Doe",
            Age = 25
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("John Doe", dto.Name);
        Assert.Equal(25, dto.Age);
    }

    [Theory]
    [InlineData("Alice", 18)]
    [InlineData("Bob", 30)]
    [InlineData("Charlie", 45)]
    public void SetName_ShouldSetCorrectly(string name, int age)
    {
        // Arrange
        var dto = new CreateStudentRequestDto
        {
            Name = name,
            Age = age
        };

        // Act & Assert
        Assert.Equal(name, dto.Name);
        Assert.Equal(age, dto.Age);
    }

    [Fact]
    public void SetAge_WithZero_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = "Test",
            Age = 0
        };

        // Assert
        Assert.Equal(0, dto.Age);
    }

    [Fact]
    public void SetAge_WithNegativeValue_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = "Test",
            Age = -5
        };

        // Assert
        Assert.Equal(-5, dto.Age);
    }

    [Fact]
    public void SetAge_WithLargeValue_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = "Test",
            Age = 999
        };

        // Assert
        Assert.Equal(999, dto.Age);
    }

    [Fact]
    public void SetName_WithEmptyString_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = "",
            Age = 20
        };

        // Assert
        Assert.Equal("", dto.Name);
    }

    [Fact]
    public void SetName_WithWhitespace_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = "   ",
            Age = 20
        };

        // Assert
        Assert.Equal("   ", dto.Name);
    }

    [Fact]
    public void SetName_WithSpecialCharacters_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = "Test@123!",
            Age = 20
        };

        // Assert
        Assert.Equal("Test@123!", dto.Name);
    }
}
