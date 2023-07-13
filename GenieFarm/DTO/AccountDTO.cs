// AccountDTO.cs : AccountController에서 사용하는 DTO 정의
// Login : 로그인

public class ReqLoginDTO : AuthDTO
{
    public String AppVersion { get; set; }
    public String MasterDataVersion { get; set; }
}

public class ResLoginDTO : ErrorCodeDTO
{
    public AccountModel UserData { get; set; }
}


// Register : 회원가입

public class ReqRegisterDTO : AuthDTO
{
    public String Nickname { get; set; }
    public String AppVersion { get; set; }
    public String MasterDataVersion { get; set; }
}

public class ResRegisterDTO : ErrorCodeDTO
{

}


// Logout : 로그아웃

public class ReqLogoutDTO : AuthDTO
{

}

public class ResLogoutDTO : ErrorCodeDTO
{

}

// ChangeNickname : 닉네임 변경

public class ReqChangeNicknameDTO : AuthDTO
{
    public String Nickname { get; set; }
}

public class ResChangeNicknameDTO : ErrorCodeDTO
{

}