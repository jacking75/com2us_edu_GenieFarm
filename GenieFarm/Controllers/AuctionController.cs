using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auction")]

public class AuctionController : ControllerBase
{
    readonly ILogger<AuctionController> _logger;
    readonly IAuctionService _auctionService;

    public AuctionController(ILogger<AuctionController> logger, IAuctionService auctionService)
    {
        _logger = logger;
        _auctionService = auctionService;
    }

    /// <summary>
    /// 경매장 페이지 별 조회 API <br/>
    /// 클라이언트가 요청한 페이지 및 아이템 카테고리에 해당하는 경매 물품 리스트를 반환합니다.
    /// </summary>
    [HttpPost("loadByPageFromTypeCode")]
    public async Task<ResLoadAuctionPageDTO> LoadAuctionListByPageFromTypeCode(ReqLoadAuctionPageDTO request)
    {
        (var errorCode, var itemList) = await _auctionService.GetAuctionListByPageFromTypeCode(request.Page, request.TypeCode, request.MinPrice, request.MaxPrice, request.SortBy, request.SortOrder);

        return new ResLoadAuctionPageDTO() { Result = errorCode, ItemList = itemList };

    }

    /// <summary>
    /// 경매장 페이지 별 조회 API <br/>
    /// 클라이언트가 요청한 페이지 및 아이템 이름에 해당하는 경매 물품 리스트를 반환합니다.
    /// </summary>
    [HttpPost("loadByPageFromItemName")]
    public async Task<ResLoadAuctionPageDTO> LoadAuctionListByPageFromItemName(ReqLoadAuctionPageDTO request)
    {
        (var errorCode, var itemList) = await _auctionService.GetAuctionListByPageFromItemName(request.Page, request.ItemName, request.MinPrice, request.MaxPrice, request.SortBy, request.SortOrder);

        return new ResLoadAuctionPageDTO() { Result = errorCode, ItemList = itemList };
    }

    /// <summary>
    /// 경매 물품 조회 API <br/>
    /// 클라이언트가 요청한 경매 물품에 관한 통계 정보를 반환합니다.
    /// </summary>
    [HttpPost("loadAuctionItemStatistic")]
    public async Task<ResLoadAuctionItemStatisticDTO> LoadAuctionItemStatistics(ReqLoadAuctionItemStatisticDTO request)
    {
        (var errorCode, var auctionStatistic) = await _auctionService.GetAuctionItemStatistic(request.ItemCode);

        return new ResLoadAuctionItemStatisticDTO() { Result = errorCode, AuctionStatistic = auctionStatistic };
    }


    /// <summary>
    /// 경매 물품 입찰 참여 API <br/>
    /// 지정한 경매 물품에 대해 클라이언트가 요청한 가격으로 입찰에 참여합니다.
    /// 현재 입찰가보다 낮은 가격으로는 입찰할 수 없습니다.
    /// </summary>
    [HttpPost("bidAuction")]
    public async Task<ResBidAuctionDTO> BidAuction(ReqBidAuctionDTO request)
    {
        var errorCode = await _auctionService.BidAuction(request.UserID, request.AuctionID, request.BidPrice);

        return new ResBidAuctionDTO() { Result = errorCode };
    }

    /// <summary>
    /// 경매 물품 즉시 구매 API <br/>
    /// 지정한 경매 물품을 즉시 구매합니다.
    /// </summary>
    [HttpPost("buyNowAuction")]
    public async Task<ResBuyNowAuctionDTO> BuyNowAuction(ReqBuyNowAuctionDTO request)
    {
        (var errorCode, var item) = await _auctionService.BuyNowAuction(request.UserID, request.AuctionID);

        return new ResBuyNowAuctionDTO() { Result = errorCode, UserItem = item };
    }

    /// <summary>
    /// 경매장 물품 등록 API <br/>
    /// 클라이언트가 요청한 아이템을 경매장에 등록합니다.
    /// 최소 입찰가, 즉시 구매가가 설정되어야합니다.
    /// </summary>
    [HttpPost("registerAuction")]
    public async Task<ResRegisterAuctionDTO> RegisterAuction(ReqRegisterAuctionDTO request)
    {
        var errorCode = await _auctionService.RegisterAuction(request.UserID, request.ItemId, request.BidPrice, request.BuyNowPrice);

        return new ResRegisterAuctionDTO() { Result = errorCode };
    }

    /// <summary>
    /// 경매장 물품 등록 취소 API <br/>
    /// 경매장에 등록되어있던 물품을 회수합니다.
    /// </summary>
    [HttpPost("cancleAuction")]
    public async Task<ResCancleAuctionDTO> CancleAuction(ReqCancleAuctionDTO request)
    {
        (var errorCode, var item) = await _auctionService.CancleAuction(request.UserID, request.AuctionID);

        return new ResCancleAuctionDTO() { Result = errorCode, UserItem = item };
    }
}
