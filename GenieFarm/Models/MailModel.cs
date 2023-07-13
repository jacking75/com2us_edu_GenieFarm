public class MailModel
{
    public Int64 MailId { get; set; }
    public Int64 ReceiverId { get; set; }
    public Int64 SenderId { get; set; }
    public String Title { get; set; }
    public String Content { get; set; }
    public DateTime ObtainedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public Boolean IsRead { get; set; }
    public Boolean IsDeleted { get; set; }
    public Int64 ItemId { get; set; }
    public Boolean IsReceived { get; set; }
}