using Microsoft.EntityFrameworkCore;
using PRN232.LMSSystem.Repositories.Entities;
using PRN232.LMSSystem.Repositories.Interfaces;
using PRN232.LMSSystem.Services.Helpers;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;
using System.Linq.Expressions;

namespace PRN232.LMSSystem.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<(IEnumerable<CourseResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams)
    {
        Expression<Func<Course, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = c => c.CourseName.ToLower().Contains(searchLower);
        }

        int totalItems = await _courseRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        var includes = new List<string> { "Semester", "Enrollments" };

        Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            orderBy = q => (IOrderedQueryable<Course>)QueryHelper.ApplySort(q, queryParams.Sort);
        }
        else
        {
            orderBy = q => q.OrderBy(c => c.CourseId);
        }

        var courses = await _courseRepository.GetAllAsync(
            filter: filter,
            orderBy: orderBy,
            includeProperties: includes,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        var responseList = courses.Select(MapToResponse);

        return (responseList, pagination);
    }

    public async Task<CourseResponse?> GetByIdAsync(int id)
    {
        var includes = new List<string> { "Semester", "Enrollments" };
        var course = await _courseRepository.GetByIdAsync(id, includes);
        if (course == null) return null;

        return MapToResponse(course);
    }

    public async Task<CourseResponse> CreateAsync(CourseRequest request)
    {
        var course = new Course
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId
        };

        await _courseRepository.AddAsync(course);
        await _courseRepository.SaveAsync();

        var loadedCourse = await _courseRepository.GetByIdAsync(course.CourseId, new List<string> { "Semester", "Enrollments" });
        return MapToResponse(loadedCourse ?? course);
    }

    public async Task<bool> UpdateAsync(int id, CourseRequest request)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return false;

        course.CourseName = request.CourseName;
        course.SemesterId = request.SemesterId;

        _courseRepository.Update(course);
        await _courseRepository.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return false;

        _courseRepository.Delete(course);
        await _courseRepository.SaveAsync();
        return true;
    }

    private CourseResponse MapToResponse(Course course)
    {
        return new CourseResponse
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId,
            SemesterName = course.Semester?.SemesterName,
            EnrollmentCount = course.Enrollments?.Count ?? 0
        };
    }
}
