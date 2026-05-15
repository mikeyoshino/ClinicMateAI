using ClinicMateAI.Domain.Appointments;

namespace ClinicMateAI.Logic.Appointments;

public static class AvailabilityService
{
    public static IReadOnlyList<DateTime> GetAvailableSlots(
        DateOnly date,
        DoctorAvailability availability,
        IEnumerable<TimeRange> busySlots)
    {
        if (date.DayOfWeek != availability.DayOfWeek)
        {
            return [];
        }

        var slots = new List<DateTime>();
        var current = date.ToDateTime(availability.StartsAt);
        var end = date.ToDateTime(availability.EndsAt);
        var slotDuration = TimeSpan.FromMinutes(availability.SlotMinutes);
        var busy = busySlots.ToList();

        while (current + slotDuration <= end)
        {
            var slotEnd = current + slotDuration;
            if (!busy.Any(range => range.Overlaps(current, slotEnd)))
            {
                slots.Add(current);
            }

            current += slotDuration;
        }

        return slots;
    }
}
