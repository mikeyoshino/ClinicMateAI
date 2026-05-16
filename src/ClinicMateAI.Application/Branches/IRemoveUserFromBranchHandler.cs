namespace ClinicMateAI.Application.Branches;

public interface IRemoveUserFromBranchHandler
{
    Task HandleAsync(RemoveUserFromBranchCommand command, CancellationToken cancellationToken = default);
}
