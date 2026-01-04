using Xunit;
using Moq;
using MinimalAPIProject.Service;
using MinimalAPIProject.Repository;
using MinimalAPIProject.Model;
using MinimalAPIProject.Dto.Request;

namespace MinimalAPIProject.Service.Tests;

public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _mockRepository;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _mockRepository = new Mock<IStudentRepository>();
        _service = new StudentService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllStudents_ReturnsAllStudents()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student { Id = Guid.NewGuid(), Name = "Student 1", Age = 25 },
            new Student { Id = Guid.NewGuid(), Name = "Student 2", Age = 30 }
        };
        _mockRepository.Setup(r => r.GetAllStudents()).ReturnsAsync(students);

        // Act
        var result = await _service.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllStudents(), Times.Once);
    }

    [Fact]
    public async Task GetAllStudents_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllStudents()).ReturnsAsync(new List<Student>());

        // Act
        var result = await _service.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStudentById_WithValidId_ReturnsStudent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var student = new Student { Id = id, Name = "Test Student", Age = 25 };
        _mockRepository.Setup(r => r.GetStudentById(id)).ReturnsAsync(student);

        // Act
        var result = await _service.GetStudentById(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Test Student", result.Name);
        _mockRepository.Verify(r => r.GetStudentById(id), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetStudentById(id)).ReturnsAsync((Student?)null);

        // Act
        var result = await _service.GetStudentById(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStudentById_WithEmptyGuid_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetStudentById(Guid.Empty)).ReturnsAsync((Student?)null);

        // Act
        var result = await _service.GetStudentById(Guid.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateStudent_WithValidDto_ReturnsCreatedStudent()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "New Student", Age = 28 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Student", result.Name);
        Assert.Equal(28, result.Age);
        Assert.NotEqual(Guid.Empty, result.Id);
        _mockRepository.Verify(r => r.CreateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task CreateStudent_GeneratesNewGuid()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "Test", Age = 25 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateStudent_WithEmptyName_CreatesStudent()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "", Age = 25 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(dto);

        // Assert
        Assert.Equal("", result.Name);
    }

    [Fact]
    public async Task CreateStudent_WithZeroAge_CreatesStudent()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "Test", Age = 0 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(dto);

        // Assert
        Assert.Equal(0, result.Age);
    }

    [Fact]
    public async Task UpdateStudent_WithValidIdAndDto_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStudentRequestDto { Name = "Updated Student", Age = 35 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateStudent(id, dto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Value);
        _mockRepository.Verify(r => r.UpdateStudent(It.Is<Student>(s =>
            s.Id == id && s.Name == "Updated Student" && s.Age == 35)), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStudentRequestDto { Name = "Updated", Age = 25 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync((bool?)null);

        // Act
        var result = await _service.UpdateStudent(id, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_WithEmptyGuid_CallsRepository()
    {
        // Arrange
        var dto = new UpdateStudentRequestDto { Name = "Test", Age = 25 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync((bool?)null);

        // Act
        var result = await _service.UpdateStudent(Guid.Empty, dto);

        // Assert
        _mockRepository.Verify(r => r.UpdateStudent(It.Is<Student>(s => s.Id == Guid.Empty)), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_PassesCorrectDataToRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateStudentRequestDto { Name = "Test Name", Age = 40 };
        Student? capturedStudent = null;
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
            .Callback<Student>(s => capturedStudent = s)
            .ReturnsAsync(true);

        // Act
        await _service.UpdateStudent(id, dto);

        // Assert
        Assert.NotNull(capturedStudent);
        Assert.Equal(id, capturedStudent.Id);
        Assert.Equal("Test Name", capturedStudent.Name);
        Assert.Equal(40, capturedStudent.Age);
    }

    [Fact]
    public async Task DeleteStudent_WithValidId_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteStudent(id)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteStudent(id);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteStudent(id), Times.Once);
    }

    [Fact]
    public async Task DeleteStudent_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteStudent(id)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteStudent(id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteStudent_WithEmptyGuid_CallsRepository()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteStudent(Guid.Empty)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteStudent(Guid.Empty);

        // Assert
        _mockRepository.Verify(r => r.DeleteStudent(Guid.Empty), Times.Once);
    }

    [Fact]
    public void StudentService_Constructor_AcceptsRepository()
    {
        // Arrange
        var mockRepo = new Mock<IStudentRepository>();

        // Act
        var service = new StudentService(mockRepo.Object);

        // Assert
        Assert.NotNull(service);
    }
}
