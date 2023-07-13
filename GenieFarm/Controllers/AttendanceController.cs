using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    ILogger<AttendanceController> _logger;
    IGameDb _gameDb;
    public AttendanceController(ILogger<AttendanceController> logger, IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
    }

    [HttpPost]
    public async Task<AttendanceResponse> Attend([FromBody] AttendanceRequest request)
    {
        // 출석체크가 가능한지 확인한다.
        var lastAttendanceData = await _gameDb.GetAttendanceData(request.UserID);

        TimeSpan diff = TimeSpan.FromDays(1);
        if (lastAttendanceData.LastAttendance != null)
        {
            DateTime lastAttendanceDateShort = DateTime.Parse(lastAttendanceData.LastAttendance.ToShortDateString());
            DateTime currentDateShort = DateTime.Parse(DateTime.Now.ToShortDateString());
            diff = currentDateShort - lastAttendanceDateShort;
        }

        if (diff > TimeSpan.FromDays(0))
        {
            // 출석 가능
            var affectedRow = await _gameDb.Attend(request.UserID);
            _logger.LogInformation("[AttendanceController.Attend] AffectedRow : {}", affectedRow);

            // TODO : 아이템 보상 지급
            return new AttendanceResponse() { Result = ErrorCode.None };
        }
        else
        {
            // 출석 불가능
            return new AttendanceResponse() { Result = ErrorCode.AlreadyAttended };
        }
    }
}