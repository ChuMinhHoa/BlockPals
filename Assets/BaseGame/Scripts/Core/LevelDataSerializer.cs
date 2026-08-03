using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class LevelDataSerializer
{
    public static string Serialize(object data)
    {
        return Encrypt(JsonUtility.ToJson(data));
    }
    public static T Deserialize<T>(string json)
    {
        return JsonUtility.FromJson<T>(Decrypt(json));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] GetKey()
    {
        return Encoding.UTF8.GetBytes("1234567890123456");
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] GetIV()
    {
        return Encoding.UTF8.GetBytes("6543210987654321");
    }

    public static string Encrypt(string plainText)
    {
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = GetKey();
        aesAlg.IV = GetIV();

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new();
        using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (StreamWriter swEncrypt = new(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        byte[] encrypted = msEncrypt.ToArray();
        return Convert.ToBase64String(encrypted);
    }
    public static string Decrypt(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = GetKey();
        aesAlg.IV = GetIV();

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new(cipherBytes);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}
