using Xunit;
using MinimalAPIProject.Dto.Response;

namespace MinimalAPIProject.Dto.Response.Tests;

public class StudentResponseDtoTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Student",
            Age = 25
        };

        // Assert
        Assert.NotNull(dto);
    }

    [Fact]
    public void Id_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var dto = new StudentResponseDto
        {
            Id = expectedId,
            Name = "Test",
            Age = 20
        };

        // Act
        var actualId = dto.Id;

        // Assert
        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public void Name_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "John Smith",
            Age = 30
        };

        // Act
        var name = dto.Name;

        // Assert
        Assert.Equal("John Smith", name);
    }

    [Fact]
    public void Age_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = 45
        };

        // Act
        var age = dto.Age;

        // Assert
        Assert.Equal(45, age);
    }

    [Theory]
    [InlineData("Alice", 18)]
    [InlineData("Bob", 25)]
    [InlineData("Charlie", 60)]
    [InlineData("Diana", 100)]
    public void Properties_ShouldAcceptVariousValues(string name, int age)
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
    public void Id_ShouldAcceptEmptyGuid()
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
    public void Age_ShouldAcceptZero()
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
    public void Age_ShouldAcceptNegativeValues()
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
    public void Name_ShouldAcceptEmptyString()
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
    public void Properties_ShouldBeModifiable()
    {
        // Arrange
        var dto = new StudentResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "Initial",
            Age = 20
        };
        var newId = Guid.NewGuid();

        // Act
        dto.Id = newId;
        dto.Name = "Modified";
        dto.Age = 35;

        // Assert
        Assert.Equal(newId, dto.Id);
        Assert.Equal("Modified", dto.Name);
        Assert.Equal(35, dto.Age);
    }

    [Fact]
    public void MultipleInstances_ShouldHaveDifferentIds()
    {
        // Arrange & Act
        var dto1 = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Student1", Age = 20 };
        var dto2 = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Student2", Age = 30 };

        // Assert
        Assert.NotEqual(dto1.Id, dto2.Id);
    }

    [Fact]
    public void AllProperties_ShouldBeIndependent()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var dto1 = new StudentResponseDto { Id = id1, Name = "Name1", Age = 25 };
        var dto2 = new StudentResponseDto { Id = id2, Name = "Name2", Age = 30 };

        // Act & Assert
        Assert.NotEqual(dto1.Id, dto2.Id);
        Assert.NotEqual(dto1.Name, dto2.Name);
        Assert.NotEqual(dto1.Age, dto2.Age);
    }
}
