public enum ErrorCode : UInt16
{
    None = 0,
    AuthCheckFail = 10,
    RegisterException = 11,
    RegisterAttendanceException = 12,
    UserNotExists = 13,
    UserAlreadyExists = 14,
    DuplicateNickname = 15,

    AlreadyAttended = 20,

    InvalidMailPage = 23,
    MailOpenException = 24,
    MailNotExists = 25,
    MailSendException = 26,
}