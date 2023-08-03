public interface IAuctionService
{
    /// <summary>
    /// 요청한 페이지에 따라 지정한 TypeCode와 일치하는 아이템 리스트를 반환합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, List<AuctionModel>>> GetItemListByPageFromTypeCode(Int32 page, Int32 typeCode, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder);

    /// <summary>
    /// 요청한 페이지에 따라 지정한 ItemName과 일치하는 아이템 리스트를 반환합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, List<AuctionModel>>> GetItemListByPageFromItemName(Int32 page, string itemName, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder);

    /// <summary>
    /// 지정한 경매 물품에 대해 클라이언트가 요청한 가격으로 입찰에 참여합니다.
    /// 현재 입찰가보다 낮은 가격으로는 입찰할 수 없습니다.
    /// </summary>
    public Task<ErrorCode> BidAuction(Int64 userID, Int64 auctionID, Int32 bidPrice);

    /// <summary>
    /// 지정한 경매 물품을 즉시 구매합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, UserItemModel>> BuyNowAuction(Int64 userId, Int64 auctionId);

    /// <summary>
    /// 클라이언트가 요청한 아이템을 경매장에 등록합니다.
    /// 최소 입찰가, 즉시 구매가가 설정되어야합니다.
    /// </summary>
    public Task<ErrorCode> RegisterAuction(int itemId, int bidPrice, int buyNowPrice);
}
