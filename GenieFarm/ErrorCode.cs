public enum ErrorCode : UInt16
{
    None = 0,

    MasterDB_Fail_LoadData = 1,
    
    Hive_Fail_InvalidResponse = 10,
    Hive_Fail_AuthCheck = 11,
    Hive_Fail_AuthCheckOnLogin = 12,
    Hive_Fail_AuthCheckException = 13,
    
    Redis_Fail_SetToken = 20,

    Account_Fail_CreateDefaultAttendanceData = 30,
    Account_Fail_CreateDefaultFarmData = 31,
    Account_Fail_InsertDefaultItem = 32,
    Account_Fail_UserNotExists = 33,
    Account_Fail_UserAlreadyExists = 34,
    Account_Fail_UserInfoNotExists = 35,
    Account_Fail_AttendDataNotExists = 36,
    Account_Fail_FarmInfoNotExists = 37,
    Account_Fail_DuplicateNickname = 38,
    Account_Fail_UpdateLastLogin = 39,
}