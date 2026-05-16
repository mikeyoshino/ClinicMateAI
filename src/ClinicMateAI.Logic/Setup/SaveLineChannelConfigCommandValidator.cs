using ClinicMateAI.Application.Setup;
using FluentValidation;

namespace ClinicMateAI.Logic.Setup;

public sealed class SaveLineChannelConfigCommandValidator : AbstractValidator<SaveLineChannelConfigCommand>
{
    public SaveLineChannelConfigCommandValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.ChannelSecret).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AccessToken).NotEmpty().MaximumLength(500);
    }
}
