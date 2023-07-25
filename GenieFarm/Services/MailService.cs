public class MailService : IMailService
{
    readonly ILogger<MailService> _logger;
    readonly IGameDb _gameDb;
    readonly IMasterDb _masterDb;

    public MailService(ILogger<MailService> logger, IGameDb gameDb, IMasterDb masterDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _masterDb = masterDb;
    }

    /// <summary>
    /// 요청한 페이지에 따라 MailWithItemDTO 리스트를 반환합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, List<MailModel>?>> GetMailListByPage(Int64 userId, Int32 page)
    {
        // 유효한 페이지 번호인지 검증
        if (!ValidatePage(page))
        {
            return new (ErrorCode.MailService_InvalidPageNum, null);
        }

        // 페이지에 해당하는 메일 리스트 불러오기
        var mailList = await _gameDb.GetMailListByPage(userId, page);
        return new (ErrorCode.None, mailList);
    }

    /// <summary>
    /// 유효한 페이지 번호인지 체크합니다.
    /// </summary>
    bool ValidatePage(Int32 page)
    {
        return page > 0;
    }
}
