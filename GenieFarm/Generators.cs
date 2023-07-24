public static class RedisLockKeyGenerator
{
    public static string Create(string userId)
    {
        return $"lock:{userId}";
    }
}

public static class AttendanceRewardMailGenerator
{
    public static MailModel Create(Int64 receiverId, Int64 senderId, Int32 attendanceCount, Int32 expiry, Int64 itemId, Int32 money)
    {
        return new MailModel { Title = $"출석 보상 지급",
                               Content = $"{attendanceCount}일차 출석 보상입니다.",
                               ReceiverId = receiverId, SenderId = senderId,
                               ExpiredAt = DateTime.Now.AddDays(expiry),
                               ItemId = itemId, Gold = money,
                               IsDeleted = false, IsRead = false, IsReceived = false };
    }
}

public static class EventIdGenerator
{
    public static EventId Create(ErrorCode errorCode)
    {
        return new EventId((UInt16)errorCode, errorCode.ToString());
    }

    public static EventId Create(UInt16 eventId, string eventIdName)
    {
        return new EventId(eventId, eventIdName);
    }
}