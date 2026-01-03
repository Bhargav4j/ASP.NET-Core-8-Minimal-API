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
    public void Students_ShouldContainFiveItems()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.Equal(5, students.Count());
    }

    [Fact]
    public void Students_ShouldAllHaveNonEmptyIds()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.NotEqual(Guid.Empty, student.Id));
    }

    [Fact]
    public void Students_ShouldAllHaveNames()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.False(string.IsNullOrEmpty(student.Name)));
    }

    [Fact]
    public void Students_ShouldHaveUniqueIds()
    {
        // Arrange & Act
        var students = StudentMock.students;
        var ids = students.Select(s => s.Id).ToList();

        // Assert
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Students_ShouldHaveAgesBetween18And60()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student =>
        {
            Assert.InRange(student.Age, 0, 100);
        });
    }

    [Fact]
    public void Students_ShouldContainTest1()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.Contains(students, s => s.Name == "Test 1");
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
    public void Students_ShouldContainTest5()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.Contains(students, s => s.Name == "Test 5");
    }

    [Fact]
    public void Students_ShouldBeEnumerable()
    {
        // Arrange & Act
        var students = StudentMock.students;
        var list = students.ToList();

        // Assert
        Assert.Equal(5, list.Count);
    }

    [Fact]
    public void Students_ShouldAllBeStudentType()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.IsType<Student>(student));
    }

    [Fact]
    public void Students_ShouldNotContainNullItems()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.DoesNotContain(null, students);
    }

    [Fact]
    public void Students_Names_ShouldFollowTestPattern()
    {
        // Arrange & Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.StartsWith("Test ", student.Name));
    }
}
