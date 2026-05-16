namespace ClinicMateAI.Domain.Clinics;

public sealed class UserBranchAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
}
