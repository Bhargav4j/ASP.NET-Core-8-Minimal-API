using Xunit;
using MinimalAPIProject.Repository;
using MinimalAPIProject.Model;
using MinimalAPIProject.Mock_Data;

namespace MinimalAPIProject.Repository.Tests;

public class StudentRepositoryTests
{
    [Fact]
    public async Task GetAllStudents_ShouldReturnAllStudents()
    {
        // Arrange
        var repository = new StudentRepository();

        // Act
        var students = await repository.GetAllStudents();

        // Assert
        Assert.NotNull(students);
        Assert.NotEmpty(students);
    }

    [Fact]
    public async Task GetStudentById_WithValidId_ShouldReturnStudent()
    {
        // Arrange
        var repository = new StudentRepository();
        var allStudents = await repository.GetAllStudents();
        var existingStudent = allStudents.First();

        // Act
        var student = await repository.GetStudentById(existingStudent.Id);

        // Assert
        Assert.NotNull(student);
        Assert.Equal(existingStudent.Id, student.Id);
    }

    [Fact]
    public async Task GetStudentById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var repository = new StudentRepository();
        var nonExistentId = Guid.NewGuid();

        // Act
        var student = await repository.GetStudentById(nonExistentId);

        // Assert
        Assert.Null(student);
    }

    [Fact]
    public async Task GetStudentById_WithEmptyGuid_ShouldReturnNull()
    {
        // Arrange
        var repository = new StudentRepository();

        // Act
        var student = await repository.GetStudentById(Guid.Empty);

        // Assert
        Assert.Null(student);
    }

    [Fact]
    public async Task CreateStudent_WithValidStudent_ShouldReturnCreatedStudent()
    {
        // Arrange
        var repository = new StudentRepository();
        var newStudent = new Student
        {
            Id = Guid.NewGuid(),
            Name = "New Student",
            Age = 25
        };

        // Act
        var createdStudent = await repository.CreateStudent(newStudent);

        // Assert
        Assert.NotNull(createdStudent);
        Assert.Equal(newStudent.Id, createdStudent.Id);
        Assert.Equal(newStudent.Name, createdStudent.Name);
        Assert.Equal(newStudent.Age, createdStudent.Age);
    }

    [Fact]
    public async Task CreateStudent_WithMinimumAge_ShouldCreateSuccessfully()
    {
        // Arrange
        var repository = new StudentRepository();
        var newStudent = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Young Student",
            Age = 0
        };

        // Act
        var createdStudent = await repository.CreateStudent(newStudent);

        // Assert
        Assert.NotNull(createdStudent);
        Assert.Equal(0, createdStudent.Age);
    }

    [Fact]
    public async Task CreateStudent_WithEmptyName_ShouldCreateSuccessfully()
    {
        // Arrange
        var repository = new StudentRepository();
        var newStudent = new Student
        {
            Id = Guid.NewGuid(),
            Name = "",
            Age = 20
        };

        // Act
        var createdStudent = await repository.CreateStudent(newStudent);

        // Assert
        Assert.NotNull(createdStudent);
        Assert.Equal("", createdStudent.Name);
    }

    [Fact]
    public async Task UpdateStudent_WithValidStudent_ShouldReturnTrue()
    {
        // Arrange
        var repository = new StudentRepository();
        var allStudents = await repository.GetAllStudents();
        var existingStudent = allStudents.First();

        var updatedStudent = new Student
        {
            Id = existingStudent.Id,
            Name = "Updated Name",
            Age = 99
        };

        // Act
        var result = await repository.UpdateStudent(updatedStudent);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateStudent_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var repository = new StudentRepository();
        var nonExistentStudent = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Non-existent",
            Age = 30
        };

        // Act
        var result = await repository.UpdateStudent(nonExistentStudent);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_WithEmptyGuid_ShouldReturnNull()
    {
        // Arrange
        var repository = new StudentRepository();
        var student = new Student
        {
            Id = Guid.Empty,
            Name = "Test",
            Age = 25
        };

        // Act
        var result = await repository.UpdateStudent(student);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteStudent_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var repository = new StudentRepository();
        var newStudent = new Student
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Age = 30
        };
        await repository.CreateStudent(newStudent);

        // Act
        var result = await repository.DeleteStudent(newStudent.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteStudent_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var repository = new StudentRepository();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.DeleteStudent(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteStudent_WithEmptyGuid_ShouldReturnFalse()
    {
        // Arrange
        var repository = new StudentRepository();

        // Act
        var result = await repository.DeleteStudent(Guid.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateStudent_MultipleTimes_ShouldIncreaseCount()
    {
        // Arrange
        var repository = new StudentRepository();
        var initialCount = (await repository.GetAllStudents()).Count();

        var student1 = new Student { Id = Guid.NewGuid(), Name = "Student 1", Age = 20 };
        var student2 = new Student { Id = Guid.NewGuid(), Name = "Student 2", Age = 21 };

        // Act
        await repository.CreateStudent(student1);
        await repository.CreateStudent(student2);
        var finalCount = (await repository.GetAllStudents()).Count();

        // Assert
        Assert.Equal(initialCount + 2, finalCount);
    }

    [Fact]
    public async Task GetAllStudents_AfterCreate_ShouldContainNewStudent()
    {
        // Arrange
        var repository = new StudentRepository();
        var newStudent = new Student
        {
            Id = Guid.NewGuid(),
            Name = "Verification Student",
            Age = 28
        };

        // Act
        await repository.CreateStudent(newStudent);
        var allStudents = await repository.GetAllStudents();

        // Assert
        Assert.Contains(allStudents, s => s.Id == newStudent.Id);
    }
}
