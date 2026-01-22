using Xunit;
using MinimalAPIProject.Repository;
using MinimalAPIProject.Model;
using MinimalAPIProject.Mock_Data;

namespace MinimalAPIProject.Tests.Repository;

public class StudentRepositoryTests
{
    private readonly StudentRepository _repository;

    public StudentRepositoryTests()
    {
        _repository = new StudentRepository();
        // Reset mock data before each test
        StudentMock.students = new List<Student>
        {
            new Student { Id = Guid.NewGuid(), Name = "Test 1", Age = 20 },
            new Student { Id = Guid.NewGuid(), Name = "Test 2", Age = 25 },
            new Student { Id = Guid.NewGuid(), Name = "Test 3", Age = 30 }
        };
    }

    [Fact]
    public async Task CreateStudent_ValidStudent_ReturnsCreatedStudent()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "New Student", Age = 22 };
        var initialCount = StudentMock.students.Count();

        // Act
        var result = await _repository.CreateStudent(student);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(student.Id, result.Id);
        Assert.Equal(student.Name, result.Name);
        Assert.Equal(student.Age, result.Age);
        Assert.Equal(initialCount + 1, StudentMock.students.Count());
    }

    [Fact]
    public async Task CreateStudent_AddsStudentToCollection()
    {
        // Arrange
        var student = new Student { Id = Guid.NewGuid(), Name = "Test Student", Age = 28 };

        // Act
        await _repository.CreateStudent(student);

        // Assert
        Assert.Contains(StudentMock.students, s => s.Id == student.Id);
    }

    [Fact]
    public async Task DeleteStudent_ExistingId_ReturnsTrue()
    {
        // Arrange
        var student = StudentMock.students.First();
        var id = student.Id;

        // Act
        var result = await _repository.DeleteStudent(id);

        // Assert
        Assert.True(result);
        Assert.DoesNotContain(StudentMock.students, s => s.Id == id);
    }

    [Fact]
    public async Task DeleteStudent_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteStudent(nonExistingId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteStudent_RemovesStudentFromCollection()
    {
        // Arrange
        var student = StudentMock.students.First();
        var id = student.Id;
        var initialCount = StudentMock.students.Count();

        // Act
        await _repository.DeleteStudent(id);

        // Assert
        Assert.Equal(initialCount - 1, StudentMock.students.Count());
    }

    [Fact]
    public async Task GetAllStudents_ReturnsAllStudents()
    {
        // Arrange
        var expectedCount = StudentMock.students.Count();

        // Act
        var result = await _repository.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCount, result.Count());
    }

    [Fact]
    public async Task GetAllStudents_ReturnsEnumerableOfStudents()
    {
        // Act
        var result = await _repository.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.All(result, student => Assert.IsType<Student>(student));
    }

    [Fact]
    public async Task GetStudentById_ExistingId_ReturnsStudent()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();
        var id = existingStudent.Id;

        // Act
        var result = await _repository.GetStudentById(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(existingStudent.Name, result.Name);
        Assert.Equal(existingStudent.Age, result.Age);
    }

    [Fact]
    public async Task GetStudentById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetStudentById(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_ExistingStudent_ReturnsTrue()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();
        var updatedStudent = new Student
        {
            Id = existingStudent.Id,
            Name = "Updated Name",
            Age = 99
        };

        // Act
        var result = await _repository.UpdateStudent(updatedStudent);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateStudent_NonExistingStudent_ReturnsNull()
    {
        // Arrange
        var nonExistingStudent = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Non Existing",
            Age = 50
        };

        // Act
        var result = await _repository.UpdateStudent(nonExistingStudent);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_UpdatesStudentData()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();
        var updatedStudent = new Student
        {
            Id = existingStudent.Id,
            Name = "Updated Name",
            Age = 35
        };

        // Act
        await _repository.UpdateStudent(updatedStudent);
        var result = await _repository.GetStudentById(existingStudent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedStudent.Name, result.Name);
        Assert.Equal(updatedStudent.Age, result.Age);
    }

    [Fact]
    public async Task CreateStudent_WithEmptyGuid_AddsToCollection()
    {
        // Arrange
        var student = new Student { Id = Guid.Empty, Name = "Empty Guid Student", Age = 18 };

        // Act
        var result = await _repository.CreateStudent(student);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetAllStudents_EmptyCollection_ReturnsEmptyEnumerable()
    {
        // Arrange
        StudentMock.students = new List<Student>();

        // Act
        var result = await _repository.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteStudent_EmptyGuid_ReturnsFalse()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var result = await _repository.DeleteStudent(emptyGuid);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateStudent_MultipleStudents_AllAdded()
    {
        // Arrange
        var initialCount = StudentMock.students.Count();
        var student1 = new Student { Id = Guid.NewGuid(), Name = "Student 1", Age = 20 };
        var student2 = new Student { Id = Guid.NewGuid(), Name = "Student 2", Age = 22 };

        // Act
        await _repository.CreateStudent(student1);
        await _repository.CreateStudent(student2);

        // Assert
        Assert.Equal(initialCount + 2, StudentMock.students.Count());
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Act
        var repository = new StudentRepository();

        // Assert
        Assert.NotNull(repository);
    }

    [Fact]
    public async Task UpdateStudent_WithBoundaryAge_UpdatesSuccessfully()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();
        var updatedStudent = new Student
        {
            Id = existingStudent.Id,
            Name = "Updated",
            Age = 0
        };

        // Act
        var result = await _repository.UpdateStudent(updatedStudent);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetStudentById_EmptyGuid_ReturnsNull()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var result = await _repository.GetStudentById(emptyGuid);

        // Assert
        Assert.Null(result);
    }
}
