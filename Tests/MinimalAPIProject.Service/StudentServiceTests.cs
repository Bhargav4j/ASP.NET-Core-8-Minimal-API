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
    public void Constructor_WithRepository_ShouldInitialize()
    {
        // Arrange & Act
        var service = new StudentService(_mockRepository.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetAllStudents_ShouldReturnAllStudents()
    {
        // Arrange
        var expectedStudents = new List<Student>
        {
            new Student { Id = Guid.NewGuid(), Name = "Student1", Age = 20 },
            new Student { Id = Guid.NewGuid(), Name = "Student2", Age = 25 }
        };
        _mockRepository.Setup(r => r.GetAllStudents()).ReturnsAsync(expectedStudents);

        // Act
        var result = await _service.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllStudents(), Times.Once);
    }

    [Fact]
    public async Task GetAllStudents_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllStudents()).ReturnsAsync(new List<Student>());

        // Act
        var result = await _service.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(r => r.GetAllStudents(), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_WithValidId_ShouldReturnStudent()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedStudent = new Student { Id = studentId, Name = "Test Student", Age = 22 };
        _mockRepository.Setup(r => r.GetStudentById(studentId)).ReturnsAsync(expectedStudent);

        // Act
        var result = await _service.GetStudentById(studentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(studentId, result.Id);
        Assert.Equal("Test Student", result.Name);
        _mockRepository.Verify(r => r.GetStudentById(studentId), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_WithInvalidId_ShouldReturnNull()
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
    public async Task GetStudentById_WithEmptyGuid_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetStudentById(Guid.Empty)).ReturnsAsync((Student?)null);

        // Act
        var result = await _service.GetStudentById(Guid.Empty);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetStudentById(Guid.Empty), Times.Once);
    }

    [Fact]
    public async Task CreateStudent_WithValidDto_ShouldReturnCreatedStudent()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto
        {
            Name = "New Student",
            Age = 25
        };

        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Student", result.Name);
        Assert.Equal(25, result.Age);
        Assert.NotEqual(Guid.Empty, result.Id);
        _mockRepository.Verify(r => r.CreateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task CreateStudent_ShouldGenerateNewGuid()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto
        {
            Name = "Test",
            Age = 20
        };

        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(createDto);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        _mockRepository.Verify(r => r.CreateStudent(It.Is<Student>(s => s.Id != Guid.Empty)), Times.Once);
    }

    [Fact]
    public async Task CreateStudent_WithZeroAge_ShouldCreateSuccessfully()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto
        {
            Name = "Young",
            Age = 0
        };

        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(createDto);

        // Assert
        Assert.Equal(0, result.Age);
        _mockRepository.Verify(r => r.CreateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task CreateStudent_WithNegativeAge_ShouldCreateSuccessfully()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto
        {
            Name = "Test",
            Age = -5
        };

        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(createDto);

        // Assert
        Assert.Equal(-5, result.Age);
        _mockRepository.Verify(r => r.CreateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_WithValidIdAndDto_ShouldReturnTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var updateDto = new UpdateStudentRequestDto
        {
            Name = "Updated Name",
            Age = 30
        };

        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateStudent(studentId, updateDto);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.UpdateStudent(It.Is<Student>(
            s => s.Id == studentId && s.Name == "Updated Name" && s.Age == 30)), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var updateDto = new UpdateStudentRequestDto
        {
            Name = "Updated",
            Age = 25
        };

        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
            .ReturnsAsync((bool?)null);

        // Act
        var result = await _service.UpdateStudent(studentId, updateDto);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.UpdateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_WithEmptyGuid_ShouldPassEmptyGuid()
    {
        // Arrange
        var updateDto = new UpdateStudentRequestDto
        {
            Name = "Test",
            Age = 20
        };

        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
            .ReturnsAsync((bool?)null);

        // Act
        var result = await _service.UpdateStudent(Guid.Empty, updateDto);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.UpdateStudent(It.Is<Student>(s => s.Id == Guid.Empty)), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_WhenAlreadyCompleted_ShouldReturnFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var updateDto = new UpdateStudentRequestDto
        {
            Name = "Updated",
            Age = 25
        };

        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateStudent(studentId, updateDto);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.UpdateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStudent_WithValidId_ShouldReturnTrue()
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
    public async Task DeleteStudent_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteStudent(studentId)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteStudent(studentId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteStudent(studentId), Times.Once);
    }

    [Fact]
    public async Task DeleteStudent_WithEmptyGuid_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteStudent(Guid.Empty)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteStudent(Guid.Empty);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteStudent(Guid.Empty), Times.Once);
    }

    [Fact]
    public async Task CreateStudent_WithEmptyName_ShouldCreateSuccessfully()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto
        {
            Name = "",
            Age = 20
        };

        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.CreateStudent(createDto);

        // Assert
        Assert.Equal("", result.Name);
        _mockRepository.Verify(r => r.CreateStudent(It.IsAny<Student>()), Times.Once);
    }
}
