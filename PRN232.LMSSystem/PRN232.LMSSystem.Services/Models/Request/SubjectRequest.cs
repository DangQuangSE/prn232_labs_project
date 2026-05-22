namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>
/// Request body for creating or updating a Subject record.
/// </summary>
public class SubjectRequest
{
    /// <summary>Unique subject code (uppercase letters + digits, max 20 chars).</summary>
    /// <example>PRN232</example>
    public string SubjectCode { get; set; } = string.Empty;

    /// <summary>Full display name of the subject.</summary>
    /// <example>Advanced Web API Development with ASP.NET Core</example>
    public string SubjectName { get; set; } = string.Empty;

    /// <summary>Number of credits for this subject (typically 2-4).</summary>
    /// <example>3</example>
    public int Credit { get; set; }
}
