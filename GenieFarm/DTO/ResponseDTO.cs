// ErrorCodeDTO : 응답 시 ErrorCode를 반환하는 DTO, 기본적으로 모든 Response에 다 들어간다.

public class ErrorCodeDTO
{
    public ErrorCode Result { get; set; }
}