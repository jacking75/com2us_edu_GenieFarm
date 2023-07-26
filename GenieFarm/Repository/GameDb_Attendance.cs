using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Transactions;
using ZLogger;
using Org.BouncyCastle.Bcpg;

public partial class GameDb : IGameDb
{
    public async Task<AttendanceModel?> GetDefaultAttendDataByUserId(Int64 userId)
    {
        var attendData = (await _queryFactory.Query("user_attendance").Where("userId", userId)
                                             .GetAsync<AttendanceModel>()).FirstOrDefault();

        return attendData;
    }

    public async Task<Int32> UpdateAttendanceData(Int64 userId, Int32 attendanceCount)
    {
        return await _queryFactory.Query("user_attendance")
                                  .Where("UserId", userId)
                                  .UpdateAsync(new { AttendanceCount = attendanceCount,
                                                     LastAttendance = DateTime.Now });
    }

    public async Task<Int32> InsertAttendanceRewardMail(Int64 userId, MailModel mail)
    {
        return await _queryFactory.Query("mail_info")
                                  .InsertAsync(mail);
    }

    public async Task<DateTime?> GetPassEndDateByUserId(Int64 userId)
    {
        return (await _queryFactory.Query("user_basicinfo")
                                   .Select("PassEndDate")
                                   .Where("UserId", userId)
                                   .GetAsync<DateTime?>())
                                   .FirstOrDefault();
    }
}