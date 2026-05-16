using ClinicMateAI.Application.Setup;
using FluentValidation;

namespace ClinicMateAI.Logic.Setup;

public sealed class CompleteFacebookConnectionCommandValidator : AbstractValidator<CompleteFacebookConnectionCommand>
{
    public CompleteFacebookConnectionCommandValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AuthorizationCode).NotEmpty().MaximumLength(500);
    }
}
