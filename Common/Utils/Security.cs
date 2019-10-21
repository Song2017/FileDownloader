using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Utils
{
    public static class Security
    {
        private static readonly byte[] Keys = { 0xff, 0xee, 0xdd, 0xcc, 0xaa, 0xbb, 0x99, 0x88 };

        public static string EncryptDes(this string encryptString)
        {
            return EncryptDes(encryptString, AppConstants.Salt);
        }

        public static string EncryptDes(this string encryptString, string encryptKey)
        {
            var dCsp = new DESCryptoServiceProvider();
            var mStream = new MemoryStream();
            try
            {
                var rgbKey = Encoding.Default.GetBytes(encryptKey.Substring(0, 8));
                var rgbIv = Keys;
                var inputByteArray = Encoding.Default.GetBytes(encryptString);

                using (var cStream = new CryptoStream(mStream, dCsp.CreateEncryptor(rgbKey, rgbIv), CryptoStreamMode.Write))
                {
                    cStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cStream.FlushFinalBlock();
                    cStream.Close();
                }

                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                return encryptString;
            }
            finally
            {
                mStream.Close();
                dCsp.Dispose();
            }
        }

        public static string DecryptDes(this string decryptString)
        {
            return DecryptDes(decryptString, AppConstants.Salt);
        }

        public static string DecryptDes(this string decryptString, string decryptKey)
        {
            DESCryptoServiceProvider dcsp = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            try
            {
                byte[] rgbKey = Encoding.Default.GetBytes(decryptKey.Substring(0, 8));
                byte[] rgbIv = Keys;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);

                using (var cStream = new CryptoStream(mStream, dcsp.CreateDecryptor(rgbKey, rgbIv), CryptoStreamMode.Write))
                {
                    cStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cStream.FlushFinalBlock();
                    cStream.Close();
                }

                return Encoding.Default.GetString(mStream.ToArray());
            }
            catch
            {
                return decryptString;
            }
            finally
            {
                mStream.Close();
                dcsp.Dispose();
            }
        }

        public static string EncryptQcCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            StringBuilder sb = new StringBuilder();
            for (int i = 0, len = code.Length; i < len; i++)
            {
                sb.Append((char)(code[i] - 16 + (i + 1)));
            }
            return sb.ToString();
        }

        public static string DecryptQcCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            StringBuilder sb = new StringBuilder();
            for (int i = 0, len = code.Length; i < len; i++)
            {
                sb.Append((char)(code[i] + 16 - (i + 1)));
            }
            return sb.ToString();
        }

        public static string EncryptUrl(string url)
        {
            if (!url.Contains("?"))
            {
                return url;
            }

            string paraString = url.Substring(url.IndexOf("?", StringComparison.Ordinal) + 1, url.Length - url.IndexOf("?", StringComparison.Ordinal) - 1);
            string stringValue = "-" + BitConverter.ToString(Encoding.Default.GetBytes(paraString));
            return url.Substring(0, url.IndexOf("?", StringComparison.Ordinal) + 1) + stringValue;
        }

        public static string EncryptQuery(string query)
        {
            if (!query.Contains("?"))
            {
                return query;
            }

            string paraString = query.Substring(query.IndexOf("?", StringComparison.Ordinal) + 1, query.Length - query.IndexOf("?", StringComparison.Ordinal) - 1);
            string stringValue = "-" + BitConverter.ToString(Encoding.Default.GetBytes(paraString));
            return stringValue;
        }

        // Decrypts a previously encrypted string.
        public static string DecryptUrl(string query)
        {
            string[] arr;
            string unEncQuery = string.Empty;
            string encQuery = query;
            StringBuilder sb = new StringBuilder();

            int firstAndPos = query.IndexOf('&');

            if (firstAndPos > -1)
            {
                unEncQuery = query.Substring(firstAndPos, query.Length - firstAndPos);
                encQuery = query.Substring(0, firstAndPos);
            }

            arr = encQuery.Split("-".ToCharArray());

            for (int i = 1; i < arr.Length; i++)
            {
                sb.Append((char)Convert.ToInt32(arr[i], 16));
            }

            sb.Append(unEncQuery);

            var result = sb.ToString();

            if (result.Substring(0, 1) == "?")
            {
                result = result.Substring(1, result.Length - 1);
            }

            return result;
        }
    }

    
}
