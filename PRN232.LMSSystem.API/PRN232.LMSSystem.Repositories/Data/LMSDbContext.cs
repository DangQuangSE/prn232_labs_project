using Microsoft.EntityFrameworkCore;
using PRN232.LMSSystem.Repositories.Entities;

namespace PRN232.LMSSystem.Repositories.Data;

public class LMSDbContext : DbContext
{
    public LMSDbContext(DbContextOptions<LMSDbContext> options) : base(options)
    {
    }

    public DbSet<Semester> Semesters { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities
        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.SemesterId);
            entity.Property(e => e.SemesterName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.CourseName).IsRequired().HasMaxLength(100);
            
            entity.HasOne(d => d.Semester)
                .WithMany(p => p.Courses)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId);
            entity.Property(e => e.SubjectCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SubjectName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(d => d.Student)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Course)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        });

        // Seed Data
        // 1. Semesters (5)
        var semesters = new List<Semester>
        {
            new() { SemesterId = 1, SemesterName = "Spring 2024", StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2024, 4, 30, 0, 0, 0, DateTimeKind.Utc) },
            new() { SemesterId = 2, SemesterName = "Summer 2024", StartDate = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2024, 8, 31, 0, 0, 0, DateTimeKind.Utc) },
            new() { SemesterId = 3, SemesterName = "Fall 2024", StartDate = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc) },
            new() { SemesterId = 4, SemesterName = "Spring 2025", StartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 4, 30, 0, 0, 0, DateTimeKind.Utc) },
            new() { SemesterId = 5, SemesterName = "Summer 2025", StartDate = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2025, 8, 31, 0, 0, 0, DateTimeKind.Utc) }
        };
        modelBuilder.Entity<Semester>().HasData(semesters);

        // 2. Students (50)
        var students = new List<Student>();
        var firstNames = new[] { "Anh", "Binh", "Chi", "Dung", "Em", "Giang", "Huong", "Khanh", "Lan", "Minh", "Nam", "Phuong", "Quang", "Son", "Trang", "Tuan", "Viet", "Vy", "Yen" };
        var middleNames = new[] { "Van", "Thi", "Duc", "Minh", "Hoang", "Ngoc", "Hai", "Quoc" };
        var lastNames = new[] { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Huynh", "Phan", "Vu", "Vo", "Dang", "Bui", "Do", "Ngo" };

        var rand = new Random(42);

        for (int i = 1; i <= 50; i++)
        {
            var ln = lastNames[rand.Next(lastNames.Length)];
            var mn = middleNames[rand.Next(middleNames.Length)];
            var fn = firstNames[rand.Next(firstNames.Length)];
            var fullName = $"{ln} {mn} {fn}";
            var email = $"student{i}@fpt.edu.vn";
            var dob = new DateTime(1998, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(rand.Next(2000));
            
            students.Add(new Student
            {
                StudentId = i,
                FullName = fullName,
                Email = email,
                DateOfBirth = dob
            });
        }
        modelBuilder.Entity<Student>().HasData(students);

        // 3. Subjects (10)
        var subjects = new List<Subject>
        {
            new() { SubjectId = 1, SubjectCode = "PRN211", SubjectName = "Basic Cross-Platform Application Programming with .NET", Credit = 3 },
            new() { SubjectId = 2, SubjectCode = "PRN221", SubjectName = "Advanced Cross-Platform Application Programming with .NET", Credit = 3 },
            new() { SubjectId = 3, SubjectCode = "PRN231", SubjectName = "Web API Development with ASP.NET Core", Credit = 3 },
            new() { SubjectId = 4, SubjectCode = "PRN232", SubjectName = "Advanced Web API Development with ASP.NET Core", Credit = 3 },
            new() { SubjectId = 5, SubjectCode = "PRU211", SubjectName = "Game Development with Unity", Credit = 3 },
            new() { SubjectId = 6, SubjectCode = "PRM392", SubjectName = "Mobile Application Development", Credit = 3 },
            new() { SubjectId = 7, SubjectCode = "SWD392", SubjectName = "Software Architecture and Design", Credit = 3 },
            new() { SubjectId = 8, SubjectCode = "SWT301", SubjectName = "Software Testing", Credit = 3 },
            new() { SubjectId = 9, SubjectCode = "EXE101", SubjectName = "Experiential Entrepreneurship I", Credit = 2 },
            new() { SubjectId = 10, SubjectCode = "EXE201", SubjectName = "Experiential Entrepreneurship II", Credit = 2 }
        };
        modelBuilder.Entity<Subject>().HasData(subjects);

        // 4. Courses (20)
        var courses = new List<Course>();
        var subjectList = new[] { "PRN211", "PRN221", "PRN231", "PRN232", "PRU211", "PRM392", "SWD392", "SWT301" };
        var classSuffixes = new[] { "SE1701", "SE1702", "SE1703", "SE1704", "SE1801", "SE1802" };
        
        for (int i = 1; i <= 20; i++)
        {
            var subCode = subjectList[rand.Next(subjectList.Length)];
            var suffix = classSuffixes[rand.Next(classSuffixes.Length)];
            var semId = ((i - 1) % 5) + 1;
            
            courses.Add(new Course
            {
                CourseId = i,
                CourseName = $"{subCode}_{suffix}_S{semId}",
                SemesterId = semId
            });
        }
        modelBuilder.Entity<Course>().HasData(courses);

        // 5. Enrollments (500)
        var enrollments = new List<Enrollment>();
        var enrollmentStatuses = new[] { "Active", "Completed", "Dropped" };
        int enrollmentId = 1;
        
        var existingPairs = new HashSet<(int StudentId, int CourseId)>();
        
        for (int sId = 1; sId <= 50; sId++)
        {
            int assignedCount = 0;
            while (assignedCount < 8)
            {
                int cId = rand.Next(1, 21);
                if (existingPairs.Add((sId, cId)))
                {
                    assignedCount++;
                }
            }
        }
        
        while (existingPairs.Count < 500)
        {
            int sId = rand.Next(1, 51);
            int cId = rand.Next(1, 21);
            existingPairs.Add((sId, cId));
        }

        foreach (var pair in existingPairs)
        {
            var enrollDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(rand.Next(500));
            var status = enrollmentStatuses[rand.Next(enrollmentStatuses.Length)];
            
            enrollments.Add(new Enrollment
            {
                EnrollmentId = enrollmentId++,
                StudentId = pair.StudentId,
                CourseId = pair.CourseId,
                EnrollDate = enrollDate,
                Status = status
            });
        }
        modelBuilder.Entity<Enrollment>().HasData(enrollments);
    }
}
