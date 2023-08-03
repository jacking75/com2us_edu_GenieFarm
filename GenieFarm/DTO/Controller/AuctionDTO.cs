// AuctionDTO.cs : AuctionController에서 사용하는 DTO 정의

// LoadAuctionPage : 경매장 페이지 별 조회

public class ReqLoadAuctionPageDTO : InGameDTO
{
    public Int32 Page { get; set; }

    public Int32 TypeCode { get; set; }

    public Int32 MinPrice { get; set; }

    public Int32 MaxPrice { get; set;}

    public string? ItemName { get; set; }

    public string? SortBy { get; set; }

    public string? SortOrder { get; set; }
}

public class ResLoadAuctionPageDTO : ErrorCodeDTO
{
    public List<AuctionModel>? ItemList { get; set; }
}

// BidAuction : 경매 물품 입찰 참여

public class ReqBidAuctionDTO : InGameDTO
{
    public Int64 AuctionID { get; set; }

    public Int32 BidPrice { get; set; }
}

public class ResBidAuctionDTO : ErrorCodeDTO
{

}

// BuyAuction : 경매 물품 즉시 구매

public class ReqBuyNowAuctionDTO : InGameDTO
{
    public Int64 AuctionID { get; set; }
}

public class ResBuyNowAuctionDTO : ErrorCodeDTO
{
    public UserItemModel UserItem { get; set; }
}

// RegisterAuction : 경매장 물품 등록

public class ReqRegisterAuctionDTO : InGameDTO
{
    public Int32 ItemId { get; set; }
    
    public Int32 BidPrice { get; set; }

    public Int32 BuyNowPrice { get; set; }
}

public class ResRegisterAuctionDTO : ErrorCodeDTO
{
    
}