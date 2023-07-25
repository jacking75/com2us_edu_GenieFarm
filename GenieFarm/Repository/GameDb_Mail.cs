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
}