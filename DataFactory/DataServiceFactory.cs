using Common.Utils;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Common;

namespace DataFactory
{
    public class DataServiceFactory
    {
        public DataAccessMgr DataAccessMgr { get; set; }

        public DataServiceFactory()
        {
            DataAccessMgr = new DataAccessMgr(); 
        }

        public DataServiceFactory(string connectionString, int? sqlCommandTimeout)
        {
            DataAccessMgr = new DataAccessMgr(connectionString, sqlCommandTimeout);
        }

        public List<string> ValidateUser(string tenantCode, string vkUserName, string usrPassword)
        {
            SqlParameter[] parameters = new[] {
                DataAccessMgr.BuildParam("@TenantCode", SqlDbType.NVarChar, tenantCode),
                DataAccessMgr.BuildParam("@UserName", SqlDbType.NVarChar, vkUserName),
                DataAccessMgr.BuildParam("@Password", SqlDbType.NVarChar, usrPassword),
                DataAccessMgr.BuildParam("@LastLogin", SqlDbType.NVarChar, DateTime.Now.ToString("yyyyMMddHHmmss")),
                DataAccessMgr.BuildOutParam("@OUT_UserID", SqlDbType.NVarChar, 40, null),
                DataAccessMgr.BuildOutParam("@OUT_TenantKey", SqlDbType.NVarChar, 40, null),
                DataAccessMgr.BuildOutParam("@OUT_ValidateResult",SqlDbType.NVarChar, 80, null)
            };

            return !DataAccessMgr.ExecuteNonQuery("USPUMVALIDATEUSER", parameters, out var outParas) ? null : outParas;
        }

        public string GetUserSalt(string userName, string tenantCode)
        {
            var parameters = new[] {
                DataAccessMgr.BuildParam("@userName", SqlDbType.NVarChar, userName),
                DataAccessMgr.BuildParam("@tenantCode", SqlDbType.NVarChar, tenantCode), 
                DataAccessMgr.BuildOutParam(DBConstants.CursorParam, SqlDbType.NVarChar, null)
            };

            if (!DataAccessMgr.LoadDataTable("uspGetUserSalt", parameters, out var dt))
                return null;
            return dt.Rows.Count <= 0 ? null : dt.Rows[0][0].ToStringEx();
        }

        public DataTable ValidateRepairData(string tenantCode, string valveType, string owner, string plant,
            string tagnumber, string serialNumber, string queryMode, out List<string> outParas)
        {
            var parameters = new []
            {
                DataAccessMgr.BuildParam("@tenantCode", SqlDbType.NVarChar, tenantCode),
                DataAccessMgr.BuildParam("@valveType", SqlDbType.NVarChar, valveType),
                DataAccessMgr.BuildParam("@owner", SqlDbType.NVarChar, owner),
                DataAccessMgr.BuildParam("@plant", SqlDbType.NVarChar, plant),
                DataAccessMgr.BuildParam("@tagnumber", SqlDbType.NVarChar, tagnumber),
                DataAccessMgr.BuildParam("@sernumber", SqlDbType.NVarChar, serialNumber),
                DataAccessMgr.BuildParam("@queryMode", SqlDbType.NVarChar, queryMode),
                DataAccessMgr.BuildOutParam("@result", SqlDbType.NVarChar, 600, null),
                DataAccessMgr.BuildOutParam(DBConstants.CursorParam, SqlDbType.NVarChar, null)
            };

            if (!DataAccessMgr.LoadDataTable("uspGetVAShareDataCheck", parameters, out var dataTable,
                out outParas))
                return null;
            return dataTable.Rows.Count <= 0 ? null : dataTable;
        }

        public DataTable GetRepairHistoryData(string tenantKey, string valveType, string equipmentKey)
        {
            var parameters = new[] {
                DataAccessMgr.BuildParam("@tenantKey", SqlDbType.NVarChar, tenantKey),
                DataAccessMgr.BuildParam("@valveType", SqlDbType.NVarChar, valveType),
                DataAccessMgr.BuildParam("@equipmentKey", SqlDbType.NVarChar, equipmentKey),
                DataAccessMgr.BuildOutParam(DBConstants.CursorParam, SqlDbType.NVarChar, null)
            };

            return !DataAccessMgr.LoadDataTable("uspGetVAShareData", parameters, out DataTable dt) ? null : dt;
        }
    }
}
