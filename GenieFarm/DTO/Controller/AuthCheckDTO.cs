// AuthCheckDTO.cs : AuthCheckController���� ����ϴ� DTO ����
// Login : �α���

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


// Create : Ŭ���̾�Ʈ ���� ���� (���� ������ ����)

public class ReqCreateDTO : HiveAuthDTO
{
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