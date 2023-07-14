// AccountDTO.cs : AccountController에서 사용하는 DTO 정의
// Login : 로그인

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


// Register : 회원가입

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


// Logout : 로그아웃

public class ReqLogoutDTO : InGameDTO
{

}

public class ResLogoutDTO : ErrorCodeDTO
{

}

// ChangeNickname : 닉네임 변경

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