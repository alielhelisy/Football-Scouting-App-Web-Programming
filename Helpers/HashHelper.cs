using System.Security.Cryptography;
using System.Text;

namespace ScoutingAppMvc.Helpers;

public static class HashHelper
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
