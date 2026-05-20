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
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _studentService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
        
        var response = ApiResponse<object>.SuccessResponse(shapedData, "Students retrieved successfully", pagination);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null, [FromQuery] string? fields = null)
    {
        var student = await _studentService.GetByIdAsync(id, expand);
        if (student == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Student with ID {id} not found"));
        }

        var shapedData = _dataShaper.ShapeData(student, fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Student retrieved successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid data", ModelState));
        }

        var student = await _studentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = student.StudentId }, ApiResponse<StudentResponse>.SuccessResponse(student, "Student created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid data", ModelState));
        }

        var updated = await _studentService.UpdateAsync(id, request);
        if (!updated)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Student with ID {id} not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _studentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Student with ID {id} not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Student deleted successfully"));
    }
}
