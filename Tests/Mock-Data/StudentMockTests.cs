using Xunit;
using MinimalAPIProject.Mock_Data;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Tests.Mock_Data;

public class StudentMockTests
{
    [Fact]
    public void StudentMock_StudentsProperty_IsNotNull()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.NotNull(students);
    }

    [Fact]
    public void StudentMock_StudentsProperty_IsEnumerable()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.IsAssignableFrom<IEnumerable<Student>>(students);
    }

    [Fact]
    public void StudentMock_StudentsProperty_ContainsStudents()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.NotEmpty(students);
    }

    [Fact]
    public void StudentMock_StudentsProperty_AllHaveValidIds()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.NotEqual(Guid.Empty, student.Id));
    }

    [Fact]
    public void StudentMock_StudentsProperty_AllHaveNames()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.False(string.IsNullOrEmpty(student.Name)));
    }

    [Fact]
    public void StudentMock_StudentsProperty_AllHaveValidAge()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.InRange(student.Age, 18, 60));
    }

    [Fact]
    public void StudentMock_StudentsProperty_AllAreStudentType()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.IsType<Student>(student));
    }

    [Fact]
    public void StudentMock_StudentsProperty_CanBeEnumerated()
    {
        // Act
        var students = StudentMock.students;
        var count = 0;

        foreach (var student in students)
        {
            count++;
        }

        // Assert
        Assert.True(count > 0);
    }

    [Fact]
    public void StudentMock_StudentsProperty_HasUniqueIds()
    {
        // Act
        var students = StudentMock.students;
        var ids = students.Select(s => s.Id).ToList();
        var uniqueIds = ids.Distinct().ToList();

        // Assert
        Assert.Equal(ids.Count, uniqueIds.Count);
    }

    [Fact]
    public void StudentMock_StudentsProperty_CanBeModified()
    {
        // Arrange
        var initialStudents = StudentMock.students.ToList();
        var newStudent = new Student { Id = Guid.NewGuid(), Name = "New Test Student", Age = 25 };

        // Act
        StudentMock.students = StudentMock.students.Append(newStudent).ToList();
        var updatedStudents = StudentMock.students.ToList();

        // Assert
        Assert.Equal(initialStudents.Count + 1, updatedStudents.Count);
        Assert.Contains(updatedStudents, s => s.Id == newStudent.Id);
    }

    [Fact]
    public void StudentMock_StudentsProperty_NamePattern_MatchesExpected()
    {
        // Act
        var students = StudentMock.students;

        // Assert
        Assert.All(students, student => Assert.Contains("Test", student.Name));
    }

    [Fact]
    public void StudentMock_StudentsProperty_IsStaticField()
    {
        // Act
        var students1 = StudentMock.students;
        var students2 = StudentMock.students;

        // Assert
        Assert.Same(students1, students2);
    }

    [Fact]
    public void StudentMock_StudentsProperty_CanBeReassigned()
    {
        // Arrange
        var originalStudents = StudentMock.students;
        var newStudents = new List<Student>
        {
            new Student { Id = Guid.NewGuid(), Name = "New Student", Age = 20 }
        };

        // Act
        StudentMock.students = newStudents;

        // Assert
        Assert.Equal(1, StudentMock.students.Count());
        Assert.Contains(StudentMock.students, s => s.Name == "New Student");

        // Cleanup
        StudentMock.students = originalStudents;
    }

    [Fact]
    public void StudentMock_StudentsProperty_ToList_CreatesNewList()
    {
        // Act
        var list1 = StudentMock.students.ToList();
        var list2 = StudentMock.students.ToList();

        // Assert
        Assert.NotSame(list1, list2);
        Assert.Equal(list1.Count, list2.Count);
    }
}
