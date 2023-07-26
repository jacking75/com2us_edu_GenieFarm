// MailDTO.cs : MailController에서 사용하는 DTO 정의
// LoadPage : 우편함 페이지 별 조회

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


// LoadMail : 개별 우편 조회

public class ReqLoadMailDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResLoadMailDTO : ErrorCodeDTO
{
    public MailWithItemDTO? Mail { get; set; }
}