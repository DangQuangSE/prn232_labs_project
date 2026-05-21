using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PRN232.LMSSystem.Repositories.Entities;

namespace PRN232.LMSSystem.Repositories.Data;

public partial class LmsLab1Context : DbContext
{
    public LmsLab1Context()
    {
    }

    public LmsLab1Context(DbContextOptions<LmsLab1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("Course_pkey");

            entity.ToTable("Course");

            entity.Property(e => e.CourseName).HasMaxLength(100);

            entity.HasOne(d => d.Semester).WithMany(p => p.Courses)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Course_SemesterId_fkey");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("Enrollment_pkey");

            entity.ToTable("Enrollment");

            entity.Property(e => e.EnrollDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Enrollment_CourseId_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Enrollment_StudentId_fkey");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.SemesterId).HasName("Semester_pkey");

            entity.ToTable("Semester");

            entity.Property(e => e.EndDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SemesterName).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("Student_pkey");

            entity.ToTable("Student");

            entity.Property(e => e.DateOfBirth).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("Subject_pkey");

            entity.ToTable("Subject");

            entity.Property(e => e.SubjectCode).HasMaxLength(20);
            entity.Property(e => e.SubjectName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
