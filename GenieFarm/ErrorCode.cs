public enum ErrorCode : UInt16
{
    None = 0,
    AuthCheckFail = 10,
    InvalidVersion = 11,
    SessionSettingFail = 12,
    RegisterException = 13,
    RegisterAttendanceException = 14,
    UserNotExists = 15,
    UserAlreadyExists = 16,
    DuplicateNickname = 17,

    AlreadyAttended = 20,
    AttendException = 21,

    InvalidMailPage = 23,
    MailPageNotExists = 24,
    MailNotExists = 25,
    MailSendException = 26,
    MailReceiverNotExists = 27,
    MailSenderNotOwnItem = 28,
    MailItemRollbackFailed = 29,
}