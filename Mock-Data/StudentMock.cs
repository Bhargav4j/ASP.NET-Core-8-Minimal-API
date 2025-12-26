using MinimalAPIProject.Model;

namespace MinimalAPIProject.Mock_Data;
public class StudentMock
{
    // DEPRECATED: This static in-memory storage is not container-safe and will be replaced with a database.
    // TODO: Migrate to a proper database service (SQL Server, PostgreSQL, MySQL) using connection strings from environment variables.
    // For now, this remains as a temporary data store but should NOT be used in production containerized environments.
    public static IEnumerable<Student> students = new List<Student>(){
        new Student() { Name = "Test 1", Age = new Random().Next(18, 60), Id = Guid.NewGuid() },
        new Student() { Name = "Test 2", Age = new Random().Next(18, 60), Id = Guid.NewGuid() },
        new Student() { Name = "Test 3", Age = new Random().Next(18, 60), Id = Guid.NewGuid() },
        new Student() { Name = "Test 4", Age = new Random().Next(18, 60), Id = Guid.NewGuid() },
        new Student() { Name = "Test 5", Age = new Random().Next(18, 60), Id = Guid.NewGuid() }
    };
}