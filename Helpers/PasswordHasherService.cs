using System.Security.Cryptography;
using System.Text;

namespace Contoso.Helpers
{
    public class PasswordHasherService
    {
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);  // You can store this hashed password
            }
        }

        public bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == hashedPassword;
        }
    }
}
