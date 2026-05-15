namespace ClinicMateAI.Domain.Clinics;

public sealed class Clinic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ClinicStatus Status { get; set; } = ClinicStatus.Active;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
}
