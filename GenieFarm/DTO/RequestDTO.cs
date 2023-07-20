using System.ComponentModel.DataAnnotations;

// Version : 버전 정보를 포함하는 DTO, 기본적으로 모든 Request에 다 들어간다.
public class VersionDTO
{
    [Required]
    [MinLength(1, ErrorMessage = "AppVersion must be at least 1 characters long.")]
    public string AppVersion { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "MasterDataVersion must be at least 1 characters long.")]
    public string MasterDataVersion { get; set; } = string.Empty;
}


// InGame : UserID를 포함하여 동작하는 인게임 API 요청 DTO
public class InGameDTO : VersionDTO
{
    public Int64 UserID { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "AuthToken must be at least 1 characters long.")]
    public string AuthToken { get; set; } = string.Empty;
}


// HiveAuth : PlayerID를 포함하여 동작하는 인증용 API DTO
public class HiveAuthDTO : VersionDTO
{
    public string PlayerID { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "AuthToken must be at least 1 characters long.")]
    public string AuthToken { get; set; } = string.Empty;
}