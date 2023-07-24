public enum ErrorCode : UInt16
{
    None = 0,

    // Middlewares
    /* JsonFieldCheckMiddleware */
    JsonFieldCheck_Fail_GetPlayerID = 20,
    JsonFieldCheck_Fail_GetUserID,
    JsonFieldCheck_Fail_ValidateJSONFormat,
    JsonFieldCheck_Fail_GetRequiredField,
    JsonFieldCheck_Fail_GetTokenString,
    JsonFieldCheck_Fail_GetVersionString,

    /* AuthCheckMiddleware */
    AuthCheck_Fail_TokenNotMatch = 30,
    AuthCheck_Fail_RequestOverlapped = 31,

    // MasterDB
    MasterDB_Fail_LoadData = 100,
    MasterDB_Fail_InvalidData,

    // Redis
    Redis_Fail_SetToken = 200,
    Redis_Fail_DeleteToken,

    // GameDB
    GameDB_Fail_CreateDefaultData = 1000,
    GameDB_Fail_CreateDefaultFarmData,
    GameDB_Fail_UserInfoNotExistsByPlayerID,
    GameDB_Fail_AttendDataNotExistsByPlayerID,
    GameDB_Fail_FarmInfoNotExistsByPlayerID,
    GameDB_Fail_UserInfoNotExistsByUserID,
    GameDB_Fail_AttendDataNotExistsByUserID,
    GameDB_Fail_FarmInfoNotExistsByUserID,
    GameDB_Fail_InsertedDuplicatedNickname,
    GameDB_Fail_CreateDefaultAttendData,
    GameDB_Fail_UpdateLastLogin,
    GameDB_Fail_InsertDefaultItem,
    GameDB_Fail_UpdatedAttendanceRowOutOfRange,
    GameDB_Fail_UpdateAttendDataException,
    GameDB_Fail_SendRewardIntoMailbox,
    GameDB_Fail_SendRewardException,
    GameDB_Fail_SendMailAttendRewardException,
    GameDB_Fail_CreateAttendRewardItem,

    /* AuthCheckService */
    AuthCheckService_GetUserId_UserNotExists = 2000,
    AuthCheckService_CheckPlayerExists_NotExists,
    AuthCheckService_CreateDefaultUserData_Fail,
    AuthCheckService_CreateDefaultGameData_DuplicatedNickname,
    AuthCheckService_CreateDefaultGameData_AttendData,
    AuthCheckService_CreateDefaultAttendanceData_Fail,
    AuthCheckService_CreateDefaultGameData_FarmData,
    AuthCheckService_CreateDefaultFarmData_Fail,
    AuthCheckService_CreateDefaultGameData_Items,
    AuthCheckService_CreateDefaultItems_Fail,
    AuthCheckService_GetDefaultGameDataByPlayerId_UserData,
    AuthCheckService_GetDefaultGameDataByUserId_UserData,
    AuthCheckService_GetDefaultGameDataByPlayerId_AttendData,
    AuthCheckService_GetDefaultGameDataByUserId_AttendData,
    AuthCheckService_GetDefaultGameDataByPlayerId_FarmData,
    AuthCheckService_GetDefaultGameDataByUserId_FarmData,
    AuthCheckService_UpdateLastLoginAt_Fail,
    AuthCheckService_CreateDefaultAttendanceData_AffectedRowOutOfRange,
    AuthCheckService_CreateDefaultFarmData_AffectedRowOutOfRange,
    AuthCheckService_CreateDefaultItems_AffectedRowOutOfRange,
    AuthCheckService_UpdateLastLoginAt_AffectedRowOutOfRange,


    /* AttendanceService */
    AttendanceService_GetAttendanceData,
    AttendanceService_UpdateAttendanceData_AffectedRowOutOfRange,
    AttendanceService_UpdateAttendanceData_Fail,
    AttendanceService_UpdateAttendanceData,
    AttendanceService_SendAttendanceReward,
    AttendanceService_SendAttendanceReward_CreateItem,
    AttendanceService_SendAttendanceReward_SendRewardIntoMail,
    AttendanceService_CreateItem_Fail,
    AttendanceService_SendRewardIntoMail,
    AttendanceService_SendRewardIntoMail_InsertedRowOutOfRange,
    AttendanceService_SendRewardIntoMail_Fail,


    /* LoadDataService */
    LoadDataService_GetDefaultGameDataByUserId_UserData,
    LoadDataService_GetDefaultGameDataByUserId_AttendData,
    LoadDataService_GetDefaultGameDataByUserId_FarmData,
    LoadDataService_GetAttendanceDataByUserId,


    // API
    /* AuthCheckToHive */
    Hive_Fail_InvalidResponse = 3000,
    Hive_Fail_AuthCheck,
    Hive_Fail_AuthCheckOnLogin,
    Hive_Fail_AuthCheckException,
    
    /* AuthCheckController */
    Create_Fail_UserAlreadyExists,
    Create_Fail_CreateDefaultDataFailed,
    Login_Fail_HiveAuthCheck,
    Login_Fail_UpdateLastLogin,
    Login_Fail_TokenSetting,
    Login_Fail_UserDataNotExists,
    Logout_Fail_DeleteToken,

    /* AttendController */
    Attend_Fail_GetAttendData,
    Attend_Fail_AlreadyAttended,
    Attend_Fail_ReceivedAllMonthlyRewards,
    Attend_Fail_AttendException,

    /* LoadDataController */
    LoadDefaultData_Fail,
    LoadAttendData_Fail,
}