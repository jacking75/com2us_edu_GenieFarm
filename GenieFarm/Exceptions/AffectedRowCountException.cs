using System.Runtime.Serialization;

public class AffectedRowCountOutOfRangeException : Exception
{
    public Int32 _affectedRowCount { get; set; }
    public Int32 _defaultItemCount { get; set; }

    public AffectedRowCountOutOfRangeException()
    {
    }

    public AffectedRowCountOutOfRangeException(string? message) : base(message)
    {
    }

    public AffectedRowCountOutOfRangeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected AffectedRowCountOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public AffectedRowCountOutOfRangeException(Int32 affectedRowCount, Int32 defaultItemCount)
    {
        _affectedRowCount = affectedRowCount;
        _defaultItemCount = defaultItemCount;
    }
}
