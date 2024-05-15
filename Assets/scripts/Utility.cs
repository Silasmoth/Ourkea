using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Linq;
using System;
using System.Text;

public static class Utility 
{

    

    public const string RANDOM_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    

    public static bool IsValidEmail(string email)
    {
        

        if (email.Trim().EndsWith("."))
        {
            return false; 
        }
        try
        {
            
            return new System.Net.Mail.MailAddress(email).Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }


    

    public static byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    public static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
}
