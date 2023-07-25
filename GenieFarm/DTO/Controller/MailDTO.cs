// MailDTO.cs : MailController���� ����ϴ� DTO ����
// MailInfo : ���� 

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