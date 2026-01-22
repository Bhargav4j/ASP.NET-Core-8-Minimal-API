using Xunit;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Tests.Model;

public class StudentTests
{
    [Fact]
    public void Student_CanBeCreated()
    {
        // Act
        var student = new Student { Id = Guid.NewGuid(), Name = "Test Student", Age = 25 };

        // Assert
        Assert.NotNull(student);
    }

    [Fact]
    public void Student_IdProperty_CanBeSetAndGet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var student = new Student { Id = id, Name = "Test", Age = 20 };

        // Act
        var result = student.Id;

        // Assert
        Assert.Equal(id, result);
    }

    [Fact]
    public void Student_NameProperty_CanBeSetAndGet()
    {
        // Arrange
        var name = "John Doe";
        var student = new Student { Id = Guid.NewGuid(), Name = name, Age = 20 };

        // Act
        var result = student.Name;

        // Assert
        Assert.Equal(name, result);
    }

    [Fact]
    public void Student_AgeProperty_CanBeSetAndGet()
    {
        // Arrange
        var age = 30;
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = age };

        // Act
        var result = student.Age;

        // Assert
        Assert.Equal(age, result);
    }

    [Fact]
    public void Student_WithEmptyGuid_IsValid()
    {
        // Act
        var student = new Student { Id = Guid.Empty, Name = "Test", Age = 20 };

        // Assert
        Assert.Equal(Guid.Empty, student.Id);
    }

    [Fact]
    public void Student_WithZeroAge_IsValid()
    {
        // Act
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = 0 };

        // Assert
        Assert.Equal(0, student.Age);
    }

    [Fact]
    public void Student_WithNegativeAge_IsValid()
    {
        // Act
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = -1 };

        // Assert
        Assert.Equal(-1, student.Age);
    }

    [Fact]
    public void Student_WithMaxAge_IsValid()
    {
        // Act
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = int.MaxValue };

        // Assert
        Assert.Equal(int.MaxValue, student.Age);
    }

    [Fact]
    public void Student_AllPropertiesSet_AreAccessible()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Full Test Student";
        var age = 25;

        // Act
        var student = new Student { Id = id, Name = name, Age = age };

        // Assert
        Assert.Equal(id, student.Id);
        Assert.Equal(name, student.Name);
        Assert.Equal(age, student.Age);
    }

    [Fact]
    public void Student_IdCanBeModified()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = 20 };
        var newId = Guid.NewGuid();

        // Act
        student.Id = newId;

        // Assert
        Assert.Equal(newId, student.Id);
    }

    [Fact]
    public void Student_NameCanBeModified()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "Original Name", Age = 20 };
        var newName = "Updated Name";

        // Act
        student.Name = newName;

        // Assert
        Assert.Equal(newName, student.Name);
    }

    [Fact]
    public void Student_AgeCanBeModified()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = 20 };
        var newAge = 30;

        // Act
        student.Age = newAge;

        // Assert
        Assert.Equal(newAge, student.Age);
    }

    [Fact]
    public void Student_WithEmptyName_IsValid()
    {
        // Act
        var student = new Student { Id = Guid.NewGuid(), Name = string.Empty, Age = 20 };

        // Assert
        Assert.Equal(string.Empty, student.Name);
    }

    [Fact]
    public void Student_IsReferenceType()
    {
        // Arrange
        var student1 = new Student { Id = Guid.NewGuid(), Name = "Test", Age = 20 };
        var student2 = student1;

        // Act
        student2.Age = 30;

        // Assert
        Assert.Equal(30, student1.Age);
        Assert.Same(student1, student2);
    }

    [Fact]
    public void Student_DefaultConstructor_PropertiesCanBeInitialized()
    {
        // Act
        var student = new Student { Id = Guid.NewGuid(), Name = "Test", Age = 25 };

        // Assert
        Assert.NotEqual(Guid.Empty, student.Id);
        Assert.Equal("Test", student.Name);
        Assert.Equal(25, student.Age);
    }
}
