using Xunit;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Model.Tests;

public class StudentTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test Student",
            Age = 25
        };

        // Assert
        Assert.NotNull(student);
    }

    [Fact]
    public void Id_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var student = new Student
        {
            Id = expectedId,
            Name = "Test",
            Age = 20
        };

        // Act
        var actualId = student.Id;

        // Assert
        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public void Name_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Age = 30
        };

        // Act
        var name = student.Name;

        // Assert
        Assert.Equal("John Doe", name);
    }

    [Fact]
    public void Age_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Age = 45
        };

        // Act
        var age = student.Age;

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
    public void Id_ShouldAcceptEmptyGuid()
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
    public void Age_ShouldAcceptZero()
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
    public void Age_ShouldAcceptNegativeValues()
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
    public void Name_ShouldAcceptEmptyString()
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
    public void Properties_ShouldBeModifiable()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Initial",
            Age = 20
        };
        var newId = Guid.NewGuid();

        // Act
        student.Id = newId;
        student.Name = "Modified";
        student.Age = 35;

        // Assert
        Assert.Equal(newId, student.Id);
        Assert.Equal("Modified", student.Name);
        Assert.Equal(35, student.Age);
    }

    [Fact]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Arrange & Act
        var student1 = new Student { Id = Guid.NewGuid(), Name = "Student1", Age = 20 };
        var student2 = new Student { Id = Guid.NewGuid(), Name = "Student2", Age = 30 };

        // Assert
        Assert.NotEqual(student1.Id, student2.Id);
        Assert.NotEqual(student1.Name, student2.Name);
        Assert.NotEqual(student1.Age, student2.Age);
    }

    [Fact]
    public void Student_ShouldSupportLargeAgeValues()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Old Student",
            Age = int.MaxValue
        };

        // Assert
        Assert.Equal(int.MaxValue, student.Age);
    }

    [Fact]
    public void Student_ShouldSupportMinimumAgeValues()
    {
        // Arrange & Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Negative Age Student",
            Age = int.MinValue
        };

        // Assert
        Assert.Equal(int.MinValue, student.Age);
    }

    [Fact]
    public void Student_ShouldSupportLongNames()
    {
        // Arrange
        var longName = new string('A', 1000);

        // Act
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = longName,
            Age = 25
        };

        // Assert
        Assert.Equal(longName, student.Name);
        Assert.Equal(1000, student.Name.Length);
    }
}
