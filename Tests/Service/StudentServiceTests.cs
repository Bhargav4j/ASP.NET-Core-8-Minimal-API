using Xunit;
using Moq;
using MinimalAPIProject.Service;
using MinimalAPIProject.Repository;
using MinimalAPIProject.Model;
using MinimalAPIProject.Dto.Request;

namespace MinimalAPIProject.Tests.Service;

public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _mockRepository;
    private readonly StudentService _studentService;

    public StudentServiceTests()
    {
        _mockRepository = new Mock<IStudentRepository>();
        _studentService = new StudentService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateStudent_ValidRequest_ReturnsStudent()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto { Name = "John Doe", Age = 25 };
        var expectedStudent = new Student { Id = Guid.NewGuid(), Name = "John Doe", Age = 25 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>())).ReturnsAsync(expectedStudent);

        // Act
        var result = await _studentService.CreateStudent(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Age, result.Age);
        _mockRepository.Verify(r => r.CreateStudent(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task CreateStudent_GeneratesNewGuid()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto { Name = "Jane Smith", Age = 30 };
        Student? capturedStudent = null;
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>()))
            .Callback<Student>(s => capturedStudent = s)
            .ReturnsAsync((Student s) => s);

        // Act
        var result = await _studentService.CreateStudent(createDto);

        // Assert
        Assert.NotNull(capturedStudent);
        Assert.NotEqual(Guid.Empty, capturedStudent.Id);
    }

    [Fact]
    public async Task DeleteStudent_ExistingId_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteStudent(id)).ReturnsAsync(true);

        // Act
        var result = await _studentService.DeleteStudent(id);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteStudent(id), Times.Once);
    }

    [Fact]
    public async Task DeleteStudent_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteStudent(id)).ReturnsAsync(false);

        // Act
        var result = await _studentService.DeleteStudent(id);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteStudent(id), Times.Once);
    }

    [Fact]
    public async Task GetAllStudents_ReturnsAllStudents()
    {
        // Arrange
        var expectedStudents = new List<Student>
        {
            new Student { Id = Guid.NewGuid(), Name = "Student1", Age = 20 },
            new Student { Id = Guid.NewGuid(), Name = "Student2", Age = 22 },
            new Student { Id = Guid.NewGuid(), Name = "Student3", Age = 24 }
        };
        _mockRepository.Setup(r => r.GetAllStudents()).ReturnsAsync(expectedStudents);

        // Act
        var result = await _studentService.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStudents.Count, result.Count());
        _mockRepository.Verify(r => r.GetAllStudents(), Times.Once);
    }

    [Fact]
    public async Task GetAllStudents_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllStudents()).ReturnsAsync(new List<Student>());

        // Act
        var result = await _studentService.GetAllStudents();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStudentById_ExistingId_ReturnsStudent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedStudent = new Student { Id = id, Name = "Test Student", Age = 25 };
        _mockRepository.Setup(r => r.GetStudentById(id)).ReturnsAsync(expectedStudent);

        // Act
        var result = await _studentService.GetStudentById(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(expectedStudent.Name, result.Name);
        Assert.Equal(expectedStudent.Age, result.Age);
    }

    [Fact]
    public async Task GetStudentById_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetStudentById(id)).ReturnsAsync((Student?)null);

        // Act
        var result = await _studentService.GetStudentById(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateStudentRequestDto { Name = "Updated Name", Age = 30 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync(true);

        // Act
        var result = await _studentService.UpdateStudent(id, updateDto);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.UpdateStudent(It.Is<Student>(s =>
            s.Id == id && s.Name == updateDto.Name && s.Age == updateDto.Age)), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateStudentRequestDto { Name = "Updated Name", Age = 30 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync((bool?)null);

        // Act
        var result = await _studentService.UpdateStudent(id, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStudent_AlreadyCompleted_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateStudentRequestDto { Name = "Updated Name", Age = 30 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync(false);

        // Act
        var result = await _studentService.UpdateStudent(id, updateDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Constructor_WithValidRepository_CreatesInstance()
    {
        // Arrange
        var mockRepo = new Mock<IStudentRepository>();

        // Act
        var service = new StudentService(mockRepo.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task CreateStudent_WithMinimumAge_CreatesSuccessfully()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto { Name = "Young Student", Age = 0 };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>())).ReturnsAsync((Student s) => s);

        // Act
        var result = await _studentService.CreateStudent(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Age);
    }

    [Fact]
    public async Task CreateStudent_WithMaximumAge_CreatesSuccessfully()
    {
        // Arrange
        var createDto = new CreateStudentRequestDto { Name = "Old Student", Age = int.MaxValue };
        _mockRepository.Setup(r => r.CreateStudent(It.IsAny<Student>())).ReturnsAsync((Student s) => s);

        // Act
        var result = await _studentService.CreateStudent(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(int.MaxValue, result.Age);
    }

    [Fact]
    public async Task DeleteStudent_WithEmptyGuid_ReturnsFalse()
    {
        // Arrange
        var emptyId = Guid.Empty;
        _mockRepository.Setup(r => r.DeleteStudent(emptyId)).ReturnsAsync(false);

        // Act
        var result = await _studentService.DeleteStudent(emptyId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStudent_WithEmptyGuid_ReturnsNull()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var updateDto = new UpdateStudentRequestDto { Name = "Test", Age = 20 };
        _mockRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>())).ReturnsAsync((bool?)null);

        // Act
        var result = await _studentService.UpdateStudent(emptyId, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStudentById_WithEmptyGuid_ReturnsNull()
    {
        // Arrange
        var emptyId = Guid.Empty;
        _mockRepository.Setup(r => r.GetStudentById(emptyId)).ReturnsAsync((Student?)null);

        // Act
        var result = await _studentService.GetStudentById(emptyId);

        // Assert
        Assert.Null(result);
    }
}
