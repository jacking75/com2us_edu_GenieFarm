// AttendDTO.cs : AttendController���� ����ϴ� DTO ����
// Attend : �⼮üũ

public class ReqAttendDTO : InGameDTO
{
}

public class ResAttendDTO : ErrorCodeDTO
{
    public Int16 AttendanceCount { get; set; }
}