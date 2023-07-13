// InGame : UserID를 포함하여 동작하는 인게임 API 요청 DTO

public class InGameDTO : AuthDTO
{
    public Int64 UserID { get; set; }
}