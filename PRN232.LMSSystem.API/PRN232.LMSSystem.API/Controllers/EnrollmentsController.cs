using Microsoft.AspNetCore.Mvc;
using PRN232.LMSSystem.API.Helpers;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IDataShaper<EnrollmentResponse> _dataShaper;

    public EnrollmentsController(IEnrollmentService enrollmentService, IDataShaper<EnrollmentResponse> dataShaper)
    {
        _enrollmentService = enrollmentService;
        _dataShaper = dataShaper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _enrollmentService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
        
        var response = ApiResponse<object>.SuccessResponse(shapedData, "Enrollments retrieved successfully", pagination);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null, [FromQuery] string? fields = null)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(id, expand);
        if (enrollment == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Enrollment with ID {id} not found"));
        }

        var shapedData = _dataShaper.ShapeData(enrollment, fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Enrollment retrieved successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EnrollmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid data", ModelState));
        }

        var enrollment = await _enrollmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = enrollment.EnrollmentId }, ApiResponse<EnrollmentResponse>.SuccessResponse(enrollment, "Enrollment created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EnrollmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid data", ModelState));
        }

        var updated = await _enrollmentService.UpdateAsync(id, request);
        if (!updated)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Enrollment with ID {id} not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Enrollment updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _enrollmentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Enrollment with ID {id} not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Enrollment deleted successfully"));
    }
}
