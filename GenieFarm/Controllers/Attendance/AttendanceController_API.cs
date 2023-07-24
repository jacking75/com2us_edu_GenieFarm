using Microsoft.AspNetCore.Mvc;
using ZLogger;

[ApiController]
[Route("api/attend")]
public partial class AttendanceController : ControllerBase
{
    ILogger<AttendanceController> _logger;
    IGameDb _gameDb;
    IAttendanceService _attendanceService;

    public AttendanceController(ILogger<AttendanceController> logger, IGameDb gameDb, IAttendanceService attendanceService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _attendanceService = attendanceService;
    }

    [HttpPost]
    public async Task<ResAttendDTO> Attend(ReqAttendDTO request)
    {
        // 출석 데이터 로드
        (var errorCode, var attendData) = await _attendanceService.GetAttendanceData(request.UserID);
        if (!SuccessOrLogDebug(errorCode, new { UserID = request.UserID }))
        {
            return new ResAttendDTO() { Result = ErrorCode.Attend_Fail_GetAttendData };
        }

        // 출석 가능한지 확인
        if (!ValidateAttend(attendData!))
        {
            return new ResAttendDTO() { Result = ErrorCode.Attend_Fail_AlreadyAttended };
        }

        // 출석 체크 및 보상 지급
        var attendResult = await _attendanceService.Attend(request.UserID, attendData!);
        if (!SuccessOrLogDebug(attendResult, new { UserID = request.UserID }))
        {
            return new ResAttendDTO() { Result = ErrorCode.Attend_Fail_AttendException };
        }

        LogInfoOnSuccess("Attend", new { UserID = request.UserID });
        return new ResAttendDTO() { Result = ErrorCode.None };
    }
}