using Xunit;
using MinimalAPIProject.Dto.Request;

namespace MinimalAPIProject.Dto.Request.Tests;

public class UpdateStudentRequestDtoTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto
        {
            Name = "Jane Doe",
            Age = 30
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Jane Doe", dto.Name);
        Assert.Equal(30, dto.Age);
    }

    [Theory]
    [InlineData("Alice Updated", 20)]
    [InlineData("Bob Updated", 35)]
    [InlineData("Charlie Updated", 50)]
    public void SetName_ShouldSetCorrectly(string name, int age)
    {
        // Arrange
        var dto = new UpdateStudentRequestDto
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
        var dto = new UpdateStudentRequestDto
        {
            Name = "Test Update",
            Age = 0
        };

        // Assert
        Assert.Equal(0, dto.Age);
    }

    [Fact]
    public void SetAge_WithNegativeValue_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto
        {
            Name = "Test",
            Age = -10
        };

        // Assert
        Assert.Equal(-10, dto.Age);
    }

    [Fact]
    public void SetAge_WithLargeValue_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto
        {
            Name = "Test",
            Age = 1000
        };

        // Assert
        Assert.Equal(1000, dto.Age);
    }

    [Fact]
    public void SetName_WithEmptyString_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto
        {
            Name = "",
            Age = 25
        };

        // Assert
        Assert.Equal("", dto.Name);
    }

    [Fact]
    public void SetName_WithWhitespace_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto
        {
            Name = "   ",
            Age = 25
        };

        // Assert
        Assert.Equal("   ", dto.Name);
    }

    [Fact]
    public void SetName_WithSpecialCharacters_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto
        {
            Name = "Updated@456!",
            Age = 25
        };

        // Assert
        Assert.Equal("Updated@456!", dto.Name);
    }

    [Fact]
    public void SetName_WithLongString_ShouldSetCorrectly()
    {
        // Arrange
        string longName = new string('A', 1000);

        // Act
        var dto = new UpdateStudentRequestDto
        {
            Name = longName,
            Age = 25
        };

        // Assert
        Assert.Equal(longName, dto.Name);
    }
}
