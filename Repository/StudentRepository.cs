using Microsoft.EntityFrameworkCore;
using MinimalAPIProject.Data;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Repository;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Student> CreateStudent(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<bool> DeleteStudent(Guid id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null)
            return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Student>> GetAllStudents()
    {
        return await _context.Students.ToListAsync();
    }

    public async Task<Student?> GetStudentById(Guid id)
    {
        return await _context.Students.FindAsync(id);
    }

    public async Task<bool?> UpdateStudent(Student student)
    {
        var existingStudent = await _context.Students.FindAsync(student.Id);
        if (existingStudent is null)
            return null;

        existingStudent.Name = student.Name;
        existingStudent.Age = student.Age;

        _context.Students.Update(existingStudent);
        await _context.SaveChangesAsync();
        return true;
    }
}