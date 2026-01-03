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
    }

    [Fact]
    public async Task GetAllStudents_ShouldReturnAllStudents()
    {
        // Arrange & Act
        var result = await _repository.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetAllStudents_ShouldReturnIEnumerable()
    {
        // Arrange & Act
        var result = await _repository.GetAllStudents();

        // Assert
        Assert.IsAssignableFrom<IEnumerable<Student>>(result);
    }

    [Fact]
    public async Task CreateStudent_ShouldAddStudent()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "New Student",
            Age = 25
        };
        var initialCount = StudentMock.students.Count();

        // Act
        var result = await _repository.CreateStudent(student);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(student.Id, result.Id);
        Assert.Equal(student.Name, result.Name);
        Assert.Equal(student.Age, result.Age);
    }

    [Fact]
    public async Task CreateStudent_ShouldReturnCreatedStudent()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Test Create",
            Age = 30
        };

        // Act
        var result = await _repository.CreateStudent(student);

        // Assert
        Assert.Same(student, result);
    }

    [Fact]
    public async Task GetStudentById_ShouldReturnStudentWhenExists()
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
    public async Task GetStudentById_ShouldReturnNullWhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetStudentById(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStudentById_ShouldReturnNullForEmptyGuid()
    {
        // Arrange & Act
        var result = await _repository.GetStudentById(Guid.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteStudent_ShouldReturnTrueWhenStudentExists()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Age = 25
        };
        await _repository.CreateStudent(student);

        // Act
        var result = await _repository.DeleteStudent(student.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteStudent_ShouldReturnFalseWhenStudentNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteStudent(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteStudent_ShouldReturnFalseForEmptyGuid()
    {
        // Arrange & Act
        var result = await _repository.DeleteStudent(Guid.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStudent_ShouldReturnTrueWhenStudentExists()
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
    public async Task UpdateStudent_ShouldReturnNullWhenStudentNotExists()
    {
        // Arrange
        var nonExistentStudent = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Non Existent",
            Age = 25
        };

        // Act
        var result = await _repository.UpdateStudent(nonExistentStudent);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_ShouldReturnNullForEmptyGuid()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.Empty,
            Name = "Empty Guid",
            Age = 25
        };

        // Act
        var result = await _repository.UpdateStudent(student);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateStudent_WithEmptyGuid_ShouldStillAdd()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.Empty,
            Name = "Empty Guid Student",
            Age = 20
        };

        // Act
        var result = await _repository.CreateStudent(student);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateStudent_WithZeroAge_ShouldAdd()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Zero Age",
            Age = 0
        };

        // Act
        var result = await _repository.CreateStudent(student);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Age);
    }

    [Fact]
    public async Task CreateStudent_WithNegativeAge_ShouldAdd()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Negative Age",
            Age = -5
        };

        // Act
        var result = await _repository.CreateStudent(student);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(-5, result.Age);
    }

    [Fact]
    public async Task CreateStudent_WithEmptyName_ShouldAdd()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "",
            Age = 25
        };

        // Act
        var result = await _repository.CreateStudent(student);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.Name);
    }
}
