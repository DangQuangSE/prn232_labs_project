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

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;

    public SemesterService(ISemesterRepository semesterRepository)
    {
        _semesterRepository = semesterRepository;
    }

    public async Task<(IEnumerable<SemesterResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams)
    {
        Expression<Func<Semester, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = s => s.SemesterName.ToLower().Contains(searchLower);
        }

        int totalItems = await _semesterRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        var includes = new List<string> { "Courses" };

        Func<IQueryable<Semester>, IOrderedQueryable<Semester>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            orderBy = q => (IOrderedQueryable<Semester>)QueryHelper.ApplySort(q, queryParams.Sort);
        }
        else
        {
            orderBy = q => q.OrderBy(s => s.SemesterId);
        }

        var semesters = await _semesterRepository.GetAllAsync(
            filter: filter,
            orderBy: orderBy,
            includeProperties: includes,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        var responseList = semesters.Select(MapToResponse);

        return (responseList, pagination);
    }

    public async Task<SemesterResponse?> GetByIdAsync(int id)
    {
        var includes = new List<string> { "Courses" };
        var semester = await _semesterRepository.GetByIdAsync(id, includes);
        if (semester == null) return null;

        return MapToResponse(semester);
    }

    public async Task<SemesterResponse> CreateAsync(SemesterRequest request)
    {
        var semester = new Semester
        {
            SemesterName = request.SemesterName,
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc)
        };

        await _semesterRepository.AddAsync(semester);
        await _semesterRepository.SaveAsync();

        return MapToResponse(semester);
    }

    public async Task<bool> UpdateAsync(int id, SemesterRequest request)
    {
        var semester = await _semesterRepository.GetByIdAsync(id);
        if (semester == null) return false;

        semester.SemesterName = request.SemesterName;
        semester.StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        semester.EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);

        _semesterRepository.Update(semester);
        await _semesterRepository.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var semester = await _semesterRepository.GetByIdAsync(id);
        if (semester == null) return false;

        _semesterRepository.Delete(semester);
        await _semesterRepository.SaveAsync();
        return true;
    }

    private SemesterResponse MapToResponse(Semester semester)
    {
        return new SemesterResponse
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            CourseCount = semester.Courses?.Count ?? 0
        };
    }
}
