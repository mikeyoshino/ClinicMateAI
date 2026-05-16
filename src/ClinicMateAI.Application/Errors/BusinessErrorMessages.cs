using ClinicMateAI.Domain.Errors;

namespace ClinicMateAI.Application.Errors;

/// <summary>
/// Maps BusinessErrorCode to Thai UI messages shown to clinic staff.
/// </summary>
public static class BusinessErrorMessages
{
    private static readonly Dictionary<BusinessErrorCode, string> Thai = new()
    {
        [BusinessErrorCode.ConversationNotFound]       = "ไม่พบบทสนทนานี้ กรุณาลองใหม่อีกครั้ง",
        [BusinessErrorCode.ConversationAlreadyClaimed] = "บทสนทนานี้ถูกรับโดยเจ้าหน้าที่คนอื่นแล้ว",
        [BusinessErrorCode.ClinicChannelNotConfigured] = "คลินิกนี้ยังไม่ได้เชื่อมต่อช่องทางนี้",
        [BusinessErrorCode.InvalidLineSignature]       = "ลายเซ็น LINE ไม่ถูกต้อง",
        [BusinessErrorCode.InvalidOperation]           = "ไม่สามารถดำเนินการได้ กรุณาลองใหม่อีกครั้ง",
    };

    public static string GetThai(BusinessErrorCode code)
        => Thai.TryGetValue(code, out var msg) ? msg : "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองใหม่อีกครั้ง";
}
