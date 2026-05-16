namespace ClinicMateAI.Application.Branches;

public interface IAssignUserToBranchHandler
{
    Task HandleAsync(AssignUserToBranchCommand command, CancellationToken cancellationToken = default);
}
