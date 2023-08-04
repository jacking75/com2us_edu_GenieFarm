using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Transactions;
using ZLogger;
using Org.BouncyCastle.Bcpg;
using System;
using SqlKata;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

public partial class GameDb : IGameDb
{
    public async Task<List<AuctionModel>> GetAuctionListByTypeCode(Int32 page, Int32 typeCode, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {

        var query = _queryFactory.Query("auction_info")
                                 .Where("TypeCode", typeCode);

        SetGetItemListQuery(ref query, page, minPrice, maxPrice, sortBy, sortOrder);

        return (await query.GetAsync<AuctionModel>()).ToList();
    }

    public async Task<List<AuctionModel>> GetAuctionListByItemName(Int32 page, Int64 itemCode, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        var query = _queryFactory.Query("auction_info")
                                 .Where("ItemCode", itemCode);

        SetGetItemListQuery(ref query, page, minPrice, maxPrice, sortBy, sortOrder);

        return (await query.GetAsync<AuctionModel>()).ToList();
    }

    void SetGetItemListQuery(ref Query query, Int32 page, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        var itemPerPage = _masterDb._definedValueDictionary!["Auction_Item_Count_Per_Page"];

        query.Where("IsPurchased", false)
             .Where("ExpiredAt", ">", DateTime.Now);

        minPrice = minPrice == 0 ? 0 : minPrice;
        maxPrice = maxPrice == 0 ? Int32.MaxValue : maxPrice;

        switch (sortBy)
        {
            case "CurBidPrice":
                query = sortOrder == "asc" ? query.OrderBy("CurBidPrice") : query.OrderByDesc("CurBidPrice");
                query.WhereBetween("CurBidPrice", minPrice, maxPrice);
                break;
            case "BuyNowPrice":
                query = sortOrder == "asc" ? query.OrderBy("BuyNowPrice") : query.OrderByDesc("BuyNowPrice");
                query.WhereBetween("BuyNowPrice", minPrice, maxPrice);
                break;
            case "expiredAt":
                query = sortOrder == "asc" ? query.OrderBy("ExpiredAt") : query.OrderByDesc("ExpiredAt");
                query.WhereBetween("CurBidPrice", minPrice, maxPrice);
                break;
            default:
                query = query.OrderByDesc("expiredAt");
                query.WhereBetween("CurBidPrice", minPrice, maxPrice);
                break;
        }

        query.Offset((page - 1) * itemPerPage)
             .Limit(itemPerPage);
    }

    public async Task<AuctionStatisticModel> GetAuctionItemStatistic(Int64 ItemCode)
    {
        var auctionStatistic = new AuctionStatisticModel();

        auctionStatistic.AvgBidPrice = await _queryFactory.Query("auction_info")
                                                          .Where("ItemCode", ItemCode)
                                                          .Where("ExpiredAt", ">", DateTime.Now.AddDays(-7))
                                                          .Where("IsPurchased", true)
                                                          .AverageAsync<Int32>("CurBidPrice");

        auctionStatistic.MinBidPrice = await _queryFactory.Query("auction_info")
                                                          .Where("ItemCode", ItemCode)
                                                          .Where("IsPurchased", false)
                                                          .MinAsync<Int32>("CurBidPrice");

        auctionStatistic.AvgBuyNowPrice = await _queryFactory.Query("auction_info")
                                                             .Where("ItemCode", ItemCode)
                                                             .Where("ExpiredAt", ">", DateTime.Now.AddDays(-7))
                                                             .Where("IsPurchased", true)
                                                             .AverageAsync<Int32>("BuyNowPrice");

        auctionStatistic.MinBuyNowPrice = await _queryFactory.Query("auction_info")
                                                             .Where("ItemCode", ItemCode)
                                                             .Where("IsPurchased", false)
                                                             .MinAsync<Int32>("BuyNowPrice");

        return auctionStatistic;
    }

    public async Task<AuctionModel> GetAuctionInfo(Int64 auctionId)
    {
        return (await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .Where("IsPurchased", false)
                                   .FirstOrDefaultAsync<AuctionModel>());
    }

    public async Task<Int32> DecreaseUserMoney(Int64 userId, Int32 bidPrice)
    {
        return (await _queryFactory.Query("farm_info")
                                   .Where("UserId", userId)
                                   .Where("Money", ">=", bidPrice)
                                   .DecrementAsync("Money", bidPrice));
    }

    public async Task<Int32> UpdateAuctionBidInfo(Int64 auctionId, Int64 userId, Int32 beforeBidPrice, Int32 bidPrice)
    {
        return (await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .Where("CurBidPrice", beforeBidPrice)
                                   .UpdateAsync(new { BidderId = userId, CurBidPrice = bidPrice }));
    }

    public async Task<Int32> UpdateAuctionPurchased(Int64 auctionId)
    {
        return (await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .Where("IsPurchased", false)
                                   .UpdateAsync(new { IsPurchased = true }));
    }

    public async Task<Int32> InsertAuctionItemToUser(Int64 userId, UserItemModel item)
    {
        return (await _queryFactory.Query("user_item")
                                   .InsertAsync(new { UserId = userId, 
                                                      ItemId = item.ItemID,
                                                      ItemCode = item.ItemCode,
                                                      ItemCount = item.ItemCount }));
    }

    public async Task<UserItemModel> GetUserItem(Int64 userId, Int64 itemId)
    {
        return (await _queryFactory.Query("user_item")
                                   .Where("UserId", userId)
                                   .Where("ItemId", itemId)
                                   .FirstOrDefaultAsync<UserItemModel>());   
    }

    public async Task<Int32> DeleteUserItem(Int64 userId, Int64 itemId)
    {
        return (await _queryFactory.Query("user_Item")
                                   .Where("UserId", userId)
                                   .Where("ItemId", itemId)
                                   .DeleteAsync());
    }


    public async Task<Int32> InsertAuction(UserItemModel item, Int16 typeCode, string itemName, Int32 bidPrice, Int32 buyNowPrice)
    {
        return (await _queryFactory.Query("auction_info")
                                   .InsertAsync(new { SellerId = item.UserID,
                                                      ItemName = itemName,
                                                      ItemId = item.ItemID,
                                                      ItemCode = item.ItemCode,
                                                      ItemCount = item.ItemCount,
                                                      TypeCode = typeCode,
                                                      CurBidPrice = bidPrice,
                                                      BuyNowPrice = buyNowPrice }));
    }

    public async Task<Int32> DeleteAuctionInfo(Int64 userId, Int64 auctionId)
    {
        return (await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .Where("SellerId", userId)
                                   .Where("IsPurchased", false)
                                   .DeleteAsync());
    }

    public async Task<Int32> InsertUserItem(Int64 userId, UserItemModel item)
    {
        return (await _queryFactory.Query("user_item")
                                   .InsertAsync(new { UserId = userId,
                                                      ItemId = item.ItemID,
                                                      ItemCode = item.ItemCode,
                                                      ItemCount = item.ItemCount }));
    }
}