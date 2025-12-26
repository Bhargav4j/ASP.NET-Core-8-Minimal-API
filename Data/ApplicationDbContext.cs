using Microsoft.EntityFrameworkCore;
using MinimalAPIProject.Model;

namespace MinimalAPIProject.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema to public (PostgreSQL standard)
        modelBuilder.HasDefaultSchema("public");

        // Configure Student entity
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Age)
                .IsRequired();
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Configure DateTime to use timestamp without time zone (PostgreSQL best practice)
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<long>();
    }
}
