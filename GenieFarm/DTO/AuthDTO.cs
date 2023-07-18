// Auth : Ŭ���̾�Ʈ�� �÷������κ��� ���� ���� ������ ��� DTO

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