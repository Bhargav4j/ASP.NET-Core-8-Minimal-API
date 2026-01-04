using Xunit;
using MinimalAPIProject.Mock_Data;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Mock_Data.Tests;

public class StudentMockTests
{
    [Fact]
    public void StudentMock_Students_IsNotNull()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.NotNull(students);
    }

    [Fact]
    public void StudentMock_Students_IsIEnumerable()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.IsAssignableFrom<IEnumerable<Student>>(students);
    }

    [Fact]
    public void StudentMock_Students_ContainsData()
    {
        // Act
        var students = StudentMock.students;
        var count = students.Count();

        // Assert
        Assert.True(count > 0);
    }

    [Fact]
    public void StudentMock_Students_AllHaveValidNames()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        foreach (var student in students)
        {
            Assert.NotNull(student.Name);
            Assert.NotEmpty(student.Name);
        }
    }

    [Fact]
    public void StudentMock_Students_AllHaveValidIds()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        foreach (var student in students)
        {
            Assert.NotEqual(Guid.Empty, student.Id);
        }
    }

    [Fact]
    public void StudentMock_Students_AllHaveValidAges()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        foreach (var student in students)
        {
            Assert.InRange(student.Age, 18, 60);
        }
    }

    [Fact]
    public void StudentMock_Students_AllHaveUniqueIds()
    {
        // Act
        var students = StudentMock.students;
        var ids = students.Select(s => s.Id).ToList();
        var uniqueIds = ids.Distinct().ToList();

        // Assert
        Assert.Equal(ids.Count, uniqueIds.Count);
    }

    [Fact]
    public void StudentMock_Students_CanBeEnumerated()
    {
        // Act
        var students = StudentMock.students;
        var list = new List<Student>();

        foreach (var student in students)
        {
            list.Add(student);
        }

        // Assert
        Assert.NotEmpty(list);
    }

    [Fact]
    public void StudentMock_Students_CanBeQueried()
    {
        // Act
        var students = StudentMock.students;
        var firstStudent = students.FirstOrDefault();

        // Assert
        Assert.NotNull(firstStudent);
    }

    [Fact]
    public void StudentMock_Students_ContainsExpectedTestData()
    {
        // Act
        var students = StudentMock.students;
        var names = students.Select(s => s.Name).ToList();

        // Assert
        Assert.Contains(names, n => n.StartsWith("Test"));
    }
}
