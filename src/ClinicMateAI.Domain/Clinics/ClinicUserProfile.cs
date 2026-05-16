namespace ClinicMateAI.Domain.Clinics;

public sealed class ClinicUserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public ClinicUserRole Role { get; set; }
    public Guid? DefaultBranchId { get; set; }
}
