// AccountDTO.cs : AccountController���� ����ϴ� DTO ����
// Login : �α���

using System.ComponentModel.DataAnnotations;

public class ReqLoginDTO : AuthDTO
{
    public string PlayerID { get; set; } = string.Empty;
}

public class ResLoginDTO : ErrorCodeDTO
{
    public DefaultDataDTO? DefaultData { get; set; }
    public string? AuthToken { get; set; }
}

public class DefaultDataDTO
{
    public AccountModel? UserData { get; set; }
    public AttendanceModel? AttendData { get; set; }
    public FarmInfoModel? FarmInfoData { get; set; }
}


// Create : Ŭ���̾�Ʈ ���� ���� (���� ������ ����)

public class ReqCreateDTO : AuthDTO
{
    public string PlayerID { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Nickname must be at least 1 characters long.")]
    [MaxLength(10, ErrorMessage = "Nickname must be at most 10 characters long.")]
    public string Nickname { get; set; } = string.Empty;
}

public class ResCreateDTO : ErrorCodeDTO
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