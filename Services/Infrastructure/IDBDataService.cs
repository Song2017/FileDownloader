using System.Collections.Generic;
using System.Data;

namespace Services.Infrastructure
{
    public interface IDBDataService
    {
        List<string> ValidateUser(string tenantCode, string vkUserName, string usrPassword);

        string GetUserSalt(string userName, string tenantCode);

        DataTable ValidateRepairData(string tenantCode, string valveType, string owner, string plant,
            string tagnumber, string serialNumber, string queryMode, out List<string> outParas);

        DataTable GetRepairHistoryData(string tenantKey, string valveType, string equipmentKey);
    }
}
