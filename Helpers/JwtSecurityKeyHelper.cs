using System;
using System.Security.Cryptography;
using System.Text;

public static class JwtSecurityKeyHelper
{
    public static string GenerateSecretKey(int length = 64)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
