using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Errors;

namespace ClinicMateAI.Logic.Branches;

public sealed class DeactivateBranchHandler(
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork) : IDeactivateBranchHandler
{
    public async Task<Branch> HandleAsync(DeactivateBranchCommand command, CancellationToken cancellationToken = default)
    {
        var branch = await branchRepository.GetByIdAsync(command.ClinicId, command.BranchId, cancellationToken)
            ?? throw new BusinessException(BusinessErrorCode.BranchNotFound);

        branch.Status = BranchStatus.Inactive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return branch;
    }
}
