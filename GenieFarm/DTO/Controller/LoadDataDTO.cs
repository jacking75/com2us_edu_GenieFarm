// LoadDataDTO.cs : LoadDataController에서 사용하는 DTO 정의
// DefaultData : 게임 데이터 로드

public class ReqDefaultDataDTO : InGameDTO
{

}

public class ResDefaultDataDTO : ErrorCodeDTO
{
    public DefaultDataDTO? DefaultData { get; set; }
}
