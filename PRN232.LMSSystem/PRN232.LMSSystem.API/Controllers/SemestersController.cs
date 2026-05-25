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
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Semesters retrieved successfully", pagination));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 200)]
    public async Task<IActionResult> GetById(int id, [FromQuery] QueryParameters queryParams)
    {
        var semester = await _semesterService.GetByIdAsync(id, queryParams.Expand);
        var shapedData = _dataShaper.ShapeData(semester, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Semester retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 201)]
    public async Task<IActionResult> Create([FromBody] SemesterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        var semester = await _semesterService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = semester.SemesterId },
            ApiResponse<SemesterResponse>.SuccessResponse(semester, "Semester created successfully"));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Update(int id, [FromBody] SemesterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        await _semesterService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Semester updated successfully"));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(int id)
    {
        await _semesterService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Semester deleted successfully"));
    }
}
