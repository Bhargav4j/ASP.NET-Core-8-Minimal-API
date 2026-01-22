using MinimalAPIProject.Mock_Data;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Repository;
public class StudentRepository : IStudentRepository
{

    public async Task<Student> CreateStudent(Student student)
    {
        StudentMock.students.Add(student);
        return await Task.Run(() => student);
    }

    public async Task<bool> DeleteStudent(Guid id)
    {
        Student? student = StudentMock.students.FirstOrDefault(x => x.Id == id);
        if(student is null) return await Task.Run(() => false);
        StudentMock.students.RemoveAll(x => x.Id == id);
        return await Task.Run(() => true);
    }

    public async Task<IEnumerable<Student>> GetAllStudents()
    {
        return await Task.Run(() => StudentMock.students);
    }

    public async Task<Student?> GetStudentById(Guid id)
    {
        return await Task.Run(() => StudentMock.students.FirstOrDefault(x => x.Id == id));
    }

    public async Task<bool?> UpdateStudent(Student student)
    {
        Student? student_ = StudentMock.students.FirstOrDefault(x => x.Id == student.Id);
        if(student_ is null) return null;
        var index = StudentMock.students.FindIndex(x => x.Id == student.Id);
        if(index != -1)
        {
            StudentMock.students[index] = student;
        }
        return await Task.Run(() => true);
    }
}