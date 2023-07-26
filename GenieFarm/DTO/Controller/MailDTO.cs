// MailDTO.cs : MailController에서 사용하는 DTO 정의
// LoadPage : 우편함 페이지 별 조회

// TODO : 상속받는 DTO 내용들의 표기 방식에 대해 고민해보기

public class ReqLoadPageDTO : InGameDTO
{
    public Int32 Page { get; set; }
}

public class ResLoadPageDTO : ErrorCodeDTO
{
    public List<MailModel>? MailList { get; set; }
}


// LoadMail : 개별 우편 조회

public class ReqLoadMailDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResLoadMailDTO : ErrorCodeDTO
{
    public MailModel? Mail { get; set; }
}


// ReceiveItem : 우편 아이템 수령

public class ReqReceiveItemDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResReceiveItemDTO : ErrorCodeDTO
{

}