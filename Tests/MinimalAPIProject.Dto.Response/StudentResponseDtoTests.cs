using Xunit;
using MinimalAPIProject.Dto.Response;

namespace MinimalAPIProject.Dto.Response.Tests;

public class StudentResponseDtoTests
{
    [Fact]
    public void StudentResponseDto_CanSetAndGetId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new StudentResponseDto { Id = id, Name = "John Doe", Age = 25 };

        // Act
        var resultId = dto.Id;

        // Assert
        Assert.Equal(id, resultId);
    }

    [Fact]
    public void StudentResponseDto_CanSetAndGetName()
    {
        // Arrange
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "John Doe", Age = 25 };

        // Act
        var name = dto.Name;

        // Assert
        Assert.Equal("John Doe", name);
    }

    [Fact]
    public void StudentResponseDto_CanSetAndGetAge()
    {
        // Arrange
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "John Doe", Age = 25 };

        // Act
        var age = dto.Age;

        // Assert
        Assert.Equal(25, age);
    }

    [Fact]
    public void StudentResponseDto_WithValidData_CreatesInstance()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new StudentResponseDto { Id = id, Name = "Jane Smith", Age = 30 };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id, dto.Id);
        Assert.Equal("Jane Smith", dto.Name);
        Assert.Equal(30, dto.Age);
    }

    [Theory]
    [InlineData("Alice", 18)]
    [InlineData("Bob", 60)]
    [InlineData("Charlie", 45)]
    public void StudentResponseDto_WithVariousValidInputs_CreatesInstance(string name, int age)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new StudentResponseDto { Id = id, Name = name, Age = age };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(name, dto.Name);
        Assert.Equal(age, dto.Age);
    }

    [Fact]
    public void StudentResponseDto_WithEmptyGuid_CreatesInstance()
    {
        // Arrange & Act
        var dto = new StudentResponseDto { Id = Guid.Empty, Name = "Test", Age = 25 };

        // Assert
        Assert.Equal(Guid.Empty, dto.Id);
    }

    [Fact]
    public void StudentResponseDto_WithZeroAge_CreatesInstance()
    {
        // Arrange & Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = 0 };

        // Assert
        Assert.Equal(0, dto.Age);
    }

    [Fact]
    public void StudentResponseDto_WithNegativeAge_CreatesInstance()
    {
        // Arrange & Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Test", Age = -5 };

        // Assert
        Assert.Equal(-5, dto.Age);
    }

    [Fact]
    public void StudentResponseDto_WithEmptyName_CreatesInstance()
    {
        // Arrange & Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "", Age = 25 };

        // Assert
        Assert.Equal("", dto.Name);
    }

    [Fact]
    public void StudentResponseDto_ModifyProperties_ReflectsChanges()
    {
        // Arrange
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = "Initial", Age = 25 };
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
    public void StudentResponseDto_WithLongName_CreatesInstance()
    {
        // Arrange
        var longName = new string('A', 1000);

        // Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = longName, Age = 25 };

        // Assert
        Assert.Equal(longName, dto.Name);
    }

    [Fact]
    public void StudentResponseDto_WithSpecialCharactersInName_CreatesInstance()
    {
        // Arrange
        var name = "John@#$%^&*()_+{}|:\"<>?";

        // Act
        var dto = new StudentResponseDto { Id = Guid.NewGuid(), Name = name, Age = 25 };

        // Assert
        Assert.Equal(name, dto.Name);
    }
}
