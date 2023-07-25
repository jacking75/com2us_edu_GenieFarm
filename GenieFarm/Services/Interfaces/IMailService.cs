public interface IMailService
{
    /// <summary>
    /// 요청한 페이지에 따라 MailWithItemDTO 리스트를 반환합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, List<MailModel>?>> GetMailListByPage(Int64 userId, Int32 page);

    /// <summary>
    /// 요청한 MailID와 UserID가 모두 일치하는 메일 데이터를 반환합니다. <br/>
    /// 아이템이 첨부되어 있다면 아이템 정보도 추가합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, MailWithItemDTO?>> GetMailByMailId(Int64 userId, Int64 mailId);

    /// <summary>
    /// 요청한 MailID와 UserID가 모두 일치하는 메일을 읽음 처리합니다.
    /// </summary>
    public Task<ErrorCode> SetMailIsRead(Int64 userId, Int64 mailId);
}