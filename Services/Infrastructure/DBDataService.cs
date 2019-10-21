using System;
using System.Collections.Generic;
using System.Data;

using Common;
using Common.Utils;

namespace Services.Infrastructure
{
    public class DBDataService : IDBDataService
    {
        private readonly IDBManageService _manageService;

        public DBDataService(IDBManageService manageService)
        {
            _manageService = manageService;
        }


        public List<string> ValidateUser(string tenantCode, string vkUserName, string usrPassword)
        {
            var parameters = new[] {
                _manageService.BuildParam("@TenantCode", SqlDbType.NVarChar, tenantCode),
                _manageService.BuildParam("@UserName", SqlDbType.NVarChar, vkUserName),
                _manageService.BuildParam("@Password", SqlDbType.NVarChar, usrPassword),
                _manageService.BuildParam("@LastLogin", SqlDbType.NVarChar, DateTime.Now.ToString("yyyyMMddHHmmss")),
                _manageService.BuildOutParam("@OUT_UserID", SqlDbType.NVarChar, 40, null),
                _manageService.BuildOutParam("@OUT_TenantKey", SqlDbType.NVarChar, 40, null),
                _manageService.BuildOutParam("@OUT_ValidateResult",SqlDbType.NVarChar, 80, null)
            };

            return !_manageService.ExecuteNonQuery("USPUMVALIDATEUSER", parameters, out var outParas) ? null : outParas;
        }

        public string GetUserSalt(string userName, string tenantCode)
        {
            var parameters = new[] {
                _manageService.BuildParam("@userName", SqlDbType.NVarChar, userName),
                _manageService.BuildParam("@tenantCode", SqlDbType.NVarChar, tenantCode),
                _manageService.BuildOutParam(DBConstants.CursorParam, SqlDbType.NVarChar, null)
            };

            if (!_manageService.LoadDataTable("uspGetUserSalt", parameters, out var dt))
                return null;
            return dt.Rows.Count <= 0 ? null : dt.Rows[0][0].ToStringEx();
        }

        public DataTable ValidateRepairData(string tenantCode, string valveType, string owner, string plant,
            string tagnumber, string serialNumber, string queryMode, out List<string> outParas)
        {
            var parameters = new[]
            {
                _manageService.BuildParam("@tenantCode", SqlDbType.NVarChar, tenantCode),
                _manageService.BuildParam("@valveType", SqlDbType.NVarChar, valveType),
                _manageService.BuildParam("@owner", SqlDbType.NVarChar, owner),
                _manageService.BuildParam("@plant", SqlDbType.NVarChar, plant),
                _manageService.BuildParam("@tagnumber", SqlDbType.NVarChar, tagnumber),
                _manageService.BuildParam("@sernumber", SqlDbType.NVarChar, serialNumber),
                _manageService.BuildParam("@queryMode", SqlDbType.NVarChar, queryMode),
                _manageService.BuildOutParam("@result", SqlDbType.NVarChar, 600, null),
                _manageService.BuildOutParam(DBConstants.CursorParam, SqlDbType.NVarChar, null)
            };

            if (!_manageService.LoadDataTable("uspGetVAShareDataCheck", parameters, out var dataTable,
                out outParas))
                return null;
            return dataTable.Rows.Count <= 0 ? null : dataTable;
        }

        public DataTable GetRepairHistoryData(string tenantKey, string valveType, string equipmentKey)
        {
            var parameters = new[] {
                _manageService.BuildParam("@tenantKey", SqlDbType.NVarChar, tenantKey),
                _manageService.BuildParam("@valveType", SqlDbType.NVarChar, valveType),
                _manageService.BuildParam("@equipmentKey", SqlDbType.NVarChar, equipmentKey),
                _manageService.BuildOutParam(DBConstants.CursorParam, SqlDbType.NVarChar, null)
            };

            return !_manageService.LoadDataTable("uspGetVAShareData", parameters, out DataTable dt) ? null : dt;
        }



    }
}
