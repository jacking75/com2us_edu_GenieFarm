public interface IGameDb
{
    // GameDb_AuthCheck.cs
    public Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultDataByPlayerId(string playerId);
    public Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultDataByUserId(Int64 userId);
    public Task<ErrorCode> CreateDefaultData(string playerId, string nickname);
    public Task<Int64> GetUserIdByPlayerId(string playerId);
    public Task<bool> UpdateLastLoginAt(Int64 userId);

    // GameDb_Attendance.cs
    public Task<ErrorCode> Attend(Int64 userId, AttendanceModel attendData);
    public Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceDataByUserId(Int64 userId);

    //public Task<bool> Attend(Int64 userId);

    //// GameDb_Mail.cs
    //public Task<List<MailModel>> OpenMail(Int64 userId, Int32 page);
    //public Task<MailModel?> GetMail(Int64 userId, Int64 mailId);
    //public Task<bool> DeleteMail(Int64 userId, Int64 mailId);
    //public Task<ErrorCode> SendMail(ReqMailSendDTO request);
    //public Task<ErrorCode> ReceiveMail(Int64 userId, Int64 mailId);
}