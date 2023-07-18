public enum ErrorCode : UInt16
{
    None = 0,

    CreateRequestIDFailed = 1,
    MasterDataLoadFailed = 2,

    AuthCheckFail = 10,
    InvalidVersion = 11,
    TokenSettingFailed = 12,
    CreateDefaultAttendanceFailed = 13,
    CreateDefaultFarmInfoFailed = 14,
    InsertDefaultItemFailed = 15,
    UserNotExists = 16,
    UserAlreadyExists = 17,
    UserInfoNotExists = 32,
    FarmInfoNotExists = 33,
    DuplicateNickname = 18,
    LastLoginUpdateFailed = 34,
    LogoutFailed = 19,

    AlreadyAttended = 20,
    AttendException = 21,
    AttendDataNotExists = 22,

    InvalidMailPage = 23,
    MailPageNotExists = 24,
    MailNotExists = 25,
    MailSendFailed = 26,
    MailReceiverNotExists = 27,
    MailSenderNotOwnItem = 28,
    MailItemRollbackFailed = 29,
    MailItemNotExists = 30,
    MailItemReceiveFailed = 31,
}