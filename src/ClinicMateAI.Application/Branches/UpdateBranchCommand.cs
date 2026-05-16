using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Branches;

public sealed record UpdateBranchCommand(
    Guid ClinicId,
    Guid BranchId,
    string Name,
    string Address,
    string Phone,
    string MapUrl,
    string BusinessHours,
    BranchStatus Status);
