using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Transactions;
using ZLogger;
using Org.BouncyCastle.Bcpg;

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

    public async Task<MailModel?> GetMailByMailId(Int64 userId, Int64 mailId)
    {
        return (await _queryFactory.Query("mail_info")
                                   .Where("MailId", mailId)
                                   .Where("ReceiverId", userId)
                                   .Where("IsDeleted", false)
                                   .Where("ExpiredAt", ">", DateTime.Now)
                                   .GetAsync<MailModel>())
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

    public async Task<MailModel?> GetMailByMailIdIfHasReward(Int64 userId, Int64 mailId)
    {
        return (await _queryFactory.Query("mail_info")
                                   .Where("MailId", mailId)
                                   .Where(q => q.Where("ItemCode", ">", 0).OrWhere("Money", ">", 0))
                                   .GetAsync<MailModel>())
                                   .FirstOrDefault();
    }
}