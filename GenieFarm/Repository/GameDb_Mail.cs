using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Transactions;
using ZLogger;

public partial class GameDb : IGameDb
{
    public async Task<List<MailModel>> GetMailListByPage(Int64 userId, Int32 page)
    {
        var mailPerPage = _masterDb._definedValueDictionary!["Mail_Count_Per_Page"];

        return (await _queryFactory.Query("mail_info")
                                  .Where("ReceiverId", userId)
                                  .Where("IsDeleted", false)
                                  .Where("ExpiredAt", ">", DateTime.Now)
                                  .Offset((page - 1) * mailPerPage)
                                  .Limit(mailPerPage)
                                  .GetAsync<MailModel>())
                                  .ToList();
    }

    public async Task<MailWithItemDTO?> GetMailByMailId(Int64 userId, Int64 mailId)
    {
        return (await _queryFactory.Query("mail_info")
                                   .Where("MailId", mailId)
                                   .Where("ReceiverId", userId)
                                   .Where("IsDeleted", false)
                                   .Where("ExpiredAt", ">", DateTime.Now)
                                   .GetAsync<MailWithItemDTO>())
                                   .FirstOrDefault();
    }

    public async Task<FarmItemModel?> GetItemCodeAndCountByItemId(Int64 itemId)
    {
        var ownerIdInMailbox = _masterDb._definedValueDictionary!["OwnerId_In_Mailbox"];

        return (await _queryFactory.Query("farm_item")
                                   .Where("ItemId", itemId)
                                   .Where("OwnerId", ownerIdInMailbox)
                                   .Select("ItemCode", "ItemCount")
                                   .GetAsync<FarmItemModel>())
                                   .FirstOrDefault();
    }

    public async Task<Int32> UpdateMailIsRead(Int64 userId, Int64 mailId)
    {
        return (await _queryFactory.Query("mail_info")
                                   .Where("MailId", mailId)
                                   .Where("ReceiverId", userId)
                                   .Where("IsDeleted", false)
                                   .Where("ExpiredAt", ">", DateTime.Now)
                                   .UpdateAsync(new { IsRead = true }));
    }
}