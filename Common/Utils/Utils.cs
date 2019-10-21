using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;

namespace Common.Utils
{
    public static class Utils
    {
        // Valves Extension
        public static string GetValveTableName(this object token)
        {
            var valvePairs = new Dictionary<string, string>()
            {
                {"RV", " RELIEFD "},
                {"CV", " CV "},
                {"MOV", " MOV "},
                {"LV", " LINEV "},
                {"", " RELIEFD "},
            };

            return token.IsNullOrEmptyOrSpace()
                ? valvePairs["RV"]
                : valvePairs[token.ToStringEx().Trim().ToUpper()];
        }


        // Json Web Token Extension
        public static JwtSecurityToken GetToken(this object token)
        {
            if (token.IsNullOrEmptyOrSpace())
                return null;

            var strToken = token.ToStringEx();
            if (strToken.StartsWith(TokenConstants.Jwt))
                strToken = strToken.Replace(TokenConstants.Jwt, string.Empty);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadJwtToken(strToken);

            return securityToken;
        }

        public static string GetTokenName(this JwtSecurityToken token, string claimName)
        {
            var tokenClaim = token.Claims.
                First(claim => claim.Type == claimName).Value;

            return tokenClaim;
        }


        // Files Extension
        public static bool CreateDirectory(string directoryName)
        {
            var directoryFullName = Path.Combine(Environment.CurrentDirectory,
                AppConstants.DownloadFolderName, directoryName);

            if (Directory.Exists(directoryFullName))
                return false;

            Directory.CreateDirectory(directoryFullName);

            return true;
        }

        public static bool CreateXmlFile<T>(this T valve, string directory, string fileName)
        {
            var fileFullName = Path.Combine(Environment.CurrentDirectory,
                AppConstants.DownloadFolderName, directory, fileName);
            var fileWriter = new StreamWriter(fileFullName);
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(fileWriter, valve);
            fileWriter.Close();
            return true;
        }

        public static bool CreateZipFile(string directoryName, ref string filename)
        {
            var directoryFullName = Path.Combine(Environment.CurrentDirectory,
                AppConstants.DownloadFolderName, directoryName);

            if (!Directory.Exists(directoryFullName))
                return false;

            ZipFile.CreateFromDirectory(directoryFullName, Path.Combine(Environment.CurrentDirectory,
                AppConstants.DownloadFolderName, filename));
            Directory.Delete(directoryFullName, true);

            return true;
        }


        // Integer Extension
        public static int ToInt(this object obj)
        {
            return int.Parse(obj.ToStringEx());
        }


        // String Extension
        public static string ToStringEx(this object obj)
        {
            return obj == null ? string.Empty : obj.ToString();
        }

        public static bool IsNullOrEmptyOrSpace(this object str)
        {
            if (str == null)
                return true;

            var s = str.ToString();
            return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
        }

        public static bool EqualsEx(this string str, string pattern)
        {
            return str.Equals(pattern.ToStringEx(), StringComparison.OrdinalIgnoreCase);
        }


        // Configuration Extension
        public static string GetDBSetting(this IConfiguration configuration, string keyName)
        {
            return configuration.GetAppSetting(keyName, "DataFactorySetting");
        }

        public static string GetAppSetting(this IConfiguration configuration,
            string keyName, string section = "AppSetting")
        {
            return configuration[$"{section}:{keyName}"].ToStringEx();
        }
    }

}
