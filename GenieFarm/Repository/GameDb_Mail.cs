using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Transactions;
using ZLogger;

public partial class GameDb : IGameDb
{
    public async Task<List<MailData>> OpenMail(Int64 userId, Int32 page)
    {
        List<MailData> mailList = null;

        // 페이지에 해당하는 메일 불러오기
        var query = _queryFactory.Query("mail_info").Where("UserId", userId).Where("IsDeleted", false).Where("ExpiredAt", ">", DateTime.Now).OrderByDesc("MailId").Offset((page - 1) * 20).Limit(20);
        var result = await query.GetAsync<MailData>();
        _logger.LogInformation("[GameDb.OpenMail] Mail Count: {0}", result.Count());
        mailList = result.ToList();

        return mailList;
    }

    public async Task<MailData> GetMail(Int64 userId, Int64 mailId)
    {
        var query = _queryFactory.Query("mail_info").Where("UserId", userId).Where("MailId", mailId).Where("ExpiredAt", ">", DateTime.Now);
        var result = await query.GetAsync<MailData>();

        // 가져온 메일에 대해 읽음 처리
        var affectedRow = _queryFactory.Query("mail_info").Where("MailId", mailId).Update(new { IsRead = true });
        return result.FirstOrDefault<MailData>();
    }

    public async Task<Boolean> DeleteMail(Int64 userId, Int64 mailId)
    {
        var affectedRow = await _queryFactory.Query("mail_info").Where("MailId", mailId).Where("UserId", userId).Where("IsDeleted", false).UpdateAsync(new { IsDeleted = true });

        return affectedRow == 1;
    }

    public async Task<Boolean> SendMail(MailSendRequest request)
    {
        using (var transaction = _dbConn.BeginTransaction())
        {
            if (request.HasItem)
            {
                var query = await _queryFactory.Query("mail_info").InsertAsync(new
                {
                    SenderId = request.UserID,
                    UserId = request.ReceiverID,
                    Title = request.Title,
                    Content = request.Content,
                    HasItem = request.HasItem,
                    ExpiredAt = DateTime.Now.AddDays(30),
                });

                if (query == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // 메일ID 가져오기
                var mailId = await _queryFactory.Query("mail_info").OrderByDesc("MailId").Limit(1).GetAsync<Int64>();

                // 아이템 전송
                // TODO : 아이템을 실제로 가지고 있는지 유효성 검사해야 함!

                var itemQuery = await _queryFactory.Query("mail_item").InsertAsync(new
                {
                    MailId = mailId,
                    ItemId = request.ItemID,
                    ItemCode = request.ItemCode,
                    ItemCount = request.ItemCount,
                });

                if (itemQuery == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();
                return true;
            }
            else
            {
                var query = await _queryFactory.Query("mail_info").InsertAsync(new
                {
                    SenderId = request.UserID,
                    UserId = request.ReceiverID,
                    Title = request.Title,
                    Content = request.Content,
                    HasItem = request.HasItem,
                    ExpiredAt = DateTime.Now.AddDays(7),
                });

                if (query == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();
                return true;
            }
        }
    }
}