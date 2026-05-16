namespace ClinicMateAI.Domain.Appointments;

public sealed class DoctorAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public Guid BranchId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
    public int SlotMinutes { get; set; } = 30;
}
