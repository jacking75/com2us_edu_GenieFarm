// AccountDTO.cs : AccountController에서 사용하는 DTO 정의
// Login : 로그인

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


// Create : 클라이언트 최초 접속 (게임 데이터 생성)

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