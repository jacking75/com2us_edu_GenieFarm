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

        return result == 1;
    }

    public async Task<AttendanceModel?> GetAttendanceData(Int64 userId)
    {
        var query = _queryFactory.Query("user_attendance").Where("UserId", userId);
        var result = (await query.GetAsync<AttendanceModel>()).FirstOrDefault();

        return result;
    }
}