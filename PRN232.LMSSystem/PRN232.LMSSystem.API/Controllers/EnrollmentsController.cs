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
    [ExpandOptions("student", "course")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentResponse>>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _enrollmentService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Enrollments retrieved successfully", pagination));
    }

    [HttpGet("{id}")]
    [ExpandOptions("student", "course")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(int id, [FromQuery] QueryParameters queryParams)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(id, queryParams.Expand);
        var shapedData = _dataShaper.ShapeData(enrollment, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Enrollment retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] EnrollmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        var enrollment = await _enrollmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = enrollment.EnrollmentId },
            ApiResponse<EnrollmentResponse>.SuccessResponse(enrollment, "Enrollment created successfully"));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update(int id, [FromBody] EnrollmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        await _enrollmentService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Enrollment updated successfully"));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete(int id)
    {
        await _enrollmentService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Enrollment deleted successfully"));
    }
}
