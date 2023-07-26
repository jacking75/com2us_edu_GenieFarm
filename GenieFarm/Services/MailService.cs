using ZLogger;

public class MailService : IMailService
{
    readonly ILogger<MailService> _logger;
    readonly IGameDb _gameDb;

    public MailService(ILogger<MailService> logger, IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
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
    public async Task<Tuple<ErrorCode, MailModel?>> GetMailAndSetRead(Int64 userId, Int64 mailId)
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
    /// 요청한 메일ID의 아이템 및 재화를 실제 지급하고 수령 완료 처리합니다.
    /// </summary>
    public async Task<ErrorCode> ReceiveMailItem(Int64 userId, Int64 mailId)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 메일에 아이템이나 재화가 있는지 확인하고, 데이터를 가져옴
        var mail = await _gameDb.GetMailByMailIdIfHasReward(userId, mailId);
        if (!ValidateMail(mail))
        {
            return ErrorCode.MailService_ReceiveMailItem_MailNotExists;
        }

        // 아이템 및 재화 지급
        var rewardResult = await GiveReward(userId, mail!, rollbackQueries);
        if (!Successed(rewardResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(rewardResult),
                                         new { UserID = userId, MailID = mailId }, "Failed");

            await Rollback(rewardResult, rollbackQueries);

            return ErrorCode.MailService_ReceiveMailItem_GiveReward_Fail;
        }

        // 수령 완료 처리
        var receiveResult = await SetReceived(userId, mailId);
        if (!Successed(receiveResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(receiveResult),
                                         new { UserID = userId, MailID = mailId }, "Failed");

            await Rollback(receiveResult, rollbackQueries);

            return ErrorCode.MailService_ReceiveMailItem_SetReceived;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 아이템이나 재화가 첨부되어 있는지 확인하고 지급 처리합니다.
    /// </summary>
    async Task<ErrorCode> GiveReward(Int64 userId, MailModel mail, List<SqlKata.Query> rollbackQueries)
    {
        // 아이템이 첨부되어 있는지 확인 후, 아이템 지급
        if (HasItem(mail!))
        {
            var itemInsertResult = await InsertRewardItem(userId, mail!.ItemCode, mail!.ItemCount, rollbackQueries);
            if (!Successed(itemInsertResult))
            {
                return itemInsertResult;
            }
        }

        // 재화가 첨부되어 있는지 확인 후, 재화 지급
        if (HasMoney(mail!))
        {
            var moneyUpdateResult = await IncreaseUserMoney(userId, mail!.Money, rollbackQueries);
            if (!Successed(moneyUpdateResult))
            {
                return moneyUpdateResult;
            }
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 메일 아이템 수령 완료 처리를 합니다.
    /// </summary>
    async Task<ErrorCode> SetReceived(Int64 userId, Int64 mailId)
    {
        try
        {
            // 메일 아이템 수령 완료 처리
            var affectedRow = await _gameDb.SetMailReceived(userId, mailId);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.MailService_SetReceived_Fail;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.MailService_SetReceived_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserID = userId, MailID = mailId }, "Failed");

            return errorCode;
        }
    }

    /// <summary>
    /// 유저의 소지금을 증가시킵니다.
    /// </summary>
    async Task<ErrorCode> IncreaseUserMoney(Int64 userId, Int32 money, List<SqlKata.Query> queries)
    {
        try
        {
            // 유저 소지금을 money만큼 증가
            var affectedRow = await _gameDb.IncreaseUserMoney(userId, money);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.MailService_IncreaseUserMoney_Fail;
            }

            // 성공 시, 롤백 쿼리 추가
            var query = _gameDb.GetQuery("farm_info").Where("UserId", userId).AsIncrement("Money", money * -1);
            queries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.MailService_IncreaseUserMoney_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserID = userId, Money = money }, "Failed");

            return errorCode;
        }
    }

    /// <summary>
    /// 보상 아이템을 지급합니다.
    /// </summary>
    async Task<ErrorCode> InsertRewardItem(Int64 userId, Int64 itemCode, Int16 itemCount, List<SqlKata.Query> queries)
    {
        try
        {
            // 아이템을 지급하고 지급된 아이템의 ID를 가져옴
            var itemId = await _gameDb.InsertGetIdRewardItem(userId, itemCode, itemCount);
            if (!ValidateItemId(itemId))
            {
                var errorCode = ErrorCode.MailService_InsertRewardItem_InvalidItemId;

                _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                             new { UserID = userId, ItemCode = itemCode,
                                                   itemCount = itemCount },
                                             "Failed");

                return errorCode;
            }

            // 성공 시, 롤백 쿼리 추가
            var query = _gameDb.GetQuery("user_item").Where("ItemId", itemId).AsDelete();
            queries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.MailService_InsertRewardItem_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserID = userId,
                                               ItemCode = itemCode,
                                               itemCount = itemCount },
                                         "Failed");

            return errorCode;
        }
    }

    /// <summary>
    /// 요청한 메일ID의 메일 데이터를 반환합니다. <br/>
    /// 아이템이 첨부되어 있다면 아이템 정보도 추가합니다.
    /// </summary>
    async Task<Tuple<ErrorCode, MailModel?>> GetMailByMailId(long userId, long mailId)
    {
        // 게임 DB에서 메일 데이터를 가져옴
        var mail = await _gameDb.GetMailByMailId(userId, mailId);
        if (!ValidateMail(mail))
        {
            return new (ErrorCode.MailService_MailNotExists, null);
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
    /// 메일이 아이템을 가지고 있는지 확인합니다.
    /// </summary>
    bool HasItem(MailModel mail)
    {
        return mail.ItemCode > 0;
    }

    /// <summary>
    /// 메일이 재화를 가지고 있는지 확인합니다.
    /// </summary>
    bool HasMoney(MailModel mail)
    {
        return mail.Money > 0;
    }

    /// <summary>
    /// 아이템 ID가 유효한지 체크합니다.
    /// </summary>
    bool ValidateItemId(Int64 itemId)
    {
        return itemId > 0;
    }

    /// <summary>
    /// 메일이 존재하는지 체크합니다.
    /// </summary>
    bool ValidateMail(MailModel? mail)
    {
        return mail != null && mail.MailId > 0;
    }

    /// <summary>
    /// 메일이 읽음 처리된 상태인지 체크합니다.
    /// </summary>
    bool NotRead(MailModel mail)
    {
        return !mail.IsRead;
    }

    /// <summary>
    /// GameDB에 쿼리 롤백을 요청합니다.
    /// </summary>
    async Task Rollback(ErrorCode errorCode, List<SqlKata.Query> queries)
    {
        await _gameDb.Rollback(errorCode, queries);
    }
}
