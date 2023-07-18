// Auth : 클라이언트가 플랫폼으로부터 받은 인증 정보를 담는 DTO

using System.ComponentModel.DataAnnotations;

public class AuthDTO
{
    public string AuthToken { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "AppVersion must be at least 1 characters long.")]
    public string AppVersion { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "MasterDataVersion must be at least 1 characters long.")]
    public string MasterDataVersion { get; set; } = string.Empty;
}