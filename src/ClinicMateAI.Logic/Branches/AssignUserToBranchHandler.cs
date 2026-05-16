using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Errors;
using FluentValidation;

namespace ClinicMateAI.Logic.Branches;

public sealed class AssignUserToBranchHandler(
    IValidator<AssignUserToBranchCommand> validator,
    IClinicUserProfileRepository clinicUserProfileRepository,
    IBranchRepository branchRepository,
    IUserBranchAssignmentRepository userBranchAssignmentRepository,
    IUnitOfWork unitOfWork) : IAssignUserToBranchHandler
{
    public async Task HandleAsync(AssignUserToBranchCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var profile = await clinicUserProfileRepository.GetByUserAndClinicAsync(command.UserId, command.ClinicId, cancellationToken)
            ?? throw new BusinessException(BusinessErrorCode.AccessDenied, "User does not belong to this clinic.");

        var branch = await branchRepository.GetByIdAsync(command.ClinicId, command.BranchId, cancellationToken)
            ?? throw new BusinessException(BusinessErrorCode.BranchNotFound);

        if (profile.Role == ClinicUserRole.Owner)
        {
            return;
        }

        if (!await userBranchAssignmentRepository.IsAssignedAsync(command.UserId, branch.Id, cancellationToken))
        {
            await userBranchAssignmentRepository.AddAsync(new UserBranchAssignment
            {
                UserId = command.UserId,
                BranchId = branch.Id,
                ClinicId = command.ClinicId
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
