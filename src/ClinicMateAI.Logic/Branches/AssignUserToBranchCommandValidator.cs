using ClinicMateAI.Application.Branches;
using FluentValidation;

namespace ClinicMateAI.Logic.Branches;

public sealed class AssignUserToBranchCommandValidator : AbstractValidator<AssignUserToBranchCommand>
{
    public AssignUserToBranchCommandValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(450);
    }
}
