using ClinicMateAI.Application.Messaging;
using FluentValidation;

namespace ClinicMateAI.Logic.Messaging;

public sealed class ReceiveMessageCommandValidator : AbstractValidator<ReceiveMessageCommand>
{
    private static readonly string[] AllowedChannels = ["LINE", "Facebook"];

    public ReceiveMessageCommandValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Channel)
            .NotEmpty()
            .Must(channel => AllowedChannels.Contains(channel))
            .WithMessage("Channel must be LINE or Facebook.");
        RuleFor(x => x.ExternalConversationId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerDisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ReceivedAt).NotEqual(default(DateTimeOffset));
    }
}
