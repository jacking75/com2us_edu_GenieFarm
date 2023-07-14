public class AccountModel
{
    public Int64 UserId { get; set; }

    public string AuthId { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public Int16 FarmLevel { get; set; }

    public Int64 FarmExp { get; set; }

    public DateTime LastLoginAt { get; set; }

    public bool PurchasedPass { get; set; }

    public Int16 MaxStorage { get; set; }

    public Int16 Love { get; set; }
}