public class MailOpenResponse
{
    public ErrorCode Result { get; set; }
    public Int32 Page { get; set; }
    public List<MailData> Mails { get; set; }
}