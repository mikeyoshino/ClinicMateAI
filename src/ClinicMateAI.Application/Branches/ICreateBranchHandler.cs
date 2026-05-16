using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Branches;

public interface ICreateBranchHandler
{
    Task<Branch> HandleAsync(CreateBranchCommand command, CancellationToken cancellationToken = default);
}
