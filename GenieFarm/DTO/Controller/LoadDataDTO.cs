// LoadDataDTO.cs : LoadDataController에서 사용하는 DTO 정의
// DefaultData : 게임 데이터 로드

public class ReqDefaultDataDTO : InGameDTO
{

}

public class ResDefaultDataDTO : ErrorCodeDTO
{
    public DefaultDataDTO? DefaultData { get; set; }
}


// LoadAttendData : 출석 정보 조회

public class ReqAttendDataDTO : InGameDTO
{
}

public class ResAttendDataDTO : ErrorCodeDTO
{
    public List<AttendanceRewardModel>? RewardList { get; set; }
    public AttendanceModel? AttendData { get; set; }
}