using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Common;
using Common.Utils;
using Services.Infrastructure;

namespace Services.Business
{
    public class UserService : IUserService
    {
        private readonly IDBDataService _dataService;
        private readonly IEncryptionService _encryptor;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public UserService(ILoggerFactory logger, IConfiguration config, IDBDataService dataService,
            IEncryptionService encryptionService)
        {
            _dataService = dataService;
            _encryptor = encryptionService;
            _logger = logger.CreateLogger<UserService>();
            _config = config;
        }


        private bool IsSuperUser(string password)
        {
            var enPassword = _encryptor.GetHash(password.ToLower(),
                AppConstants.SaltPassword).ToStringEx();

            return enPassword.EqualsEx(_config.GetAppSetting("SALT"));
        }

        public bool ValidateUser(string tenantCode, string userName, string password,
            out List<string> results)
        {
            results = new List<string>();

            try
            {
                if (IsSuperUser(password))
                {
                    var appSuperPass = _config.GetAppSetting("SALT");
                    results = _dataService.ValidateUser(tenantCode, userName, appSuperPass);
                    return results.Count <= 0 || !results[2].EqualsEx("NULLUSER");
                }

                var saltP = _dataService.GetUserSalt(userName, tenantCode);
                if (saltP.IsNullOrEmptyOrSpace())
                {
                    results.AddRange(new[] {string.Empty, string.Empty, "NULLUSER"});
                }
                else
                {
                    var encryptedPassword = _encryptor.GetHash(password, saltP);
                    results = _dataService.ValidateUser(tenantCode, userName, encryptedPassword);
                }

                if (results[2].EqualsEx("True"))
                    return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Validate User Error: {ex}");
            }

            return false;
        }

        public bool Authenticate(string tenantCode, string userName, string password, 
            ref string result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (!ValidateUser(tenantCode, userName, password, out var results))
            {
                result = GetFailResons(results[2]);
                return false;
            }

            // authentication successful so generate jwt token
            var key = Encoding.ASCII.GetBytes(_config.GetAppSetting("Secret"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(TokenConstants.TokenName,
                        $"{tenantCode}|{results[0].ToStringEx()}".EncryptDes())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_config.GetAppSetting("TokenExpire_s").ToInt()),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            result = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            _logger.LogInformation($"User {userName}: Token created successfully {result} !");

            return true;
        }

        // transfer fail code to message
        protected string GetFailResons(string fail)
        {
            switch (fail.ToUpper().Split(";")[0])
            {
                case "NULLUSER":
                case "FALSE":
                    return "Get token failed: User name or password not correct";
                case "LOCKED":
                case "USERISPREVENTLOGIN":
                    return "Get token failed: This User has been locked out";
                case "EXPIRED":
                    return "Get token failed: Password expires";
                default:
                    return "Get token failed: Please check credentials in VKc";
            }
        }

    }
}
