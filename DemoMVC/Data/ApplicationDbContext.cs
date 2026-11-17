using DemoMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<StudentClass> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
                .HasIndex(student => student.StudentCode)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasOne(student => student.Class)
                .WithMany(studentClass => studentClass.Students)
                .HasForeignKey(student => student.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentClass>()
                .HasIndex(studentClass => studentClass.MaLop)
                .IsUnique();

            modelBuilder.Entity<Subject>()
                .HasIndex(subject => subject.MaMonHoc)
                .IsUnique();

            modelBuilder.Entity<Grade>()
                .HasIndex(grade => new { grade.StudentId, grade.SubjectId })
                .IsUnique();

            modelBuilder.Entity<Grade>()
                .Property(grade => grade.DiemQuaTrinh)
                .HasPrecision(4, 2);

            modelBuilder.Entity<Grade>()
                .Property(grade => grade.DiemThi)
                .HasPrecision(4, 2);

            modelBuilder.Entity<Grade>()
                .Property(grade => grade.DiemTongKet)
                .HasPrecision(4, 2);

            modelBuilder.Entity<Grade>()
                .HasOne(grade => grade.Student)
                .WithMany()
                .HasForeignKey(grade => grade.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Grade>()
                .HasOne(grade => grade.Subject)
                .WithMany(subject => subject.Grades)
                .HasForeignKey(grade => grade.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}