namespace ClinicMateAI.Domain.Errors;

public enum BusinessErrorCode
{
    // Conversation
    ConversationNotFound,
    ConversationAlreadyClaimed,

    // LINE / Channel
    ClinicChannelNotConfigured,
    InvalidLineSignature,

    // Branches / access
    ClinicNotFound,
    BranchNotFound,
    BranchLimitExceeded,
    AccessDenied,

    // General
    InvalidOperation,
}
