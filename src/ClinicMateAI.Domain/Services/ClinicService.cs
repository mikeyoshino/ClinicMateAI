namespace ClinicMateAI.Domain.Services;

public sealed class ClinicService
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public int DurationMinutes { get; set; }
    public bool RequiresDoctorAssessment { get; set; }
    public string ApprovedAiWording { get; set; } = string.Empty;
}
