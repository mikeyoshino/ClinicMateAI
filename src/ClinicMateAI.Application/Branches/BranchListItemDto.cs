using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Branches;

public sealed record BranchListItemDto(
    Guid BranchId,
    string Name,
    string Address,
    string Phone,
    bool IsDefault,
    BranchStatus Status);
