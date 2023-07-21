public class AttendanceModel
{
    public Int64 UserId { get; set; }

    public Int16 AttendanceCount { get; set; }

    public DateTime LastAttendance { get; set; }
    public DateTime PassEndDate { get; set; }
}