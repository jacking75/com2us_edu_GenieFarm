public class MailSendRequest : GamePacket
{
    public Int64 UserID { get; set; }
    public Int64 ReceiverID { get; set; }
    public String Title { get; set; }
    public String Content { get; set; }
    public Boolean HasItem { get; set; }
    public Int64 ItemID { get; set; }
    public Int64 ItemCode { get; set; }
    public Int64 ItemCount { get; set; }
}