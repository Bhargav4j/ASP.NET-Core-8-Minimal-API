using Xunit;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Model.Tests;

public class StudentTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var student = new Student
        {
            Id = id,
            Name = "Test Student",
            Age = 22
        };

        // Assert
        Assert.NotNull(student);
        Assert.Equal(id, student.Id);
        Assert.Equal("Test Student", student.Name);
        Assert.Equal(22, student.Age);
    }

    [Theory]
    [InlineData("Alice", 18)]
    [InlineData("Bob", 30)]
    [InlineData("Charlie", 45)]
    [InlineData("David", 60)]
    public void SetProperties_ShouldSetCorrectly(string name, int age)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var student = new Student
        {
            Id = id,
            Name = name,
            Age = age
        };

        // Assert
        Assert.Equal(id, student.Id);
        Assert.Equal(name, student.Name);
        Assert.Equal(age, student.Age);
    }

    [Fact]
    public void SetId_WithEmptyGuid_ShouldSetCorrectly()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.Empty,
            Name = "Test",
            Age = 20
        };

        // Assert
        Assert.Equal(Guid.Empty, student.Id);
    }

    [Fact]
    public void SetAge_WithZero_ShouldSetCorrectly()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = 0
        };

        // Assert
        Assert.Equal(0, student.Age);
    }

    [Fact]
    public void SetAge_WithNegativeValue_ShouldSetCorrectly()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = -5
        };

        // Assert
        Assert.Equal(-5, student.Age);
    }

    [Fact]
    public void SetAge_WithMaxValue_ShouldSetCorrectly()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = int.MaxValue
        };

        // Assert
        Assert.Equal(int.MaxValue, student.Age);
    }

    [Fact]
    public void SetAge_WithMinValue_ShouldSetCorrectly()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = int.MinValue
        };

        // Assert
        Assert.Equal(int.MinValue, student.Age);
    }

    [Fact]
    public void SetName_WithEmptyString_ShouldSetCorrectly()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "",
            Age = 20
        };

        // Assert
        Assert.Equal("", student.Name);
    }

    [Fact]
    public void SetName_WithWhitespace_ShouldSetCorrectly()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "   ",
            Age = 20
        };

        // Assert
        Assert.Equal("   ", student.Name);
    }

    [Fact]
    public void SetName_WithSpecialCharacters_ShouldSetCorrectly()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test@Student#123!",
            Age = 20
        };

        // Assert
        Assert.Equal("Test@Student#123!", student.Name);
    }

    [Fact]
    public void SetName_WithLongString_ShouldSetCorrectly()
    {
        // Arrange
        string longName = new string('A', 1000);

        // Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = longName,
            Age = 25
        };

        // Assert
        Assert.Equal(longName, student.Name);
    }

    [Fact]
    public void TwoStudents_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange & Act
        var student1 = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = 20
        };

        var student2 = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = 20
        };

        // Assert
        Assert.NotEqual(student1.Id, student2.Id);
    }
}
