namespace ClinicMateAI.Logic.Ai;

public static class RedFlagDetector
{
    private static readonly string[] Keywords =
    [
        "แพ้",
        "หายใจไม่ออก",
        "บวมมาก",
        "ปวดมาก",
        "มีไข้",
        "หนอง",
        "ติดเชื้อ",
        "หน้าชา",
        "ตามัว",
        "เลือดออก",
        "ฟิลเลอร์ไหล",
        "ฉีดแล้วเป็นก้อน",
        "ขอคืนเงิน",
        "ร้องเรียน"
    ];

    public static bool ContainsRedFlag(string message)
    {
        return Keywords.Any(keyword => message.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
