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
    /// 요청한 페이지에 따라 지정한 TypeCode와 일치하는 경매 물품 리스트를 반환합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, List<AuctionModel>>> GetAuctionListByPageFromTypeCode(Int32 page, Int32 typeCode, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        var itemList = new List<AuctionModel>();
        var errorCode = ErrorCode.None;

        try
        {
            // 올바른 TypeCode인지 확인
            var itemType = _masterDb._itemTypeList.Find(x => x.TypeCode == typeCode);
            if (itemType == null)
            {
                return new(ErrorCode.AuctionService_GetItemListByPageFromTypeCode_InvalidTypeCode, itemList);
            }

            // 경매 물품 리스트 가져오기
            itemList = await _gameDb.GetAuctionListByTypeCode(page, typeCode, minPrice, maxPrice, sortBy, sortOrder);
            if (!ValidateAuctionList(itemList))
            {
                errorCode = ErrorCode.AuctionService_GetAuctionListByPage_EmptyItemList;
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
    /// 요청한 페이지에 따라 지정한 ItemName과 일치하는 경매 물품 리스트를 반환합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, List<AuctionModel>>> GetAuctionListByPageFromItemName(Int32 page, string itemName, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        var itemList = new List<AuctionModel>();
        var errorCode = ErrorCode.None;

        try
        {
            // 올바른 itemName인지 확인
            var item = _masterDb._itemAttributeList.Find(x => x.Name == itemName);
            if (item == null)
            {
                return new(ErrorCode.AuctionService_GetItemListByPageFromItemName_InvalidItemName, itemList);
            }

            // 경매 물품 리스트 가져오기
            itemList = await _gameDb.GetAuctionListByItemName(page, item.Code, minPrice, maxPrice, sortBy, sortOrder);
            if (!ValidateAuctionList(itemList))
            {
                errorCode = ErrorCode.AuctionService_GetAuctionListByPage_EmptyItemList;
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
    /// 경매 물품 조회 API <br/>
    /// 클라이언트가 요청한 경매 물품에 관한 통계 정보를 반환합니다.
    /// 통계 정보에는 최근 일주일동안의 평균 입찰가 및 평균 즉시 구매가,
    /// 현재 존재하는 매물의 최소 입찰가 및 최소 즉시 구매가가 포함됩니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, AuctionStatisticModel>> GetAuctionItemStatistic(Int64 ItemCode)
    {
        // 통계정보 가져오기
        var auctionStatistic = await _gameDb.GetAuctionItemStatistic(ItemCode);
        if (!ValidateStatistic(auctionStatistic))
        {
            return new(ErrorCode.AuctionService_GetAUctionItemStatistic_InvalidItemCode, auctionStatistic);
        }

        return new(ErrorCode.None, auctionStatistic);
    }

    /// <summary>
    /// 지정한 경매 물품에 대해 클라이언트가 요청한 가격으로 입찰에 참여합니다.
    /// 현재 입찰가보다 낮은 가격으로는 입찰할 수 없습니다.
    /// </summary>
    public async Task<ErrorCode> BidAuction(Int64 userId, Int64 auctionId, Int32 bidPrice)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 경매 정보 가져오기
        var auction = await _gameDb.GetAuctionInfo(auctionId);
        if (!ValidateAuction(auction))
        {
            return ErrorCode.AuctionService_BidAuction_InvalidAuctionId;
        }
            
        // 입찰가 유효성 검증
        var checkResult = CheckAuctionPrice(auction, bidPrice);
        if (!Successed(checkResult))
        {
            return checkResult;
        }

        // 유저의 재화를 입찰가만큼 차감
        var checkUpdateResult = await DecreaseUserMoneyForAuctionPrice(userId, bidPrice, rollbackQueries);
        if (!Successed(checkUpdateResult))
        {
            return checkUpdateResult;
        }

        // 입찰가 및 유저 갱신
        var updateResult = await RenewAuctionBidInfo(auction, userId, bidPrice, rollbackQueries);
        if (!Successed(updateResult))
        {
            await Rollback(updateResult, rollbackQueries);

            return updateResult;
        }

        // 이전 입찰자에게 입찰가 반환
        var returnResult = await ReturnBidPriceToBidder(auction.BidderID, auction.CurBidPrice, rollbackQueries);
        if (!Successed(returnResult))
        {
            await Rollback(returnResult, rollbackQueries);

            return returnResult;
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

        // 경매 정보 가져오기
        var auction = await _gameDb.GetAuctionInfo(auctionId);
        if (!ValidateAuction(auction))
        {
            return new(ErrorCode.AuctionService_BuyNowAuction_InvalidAuctionId, item);
        }

        // 아이템 데이터 세팅
        SetItem(ref item, auction, userId);

        // 유저의 재화를 즉시구매가만큼 차감
        var decreaseResult = await DecreaseUserMoneyForAuctionPrice(userId, auction.BuyNowPrice, rollbackQueries);
        if (!Successed(decreaseResult))
        {
            return new(decreaseResult, item);
        }

        // 경매 물품 구매 완료 처리하기
        var terminateResult = await TerminateAuctionSoldOut(auctionId, rollbackQueries);
        if (!Successed(terminateResult))
        {
            await Rollback(terminateResult, rollbackQueries);

            return new(terminateResult, item);
        }

        // 판매자에게 즉시구매가 지급
        var paymentResult = await PayMoneyToSeller(auction.SellerID, auction.BuyNowPrice, rollbackQueries);
        if (!Successed(paymentResult))
        {
            await Rollback(paymentResult, rollbackQueries);

            return new(paymentResult, item);
        }

        // 이전 입찰자에게 입찰가 반환
        var returnResult = await ReturnBidPriceToBidder(auction.BidderID, auction.CurBidPrice, rollbackQueries);
        if (!Successed(returnResult))
        {
            await Rollback(returnResult, rollbackQueries);

            return new(returnResult, item);
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

    void SetItem(ref UserItemModel item, AuctionModel auction, Int64 userId)
    {
        item.ItemID = auction.ItemID;
        item.UserID = userId;
        item.ItemCode = auction.ItemCode;
        item.ItemCount = auction.ItemCount;
    }


    /// <summary>
    /// 클라이언트가 요청한 아이템을 경매장에 등록합니다.
    /// 최소 입찰가, 즉시 구매가가 설정되어야합니다.
    /// </summary>
    public async Task<ErrorCode> RegisterAuction(Int64 userId, Int64 itemId, Int32 bidPrice, Int32 buyNowPrice)
    {
        var rollbackQueries = new List<SqlKata.Query>();

        // 유저가 지정한 아이템 정보를 가져온 후 삭제 처리
        (var item, var getDeleteResult) = await GetAndDeleteUserItem(userId, itemId, rollbackQueries);
        if (!Successed(getDeleteResult))
        {
            return getDeleteResult;
        }

        // 경매장 등록
        var registerResult = await RegisterUserItemToAuction(item, bidPrice, buyNowPrice);
        if (!Successed(registerResult))
        {
            await Rollback(registerResult, rollbackQueries);

            return registerResult;
        }

        return ErrorCode.None;
    }

    /// <summary>
    /// 경매장 등록을 취소하고 물품을 회수합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, UserItemModel>> CancleAuction(Int64 userId, Int64 auctionId)
    {
        var rollbackQueries = new List<SqlKata.Query>();
        var item = new UserItemModel();

        // 경매 정보 가져오기
        var auction = await _gameDb.GetAuctionInfo(auctionId);
        if (!ValidateAuction(auction))
        {
            return new(ErrorCode.AuctionService_CancleAuction_InvalidAuctionId, item);
        }

        // 아이템 데이터 세팅
        SetItem(ref item, auction, userId);

        // 경매장 등록 취소 처리
        var terminateResult = await TerminateAuctionCancle(userId, auction, rollbackQueries);
        if (!Successed(terminateResult))
        {
            return new(terminateResult, item);
        }

        // 이전 입찰자에게 입찰가 반환
        var returnResult = await ReturnBidPriceToBidder(auction.BidderID, auction.CurBidPrice, rollbackQueries);
        if (!Successed(returnResult))
        {
            await Rollback(returnResult, rollbackQueries);

            return new(returnResult, item);
        }

        // TODO : 우편으로 아이템 반환하는 형식으로 바꾸기
        // 유저에게 아이템 반환
        returnResult = await ReturnAuctionItemToUser(userId, item);
        if (!Successed(returnResult))
        {
            await Rollback(returnResult, rollbackQueries);

            return new(returnResult, item);
        }

        return new(ErrorCode.None, item);
    }










    /// <summary>
    /// 새로운 입찰가가 유효한지 검증합니다.
    /// 입찰하고자 하는 금액이 현재입찰가 초과 즉시구매가 미만인지 판별합니다.
    /// </summary>
    ErrorCode CheckAuctionPrice(AuctionModel auction, Int32 bidPrice)
    {
        if (auction.CurBidPrice == 0 || auction.BuyNowPrice == 0)
        {
            return ErrorCode.AuctionService_GetAuctionPriceInfo_InvalidAuctionId;
        }
        else if (bidPrice <= auction.CurBidPrice)
        {
            return ErrorCode.AuctionService_GetAuctionPriceInfo_LowBidPrice;
        }
        else if (bidPrice >= auction.BuyNowPrice)
        {
            return ErrorCode.AuctionService_GetAuctionPriceInfo_HighBidPrice;
        }
        
        return ErrorCode.None;
    }

    /// <summary>
    /// 유저가 설정한 입찰가 혹은 즉시구매가만큼의 재화를 보유했을시 차감합니다.
    /// </summary>
    async Task<ErrorCode> DecreaseUserMoneyForAuctionPrice(Int64 userId, Int32 bidPrice, List<SqlKata.Query> rollbackQueries)
    {
        try
        {
            var affectedRow = await _gameDb.DecreaseUserMoney(userId, bidPrice);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuctionService_DecreaseUserMoney_NotEnoughMoney;
            }

            // 성공 시, 롤백 쿼리 추가
            var query = _gameDb.GetQuery("farm_info").Where("UserId", userId).AsIncrement("Money", bidPrice);
            rollbackQueries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_DecreaseUserMoneyForAuctionPrice_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             UserID = userId,
                                             BidPrice = bidPrice
                                         },
                                         "Failed");
            return errorCode;
        }
    }

    /// <summary>
    /// 이전 입찰자에게 입찰 금액을 반환합니다.
    /// </summary>
    private async Task<ErrorCode> ReturnBidPriceToBidder(Int64 bidderId, Int32 curBidPrice, List<Query> rollbackQueries)
    {
        try
        {
            if (bidderId == 0)
            {
                return ErrorCode.None;
            }

            var affectedRow = await _gameDb.IncreaseUserMoney(bidderId, curBidPrice);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuctionService_IncreaseUserMoney_InvalidBidderId;
            }

            // 성공 시, 롤백 쿼리 추가
            var query = _gameDb.GetQuery("user_info").Where("UserId", bidderId).AsDecrement("Money", curBidPrice);
            rollbackQueries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_ReturnBidPriceToBidder_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             BidderID = bidderId,
                                             BidPrice = curBidPrice,
                                         },
                                         "Failed");
            return errorCode;
        }
    }

    /// <summary>
    /// 경매 물품의 현재 입찰 정보를 갱신합니다.
    /// </summary>
    async Task<ErrorCode> RenewAuctionBidInfo(AuctionModel auction, Int64 userId, Int32 bidPrice, List<SqlKata.Query> rollbackQueries)
    {
        try
        {
            var affectedRow = await _gameDb.UpdateAuctionBidInfo(auction.AuctionID, userId, auction.CurBidPrice, bidPrice);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuctionService_UpdateAuctionItemBidInfo_Fail;
            }

            // 성공 시, 롤백 쿼리 추가
            var query = _gameDb.GetQuery("auction_info")
                               .Where("AuctionId", auction.AuctionID)
                               .AsUpdate(new { CurBidPrice = auction.CurBidPrice,
                                               BidderID = auction.BidderID});
            rollbackQueries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_RenewAuctionBid_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             AuctionID = auction.AuctionID,
                                             UserID = userId,
                                             BidPrice = bidPrice
                                         },
                                         "Failed");
            return errorCode;
        }

    }

    /// <summary>
    /// 경매 물품을 구매 완료 처리합니다.
    /// </summary>
    async Task<ErrorCode> TerminateAuctionSoldOut(Int64 auctionId, List<SqlKata.Query> rollbackQueries)
    {
        try
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
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_TerminateAuctionSoldOut_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             AuctionID = auctionId
                                         },
                                         "Failed");
            return errorCode;
        }
    }

    /// <summary>
    /// 판매자에게 대금을 지급합니다.
    /// </summary>
    private async Task<ErrorCode> PayMoneyToSeller(Int64 sellerId, Int32 buyNowPrice, List<Query> rollbackQueries)
    {
        try
        {
            var affectedRow = await _gameDb.IncreaseUserMoney(sellerId, buyNowPrice);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuctionService_IncreaseUserMoney_Fail;
            }

            // 성공 시, 롤백 쿼리 추가
            var query = _gameDb.GetQuery("farm_info").Where("UserId", sellerId).AsDecrement("Money", buyNowPrice);
            rollbackQueries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_PayMoneyToSeller_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             SellerID = sellerId,
                                             BuyNowPrice = buyNowPrice
                                         },
                                         "Failed");
            return errorCode;
        }
    }


    /// <summary>
    /// 경매 물품을 유저에게 지급합니다.
    /// </summary>
    async Task<ErrorCode> ProvideItemToUser(Int64 userId, UserItemModel item)
    {
        try
        {
            var affectedRow = await _gameDb.InsertAuctionItemToUser(userId, item);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuctionService_InsertAuctionItemToUser_Fail;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_ProvideItemToUser_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             userID = userId,
                                             ItemID = item.ItemID,
                                         },
                                         "Failed");
            return errorCode;
        }
    }

    /// <summary>
    /// 유저가 경매 물품으로 등록할 아이템 정보를 가져오고 user_item 테이블에서 제거합니다.
    /// </summary>
    async Task<Tuple<UserItemModel, ErrorCode>> GetAndDeleteUserItem(Int64 userId, Int64 itemId, List<Query> rollbackQueries)
    {
        try
        {
            var item = await _gameDb.GetUserItem(userId, itemId);
            if (!ValidateItem(item))
            {
                return new(item, ErrorCode.AuctionService_GetUserItem_InvalidItemId);
            }

            var affectedRow = await _gameDb.DeleteUserItem(userId, itemId);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return new(item, ErrorCode.AuctionService_DeleteUserItem_Fail);
            }

            // 성공 시, 롤백 쿼리 추가
            var query = _gameDb.GetQuery("user_item").AsInsert(new { UserId = userId, ItemId = itemId, ItemCode = item.ItemCode, ItemCount = item.ItemCount });
            rollbackQueries.Add(query);

            return new(item, ErrorCode.None);
        }
        catch(Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_GetAndDeleteUserItem_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             userID = userId,
                                             ItemID = itemId,
                                         },
                                         "Failed");
            return new(null, errorCode);
        }
    }

    /// <summary>
    /// 유저 아이템을 경매장에 등록합니다.
    /// </summary>
    async Task<ErrorCode> RegisterUserItemToAuction(UserItemModel item, Int32 bidPrice, Int32 buyNowPrice)
    {
        try
        {
            var iteminfo = _masterDb._itemAttributeList.Find(x => x.Code == item.ItemCode);
            if (iteminfo == null)
            {
                return ErrorCode.AuctionService_RegisterUserItemToAuction_InvalidItemCode;
            }

            var affectedRow = await _gameDb.InsertAuction(item, iteminfo.TypeCode, iteminfo.Name, bidPrice, buyNowPrice);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuctionService_RegisterUserItemToAuction_Fail;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_RegisterUserItemToAuction_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             ItemID = item.ItemID,
                                             ItemCode = item.ItemCode,
                                             itemCount = item.ItemCount
                                         },
                                         "Failed");
            return errorCode;
        }
    }

    /// <summary>
    /// 경매 물품을 등록자에게 반환합니다.
    /// </summary>
    async Task<ErrorCode> ReturnAuctionItemToUser(Int64 userId, UserItemModel item)
    {
        try
        {
            var affectedRow = await _gameDb.InsertUserItem(userId, item);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuctionService_InsertUserItem_Fail;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_ReturnAuctionItemToUser_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             UserID = userId
                                         },
                                         "Failed");
            return errorCode;
        }
    }

    /// <summary>
    /// 경매 물품을 등록 취소 처리합니다.
    /// </summary>
    async Task<ErrorCode> TerminateAuctionCancle(Int64 userId, AuctionModel auction, List<Query> rollbackQueries)
    {
        try
        {
            var affectedRow = await _gameDb.DeleteAuctionInfo(userId, auction.AuctionID);
            if (!ValidateAffectedRow(affectedRow, 1))
            {
                return ErrorCode.AuctionService_UpdateAuctionItemPurchased_Fail;
            }

            // 성공 시, 롤백 쿼리 추가
            var query = _gameDb.GetQuery("auction_info")
                               .AsInsert(new { AuctionId = auction.AuctionID,
                                               SellerId = auction.SellerID,
                                               ItemName = auction.ItemName,
                                               ItemCode = auction.ItemCode,
                                               ItemCount = auction.ItemCount,
                                               TypeCode = auction.TypeCode,
                                               BidderId = auction.BidderID,
                                               CurBidPrice = auction.CurBidPrice,
                                               BuyNowPrice = auction.BuyNowPrice,
                                               ExpiredAt = auction.ExpiredAt,
                                               IsPurchased = false });
            rollbackQueries.Add(query);

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.AuctionService_TerminateAuctionSoldOut_Exception;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new
                                         {
                                             AuctionID = auction.AuctionID
                                         },
                                         "Failed");
            return errorCode;
        }
    }

    /// <summary>
    /// 유효한 경매 물품 리스트인지 체크합니다.
    /// </summary>
    bool ValidateAuctionList(List<AuctionModel>? auctionList)
    {
        return auctionList != null && auctionList.Count > 0;
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
    /// 유효한 경매 물품 통계인지 체크합니다.
    /// </summary>
    bool ValidateStatistic(AuctionStatisticModel? auctionStatistic)
    {
        return auctionStatistic != null;
    }

    /// <summary>
    /// 유효한 경매 정보인지 체크합니다.
    /// </summary>
    bool ValidateAuction(AuctionModel? auction)
    {
        return auction != null && auction.AuctionID > 0;
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