// AuthCheckDTO.cs : AuthCheckController에서 사용하는 DTO 정의
// Login : 로그인

using System.ComponentModel.DataAnnotations;

public class ReqLoginDTO : HiveAuthDTO
{
}

public class ResLoginDTO : ErrorCodeDTO
{
    public DefaultDataDTO? DefaultData { get; set; }
    public string? AuthToken { get; set; }
}

public class DefaultDataDTO
{
    public UserBasicInfoModel? UserData { get; set; }
    public AttendanceModel? AttendData { get; set; }
    public FarmInfoModel? FarmInfoData { get; set; }
}


// Create : 클라이언트 최초 접속 (게임 데이터 생성)

public class ReqCreateDTO : HiveAuthDTO
{
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