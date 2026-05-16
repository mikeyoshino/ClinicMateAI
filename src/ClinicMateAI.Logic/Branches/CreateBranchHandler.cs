using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Errors;
using ClinicMateAI.Domain.Packages;
using FluentValidation;

namespace ClinicMateAI.Logic.Branches;

public sealed class CreateBranchHandler(
    IValidator<CreateBranchCommand> validator,
    IClinicRepository clinicRepository,
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork) : ICreateBranchHandler
{
    public async Task<Branch> HandleAsync(CreateBranchCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var clinic = await clinicRepository.GetByIdAsync(command.ClinicId, cancellationToken)
            ?? throw new BusinessException(BusinessErrorCode.ClinicNotFound);

        var existingBranches = await branchRepository.ListByClinicAsync(command.ClinicId, cancellationToken);
        if (clinic.PackageTier == PackageTier.Starter && existingBranches.Count >= 1)
        {
            throw new BusinessException(BusinessErrorCode.BranchLimitExceeded, "Starter package supports exactly one branch.");
        }

        if (await branchRepository.ExistsByNameAsync(command.ClinicId, command.Name.Trim(), cancellationToken))
        {
            throw new BusinessException(BusinessErrorCode.InvalidOperation, "Branch name already exists.");
        }

        var branch = new Branch
        {
            ClinicId = command.ClinicId,
            Name = command.Name.Trim(),
            Address = command.Address.Trim(),
            Phone = command.Phone.Trim(),
            MapUrl = command.MapUrl.Trim(),
            BusinessHours = command.BusinessHours.Trim(),
            IsDefault = existingBranches.Count == 0
        };

        await branchRepository.AddAsync(branch, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return branch;
    }
}
