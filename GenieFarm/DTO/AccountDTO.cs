// AccountDTO.cs : AccountController���� ����ϴ� DTO ����
// Login : �α���

using System.ComponentModel.DataAnnotations;

public class ReqLoginDTO : AuthDTO
{
    [Required]
    [MinLength(1, ErrorMessage = "AppVersion must be at least 1 characters long.")]
    public string AppVersion { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "MasterDataVersion must be at least 1 characters long.")]
    public string MasterDataVersion { get; set; } = string.Empty;
}

public class ResLoginDTO : ErrorCodeDTO
{
    public AccountModel? UserData { get; set; }
}


// Register : ȸ������

public class ReqRegisterDTO : AuthDTO
{
    [Required]
    [MinLength(1, ErrorMessage = "Nickname must be at least 1 characters long.")]
    [MaxLength(10, ErrorMessage = "Nickname must be at most 10 characters long.")]
    public string Nickname { get; set; } = string.Empty;
}

public class ResRegisterDTO : ErrorCodeDTO
{

}


// Logout : �α׾ƿ�

public class ReqLogoutDTO : InGameDTO
{

}

public class ResLogoutDTO : ErrorCodeDTO
{

}

// ChangeNickname : �г��� ����

public class ReqChangeNicknameDTO : AuthDTO
{
    [Required]
    [MinLength(1, ErrorMessage = "Nickname must be at least 1 characters long.")]
    [MaxLength(10, ErrorMessage = "Nickname must be at most 10 characters long.")]
    public string Nickname { get; set; } = string.Empty;
}

public class ResChangeNicknameDTO : ErrorCodeDTO
{

}