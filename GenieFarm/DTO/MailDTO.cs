// MailDTO.cs : MailController���� ����ϴ� DTO ����
// MailInfo : ���� ���� ��ȸ

public class ReqMailInfoDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResMailInfoDTO : ErrorCodeDTO
{
    public MailModel Mail { get; set; }
}


// MailDelete : ���� ����

public class ReqMailDeleteDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResMailDeleteDTO : ErrorCodeDTO
{

}


// MailOpen : ���� ����

public class ReqMailOpenDTO : InGameDTO
{
    public Int32 Page { get; set; }
}

public class ResMailOpenDTO : ErrorCodeDTO
{
    public Int32 Page { get; set; }
    public List<MailModel> MailList { get; set; }
}


// MailSend : ���� �߼�

public class ReqMailSendDTO : InGameDTO
{
    public Int64 ReceiverID { get; set; }
    public String Title { get; set; }
    public String Content { get; set; }
    public Int64 ItemID { get; set; }
}

public class ResMailSendDTO : ErrorCodeDTO
{

}


// MailReceive : ���� ������ ����

public class ReqMailReceiveDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResMailReceiveDTO : ErrorCodeDTO
{

}