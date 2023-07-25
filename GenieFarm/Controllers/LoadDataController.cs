using Microsoft.AspNetCore.Mvc;
using ZLogger;

[ApiController]
[Route("api/load")]

public class LoadDataController : ControllerBase
{
    ILogger<LoadDataController> _logger;
    IMasterDb _masterDb;
    ILoadDataService _loadDataService;

    public LoadDataController(ILogger<LoadDataController> logger, IMasterDb masterDb, ILoadDataService loadDataService)
    {
        _logger = logger;
        _masterDb = masterDb;
        _loadDataService = loadDataService;
    }

    /// <summary>
    /// 게임 데이터 로드 API <br/>
    /// 유저의 기본 게임 데이터(기본 유저 데이터, 농장, 출석 데이터)를 로드합니다.
    /// </summary>
    [HttpPost("defaultData")]
    public async Task<ResDefaultDataDTO> LoadDefaultData(ReqDefaultDataDTO request)
    {
        // 게임 데이터 로드
        (var defaultDataResult, var defaultData) = await _loadDataService.GetDefaultGameData(request.UserID);
        if (!Successed(defaultDataResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(defaultDataResult),
                                         new { UserID = request.UserID }, "Failed");

            return new ResDefaultDataDTO() { Result = ErrorCode.LoadDefaultData_Fail };
        }

        LogInfoOnSuccess("LoadDefaultData", new { UserID = request.UserID });
        return new ResDefaultDataDTO() { Result = ErrorCode.None, DefaultData = defaultData };
    }

    /// <summary>
    /// 출석 데이터 로드 API <br/>
    /// 유저의 출석 데이터를 로드합니다.
    /// </summary>
    [HttpPost("attendData")]
    public async Task<ResAttendDataDTO> LoadAttendData(ReqAttendDataDTO request)
    {
        // 출석 데이터 로드
        (var attendDataResult, var attendData) = await _loadDataService.GetAttendanceDataByUserId(request.UserID);
        if (!Successed(attendDataResult))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(attendDataResult),
                                         new { UserID = request.UserID }, "Failed");

            return new ResAttendDataDTO() { Result = ErrorCode.LoadAttendData_Fail };
        }

        // 마스터DB에서 출석 보상 로드
        var rewardList = _masterDb._attendanceRewardList;

        LogInfoOnSuccess("LoadAttendData", new { UserID = request.UserID });
        return new ResAttendDataDTO() { Result = ErrorCode.None, RewardList = rewardList, AttendData = attendData };
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