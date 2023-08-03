using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Bcpg;
using SqlKata;
using ZLogger;

/// <summary>
/// 경매장과 관련된 비즈니스 로직을 처리하고 <br/>
/// DB Operation Call을 수행하는 서비스 클래스
/// </summary>
public class AuctionService : IAuctionService
{
    ILogger<AuctionService> _logger;
    IGameDb _gameDb;
    IMasterDb _masterDb;

    public AuctionService(ILogger<AuctionService> logger, IGameDb gameDb, IMasterDb masterDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _masterDb = masterDb;
    }

    /// <summary>
    /// 요청한 페이지에 따라 지정한 TypeCode와 일치하는 아이템 리스트를 반환합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, List<AuctionModel>>> GetItemListByPageFromTypeCode(Int32 page, Int32 typeCode, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        var itemList = new List<AuctionModel>();
        var errorCode = ErrorCode.None;

        try
        {
            minPrice = minPrice == 0 ? 0 : minPrice;
            maxPrice = maxPrice == 0 ? Int32.MaxValue : maxPrice;

            (errorCode, itemList) = await _gameDb.GetAuctionListByTypeCode(page, typeCode, minPrice, maxPrice, sortBy, sortOrder);

            if (errorCode == ErrorCode.None && itemList.Count == 0)
            {
                errorCode = ErrorCode.AuctionService_GetItemListByPage_EmptyItemList;
            }   

            return new(errorCode, itemList);
        }
        catch (Exception ex)
        {
            errorCode = ErrorCode.AuctionService_GetItemListByPageFromTypeCode_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new {}, "Failed");

            return new(errorCode, itemList);
        }
    }

    /// <summary>
    /// 요청한 페이지에 따라 지정한 ItemName과 일치하는 아이템 리스트를 반환합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, List<AuctionModel>>> GetItemListByPageFromItemName(Int32 page, string itemName, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        var itemList = new List<AuctionModel>();
        var errorCode = ErrorCode.None;

        try
        {
            minPrice = minPrice == 0 ? 0 : minPrice;
            maxPrice = maxPrice == 0 ? Int32.MaxValue : maxPrice;

            (errorCode, itemList) = await _gameDb.GetAuctionListByItemName(page, itemName, minPrice, maxPrice, sortBy, sortOrder);

            if (errorCode == ErrorCode.None && itemList.Count == 0)
            {
                errorCode = ErrorCode.AuctionService_GetItemListByPage_EmptyItemList;
            }

            return new(errorCode, itemList);
        }
        catch (Exception ex)
        {
            errorCode = ErrorCode.AuctionService_GetItemListByPageFromItemName_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { }, "Failed");

            return new(errorCode, itemList);
        }
    }

    /// <summary>
    /// 지정한 경매 물품에 대해 클라이언트가 요청한 가격으로 입찰에 참여합니다.
    /// 현재 입찰가보다 낮은 가격으로는 입찰할 수 없습니다.
    /// </summary>
    public async Task<ErrorCode> BidAuction(Int64 userId, Int64 auctionId, Int32 bidPrice)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 입찰가 확인
        var checkResult = await CheckAuctionPrice(auctionId, bidPrice);
        if (!Successed(checkResult))
        {
            return checkResult;
        }

        // 유저가 입찰가만큼의 재화를 보유중인지 확인하고 차감
        var checkUpdateResult = await CheckAndUpdateUserMoneyForBidPrice(userId, bidPrice, rollbackQueries);
        if (!Successed(checkUpdateResult))
        {
            return checkUpdateResult;
        }

        // 입찰가 및 유저 갱신
        var updateResult = await RenewAuctionBid(auctionId, userId, bidPrice);
        if (!Successed(updateResult))
        {
            await Rollback(updateResult, rollbackQueries);

            return updateResult;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 지정한 경매 물품을 즉시 구매합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, UserItemModel>> BuyNowAuction(Int64 userId, Int64 auctionId)
    {
        var rollbackQueries = new List<SqlKata.Query>();
        var item = new UserItemModel();

        // 즉시구매가 확인
        var buyNowPrice = await _gameDb.GetAuctionBuyNowPrice(auctionId);
        if (buyNowPrice == 0)
        {
            return new(ErrorCode.AuctionService_GetAuctionBuyNowPrice_InvalidAuctionId, item);
        }

        // 유저가 즉시구매가만큼의 재화를 보유중인지 확인하고 차감
        var checkUpdateResult = await CheckAndUpdateUserMoneyForBidPrice(userId, buyNowPrice, rollbackQueries);
        if (!Successed(checkUpdateResult))
        {
            return new(checkUpdateResult, item);
        }

        // 경매 물품 정보 가져오기
        item = await _gameDb.GetAuctionItem(auctionId);
        if (!ValidateItem(item))
        {
            return new(ErrorCode.AuctionService_GetAuctionItem_InvalidAuctionId, item);
        }

        // 경매 물품 구매 완료 처리하기
        var terminateResult = await TerminateAuctionSoldOut(auctionId, rollbackQueries);
        if (!Successed(terminateResult))
        {
            return new(terminateResult, item);
        }

        // 유저에게 아이템 지급
        var provideResult = await ProvideItemToUser(userId, item);
        if (!Successed(provideResult))
        {
            await Rollback(provideResult, rollbackQueries);

            return new(provideResult, item);
        }

        return new(ErrorCode.None, item);
    }

    /// <summary>
    /// 클라이언트가 요청한 아이템을 경매장에 등록합니다.
    /// 최소 입찰가, 즉시 구매가가 설정되어야합니다.
    /// </summary>
    public async Task<ErrorCode> RegisterAuction(Int64 userId, Int64 itemId, Int32 bidPrice, Int32 buyNowPrice)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 유저가 지정한 아이템 확인 후 삭제 처리
        var item = await _gameDb.GetUserItem(itemId, rollbackQueries);
        if (!ValidateItem(item))
        {
            return ErrorCode.AuctionService_RegisterAuction_InvalidItemId;
        }

        // 경매장 등록
        var registerResult = await RegisterUserItemToAuction(item, bidPrice, buyNowPrice);
        if (!Successed(registerResult))
        {
            return registerResult;

            await Rollback(registerResult, rollbackQueries);
        }

        return ErrorCode.None;
    }


    /// <summary>
    /// 현재 설정되어있는 입찰가를 확인합니다.
    /// 입찰하고자 하는 금액이 현재입찰가 초과 즉시구매가 미만인지 판별합니다.
    /// </summary>
    async Task<ErrorCode> CheckAuctionPrice(Int64 auctionId, Int32 bidPrice)
    {
        (var curBidPrice, var buyNowPrice) = await _gameDb.GetAuctionPriceInfo(auctionId);
        if (curBidPrice == 0 || buyNowPrice == 0)
        {
            return ErrorCode.AuctionService_GetAUctionPriceInfo_InvalidAuctionId;
        }
        else if (bidPrice <= curBidPrice)
        {
            return ErrorCode.AuctionService_GetAuctionPriceInfo_LowBidPrice;
        }
        else if (bidPrice >= buyNowPrice)
        {
            return ErrorCode.AuctionService_GetAuctionPriceInfo_HighBidPrice;
        }
        
        return ErrorCode.None;
    }

    /// <summary>
    /// 유저가 설정한 입찰가만큼의 재화를 보유했을시 차감합니다.
    /// </summary>
    async Task<ErrorCode> CheckAndUpdateUserMoneyForBidPrice(Int64 userId, Int32 bidPrice, List<SqlKata.Query> rollbackQueries)
    {
        var affectedRow = await _gameDb.DecrementUserMoney(userId, bidPrice);
        if (!ValidateAffectedRow(affectedRow, 1))
        {
            return ErrorCode.AuctionService_CheckAndUpdateUserMoneyForBidPrice_NotEnoughMoney;
        }

        // 성공 시, 롤백 쿼리 추가
        var query = _gameDb.GetQuery("farm_info").Where("user_id", userId).AsIncrement("Money", bidPrice);
        rollbackQueries.Add(query);

        return ErrorCode.None;
    }

    /// <summary>
    /// 경매 물품의 현재 입찰 정보를 갱신합니다.
    /// </summary>
    async Task<ErrorCode> RenewAuctionBid(Int64 auctionId, Int64 userId, Int32 bidPrice)
    {
        var affectedRow = await _gameDb.UpdateAuctionBidInfo(auctionId, userId, bidPrice);
        if (!ValidateAffectedRow(affectedRow, 1))
        {
            return ErrorCode.AuctionService_UpdateAuctionItemBidInfo_Fail;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 경매 물품을 구매 완료 처리합니다.
    /// </summary>
    async Task<ErrorCode> TerminateAuctionSoldOut(Int64 auctionId, List<SqlKata.Query> rollbackQueries)
    {
        var affectedRow = await _gameDb.UpdateAuctionPurchased(auctionId);
        if (!ValidateAffectedRow(affectedRow, 1))
        {
            return ErrorCode.AuctionService_UpdateAuctionItemPurchased_Fail;
        }

        // 성공 시, 롤백 쿼리 추가
        var query = _gameDb.GetQuery("auction_info").Where("AuctionId", auctionId).AsUpdate(new { IsPurchased = false });
        rollbackQueries.Add(query);

        return ErrorCode.None;
    }

    /// <summary>
    /// 경매 물품을 유저에게 지급합니다.
    /// </summary>
    async Task<ErrorCode> ProvideItemToUser(Int64 userId, UserItemModel item)
    {
        var affectedRow = await _gameDb.InsertAuctionItemToUser(userId, item);
        if (!ValidateAffectedRow(affectedRow, 1))
        {
            return ErrorCode.AuctionService_InsertAuctionItemToUser_Fail;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 유저 아이템을 경매장에 등록합니다.
    /// </summary>
    async Task<ErrorCode> RegisterUserItemToAuction(UserItemModel item, Int32 bidPrice, Int32 buyNowPrice)
    {
        var typeCode = _masterDb._itemAttributeList.Find(x => x.Code == item.ItemCode).TypeCode;
        if (typeCode == null)
        {
            return ErrorCode.AuctionService_RegisterUserItemToAuction_InvalidItemCode;
        }

        var affectedRow = await _gameDb.InsertUserItemToAuction(item, typeCode, bidPrice, buyNowPrice);
        if(!ValidateAffectedRow(affectedRow, 1))
        {
            return ErrorCode.AuctionService_RegisterUserItemToAuction_Fail;
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
    /// 유효한 아이템인지 체크합니다.
    /// </summary>
    bool ValidateItem(UserItemModel? item)
    {
        return item != null && item.ItemID > 0;
    }

    /// <summary>
    /// 에러코드가 ErrorCode.None이면 true를 리턴하고, 아니면 false를 리턴합니다.
    /// </summary>
    bool Successed(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.None;
    }

    /// <summary>
    /// GameDB에 쿼리 롤백을 요청합니다.
    /// </summary>
    async Task Rollback(ErrorCode errorCode, List<SqlKata.Query> queries)
    {
        await _gameDb.Rollback(errorCode, queries);
    }
}