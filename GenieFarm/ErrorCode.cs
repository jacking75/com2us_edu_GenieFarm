public enum ErrorCode : UInt16
{
    None = 0,

    // Middlewares
    /* JsonFieldCheckMiddleware */
    JsonFieldCheck_Fail_GetPlayerID = 20,
    JsonFieldCheck_Fail_GetUserID = 21,
    JsonFieldCheck_Fail_ValidateJSONFormat = 22,
    JsonFieldCheck_Fail_GetRequiredField = 23,
    JsonFieldCheck_Fail_GetTokenString = 24,
    JsonFieldCheck_Fail_GetVersionString = 25,

    /* AuthCheckMiddleware */
    AuthCheck_Fail_TokenNotMatch = 30,
    AuthCheck_Fail_RequestOverlapped = 31,

    // MasterDB
    MasterDB_Fail_LoadData = 100,
    MasterDB_Fail_InvalidData = 101,

    // Redis
    Redis_Fail_SetToken = 200,
    Redis_Fail_DeleteToken = 201,

    // GameDB
    GameDB_Fail_CreateDefaultData = 1000,
    GameDB_Fail_CreateDefaultFarmData = 1001,
    GameDB_Fail_UserInfoNotExistsByPlayerID = 1002,
    GameDB_Fail_AttendDataNotExistsByPlayerID = 1003,
    GameDB_Fail_FarmInfoNotExistsByPlayerID = 1004,
    GameDB_Fail_UserInfoNotExistsByUserID = 1005,
    GameDB_Fail_AttendDataNotExistsByUserID = 1006,
    GameDB_Fail_FarmInfoNotExistsByUserID = 1007,
    GameDB_Fail_InsertedDuplicatedNickname = 1008,
    GameDB_Fail_CreateDefaultAttendData = 1009,
    GameDB_Fail_UpdateLastLogin = 1010,
    GameDB_Fail_InsertDefaultItem = 1011,
    GameDB_Fail_UpdatedAttendanceRowOutOfRange = 1012,
    GameDB_Fail_UpdateAttendDataException = 1013,
    GameDB_Fail_SendRewardIntoMailbox = 1014,
    GameDB_Fail_SendRewardException = 1015,
    GameDB_Fail_SendMailAttendRewardException = 1016,
    GameDB_Fail_CreateAttendRewardItem = 1017,

    // API
    /* AuthCheckToHive */
    Hive_Fail_InvalidResponse = 3000,
    Hive_Fail_AuthCheck = 3001,
    Hive_Fail_AuthCheckOnLogin = 3002,
    Hive_Fail_AuthCheckException = 3003,
    
    /* AuthCheckController */
    Create_Fail_UserAlreadyExists = 3004,
    Login_Fail_HiveAuthCheck = 3005,
    Login_Fail_UpdateLastLogin = 3005,
    Login_Fail_TokenSetting = 3006,
    Logout_Fail_DeleteToken = 3007,

    /* AttendController */
    Attend_Fail_GetAttendData = 3008,
    Attend_Fail_AlreadyAttended = 3009,
    Attend_Fail_ReceivedAllMonthlyRewards = 3010,
    Attend_Fail_AttendException = 3011,
}