using System.Security.Cryptography;
using System.Text;

namespace TestFlowStudio.Core.Helpers;

public static class EncryptionHelper
{
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return "";
        var data = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return "";
        try
        {
            var data = Convert.FromBase64String(encryptedText);
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return "";
        }
    }
}
