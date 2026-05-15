using ClinicMateAI.Domain.Appointments;
using ClinicMateAI.Logic.Appointments;
using FluentAssertions;

namespace ClinicMateAI.Tests.Appointments;

public class AvailabilityServiceTests
{
    [Fact]
    public void GetAvailableSlots_RemovesBusyCalendarSlots()
    {
        var availability = new DoctorAvailability
        {
            DoctorId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Saturday,
            StartsAt = new TimeOnly(13, 0),
            EndsAt = new TimeOnly(16, 0),
            SlotMinutes = 60
        };
        var date = new DateOnly(2026, 5, 16);
        var busy = new[]
        {
            new TimeRange(new DateTime(2026, 5, 16, 14, 0, 0), new DateTime(2026, 5, 16, 15, 0, 0))
        };

        var slots = AvailabilityService.GetAvailableSlots(date, availability, busy);

        slots.Should().Equal(
            new DateTime(2026, 5, 16, 13, 0, 0),
            new DateTime(2026, 5, 16, 15, 0, 0));
    }
}
