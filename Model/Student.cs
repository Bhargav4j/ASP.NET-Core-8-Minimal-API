using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalAPIProject.Model;

[Table("students")]
public class Student
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Required]
    [Column("age")]
    public int Age { get; set; }
}