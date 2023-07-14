using System.Net;

public class LogModel
{
    public Int64 RequestId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string ClientIP { get; set; } = string.Empty;

    public bool IsRequest { get; set; }

    public ErrorCode ErrorCode { get; set; }

    public HttpStatusCode StatusCode { get; set; }

    public string RawBody { get; set; } = string.Empty;
}