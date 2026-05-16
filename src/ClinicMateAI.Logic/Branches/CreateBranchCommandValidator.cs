using ClinicMateAI.Application.Branches;
using FluentValidation;

namespace ClinicMateAI.Logic.Branches;

public sealed class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MapUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BusinessHours).NotEmpty().MaximumLength(1000);
    }
}
