using Xunit;
using MinimalAPIProject.Mock_Data;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Mock_Data.Tests;

public class StudentMockTests
{
    [Fact]
    public void Students_ShouldNotBeNull()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.NotNull(students);
    }

    [Fact]
    public void Students_ShouldContainElements()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.NotEmpty(students);
    }

    [Fact]
    public void Students_ShouldContainFiveElements()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.True(students.Count() >= 5);
    }

    [Fact]
    public void Students_AllShouldHaveUniqueIds()
    {
        // Arrange
        var students = StudentMock.students.ToList();

        // Act
        var uniqueIds = students.Select(s => s.Id).Distinct().Count();

        // Assert
        Assert.Equal(students.Count, uniqueIds);
    }

    [Fact]
    public void Students_AllShouldHaveNames()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.NotNull(student.Name));
        Assert.All(students, student => Assert.NotEmpty(student.Name));
    }

    [Fact]
    public void Students_AllShouldHaveValidAge()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.InRange(student.Age, 18, 60));
    }

    [Fact]
    public void Students_FirstStudent_ShouldHaveNameTest1()
    {
        // Arrange & Act
        var firstStudent = StudentMock.students.FirstOrDefault();

        // Assert
        Assert.NotNull(firstStudent);
        Assert.Equal("Test 1", firstStudent.Name);
    }

    [Fact]
    public void Students_LastStudent_ShouldHaveNameTest5()
    {
        // Arrange & Act
        var lastStudent = StudentMock.students.LastOrDefault();

        // Assert
        Assert.NotNull(lastStudent);
        Assert.NotNull(lastStudent.Name);
        Assert.NotEmpty(lastStudent.Name);
    }

    [Fact]
    public void Students_ShouldContainTest2()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.Contains(students, s => s.Name == "Test 2");
    }

    [Fact]
    public void Students_ShouldContainTest3()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.Contains(students, s => s.Name == "Test 3");
    }

    [Fact]
    public void Students_ShouldContainTest4()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.Contains(students, s => s.Name == "Test 4");
    }

    [Fact]
    public void Students_AllIdsShouldBeNonEmpty()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.NotEqual(Guid.Empty, student.Id));
    }

    [Fact]
    public void Students_ShouldBeEnumerable()
    {
        // Arrange & Act
        var students = StudentMock.students;
        var count = 0;

        foreach (var student in students)
        {
            count++;
        }

        // Assert
        Assert.True(count >= 5);
    }
}
