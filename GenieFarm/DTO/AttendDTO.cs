// AttendDTO.cs : AttendController에서 사용하는 DTO 정의
// Attend : 출석체크

public class ReqAttendDTO : InGameDTO
{
}

public class ResAttendDTO : ErrorCodeDTO
{
    public Int16 AttendanceCount { get; set; }
}