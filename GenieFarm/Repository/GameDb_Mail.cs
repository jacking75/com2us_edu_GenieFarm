using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Transactions;
using ZLogger;

public partial class GameDb : IGameDb
{
    public async Task<List<MailModel>> OpenMail(Int64 userId, Int32 page)
    {
        // 페이지에 해당하는 메일 불러오기
        var query = _queryFactory.Query("mail_info").Where("UserId", userId).Where("IsDeleted", false).Where("ExpiredAt", ">", DateTime.Now).Offset((page - 1) * 20).Limit(20);
        var result = await query.GetAsync<MailModel>();
        _logger.LogInformation("[GameDb.OpenMail] Mail Count: {0}", result.Count());
        var mailList = result.ToList();

        return mailList;
    }


    public async Task<MailModel> GetMail(Int64 userId, Int64 mailId)
    {
        var query = _queryFactory.Query("mail_info").Where("UserId", userId).Where("MailId", mailId).Where("ExpiredAt", ">", DateTime.Now);
        var result = await query.GetAsync<MailModel>();

        // 가져온 메일에 대해 읽음 처리
        var affectedRow = _queryFactory.Query("mail_info").Where("MailId", mailId).Update(new { IsRead = true });
        return result.FirstOrDefault();
    }


    public async Task<Boolean> DeleteMail(Int64 userId, Int64 mailId)
    {
        var affectedRow = await _queryFactory.Query("mail_info").Where("MailId", mailId).Where("UserId", userId).Where("IsDeleted", false).UpdateAsync(new { IsDeleted = true });

        return affectedRow == 1;
    }


    public async Task<ErrorCode> SendMail(ReqMailSendDTO request)
    {
        // ReceiverID가 실제 존재하는 유저인지 확인
        var query = await _queryFactory.Query("user_basicinfo").Where("UserId", request.ReceiverID).GetAsync<AccountModel>();
        if (query.FirstOrDefault() == null)
        {
            _logger.LogInformation("receiverCheck : {0}", query.FirstOrDefault());
            return ErrorCode.MailReceiverNotExists;
        }

        // ItemID가 실제 해당 유저 소유인지 확인, 소유권 변경
        if (request.ItemID > 0)
        {
            var itemQuery = await _queryFactory.Query("farm_item").Where("OwnerId", request.UserID).Where("ItemId", request.ItemID).UpdateAsync(new { OwnerId = 0 });
            if (itemQuery == 0)
            {
                return ErrorCode.MailSenderNotOwnItem;
            }
        }

        // 메일 전송
        var sendQuery = await _queryFactory.Query("mail_info").InsertAsync(new
        {
            SenderId = request.UserID,
            ReceiverId = request.ReceiverID,
            Title = request.Title,
            Content = request.Content,
            ExpiredAt = request.ItemID > 0 ? DateTime.Now.AddDays(30) : DateTime.Now.AddDays(7),
            ItemId = request.ItemID,
        });

        // 전송 실패
        if (sendQuery == 0)
        {
            // 아이템 소유권 롤백
            if (request.ItemID > 0)
            {
                var itemQuery = await _queryFactory.Query("farm_item").Where("OwnerId", 0).Where("ItemId", request.ItemID).UpdateAsync(new { OwnerId = request.UserID });
                if (itemQuery == 0)
                {
                    return ErrorCode.MailItemRollbackFailed;
                }
            }

            return ErrorCode.MailSendException;
        }

        return ErrorCode.None;
    }
}