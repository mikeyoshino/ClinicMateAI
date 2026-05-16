using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Branches;

public interface IUpdateBranchHandler
{
    Task<Branch> HandleAsync(UpdateBranchCommand command, CancellationToken cancellationToken = default);
}
