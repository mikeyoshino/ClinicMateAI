using ClinicMateAI.Application.Setup;
using FluentValidation;

namespace ClinicMateAI.Logic.Setup;

public sealed class RenewFacebookConnectionCommandValidator : AbstractValidator<RenewFacebookConnectionCommand>
{
    public RenewFacebookConnectionCommandValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}
