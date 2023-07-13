public class MailData
{
    public Int64 MailId { get; set; }
    public Int64 UserId { get; set; }
    public Int64 SenderId { get; set; }
    public String Title { get; set; }
    public String Content { get; set; }
    public Boolean IsRead { get; set; }
    public Boolean HasItem { get; set; }
    public Boolean IsDeleted { get; set; }
    public DateTime ObtainedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
}