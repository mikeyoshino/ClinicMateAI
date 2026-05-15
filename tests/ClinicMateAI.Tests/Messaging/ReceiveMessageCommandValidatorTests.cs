using ClinicMateAI.Application.Messaging;
using ClinicMateAI.Logic.Messaging;
using FluentAssertions;

namespace ClinicMateAI.Tests.Messaging;

public class ReceiveMessageCommandValidatorTests
{
    private readonly ReceiveMessageCommandValidator _validator = new();

    [Fact]
    public void Validate_Fails_WhenClinicIdIsEmpty()
    {
        var command = CreateValidCommand() with { ClinicId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(ReceiveMessageCommand.ClinicId));
    }

    [Fact]
    public void Validate_Fails_WhenChannelIsNotSupported()
    {
        var command = CreateValidCommand() with { Channel = "WhatsApp" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(ReceiveMessageCommand.Channel));
    }

    [Fact]
    public void Validate_Fails_WhenRequiredTextFieldsAreMissing()
    {
        var command = CreateValidCommand() with
        {
            ExternalConversationId = "",
            CustomerDisplayName = "",
            Text = ""
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(ReceiveMessageCommand.ExternalConversationId));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(ReceiveMessageCommand.CustomerDisplayName));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(ReceiveMessageCommand.Text));
    }

    [Fact]
    public void Validate_Fails_WhenTextIsTooLong()
    {
        var command = CreateValidCommand() with { Text = new string('A', 4001) };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(ReceiveMessageCommand.Text));
    }

    [Fact]
    public void Validate_Passes_ForValidCommand()
    {
        var result = _validator.Validate(CreateValidCommand());

        result.IsValid.Should().BeTrue();
    }

    private static ReceiveMessageCommand CreateValidCommand()
    {
        return new ReceiveMessageCommand(
            ClinicId: Guid.NewGuid(),
            Channel: "LINE",
            ExternalConversationId: "line-001",
            CustomerDisplayName: "Customer A",
            Text: "โบท็อกกรามเท่าไรคะ",
            ReceivedAt: DateTimeOffset.UtcNow);
    }
}
