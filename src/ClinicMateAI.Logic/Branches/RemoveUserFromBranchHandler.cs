using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;

namespace ClinicMateAI.Logic.Branches;

public sealed class RemoveUserFromBranchHandler(
    IUserBranchAssignmentRepository userBranchAssignmentRepository,
    IUnitOfWork unitOfWork) : IRemoveUserFromBranchHandler
{
    public async Task HandleAsync(RemoveUserFromBranchCommand command, CancellationToken cancellationToken = default)
    {
        await userBranchAssignmentRepository.RemoveAsync(command.UserId, command.BranchId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
