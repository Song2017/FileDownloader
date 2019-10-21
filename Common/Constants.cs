using System.Collections.Generic;
using System.Globalization;

namespace Common
{
    public static class Constants
    {
        public const string WhiteSpace = @" ";
        public const string Comma = @",";
        public const string DateFormat = @"yyyy-MM-dd hh:mm:ss";
        public const string DateFormatSmall = @"yyyyMMdd_hhmmss";
        public const string FileNameDateFormat = @"yyyyMMdd_hhmmss_fffffffK";
    }

    public static class AppConstants
    {
        // security
        public const string Salt = "APP_Salt";
        public const string SaltPassword = "APP_SaltPassword_Unicode";

        public const int SaltBytes = 24;
        public const int HashBytes = 32;
        public const int Pbkdf2Iterations = 1000;

        // files
        public const string DownloadFolderName = "DownloadFiles";
        public const string DownloadFolderUrl = "/files";

        public const string DownloadFileTypes = "XML,DOCX,PDF";
        public const string Xml = "XML";
        public const string NameTemplateXml = "APP_RH_{0}_{1}_{2}.XML";

        public const string Docx = "DOCX";
        public const string NameTemplateDoc = "APP_RH_{0}_{1}_{2}.DOCX";

        public const string NameTemplateZip = "APP_RH_{0}_{1}_{2}.ZIP";

        //valve
        public const string ValveTypes = "RV,CV,MOV,LV";

        //languages
        public static readonly List<CultureInfo> SupportLanguages = new List<CultureInfo>
        {
            new CultureInfo("en-US"), // english
            new CultureInfo("zh-CN") // chinese
        };

    }

    public class TokenConstants
    {
        public const string Jwt = "Bearer ";
        public const string TokenName = "APP_App";
    }

    public class DBConstants
    {

        public const string NulValue = "NUL";

        public const string AllValue = "All";

        public const string CursorParam = "OUT_CURSOR";

        public static string SqlInjectionKeyword = @"^exec(\s+)|(\s+)exec(\s+)|[;']exec(\s+)|^execute(\s+)immediate|(\s+)execute(\s+)immediate" +
                                                   @"|[;']execute(\s+)immediate|^select(\s+)|(\s+)select(\s+)|[;']select(\s+)|^insert(\s+)into|(\s+)insert(\s+)into|[;']" +
                                                   @"insert(\s+)into|^delete(\s+)from|(\s+)delete(\s+)from|[;']delete(\s+)from|^drop(\s+)table(\s*)|[;']drop(\s+)table(\s*)|" +
                                                   @"(\s+)drop(\s+)table(\s*)|^update(\s+)|(\s+)update(\s+)|[;']update(\s+)|^truncate(\s+)table(\s*)|(\s+)truncate(\s+)table(\s*)|" +
                                                   @"[;']truncate(\s+)table(\s*)|^(create|drop)(\s+)(tablespace|user|table|view|index|procedure|function|trigger)|" +
                                                   @"(\s+)(create|drop)(\s+)(tablespace|user|table|view|index|procedure|function|trigger)|" +
                                                   @"[;'](create|drop)(\s+)(tablespace|user|table|view|index|procedure|function|trigger)|" +
                                                   @"^create(\s+)or(\s+)replace|(\s+)create(\s+)or(\s+)replace|[;']create(\s+)or(\s+)replace|^alter(\s+)table|(\s+)alter(\s+)table|" +
                                                   @"[;']alter(\s+)table|(\s+)asc$|(\s+)asc'|(\s+)asc(\s+)|substr(\s*)\(|chr(\s*)\(";
    }

}
