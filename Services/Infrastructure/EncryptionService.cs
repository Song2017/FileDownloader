using System;
using System.Security.Cryptography;
using Common;

namespace Services.Infrastructure
{
    // Hash helper class based on MD5
    public class EncryptionService : IEncryptionService
    {

        public string GetRandomPassword()
        {
            var arr = new[] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z",
                "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
                "1","2","3","4","5","6","7","8","9","0","~","!","@","#","$","%","^","&","*","(",")","+","=","_","-","?" };
            var passwordString = "";
            var rand = new Random();
            for (var i = 0; i < 13; i++)
            {
                var temp = arr[rand.Next(0, arr.Length)];
                passwordString += temp;
            }
            passwordString += (char)rand.Next(48, 57);
            passwordString += "=";
            return passwordString;
        }
        public string CreateSaltAndPassword(string sourcePassword, out string saltP)
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            byte[] salt = new byte[AppConstants.SaltBytes];
            rngCsp.GetBytes(salt);
            saltP = Convert.ToBase64String(salt);

            byte[] hash = Pbkdf2(sourcePassword, salt, AppConstants.HashBytes);
            rngCsp.Dispose();
            return Convert.ToBase64String(hash);
        }
        public string CreateSaltAndPassword(out string originalPassword, out string saltP)
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            byte[] salt = new byte[AppConstants.SaltBytes];
            rngCsp.GetBytes(salt);
            saltP = Convert.ToBase64String(salt);
            originalPassword = GetRandomPassword();

            byte[] hash = Pbkdf2(originalPassword, salt, AppConstants.HashBytes);
            rngCsp.Dispose();
            return Convert.ToBase64String(hash);
        }
        public string GetHash(string sourcePassword, string saltPa)
        {
            byte[] hash = Pbkdf2(sourcePassword, Convert.FromBase64String(saltPa), AppConstants.HashBytes);
            return Convert.ToBase64String(hash);
        }
        private static byte[] Pbkdf2(string password, byte[] salt, int outputBytes)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt))
            {
                pbkdf2.IterationCount = AppConstants.Pbkdf2Iterations;
                return pbkdf2.GetBytes(outputBytes);
            }
        }
    }
}
