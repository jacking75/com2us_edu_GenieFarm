using ZLogger;

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
    /// 요청한 메일ID의 메일을 읽음 처리합니다. <br/>
    /// 읽음 처리는 읽지 않은 상태였을 때에만 수행합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, MailWithItemDTO?>> GetMailAndSetRead(Int64 userId, Int64 mailId)
    {
        // 메일 데이터 로드
        (var getMailResult, var mail) = await GetMailByMailId(userId, mailId);
        if (!Successed(getMailResult))
        {
            return new (ErrorCode.MailService_GetMailAndSetRead_MailNotExists, null);
        }

        // 읽지 않은 메일이라면 읽음 처리
        if (NotRead(mail!))
        {
            var readResult = await SetReadIfNotRead(userId, mailId);
            if (!Successed(readResult))
            {
                return new (ErrorCode.MailService_GetMailAndSetRead_SetRead, null);
            }
        }

        return new (ErrorCode.None, mail);
    }

    /// <summary>
    /// 요청한 메일ID의 메일 데이터를 반환합니다. <br/>
    /// 아이템이 첨부되어 있다면 아이템 정보도 추가합니다.
    /// </summary>
    async Task<Tuple<ErrorCode, MailWithItemDTO?>> GetMailByMailId(long userId, long mailId)
    {
        // 게임 DB에서 메일 데이터를 가져옴
        var mail = await _gameDb.GetMailByMailId(userId, mailId);
        if (!ValidateMail(mail))
        {
            return new (ErrorCode.MailService_MailNotExists, null);
        }

        // 아이템이 첨부되어 있으면 아이템 정보 추가
        if (HasItem(mail!))
        {
            var errorCode = await SetItemAttribute(mail!);
            if (!Successed(errorCode))
            {
                _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                             new { UserID = userId, MailID = mailId,
                                                   ItemID = mail!.ItemId }, "Failed");

                return new (ErrorCode.MailService_GetMailByMailId_SetItemAttribute, null);
            }
        }

        return new (ErrorCode.None, mail);
    }

    /// <summary>
    /// 메일을 읽음 처리합니다.
    /// </summary>
    async Task<ErrorCode> SetReadIfNotRead(Int64 userId, Int64 mailId)
    {
        var affectedRow = await _gameDb.UpdateMailIsRead(userId, mailId);
        if (!ValidateAffectedRow(affectedRow, 1))
        {
            var errorCode = ErrorCode.MailService_UpdateMailIsRead;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new { UserID = userId, MailID = mailId }, "Failed");

            return errorCode;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 메일에 첨부되어있는 아이템 데이터를 추가합니다.
    /// </summary>
    async Task<ErrorCode> SetItemAttribute(MailWithItemDTO mail)
    {
        // 아이템 종류와 개수 정보를 가져옴
        var itemCodeAndCount = await _gameDb.GetItemCodeAndCountByItemId(mail!.ItemId);
        if (!ValidateItem(itemCodeAndCount))
        {
            return ErrorCode.MailService_SetItemAttribute_InvalidItemCodeAndCount;
        }

        // 마스터DB에서 아이템 Code에 해당하는 데이터를 가져옴
        var itemAttribute = _masterDb._itemAttributeList!.Find(x => x.Code == itemCodeAndCount!.ItemCode);
        mail.ItemCount = itemCodeAndCount!.ItemCount;
        mail.ItemAttribute = itemAttribute;

        return ErrorCode.None;
    }

    /// <summary>
    /// Update, Insert, Delete 쿼리의 영향을 받은 행의 개수가 <br/>
    /// 기대한 값과 동일한지 판단해 true, false를 리턴합니다.
    /// </summary>
    bool ValidateAffectedRow(Int32 affectedRow, Int32 expected)
    {
        return affectedRow == expected;
    }

    /// <summary>
    /// 에러코드가 ErrorCode.None이면 true를 리턴하고, 아니면 false를 리턴합니다.
    /// </summary>
    bool Successed(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.None;
    }

    /// <summary>
    /// 유효한 페이지 번호인지 체크합니다.
    /// </summary>
    bool ValidatePage(Int32 page)
    {
        return page > 0;
    }

    /// <summary>
    /// 아이템 데이터가 유효한지 확인합니다.
    /// </summary>
    bool ValidateItem(FarmItemModel? itemData)
    {
        if (itemData == null || itemData.ItemCode == 0 || itemData.ItemCount == 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 메일이 존재하는지 체크합니다.
    /// </summary>
    bool ValidateMail(MailWithItemDTO? mail)
    {
        return mail != null;
    }

    /// <summary>
    /// 아이템이 첨부된 메일인지 체크합니다.
    /// </summary>
    bool HasItem(MailWithItemDTO mail)
    {
        return mail.ItemId != 0;
    }

    /// <summary>
    /// 메일이 읽음 처리된 상태인지 체크합니다.
    /// </summary>
    bool NotRead(MailWithItemDTO mail)
    {
        return !mail.IsRead;
    }
}
