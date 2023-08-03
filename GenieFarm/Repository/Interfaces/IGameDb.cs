using SqlKata;

public interface IGameDb
{
    public Query GetQuery(string tableName);
    public Task Rollback(ErrorCode errorCode, List<Query> queries);

    // GameDb_AuthCheck.cs
    public Task<Int64> GetUserIdByPlayerId(string playerId);
    public Task<UserBasicInfoModel?> GetDefaultUserDataByPlayerId(string playerId);
    public Task<UserBasicInfoModel?> GetDefaultUserDataByUserId(Int64 userId);
    public Task<FarmInfoModel?> GetDefaultFarmDataByUserId(Int64 userId);

    public Task<Int64> InsertGetIdDefaultUserData(string playerId, string nickname);
    public Task<Int32> InsertDefaultAttendanceData(Int64 userId);
    public Task<Int32> InsertDefaultFarmData(Int64 userId);
    public Task<Int32> InsertDefaultItems(Int64 userId);
    public Task<Int32> UpdateLastLoginAt(Int64 userId);


    // GameDb_Attendance.cs
    public Task<AttendanceModel?> GetDefaultAttendDataByUserId(Int64 userId);
    public Task<DateTime?> GetPassEndDateByUserId(Int64 userId);
    public Task<Int32> UpdateAttendanceData(Int64 userId, Int32 newAttendanceCount);
    public Task<Int32> InsertAttendanceRewardMail(Int64 userId, MailModel mail);


    // GameDb_Mail.cs
    public Task<List<MailModel>> GetMailListByPage(Int64 userId, Int32 page);
    public Task<MailModel?> GetMailByMailId(Int64 userId, Int64 mailId);
    public Task<Int32> UpdateMailIsRead(Int64 userId, Int64 mailId);
    public Task<MailModel?> GetMailByMailIdIfHasReward(Int64 userId, Int64 mailId);
    public Task<Int64> InsertGetIdRewardItem(Int64 userId, Int64 itemCode, Int16 itemCount);
    public Task<Int32> IncreaseUserMoney(Int64 userId, Int32 money);
    public Task<Int32> SetMailReceived(Int64 userId, Int64 mailId);


    // GameDb_Auction.cs
    public Task<Tuple<ErrorCode, List<AuctionModel>>> GetAuctionListByTypeCode(Int32 page, Int32 typeCode, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder);
    public Task<Tuple<ErrorCode, List<AuctionModel>>> GetAuctionListByItemName(Int32 page, string itemName, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder);
    public Task<Int32> DecrementUserMoney(Int64 userId, Int32 bidPrice);
    public Task<Tuple<Int64, Int64>> GetAuctionPriceInfo(Int64 auctionId);
    public Task<Int32> GetAuctionBuyNowPrice(Int64 auctionId);
    public Task<Int32> UpdateAuctionBidInfo(Int64 auctionId, Int64 userId, Int32 bidPrice);
    public Task<UserItemModel> GetAuctionItem(Int64 auctionId);
    public Task<Int32> UpdateAuctionPurchased(Int64 auctionId);
    public Task<Int32> InsertAuctionItemToUser(Int64 userId, UserItemModel item);
    public Task<int> InsertUserItemToAuction(UserItemModel item, Int16 typeCode, Int32 bidPrice, Int32 buyNowPrice);
}
    