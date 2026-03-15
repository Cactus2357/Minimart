using System.Security.Cryptography;

namespace MinimartApi.Utilities
{
    public class Util
    {
        public static string GenerateSecureCode(int length = 6)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var data = RandomNumberGenerator.GetBytes(length);

            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[data[i] % chars.Length];
            }
            return new string(result);
        }

        //public static string GetConfirmCodeKey(string email)
        //    => $"confirm:code:{email}";

        //public static string GetConfirmCooldownKey(string email)
        //    => $"confirm:cooldown:{email}";

        //public static string GetConfirmAttemptKey(string email)
        //    => $"confirm:attempts:{email}";

    }
}
