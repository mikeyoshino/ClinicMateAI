using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Errors;

namespace ClinicMateAI.Logic.Branches;

public sealed class UpdateBranchHandler(
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork) : IUpdateBranchHandler
{
    public async Task<Branch> HandleAsync(UpdateBranchCommand command, CancellationToken cancellationToken = default)
    {
        var branch = await branchRepository.GetByIdAsync(command.ClinicId, command.BranchId, cancellationToken)
            ?? throw new BusinessException(BusinessErrorCode.BranchNotFound);

        branch.Name = command.Name.Trim();
        branch.Address = command.Address.Trim();
        branch.Phone = command.Phone.Trim();
        branch.MapUrl = command.MapUrl.Trim();
        branch.BusinessHours = command.BusinessHours.Trim();
        branch.Status = command.Status;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return branch;
    }
}
