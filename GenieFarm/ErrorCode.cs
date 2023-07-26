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

    // Services
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
    AttendanceService_ValidateLastAttendance,
    AttendanceService_UpdateAttendanceData_AffectedRowOutOfRange,
    AttendanceService_UpdateAttendanceData_Fail,
    AttendanceService_UpdateAttendanceData,
    AttendanceService_CreateItem_Fail,
    AttendanceService_SendRewardIntoMail,
    AttendanceService_SendRewardIntoMail_InsertedRowOutOfRange,
    AttendanceService_SendRewardIntoMail_Fail,
    AttendanceService_SendAttendanceReward,

    /* LoadDataService */
    LoadDataService_GetDefaultGameDataByUserId_UserData,
    LoadDataService_GetDefaultGameDataByUserId_AttendData,
    LoadDataService_GetDefaultGameDataByUserId_FarmData,
    LoadDataService_GetAttendanceDataByUserId,

    /* MailService */
    MailService_InvalidPageNum,
    MailService_MailNotExists,
    MailService_SetItemAttribute_InvalidItemCodeAndCount,
    MailService_GetMailByMailId_SetItemAttribute,
    MailService_UpdateMailIsRead,
    MailService_GetMailAndSetRead_MailNotExists,
    MailService_GetMailAndSetRead_SetRead,
    MailService_SetItemAttribute_InvalidItemCode,
    MailService_ReceiveMailItem_SetReceive,


    // API
    /* AuthCheckToHive */
    Hive_Fail_InvalidResponse = 3000,
    Hive_Fail_AuthCheck,
    Hive_Fail_AuthCheckOnLogin,
    Hive_Fail_AuthCheckException,
    
    /* AuthCheckController */
    Create_Fail_UserAlreadyExists,
    Create_Fail_CreateDefaultDataFailed,
    Create_Fail_DuplicatedNickname,
    Login_Fail_HiveAuthCheck,
    Login_Fail_UpdateLastLogin,
    Login_Fail_TokenSetting,
    Login_Fail_UserDataNotExists,
    Logout_Fail_DeleteToken,

    /* AttendController */
    Attend_Fail_GetAttendData,
    Attend_Fail_NotAttendable,
    Attend_Fail_PassDataNotExists,
    Attend_Fail_AttendException,

    /* LoadDataController */
    LoadDefaultData_Fail,
    LoadAttendData_Fail,

    /* MailController */
    LoadMailListByPage_Fail_InvalidPage,
    LoadMail_Fail,
    ReceiveMailItem_Fail,
}