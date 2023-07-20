using System.ComponentModel.DataAnnotations;

// Version : ���� ������ �����ϴ� DTO, �⺻������ ��� Request�� �� ����.
public class VersionDTO
{
    [Required]
    [MinLength(1, ErrorMessage = "AppVersion must be at least 1 characters long.")]
    public string AppVersion { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "MasterDataVersion must be at least 1 characters long.")]
    public string MasterDataVersion { get; set; } = string.Empty;
}


// InGame : UserID�� �����Ͽ� �����ϴ� �ΰ��� API ��û DTO
public class InGameDTO : VersionDTO
{
    public Int64 UserID { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "AuthToken must be at least 1 characters long.")]
    public string AuthToken { get; set; } = string.Empty;
}


// HiveAuth : PlayerID�� �����Ͽ� �����ϴ� ������ API DTO
public class HiveAuthDTO : VersionDTO
{
    public string PlayerID { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "AuthToken must be at least 1 characters long.")]
    public string AuthToken { get; set; } = string.Empty;
}