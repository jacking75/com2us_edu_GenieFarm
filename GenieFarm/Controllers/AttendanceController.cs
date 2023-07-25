using Microsoft.AspNetCore.Mvc;
using ZLogger;

[ApiController]
[Route("api/attend")]
public class AttendanceController : ControllerBase
{
    ILogger<AttendanceController> _logger;
    IAttendanceService _attendanceService;

    public AttendanceController(ILogger<AttendanceController> logger, IAttendanceService attendanceService)
    {
        _logger = logger;
        _attendanceService = attendanceService;
    }

    /// <summary>
    /// 출석 체크 API </br>
    /// 출석 가능한지 체크하고, 출석 체크 및 보상 지급까지 수행합니다.
    /// </summary>
    [HttpPost]
    public async Task<ResAttendDTO> Attend(ReqAttendDTO request)
    {
        // 출석 데이터 로드 및 출석 가능 여부 확인
        (var errorCode, var attendData) = await _attendanceService.GetAttendanceData(request.UserID);
        if (!Successed(errorCode))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new { UserID = request.UserID }, "Failed");

            return new ResAttendDTO() { Result = ErrorCode.Attend_Fail_NotAttendable };
        }

        // 월간 구독 이용권 여부 조회
        var usingPass = await _attendanceService.CheckUsingPass(request.UserID);

        // 출석 체크 및 보상 지급
        var attendResult = await _attendanceService.Attend(request.UserID, attendData!, usingPass);
        if (!Successed(attendResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(attendResult),
                                         new { UserID = request.UserID }, "Failed");

            return new ResAttendDTO() { Result = ErrorCode.Attend_Fail_AttendException };
        }

        LogInfoOnSuccess("Attend", new { UserID = request.UserID });
        return new ResAttendDTO() { Result = ErrorCode.None };
    }

    /// <summary>
    /// 성공한 API 요청에 대해 통계용 로그를 남깁니다.
    /// </summary>
    void LogInfoOnSuccess<TPayload>(string method, TPayload payload)
    {
        _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method), payload, "Statistic");
    }

    /// <summary>
    /// 에러가 없는지 체크합니다.
    /// </summary>
    bool Successed(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.None;
    }
}