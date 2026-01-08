using Xunit;
using MinimalAPIProject.Dto.Response;

namespace MinimalAPIProject.Dto.Response.Tests;

public class StudentResponseDtoTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new StudentResponseDto
        {
            Id = id,
            Name = "Test Student",
            Age = 22
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id, dto.Id);
        Assert.Equal("Test Student", dto.Name);
        Assert.Equal(22, dto.Age);
    }

    [Theory]
    [InlineData("Alice", 18)]
    [InlineData("Bob", 30)]
    [InlineData("Charlie", 45)]
    public void SetProperties_ShouldSetCorrectly(string name, int age)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new StudentResponseDto
        {
            Id = id,
            Name = name,
            Age = age
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(name, dto.Name);
        Assert.Equal(age, dto.Age);
    }

    [Fact]
    public void SetId_WithEmptyGuid_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new StudentResponseDto
        {
            Id = Guid.Empty,
            Name = "Test",
            Age = 20
        };

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
    }

    [Fact]
    public void SetAge_WithZero_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
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
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = -5
        };

        // Assert
        Assert.Equal(-5, dto.Age);
    }

    [Fact]
    public void SetName_WithEmptyString_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
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
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "   ",
            Age = 20
        };

        // Assert
        Assert.Equal("   ", dto.Name);
    }

    [Fact]
    public void SetId_WithMultipleGuids_ShouldMaintainUniqueness()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var dto1 = new StudentResponseDto
        {
            Id = id1,
            Name = "Test1",
            Age = 20
        };

        var dto2 = new StudentResponseDto
        {
            Id = id2,
            Name = "Test2",
            Age = 25
        };

        // Assert
        Assert.NotEqual(dto1.Id, dto2.Id);
    }

    [Fact]
    public void SetName_WithSpecialCharacters_ShouldSetCorrectly()
    {
        // Arrange & Act
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "Test@Response#123",
            Age = 20
        };

        // Assert
        Assert.Equal("Test@Response#123", dto.Name);
    }
}
