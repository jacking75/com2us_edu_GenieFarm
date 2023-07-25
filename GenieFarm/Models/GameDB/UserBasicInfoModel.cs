public class UserBasicInfoModel
{
    public Int64 UserId { get; set; }

    public string PlayerId { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public DateTime LastLoginAt { get; set; }

    public DateTime PassEndDate { get; set; }
}