using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.Services.Interfaces;

public interface IEnrollmentService
{
    Task<(IEnumerable<EnrollmentResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams);
    Task<EnrollmentResponse?> GetByIdAsync(int id, string? expand = null);
    Task<EnrollmentResponse> CreateAsync(EnrollmentRequest request);
    Task<bool> UpdateAsync(int id, EnrollmentRequest request);
    Task<bool> DeleteAsync(int id);
}
