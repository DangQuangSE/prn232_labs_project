using Microsoft.AspNetCore.Mvc;
using PRN232.LMSSystem.API.Helpers;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IDataShaper<StudentResponse> _dataShaper;

    public StudentsController(IStudentService studentService, IDataShaper<StudentResponse> dataShaper)
    {
        _studentService = studentService;
        _dataShaper = dataShaper;
    }

    [HttpGet]
    [ExpandOptions("enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponse>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _studentService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Students retrieved successfully", pagination));
    }

    [HttpGet("{id}")]
    [ExpandOptions("enrollments")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 200)]
    public async Task<IActionResult> GetById(int id, [FromQuery] QueryParameters queryParams)
    {
        var student = await _studentService.GetByIdAsync(id, queryParams.Expand);
        var shapedData = _dataShaper.ShapeData(student, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Student retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 201)]
    public async Task<IActionResult> Create([FromBody] StudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        var student = await _studentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = student.StudentId },
            ApiResponse<StudentResponse>.SuccessResponse(student, "Student created successfully"));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        await _studentService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Student updated successfully"));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(int id)
    {
        await _studentService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Student deleted successfully"));
    }
}
