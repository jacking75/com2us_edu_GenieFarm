public interface IGameDb
{
    // GameDb_User.cs
    public Task<UserData> GetDefaultDataByAuthId(String authId);
    public Task<ErrorCode> CreateDefaultData(String authId, String nickname);
    public Task<Boolean> TryChangeNickname(String authId, String nickname);
    public Task<Boolean> CheckNicknameExists(String nickname);
    public Task<Boolean> CheckAuthIdExists(String authId);
    public Task<Int32> UpdateLastLoginAt(Int64 userId);

    // GameDb_Attendance.cs
    public Task<AttendanceData> GetAttendanceData(Int64 userId);
    public Task<Boolean> Attend(Int64 userId);

    // GameDb_Mail.cs
    public Task<List<MailData>> OpenMail(Int64 userId, Int32 page);
    public Task<MailData> GetMail(Int64 userId, Int64 mailId);
    public Task<Boolean> DeleteMail(Int64 userId, Int64 mailId);
    public Task<Boolean> SendMail(MailSendRequest request);
}