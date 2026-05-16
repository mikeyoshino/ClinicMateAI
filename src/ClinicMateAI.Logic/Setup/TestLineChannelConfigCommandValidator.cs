using ClinicMateAI.Application.Setup;
using FluentValidation;

namespace ClinicMateAI.Logic.Setup;

public sealed class TestLineChannelConfigCommandValidator : AbstractValidator<TestLineChannelConfigCommand>
{
    public TestLineChannelConfigCommandValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
    }
}
