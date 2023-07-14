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
    public string Nickname { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string MasterDataVersion { get; set; } = string.Empty;
}

public class ResRegisterDTO : ErrorCodeDTO
{

}


// Logout : �α׾ƿ�

public class ReqLogoutDTO : AuthDTO
{

}

public class ResLogoutDTO : ErrorCodeDTO
{

}

// ChangeNickname : �г��� ����

public class ReqChangeNicknameDTO : AuthDTO
{
    public string Nickname { get; set; } = string.Empty;
}

public class ResChangeNicknameDTO : ErrorCodeDTO
{

}