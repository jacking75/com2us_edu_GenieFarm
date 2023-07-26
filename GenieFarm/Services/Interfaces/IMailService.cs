public interface IMailService
{
    /// <summary>
    /// 요청한 페이지에 따라 MailWithItemDTO 리스트를 반환합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, List<MailModel>?>> GetMailListByPage(Int64 userId, Int32 page);

    /// <summary>
    /// 요청한 메일ID의 메일을 가져오고, 읽음 처리합니다. <br/>
    /// 읽음 처리는 읽지 않은 상태였을 때에만 수행합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, MailModel?>> GetMailAndSetRead(Int64 userId, Int64 mailId);

    ///// <summary>
    ///// 요청한 메일ID의 아이템 및 재화를 실제 지급하고 수령 완료 처리합니다.
    ///// </summary>
    //public Task<ErrorCode> ReceiveMailItem(Int64 userId, Int64 mailId);
}