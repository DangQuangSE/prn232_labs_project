using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.Services.Interfaces;

public interface ICourseService
{
    Task<(IEnumerable<CourseResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams);
    Task<CourseResponse?> GetByIdAsync(int id);
    Task<CourseResponse> CreateAsync(CourseRequest request);
    Task<bool> UpdateAsync(int id, CourseRequest request);
    Task<bool> DeleteAsync(int id);
}
