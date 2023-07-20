public enum ErrorCode : UInt16
{
    None = 0,

    MasterDB_Fail_LoadData = 10,

    AuthCheck_Fail_ValidatePlayerID = 20,
    AuthCheck_Fail_ValidateUserID = 21,
    AuthCheck_Fail_ValidateJSONFormat = 22,
    AuthCheck_Fail_GetTokenString = 23,
    AuthCheck_Fail_GetVersionString = 24,


    Hive_Fail_InvalidResponse = 100,
    Hive_Fail_AuthCheck = 101,
    Hive_Fail_AuthCheckOnLogin = 102,
    Hive_Fail_AuthCheckException = 103,
    
    Redis_Fail_SetToken = 200,

    Account_Fail_CreateDefaultAttendanceData = 300,
    Account_Fail_CreateDefaultFarmData = 301,
    Account_Fail_InsertDefaultItem = 302,
    Account_Fail_UserNotExists = 303,
    Account_Fail_UserAlreadyExists = 304,
    Account_Fail_UserInfoNotExists = 305,
    Account_Fail_AttendDataNotExists = 306,
    Account_Fail_FarmInfoNotExists = 307,
    Account_Fail_DuplicateNickname = 308,
    Account_Fail_UpdateLastLogin = 309,
}