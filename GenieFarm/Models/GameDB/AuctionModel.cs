public class AuctionModel
{
    public Int64 AuctionID { get; set; }

    public Int64 SellerID { get; set; }

    public string ItemName { get; set; }

    public Int64 ItemID { get; set; }

    public Int64 ItemCode { get; set; }

    public Int16 ItemCount { get; set; }

    public Int16 TypeCode { get; set; }

    public Int64 BidderID { get; set; }

    public Int32 CurBidPrice { get; set; }

    public Int32 BuyNowPrice { get; set; }

    public DateTime ExpiredAt { get; set; }
}
