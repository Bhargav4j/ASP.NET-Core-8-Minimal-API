using Xunit;
using MinimalAPIProject.Dto.Request;

namespace MinimalAPIProject.Dto.Request.Tests;

public class CreateStudentRequestDtoTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = "Test Student",
            Age = 25
        };

        // Assert
        Assert.NotNull(dto);
    }

    [Fact]
    public void Name_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new CreateStudentRequestDto
        {
            Name = "John Doe",
            Age = 20
        };

        // Act
        var name = dto.Name;

        // Assert
        Assert.Equal("John Doe", name);
    }

    [Fact]
    public void Age_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new CreateStudentRequestDto
        {
            Name = "John Doe",
            Age = 30
        };

        // Act
        var age = dto.Age;

        // Assert
        Assert.Equal(30, age);
    }

    [Theory]
    [InlineData("Alice", 18)]
    [InlineData("Bob", 25)]
    [InlineData("Charlie", 60)]
    public void Properties_ShouldAcceptVariousValues(string name, int age)
    {
        // Arrange & Act
        var dto = new CreateStudentRequestDto
        {
            Name = name,
            Age = age
        };

        // Assert
        Assert.Equal(name, dto.Name);
        Assert.Equal(age, dto.Age);
    }

    [Fact]
    public void Age_ShouldAcceptZero()
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
    public void Age_ShouldAcceptNegativeValues()
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
    public void Name_ShouldAcceptEmptyString()
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
    public void Properties_ShouldBeIndependent()
    {
        // Arrange
        var dto = new CreateStudentRequestDto
        {
            Name = "Initial",
            Age = 20
        };

        // Act
        dto.Name = "Updated";
        dto.Age = 30;

        // Assert
        Assert.Equal("Updated", dto.Name);
        Assert.Equal(30, dto.Age);
    }
}
