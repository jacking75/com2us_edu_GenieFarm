public interface IGameDb
{
    // GameDb_User.cs
    //public Task<AccountModel?> GetDefaultDataByPlayerId(string playerId);
    public Task<ErrorCode> CreateDefaultData(string playerId, string nickname);
    //public Task<bool> TryChangeNickname(string playerId, string nickname);
    //public Task<bool> CheckNicknameExists(string nickname);
    public Task<bool> CheckPlayerIdExists(string playerId);
    //public Task<Int32> UpdateLastLoginAt(Int64 userId);

    // GameDb_Attendance.cs
    public Task<AttendanceModel?> GetAttendanceData(Int64 userId);
    public Task<bool> Attend(Int64 userId);

    // GameDb_Mail.cs
    public Task<List<MailModel>> OpenMail(Int64 userId, Int32 page);
    public Task<MailModel?> GetMail(Int64 userId, Int64 mailId);
    public Task<bool> DeleteMail(Int64 userId, Int64 mailId);
    public Task<ErrorCode> SendMail(ReqMailSendDTO request);
    public Task<ErrorCode> ReceiveMail(Int64 userId, Int64 mailId);
}