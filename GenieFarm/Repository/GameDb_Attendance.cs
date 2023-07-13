using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using System.Transactions;
using ZLogger;

public partial class GameDb : IGameDb
{
    public async Task<Boolean> Attend(Int64 userId)
    {
        var result = await _queryFactory.StatementAsync("update user_attendance set LastAttendance = now(), AttendanceCount = AttendanceCount + 1 where UserId = @UserId", new { UserId = userId });
        // var result = _queryFactory.Query("user_attendance").Where("UserId", userId).Update(new { LastAttendance = DateTime.Now });

        return result > 0;
    }

    public async Task<AttendanceData> GetAttendanceData(Int64 userId)
    {
        var query = _queryFactory.Query("user_attendance").Where("UserId", userId);
        var result = await query.GetAsync<AttendanceData>();

        return result.FirstOrDefault();
    }
}