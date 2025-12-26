using MinimalAPIProject.Mock_Data;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Repository;
public class StudentRepository : IStudentRepository
{

    public Task<Student> CreateStudent(Student student)
    {
        StudentMock.students = StudentMock.students.Append(student);
        return Task.FromResult(student);
    }

    public Task<bool> DeleteStudent(Guid id)
    {
        Student? student = StudentMock.students.FirstOrDefault(x => x.Id == id);
        if(student is null) return Task.FromResult(false);
        StudentMock.students = StudentMock.students.Where(x => x.Id != id);
        return Task.FromResult(true);
    }

    public Task<IEnumerable<Student>> GetAllStudents()
    {
        return Task.FromResult(StudentMock.students);
    }

    public Task<Student?> GetStudentById(Guid id)
    {
        return Task.FromResult(StudentMock.students.FirstOrDefault(x => x.Id == id));
    }

    public Task<bool?> UpdateStudent(Student student)
    {
        Student? student_ = StudentMock.students.FirstOrDefault(x => x.Id == student.Id);
        if(student_ is null) return Task.FromResult<bool?>(null);
        student_ = student;
        StudentMock.students = StudentMock.students.Select(stu => {
            if(stu.Id == student_.Id)
                return student_;
            else return stu;
        });
        return Task.FromResult<bool?>(true);
    }
}