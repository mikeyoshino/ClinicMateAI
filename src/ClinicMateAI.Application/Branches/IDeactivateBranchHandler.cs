using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Branches;

public interface IDeactivateBranchHandler
{
    Task<Branch> HandleAsync(DeactivateBranchCommand command, CancellationToken cancellationToken = default);
}
