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
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange
        var mockRepo = new Mock<IStudentRepository>();

        // Act
        var service = new StudentService(mockRepo.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetAllStudents_ShouldReturnAllStudents()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student { Id = Guid.NewGuid(), Name = "Student1", Age = 20 },
            new Student { Id = Guid.NewGuid(), Name = "Student2", Age = 25 }
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
    public async Task GetAllStudents_ShouldReturnEmptyListWhenNoStudents()
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
    public async Task GetStudentById_ShouldReturnStudent()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student { Id = studentId, Name = "Test", Age = 25 };
        _mockRepository.Setup(r => r.GetStudentById(studentId)).ReturnsAsync(student);

        // Act
        var result = await _service.GetStudentById(studentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(studentId, result.Id);
        Assert.Equal("Test", result.Name);
        _mockRepository.Verify(r => r.GetStudentById(studentId), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetStudentById(studentId)).ReturnsAsync((Student?)null);

        // Act
        var result = await _service.GetStudentById(studentId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetStudentById(studentId), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_ShouldHandleEmptyGuid()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetStudentById(Guid.Empty)).ReturnsAsync((Student?)null);

        // Act
        var result = await _service.GetStudentById(Guid.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateStudent_ShouldCreateAndReturnStudent()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "New Student", Age = 30 };
        var createdStudent = new Student { Id = Guid.NewGuid(), Name = "New Student", Age = 30 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>())).ReturnsAsync(createdStudent);

        // Act
        var result = await _service.CreateStudent(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Student", result.Name);
        Assert.Equal(30, result.Age);
        _mockRepository.Verify(r => r.CreateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task CreateStudent_ShouldGenerateNewGuid()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "Test", Age = 25 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>())).ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(dto);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateStudent_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "John", Age = 40 };
        Student? capturedStudent = null;
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .Callback<Student>(s => capturedStudent = s)
            .ReturnsAsync((Student s) => s);

        // Act
        await _service.CreateStudent(dto);

        // Assert
        Assert.NotNull(capturedStudent);
        Assert.Equal("John", capturedStudent.Name);
        Assert.Equal(40, capturedStudent.Age);
    }

    [Fact]
    public async Task CreateStudent_WithZeroAge_ShouldCreate()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "Zero Age", Age = 0 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>())).ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(dto);

        // Assert
        Assert.Equal(0, result.Age);
    }

    [Fact]
    public async Task CreateStudent_WithNegativeAge_ShouldCreate()
    {
        // Arrange
        var dto = new CreateStudentRequestDto { Name = "Negative Age", Age = -5 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>())).ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(dto);

        // Assert
        Assert.Equal(-5, result.Age);
    }

    [Fact]
    public async Task UpdateStudent_ShouldReturnTrueWhenSuccessful()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var dto = new UpdateStudentRequestDto { Name = "Updated", Age = 35 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateStudent(studentId, dto);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.UpdateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_ShouldReturnNullWhenNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var dto = new UpdateStudentRequestDto { Name = "Updated", Age = 35 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync((bool?)null);

        // Act
        var result = await _service.UpdateStudent(studentId, dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var dto = new UpdateStudentRequestDto { Name = "Updated Name", Age = 50 };
        Student? capturedStudent = null;
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
            .Callback<Student>(s => capturedStudent = s)
            .ReturnsAsync(true);

        // Act
        await _service.UpdateStudent(studentId, dto);

        // Assert
        Assert.NotNull(capturedStudent);
        Assert.Equal(studentId, capturedStudent.Id);
        Assert.Equal("Updated Name", capturedStudent.Name);
        Assert.Equal(50, capturedStudent.Age);
    }

    [Fact]
    public async Task UpdateStudent_WithEmptyGuid_ShouldAttemptUpdate()
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
    public async Task DeleteStudent_ShouldReturnTrueWhenSuccessful()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteStudent(studentId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteStudent(studentId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteStudent(studentId), Times.Once);
    }

    [Fact]
    public async Task DeleteStudent_ShouldReturnFalseWhenNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteStudent(studentId)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteStudent(studentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteStudent_WithEmptyGuid_ShouldCallRepository()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteStudent(Guid.Empty)).ReturnsAsync(false);

        // Act
        await _service.DeleteStudent(Guid.Empty);

        // Assert
        _mockRepository.Verify(r => r.DeleteStudent(Guid.Empty), Times.Once);
    }

    [Fact]
    public async Task Service_ShouldUseInjectedRepository()
    {
        // Arrange
        var mockRepo = new Mock<IStudentRepository>();
        mockRepo.Setup(r => r.GetAllStudents()).ReturnsAsync(new List<Student>());
        var service = new StudentService(mockRepo.Object);

        // Act
        await service.GetAllStudents();

        // Assert
        mockRepo.Verify(r => r.GetAllStudents(), Times.Once);
    }
}
