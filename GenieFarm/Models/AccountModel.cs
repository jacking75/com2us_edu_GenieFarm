public class AccountModel
{
    public Int64 UserId { get; set; }
    public String AuthId { get; set; }
    public String Nickname { get; set; }
    public Int16 FarmLevel { get; set; }
    public Int64 FarmExp { get; set; }
    public DateTime LastLoginAt { get; set; }
    public Boolean PurchasedPass { get; set; }
    public Int16 MaxStorage { get; set; }
    public Int16 Love { get; set; }
}