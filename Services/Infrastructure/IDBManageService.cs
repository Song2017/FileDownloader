using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Services.Infrastructure
{
    public interface IDBManageService
    {
        // build parameters
        SqlParameter BuildParam(string name, SqlDbType type, object paramValue);

        SqlParameter BuildOutParam(string name, SqlDbType type, object paramValue);

        SqlParameter BuildOutParam(string name, SqlDbType type, int? size, object paramValue);

        // out paramters
        bool ExecuteNonQuery(string procedureName, SqlParameter[] parameters, out List<string> outParas);

        // datatables
        bool LoadDataTable(string procedureName, SqlParameter[] parameters, out DataTable dt);

        bool LoadDataTable(string procedureName, SqlParameter[] parameters, out DataTable dt,
            out List<string> outParas);
    }
}
