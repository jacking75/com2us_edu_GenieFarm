// MailDTO.cs : MailController에서 사용하는 DTO 정의
// MailInfo : 단일 우편 조회

public class ReqMailInfoDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResMailInfoDTO : ErrorCodeDTO
{
    public MailModel Mail { get; set; }
}


// MailDelete : 우편 삭제

public class ReqMailDeleteDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResMailDeleteDTO : ErrorCodeDTO
{

}


// MailOpen : 우편 열기

public class ReqMailOpenDTO : InGameDTO
{
    public Int32 Page { get; set; }
}

public class ResMailOpenDTO : ErrorCodeDTO
{
    public Int32 Page { get; set; }
    public List<MailModel> MailList { get; set; }
}


// MailSend : 우편 발송

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


// MailReceive : 우편 아이템 수령

public class ReqMailReceiveDTO : InGameDTO
{
    public Int64 MailID { get; set; }
}

public class ResMailReceiveDTO : ErrorCodeDTO
{

}