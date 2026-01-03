using Xunit;
using MinimalAPIProject.Dto.Request;

namespace MinimalAPIProject.Dto.Request.Tests;

public class UpdateStudentRequestDtoTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto
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
        var dto = new UpdateStudentRequestDto
        {
            Name = "Jane Doe",
            Age = 22
        };

        // Act
        var name = dto.Name;

        // Assert
        Assert.Equal("Jane Doe", name);
    }

    [Fact]
    public void Age_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto
        {
            Name = "Jane Doe",
            Age = 35
        };

        // Act
        var age = dto.Age;

        // Assert
        Assert.Equal(35, age);
    }

    [Theory]
    [InlineData("UpdatedName1", 18)]
    [InlineData("UpdatedName2", 25)]
    [InlineData("UpdatedName3", 60)]
    [InlineData("UpdatedName4", 100)]
    public void Properties_ShouldAcceptVariousValues(string name, int age)
    {
        // Arrange & Act
        var dto = new UpdateStudentRequestDto
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
        var dto = new UpdateStudentRequestDto
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
        var dto = new UpdateStudentRequestDto
        {
            Name = "Test",
            Age = -10
        };

        // Assert
        Assert.Equal(-10, dto.Age);
    }

    [Fact]
    public void Name_ShouldAcceptEmptyString()
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
    public void Properties_ShouldBeModifiable()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto
        {
            Name = "Original",
            Age = 20
        };

        // Act
        dto.Name = "Modified";
        dto.Age = 40;

        // Assert
        Assert.Equal("Modified", dto.Name);
        Assert.Equal(40, dto.Age);
    }

    [Fact]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Arrange
        var dto1 = new UpdateStudentRequestDto { Name = "Student1", Age = 20 };
        var dto2 = new UpdateStudentRequestDto { Name = "Student2", Age = 30 };

        // Act & Assert
        Assert.NotEqual(dto1.Name, dto2.Name);
        Assert.NotEqual(dto1.Age, dto2.Age);
    }
}
