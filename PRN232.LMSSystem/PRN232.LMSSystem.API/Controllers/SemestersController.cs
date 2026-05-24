using Microsoft.AspNetCore.Mvc;
using PRN232.LMSSystem.API.Helpers;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;
    private readonly IDataShaper<SemesterResponse> _dataShaper;

    public SemestersController(ISemesterService semesterService, IDataShaper<SemesterResponse> dataShaper)
    {
        _semesterService = semesterService;
        _dataShaper = dataShaper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SemesterResponse>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _semesterService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);

        var response = ApiResponse<object>.SuccessResponse(shapedData, "Semesters retrieved successfully", pagination);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? fields = null)
    {
        var semester = await _semesterService.GetByIdAsync(id);
        if (semester == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Semester with ID {id} not found"));
        }

        var shapedData = _dataShaper.ShapeData(semester, fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Semester retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] SemesterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid data", ModelState));
        }

        var semester = await _semesterService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = semester.SemesterId }, ApiResponse<SemesterResponse>.SuccessResponse(semester, "Semester created successfully"));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] SemesterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid data", ModelState));
        }

        var updated = await _semesterService.UpdateAsync(id, request);
        if (!updated)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Semester with ID {id} not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Semester updated successfully"));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _semesterService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Semester with ID {id} not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Semester deleted successfully"));
    }
}
