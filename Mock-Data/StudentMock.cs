using MinimalAPIProject.Model;

namespace MinimalAPIProject.Mock_Data;
public class StudentMock
{
    // WARNING: This in-memory storage is NOT suitable for containerized environments
    // Replace with database storage using connection string from environment variables:
    // ConnectionString: ${DB_CONNECTION_STRING} or Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD}
    public static IEnumerable<Student> students = new List<Student>(){
        new Student() { Name = "Test 1", Age = new Random().Next(18, 60), Id = Guid.NewGuid() },
        new Student() { Name = "Test 2", Age = new Random().Next(18, 60), Id = Guid.NewGuid() },
        new Student() { Name = "Test 3", Age = new Random().Next(18, 60), Id = Guid.NewGuid() },
        new Student() { Name = "Test 4", Age = new Random().Next(18, 60), Id = Guid.NewGuid() },
        new Student() { Name = "Test 5", Age = new Random().Next(18, 60), Id = Guid.NewGuid() }
    };
}