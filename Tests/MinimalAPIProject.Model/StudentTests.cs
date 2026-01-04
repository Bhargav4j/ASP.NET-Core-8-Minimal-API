using Xunit;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Model.Tests;

public class StudentTests
{
    [Fact]
    public void Student_CanSetAndGetId()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "John Doe", Age = 25 };
        var expectedId = student.Id;

        // Act
        var actualId = student.Id;

        // Assert
        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public void Student_CanSetAndGetName()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "John Doe", Age = 25 };

        // Act
        var name = student.Name;

        // Assert
        Assert.Equal("John Doe", name);
    }

    [Fact]
    public void Student_CanSetAndGetAge()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "John Doe", Age = 25 };

        // Act
        var age = student.Age;

        // Assert
        Assert.Equal(25, age);
    }

    [Fact]
    public void Student_WithValidData_CreatesInstance()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var student = new Student { Id = id, Name = "Jane Smith", Age = 30 };

        // Assert
        Assert.NotNull(student);
        Assert.Equal(id, student.Id);
        Assert.Equal("Jane Smith", student.Name);
        Assert.Equal(30, student.Age);
    }

    [Theory]
    [InlineData("Alice", 18)]
    [InlineData("Bob", 60)]
    [InlineData("Charlie", 45)]
    public void Student_WithVariousValidInputs_CreatesInstance(string name, int age)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var student = new Student { Id = id, Name = name, Age = age };

        // Assert
        Assert.NotNull(student);
        Assert.Equal(name, student.Name);
        Assert.Equal(age, student.Age);
    }

    [Fact]
    public void Student_WithEmptyGuid_CreatesInstance()
    {
        // Arrange & Act
        var student = new Student { Id = Guid.Empty, Name = "Test", Age = 25 };

        // Assert
        Assert.Equal(Guid.Empty, student.Id);
    }

    [Fact]
    public void Student_WithZeroAge_CreatesInstance()
    {
        // Arrange & Act
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = 0 };

        // Assert
        Assert.Equal(0, student.Age);
    }

    [Fact]
    public void Student_WithNegativeAge_CreatesInstance()
    {
        // Arrange & Act
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = -5 };

        // Assert
        Assert.Equal(-5, student.Age);
    }

    [Fact]
    public void Student_WithEmptyName_CreatesInstance()
    {
        // Arrange & Act
        var student = new Student { Id = Guid.NewGuid(), Name = "", Age = 25 };

        // Assert
        Assert.Equal("", student.Name);
    }

    [Fact]
    public void Student_ModifyProperties_ReflectsChanges()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "Initial", Age = 25 };
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
    public void Student_WithLongName_CreatesInstance()
    {
        // Arrange
        var longName = new string('A', 1000);

        // Act
        var student = new Student { Id = Guid.NewGuid(), Name = longName, Age = 25 };

        // Assert
        Assert.Equal(longName, student.Name);
    }

    [Fact]
    public void Student_WithSpecialCharactersInName_CreatesInstance()
    {
        // Arrange
        var name = "John@#$%^&*()_+{}|:\"<>?";

        // Act
        var student = new Student { Id = Guid.NewGuid(), Name = name, Age = 25 };

        // Assert
        Assert.Equal(name, student.Name);
    }

    [Fact]
    public void Student_TwoStudentsWithSameData_AreNotReferenceEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var student1 = new Student { Id = id, Name = "John", Age = 25 };
        var student2 = new Student { Id = id, Name = "John", Age = 25 };

        // Act & Assert
        Assert.NotSame(student1, student2);
    }
}
