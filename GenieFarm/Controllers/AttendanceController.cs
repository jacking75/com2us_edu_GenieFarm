//using Microsoft.AspNetCore.Mvc;

//[ApiController]
//[Route("api/[controller]")]
//public class AttendanceController : ControllerBase
//{
//    ILogger<AttendanceController> _logger;
//    IGameDb _gameDb;
//    public AttendanceController(ILogger<AttendanceController> logger, IGameDb gameDb)
//    {
//        _logger = logger;
//        _gameDb = gameDb;
//    }

//    [HttpPost]
//    public async Task<ResAttendDTO> Attend(ReqAttendDTO request)
//    {
//        // 마지막 출석 날짜 로드
//        var lastAttendData = await _gameDb.GetAttendanceData(request.UserID);

//        // 출석 정보가 존재하지 않음
//        if (lastAttendData == null)
//        {
//            return new ResAttendDTO() { Result = ErrorCode.AttendDataNotExists };
//        };

//        // 출석 가능한지 체크
//        if (IsAnotherDay(lastAttendData.LastAttendance))
//        {
//            if (!await _gameDb.Attend(request.UserID))
//            {
//                // 출석 실패
//                return new ResAttendDTO() { Result = ErrorCode.AttendException };
//            }

//            // TODO : 출석 보상 지급
//            return new ResAttendDTO() { Result = ErrorCode.None, AttendanceCount = (Int16)(lastAttendData.AttendanceCount + 1) };
//        }
//        else
//        {
//            // 출석 불가능
//            return new ResAttendDTO() { Result = ErrorCode.AlreadyAttended };
//        }
//    }


//    bool IsAnotherDay(DateTime lastAttendDate)
//    {
//        // 현재 날짜와 마지막 출석 날짜가 1일 이상 차이가 나면 True
//        var currentDate = DateTime.Parse(DateTime.Now.ToShortDateString());
//        lastAttendDate = DateTime.Parse(lastAttendDate.ToShortDateString());
//        var diff = currentDate - lastAttendDate;

//        return diff > TimeSpan.FromDays(0);
//    }
//}