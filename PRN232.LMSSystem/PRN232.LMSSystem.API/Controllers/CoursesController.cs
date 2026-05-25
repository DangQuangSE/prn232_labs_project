using Microsoft.AspNetCore.Mvc;
using PRN232.LMSSystem.API.Helpers;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IDataShaper<CourseResponse> _dataShaper;

    public CoursesController(ICourseService courseService, IDataShaper<CourseResponse> dataShaper)
    {
        _courseService = courseService;
        _dataShaper = dataShaper;
    }

    [HttpGet]
    [ExpandOptions("semester", "enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CourseResponse>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _courseService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Courses retrieved successfully", pagination));
    }

    [HttpGet("{id}")]
    [ExpandOptions("semester", "enrollments")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), 200)]
    public async Task<IActionResult> GetById(int id, [FromQuery] QueryParameters queryParams)
    {
        var course = await _courseService.GetByIdAsync(id, queryParams.Expand);
        var shapedData = _dataShaper.ShapeData(course, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Course retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), 201)]
    public async Task<IActionResult> Create([FromBody] CourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        var course = await _courseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = course.CourseId },
            ApiResponse<CourseResponse>.SuccessResponse(course, "Course created successfully"));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Update(int id, [FromBody] CourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        await _courseService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Course updated successfully"));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Delete(int id)
    {
        await _courseService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Course deleted successfully"));
    }
}
