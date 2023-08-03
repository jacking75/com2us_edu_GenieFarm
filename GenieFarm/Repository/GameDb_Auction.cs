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
    public async Task<Tuple<ErrorCode, List<AuctionModel>>> GetAuctionListByTypeCode(Int32 page, Int32 typeCode, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        // 올바른 TypeCode인지 확인
        var itemType = _masterDb._itemTypeList.Find(x => x.TypeCode == typeCode);
        if (itemType == null)
        {
            return new(ErrorCode.AuctionService_GetItemListByPageFromTypeCode_InvalidTypeCode, new List<AuctionModel>());
        }

        var query = _queryFactory.Query("auction_info")
                                 .Where("TypeCode", typeCode);

        SetGetItemListQuery(ref query, page, minPrice, maxPrice, sortBy, sortOrder);

        return new(ErrorCode.None, (await query.GetAsync<AuctionModel>()).ToList());
    }

    public async Task<Tuple<ErrorCode, List<AuctionModel>>> GetAuctionListByItemName(Int32 page, string itemName, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        // 올바른 itemName인지 확인
        var item = _masterDb._itemAttributeList.Find(x => x.Name == itemName);
        if (item == null)
        {
            return new(ErrorCode.AuctionService_GetItemListByPageFromItemName_InvalidItemName, new List<AuctionModel>());
        }

        var query = _queryFactory.Query("auction_info")
                                 .Where("ItemCode", item.Code);

        SetGetItemListQuery(ref query, page, minPrice, maxPrice, sortBy, sortOrder);

        return new(ErrorCode.None, (await query.GetAsync<AuctionModel>()).ToList());
    }

    void SetGetItemListQuery(ref Query query, Int32 page, Int32 minPrice, Int32 maxPrice, string? sortBy, string? sortOrder)
    {
        var itemPerPage = _masterDb._definedValueDictionary!["Auction_Item_Count_Per_Page"];

        query.Where("IsPurchased", false)
             .Where("ExpiredAt", ">", DateTime.Now);

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

    public async Task<Tuple<Int64, Int64>> GetAuctionPriceInfo(Int64 auctionId)
    { 
        var result = await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .Where("IsPurchsed", false)
                                   .Select("CurBidPrice", "BuyNowPrice")
                                   .FirstOrDefaultAsync();

        return new Tuple<Int64, Int64>(result.CurBidPrice, result.BuyNowPrice);
    }

    public async Task<Int32> GetAuctionBuyNowPrice(Int64 auctionId)
    {
        return (await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .Where("IsPurcahsed", false)
                                   .Select("BuyNowPrice")
                                   .FirstOrDefaultAsync<Int32>());
    }

    public async Task<Int32> DecrementUserMoney(Int64 userId, Int32 bidPrice)
    {
        return (await _queryFactory.Query("farm_info")
                                   .Where("UserId", userId)
                                   .Where("Money", ">=", bidPrice)
                                   .DecrementAsync("Money", bidPrice));
    }

    public async Task<Int32> UpdateAuctionBidInfo(Int64 auctionId, Int64 userId, Int32 bidPrice)
    {
        return (await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .UpdateAsync(new { BidderId = userId, CurBidPrice = bidPrice }));
    }

    public async Task<Int32> UpdateAuctionPurchased(Int64 auctionId)
    {
        return (await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .Where("IsPurchased", false)
                                   .UpdateAsync(new { IsPurchased = true }));
    }

    public async Task<UserItemModel> GetAuctionItem(Int64 auctionId)
    {
        return (await _queryFactory.Query("auction_info")
                                   .Where("AuctionId", auctionId)
                                   .Where("IsPurchased", false)
                                   .Select("ItemId", "ItemCode", "ItemCount")
                                   .FirstOrDefaultAsync<UserItemModel>());
    }

    public async Task<Int32> InsertAuctionItemToUser(Int64 userId, UserItemModel item)
    {
        return (await _queryFactory.Query("user_item")
                                   .InsertAsync(new { UserId = userId, 
                                                      ItemId = item.ItemID,
                                                      ItemCode = item.ItemCode,
                                                      ItemCount = item.ItemCount }));
    }

    public async Task<Int32> InsertUserItemToAuction(UserItemModel item, Int16 typeCode, Int32 bidPrice, Int32 buyNowPrice)
    {
        return (await _queryFactory.Query("auction_ifno")
                                   .InsertAsync(new { SellerId = item.UserID,
                                                      ItemId = item.ItemID,
                                                      ItemCode = item.ItemCode,
                                                      ItemCount = item.ItemCount,
                                                      TypeCode = typeCode,
                                                      BidPrice = bidPrice,
                                                      BuyNowPrice = buyNowPrice }));
    }
}