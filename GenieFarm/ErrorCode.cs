public enum ErrorCode : UInt16
{
    None = 0,

    MasterDB_Fail_LoadData = 10,

    JsonFieldCheck_Fail_GetPlayerID = 20,
    JsonFieldCheck_Fail_GetUserID = 21,
    JsonFieldCheck_Fail_ValidateJSONFormat = 22,
    JsonFieldCheck_Fail_GetRequiredField = 23,
    JsonFieldCheck_Fail_GetTokenString = 24,
    JsonFieldCheck_Fail_GetVersionString = 25,

    AuthCheck_Fail_TokenNotMatch = 30,
    AuthCheck_Fail_RequestOverlapped = 31,

    Hive_Fail_InvalidResponse = 100,
    Hive_Fail_AuthCheck = 101,
    Hive_Fail_AuthCheckOnLogin = 102,
    Hive_Fail_AuthCheckException = 103,
    
    Redis_Fail_SetToken = 200,
    Redis_Fail_DeleteToken = 201,

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