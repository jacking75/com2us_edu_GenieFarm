// MailDTO.cs : MailController���� ����ϴ� DTO ����
// LoadPage : ������ ������ �� ��ȸ

// TODO : ��ӹ޴� DTO ������� ǥ�� ��Ŀ� ���� ����غ���

public class ReqLoadPageDTO : InGameDTO
{
    public Int32 Page { get; set; }
}

public class ResLoadPageDTO : ErrorCodeDTO
{
    public List<MailModel>? MailList { get; set; }
}


// LoadMail : ���� ���� ��ȸ

public class ReqLoadMailDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResLoadMailDTO : ErrorCodeDTO
{
    public MailModel? Mail { get; set; }
}


// ReceiveItem : ���� ������ ����

public class ReqReceiveItemDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResReceiveItemDTO : ErrorCodeDTO
{

}