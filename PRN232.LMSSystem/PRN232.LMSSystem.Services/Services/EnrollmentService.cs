
using PRN232.LMSSystem.Repositories.Entities;
using PRN232.LMSSystem.Repositories.Interfaces;
using PRN232.LMSSystem.Services.Helpers;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Business;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;
using System.Linq.Expressions;

namespace PRN232.LMSSystem.Services.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<(IEnumerable<EnrollmentResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams)
    {
        // Always include Student and Course for search (navigations used in filter expression)
        // and additionally for Expand if requested
        var includes = new List<string> { "Student", "Course" };

        Expression<Func<Enrollment, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = e => e.Status.ToLower().Contains(searchLower) ||
                          e.Student.FullName.ToLower().Contains(searchLower) ||
                          e.Course.CourseName.ToLower().Contains(searchLower);
        }

        int totalItems = await _enrollmentRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        // Add Semester include when expanding course
        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.ToLower().Split(',');
            if (expands.Contains("course"))
            {
                includes.Add("Course.Semester");
            }
        }

        Func<IQueryable<Enrollment>, IOrderedQueryable<Enrollment>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            orderBy = q => (IOrderedQueryable<Enrollment>)QueryHelper.ApplySort(q, queryParams.Sort);
        }
        else
        {
            orderBy = q => q.OrderBy(e => e.EnrollmentId);
        }

        var enrollments = await _enrollmentRepository.GetAllAsync(
            filter: filter,
            orderBy: orderBy,
            includeProperties: includes,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        var responseList = enrollments.Select(e => MapToResponse(e, queryParams.Expand));

        return (responseList, pagination);
    }

    public async Task<EnrollmentResponse?> GetByIdAsync(int id, string? expand = null)
    {
        // Always load Student and Course for base fields (StudentName, CourseName)
        var includes = new List<string> { "Student", "Course" };

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.ToLower().Split(',');
            if (expands.Contains("course"))
            {
                includes.Add("Course.Semester");
            }
        }

        var enrollment = await _enrollmentRepository.GetByIdAsync(id, includes);
        if (enrollment == null) return null;

        return MapToResponse(enrollment, expand);
    }

    public async Task<EnrollmentResponse> CreateAsync(EnrollmentRequest request)
    {
        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = DateTime.SpecifyKind(request.EnrollDate, DateTimeKind.Utc),
            Status = request.Status
        };

        await _enrollmentRepository.AddAsync(enrollment);
        await _enrollmentRepository.SaveAsync();

        var loaded = await _enrollmentRepository.GetByIdAsync(enrollment.EnrollmentId, new List<string> { "Student", "Course" });
        return MapToResponse(loaded ?? enrollment, "student,course");
    }

    public async Task<bool> UpdateAsync(int id, EnrollmentRequest request)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id);
        if (enrollment == null) return false;

        enrollment.StudentId = request.StudentId;
        enrollment.CourseId = request.CourseId;
        enrollment.EnrollDate = DateTime.SpecifyKind(request.EnrollDate, DateTimeKind.Utc);
        enrollment.Status = request.Status;

        _enrollmentRepository.Update(enrollment);
        await _enrollmentRepository.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id);
        if (enrollment == null) return false;

        _enrollmentRepository.Delete(enrollment);
        await _enrollmentRepository.SaveAsync();
        return true;
    }

    private EnrollmentBM MapToBusinessModel(Enrollment enrollment)
    {
        return new EnrollmentBM
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };
    }

    private EnrollmentResponse MapToResponse(Enrollment enrollment, string? expand = null)
    {
        var bm = MapToBusinessModel(enrollment);
        
        var response = new EnrollmentResponse
        {
            EnrollmentId = bm.EnrollmentId,
            StudentId = bm.StudentId,
            StudentName = enrollment.Student?.FullName,
            CourseId = bm.CourseId,
            CourseName = enrollment.Course?.CourseName,
            EnrollDate = bm.EnrollDate,
            Status = bm.Status
        };

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.ToLower().Split(',');
            
            if (expands.Contains("student") && enrollment.Student != null)
            {
                response.Student = new StudentResponse
                {
                    StudentId = enrollment.Student.StudentId,
                    FullName = enrollment.Student.FullName,
                    Email = enrollment.Student.Email,
                    DateOfBirth = enrollment.Student.DateOfBirth
                };
            }
            
            if (expands.Contains("course") && enrollment.Course != null)
            {
                response.Course = new CourseResponse
                {
                    CourseId = enrollment.Course.CourseId,
                    CourseName = enrollment.Course.CourseName,
                    SemesterId = enrollment.Course.SemesterId,
                    SemesterName = enrollment.Course.Semester?.SemesterName,
                    EnrollmentCount = enrollment.Course.Enrollments?.Count ?? 0
                };
            }
        }

        return response;
    }
}
