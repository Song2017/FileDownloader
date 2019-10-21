using System.Collections.Generic;

namespace Services.Business
{
    public interface IUserService
    {
        // validate user
        bool ValidateUser(string tenantCode, string userName, string password, out List<string> results);

        bool Authenticate(string tenantCode, string userName, string password, ref string result);
    }
}
