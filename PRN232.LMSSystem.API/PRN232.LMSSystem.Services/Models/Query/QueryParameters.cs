namespace PRN232.LMSSystem.Services.Models.Query;

public class QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>Comma-separated field names to include in response, e.g. "studentId,fullName"</summary>
    public string? Fields { get; set; }

    /// <summary>Comma-separated related entities to include, e.g. "student,course"</summary>
    public string? Expand { get; set; }
}
