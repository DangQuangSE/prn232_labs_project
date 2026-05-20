namespace PRN232.LMSSystem.Services.Models.Request;

public class SemesterRequest
{
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
