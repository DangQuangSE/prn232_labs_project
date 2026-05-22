using Microsoft.EntityFrameworkCore;
using PRN232.LMSSystem.Repositories.Data;
using PRN232.LMSSystem.Repositories.Entities;
using PRN232.LMSSystem.Repositories.Interfaces;
using System.Linq.Expressions;

namespace PRN232.LMSSystem.Repositories.Repositories;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(LMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CourseWithCount>> GetCoursesWithCountAsync(
        Expression<Func<Course, bool>>? filter = null,
        Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null,
        int? page = null,
        int? pageSize = null)
    {
        IQueryable<Course> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        query = query.Include(c => c.Semester);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
        {
            int skip = (page.Value - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        return await query.Select(c => new CourseWithCount
        {
            Course = c,
            EnrollmentCount = c.Enrollments.Count
        }).ToListAsync();
    }

    public async Task<CourseWithCount?> GetCourseWithCountByIdAsync(int id)
    {
        return await _dbSet
            .Where(c => c.CourseId == id)
            .Include(c => c.Semester)
            .Select(c => new CourseWithCount
            {
                Course = c,
                EnrollmentCount = c.Enrollments.Count
            })
            .FirstOrDefaultAsync();
    }
}
