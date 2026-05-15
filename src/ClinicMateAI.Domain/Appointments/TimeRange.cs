namespace ClinicMateAI.Domain.Appointments;

public sealed record TimeRange(DateTime StartsAt, DateTime EndsAt)
{
    public bool Overlaps(DateTime startsAt, DateTime endsAt)
    {
        return StartsAt < endsAt && startsAt < EndsAt;
    }
}
