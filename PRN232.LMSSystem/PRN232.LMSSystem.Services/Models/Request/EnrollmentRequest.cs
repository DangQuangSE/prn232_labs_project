namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>
/// Request body for creating or updating an Enrollment record.
/// </summary>
public class EnrollmentRequest
{
    /// <summary>ID of the student to enroll. Must reference an existing Student.</summary>
    /// <example>1</example>
    public int StudentId { get; set; }

    /// <summary>ID of the course to enroll in. Must reference an existing Course.</summary>
    /// <example>5</example>
    public int CourseId { get; set; }

    /// <summary>Date when the student enrolled in the course (ISO 8601 format).</summary>
    /// <example>2024-09-01</example>
    public DateTime EnrollDate { get; set; }

    /// <summary>Enrollment status. Accepted values: "Active", "Completed", "Dropped".</summary>
    /// <example>Active</example>
    public string Status { get; set; } = "Active";
}
