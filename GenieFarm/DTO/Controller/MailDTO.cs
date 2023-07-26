// MailDTO.cs : MailController���� ����ϴ� DTO ����
// LoadPage : ������ ������ �� ��ȸ

public class ReqLoadPageDTO : InGameDTO
{
    public Int32 Page { get; set; }
}

public class ResLoadPageDTO : ErrorCodeDTO
{
    public List<MailModel>? MailList { get; set; }
}

public class MailWithItemDTO : MailModel
{
    public ItemAttributeModel? ItemAttribute { get; set; }
}


// LoadMail : ���� ���� ��ȸ

public class ReqLoadMailDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResLoadMailDTO : ErrorCodeDTO
{
    public MailWithItemDTO? Mail { get; set; }
}