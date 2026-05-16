namespace ClinicMateAI.Domain.Clinics;

public sealed class Branch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
    public string BusinessHours { get; set; } = string.Empty;
    public BranchStatus Status { get; set; } = BranchStatus.Active;
    public bool IsDefault { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
