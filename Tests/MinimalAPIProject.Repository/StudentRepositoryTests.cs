using Xunit;
using MinimalAPIProject.Repository;
using MinimalAPIProject.Model;
using MinimalAPIProject.Mock_Data;

namespace MinimalAPIProject.Repository.Tests;

public class StudentRepositoryTests
{
    private readonly StudentRepository _repository;

    public StudentRepositoryTests()
    {
        _repository = new StudentRepository();
        // Reset mock data before each test
        StudentMock.students = new List<Student>
        {
            new Student { Name = "Test 1", Age = 25, Id = Guid.NewGuid() },
            new Student { Name = "Test 2", Age = 30, Id = Guid.NewGuid() },
            new Student { Name = "Test 3", Age = 35, Id = Guid.NewGuid() }
        };
    }

    [Fact]
    public async Task GetAllStudents_ReturnsAllStudents()
    {
        // Act
        var result = await _repository.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetAllStudents_ReturnsIEnumerableOfStudents()
    {
        // Act
        var result = await _repository.GetAllStudents();

        // Assert
        Assert.IsAssignableFrom<IEnumerable<Student>>(result);
    }

    [Fact]
    public async Task GetStudentById_WithValidId_ReturnsStudent()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();

        // Act
        var result = await _repository.GetStudentById(existingStudent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingStudent.Id, result.Id);
    }

    [Fact]
    public async Task GetStudentById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetStudentById(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStudentById_WithEmptyGuid_ReturnsNull()
    {
        // Act
        var result = await _repository.GetStudentById(Guid.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateStudent_WithValidStudent_ReturnsCreatedStudent()
    {
        // Arrange
        var newStudent = new Student { Id = Guid.NewGuid(), Name = "New Student", Age = 28 };

        // Act
        var result = await _repository.CreateStudent(newStudent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newStudent.Id, result.Id);
        Assert.Equal(newStudent.Name, result.Name);
        Assert.Equal(newStudent.Age, result.Age);
    }

    [Fact]
    public async Task CreateStudent_AddsStudentToCollection()
    {
        // Arrange
        var initialCount = StudentMock.students.Count();
        var newStudent = new Student { Id = Guid.NewGuid(), Name = "New Student", Age = 28 };

        // Act
        await _repository.CreateStudent(newStudent);
        var finalCount = StudentMock.students.Count();

        // Assert
        Assert.Equal(initialCount + 1, finalCount);
    }

    [Fact]
    public async Task CreateStudent_WithEmptyGuid_CreatesStudent()
    {
        // Arrange
        var newStudent = new Student { Id = Guid.Empty, Name = "New Student", Age = 28 };

        // Act
        var result = await _repository.CreateStudent(newStudent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task UpdateStudent_WithValidStudent_ReturnsTrue()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();
        var updatedStudent = new Student { Id = existingStudent.Id, Name = "Updated Name", Age = 40 };

        // Act
        var result = await _repository.UpdateStudent(updatedStudent);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task UpdateStudent_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var nonExistentStudent = new Student { Id = Guid.NewGuid(), Name = "Non-existent", Age = 25 };

        // Act
        var result = await _repository.UpdateStudent(nonExistentStudent);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_WithEmptyGuid_ReturnsNull()
    {
        // Arrange
        var student = new Student { Id = Guid.Empty, Name = "Test", Age = 25 };

        // Act
        var result = await _repository.UpdateStudent(student);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_UpdatesStudentData()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();
        var updatedStudent = new Student { Id = existingStudent.Id, Name = "Updated Name", Age = 50 };

        // Act
        await _repository.UpdateStudent(updatedStudent);
        var retrievedStudent = await _repository.GetStudentById(existingStudent.Id);

        // Assert
        Assert.NotNull(retrievedStudent);
        Assert.Equal("Updated Name", retrievedStudent.Name);
        Assert.Equal(50, retrievedStudent.Age);
    }

    [Fact]
    public async Task DeleteStudent_WithValidId_ReturnsTrue()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();

        // Act
        var result = await _repository.DeleteStudent(existingStudent.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteStudent_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteStudent(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteStudent_WithEmptyGuid_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteStudent(Guid.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteStudent_RemovesStudentFromCollection()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();
        var initialCount = StudentMock.students.Count();

        // Act
        await _repository.DeleteStudent(existingStudent.Id);
        var finalCount = StudentMock.students.Count();

        // Assert
        Assert.Equal(initialCount - 1, finalCount);
    }

    [Fact]
    public async Task DeleteStudent_DeletedStudentCannotBeRetrieved()
    {
        // Arrange
        var existingStudent = StudentMock.students.First();

        // Act
        await _repository.DeleteStudent(existingStudent.Id);
        var result = await _repository.GetStudentById(existingStudent.Id);

        // Assert
        Assert.Null(result);
    }
}
