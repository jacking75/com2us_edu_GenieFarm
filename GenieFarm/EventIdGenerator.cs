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