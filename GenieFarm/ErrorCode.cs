public enum ErrorCode : UInt16
{
    None = 0,

    CreateRequestIDFail = 1,
    MasterDataLoadFail = 2,

    AuthCheckFail = 10,
    InvalidVersion = 11,
    SessionSettingFail = 12,
    CreateDefaultAttendanceDataException = 13,
    CreateDefaultFarmDataException = 14,
    InsertDefaultItemFail = 15,
    UserNotExists = 16,
    UserAlreadyExists = 17,
    DuplicateNickname = 18,
    LogoutFail = 19,

    AlreadyAttended = 20,
    AttendException = 21,
    AttendDataNotExists = 22,

    InvalidMailPage = 23,
    MailPageNotExists = 24,
    MailNotExists = 25,
    MailSendException = 26,
    MailReceiverNotExists = 27,
    MailSenderNotOwnItem = 28,
    MailItemRollbackFailed = 29,
    MailItemNotExists = 30,
    MailItemReceiveFail = 31,
}