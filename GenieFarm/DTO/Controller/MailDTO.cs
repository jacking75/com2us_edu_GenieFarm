// MailDTO.cs : MailController에서 사용하는 DTO 정의
// MailInfo : 우편 

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