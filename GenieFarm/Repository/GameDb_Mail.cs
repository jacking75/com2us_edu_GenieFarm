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
        var query = _queryFactory.Query("mail_info").Where("ReceiverId", userId).Where("IsDeleted", false).Where("ExpiredAt", ">", DateTime.Now).Offset((page - 1) * 20).Limit(20);
        var result = await query.GetAsync<MailModel>();
        var mailList = result.ToList();

        return mailList;
    }


    public async Task<MailModel?> GetMail(Int64 userId, Int64 mailId)
    {
        var query = _queryFactory.Query("mail_info").Where("ReceiverId", userId).Where("MailId", mailId).Where("ExpiredAt", ">", DateTime.Now);
        var result = await query.GetAsync<MailModel>();

        // 가져온 메일에 대해 읽음 처리
        var affectedRow = await _queryFactory.Query("mail_info").Where("MailId", mailId).UpdateAsync(new { IsRead = true });

        return result.FirstOrDefault();
    }


    public async Task<bool> DeleteMail(Int64 userId, Int64 mailId)
    {
        var affectedRow = await _queryFactory.Query("mail_info").Where("MailId", mailId).Where("ReceiverId", userId).Where("IsDeleted", false).UpdateAsync(new { IsDeleted = true });

        return affectedRow == 1;
    }


    public async Task<ErrorCode> SendMail(ReqMailSendDTO request)
    {
        // ReceiverID가 실제 존재하는 유저인지 확인
        var query = await _queryFactory.Query("user_basicinfo").Where("UserId", request.ReceiverID).GetAsync<AccountModel>();
        if (query.FirstOrDefault() == null)
        {
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

            return ErrorCode.MailSendFailed;
        }

        return ErrorCode.None;
    }


    public async Task<ErrorCode> ReceiveMail(Int64 userId, Int64 mailId)
    {
        // 아이템 ID 가져오기
        var query = await _queryFactory.Query("mail_info").Select("ItemId").Where("ReceiverId", userId).Where("MailId", mailId).Where("ExpiredAt", ">", DateTime.Now).Where("IsReceived", false).Where("IsDeleted", false).GetAsync<Int64>();
        var itemId = query.FirstOrDefault();
        if (itemId == 0)
        {
            return ErrorCode.MailItemNotExists;
        }

        // 아이템 소유권 변경
        var itemQuery = await _queryFactory.Query("farm_item").Where("ItemId", itemId).Where("OwnerId", 0).UpdateAsync(new { OwnerId = userId });
        if (itemQuery == 0)
        {
            return ErrorCode.MailItemNotExists;
        }

        // 메일 아이템 수령완료 처리
        var receiveQuery = await _queryFactory.Query("mail_info").Where("ReceiverId", userId).Where("MailId", mailId).Where("ExpiredAt", ">", DateTime.Now).Where("IsReceived", false).Where("IsDeleted", false).UpdateAsync(new { IsReceived = true, ItemId = 0 });
        if (receiveQuery == 0)
        {
            // 아이템 소유권 롤백
            var itemRollbackQuery = await _queryFactory.Query("farm_item").Where("ItemId", itemId).Where("OwnerId", userId).UpdateAsync(new { OwnerId = 0 });

            return ErrorCode.MailItemReceiveFailed;
        }

        return ErrorCode.None;
    }
}