// AccountDTO.cs : AccountController���� ����ϴ� DTO ����
// Login : �α���

public class ReqLoginDTO : AuthDTO
{
    public String AppVersion { get; set; }
    public String MasterDataVersion { get; set; }
}

public class ResLoginDTO : ErrorCodeDTO
{
    public AccountModel UserData { get; set; }
}


// Register : ȸ������

public class ReqRegisterDTO : AuthDTO
{
    public String Nickname { get; set; }
    public String AppVersion { get; set; }
    public String MasterDataVersion { get; set; }
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
    public String Nickname { get; set; }
}

public class ResChangeNicknameDTO : ErrorCodeDTO
{

}