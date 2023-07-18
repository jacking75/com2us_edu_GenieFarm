public enum ErrorCode : UInt16
{
    None = 0,

    MasterDB_Fail_LoadData = 1,
    
    Hive_Fail_AuthCheck = 10,
    
    Redis_Fail_SetToken = 11,

    Account_Fail_CreateDefaultAttendanceData = 12,
    Account_Fail_CreateDefaultFarmData = 13,
    Account_Fail_InsertDefaultItem = 14,
    Account_Fail_UserNotExists = 15,
    Account_Fail_UserAlreadyExists = 16,
    Account_Fail_UserInfoNotExists = 17,
    Account_Fail_AttendDataNotExists = 18,
    Account_Fail_FarmInfoNotExists = 19,
    Account_Fail_DuplicateNickname = 20,
    Account_Fail_UpdateLastLogin = 21,
}