// Auth : 클라이언트가 플랫폼으로부터 받은 인증 정보를 담는 DTO

public class AuthDTO
{
    public string PlayerID { get; set; } = string.Empty;

    public string AuthToken { get; set; } = string.Empty;
}