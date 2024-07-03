using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MobiusFF.Crypt;

public class Api
{
    /// <summary>
    /// Mevius.App.Api.Encrypt
    /// </summary>
    /// <param name="binary"></param>
    public static byte[] Encrypt(byte[] binary)
    {
        string text = Guid.NewGuid().ToString("N").Substring(0, 16);
        using ICryptoTransform cryptoTransform = new RijndaelManaged
        {
            BlockSize = 128,
            KeySize = 128,
            IV = Encoding.UTF8.GetBytes(text),
            Key = Encoding.UTF8.GetBytes(Program.AesKey),
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        }.CreateEncryptor();

        byte[] encBytes = cryptoTransform.TransformFinalBlock(binary, 0, binary.Length);

        byte[] encBinary = new byte[encBytes.Length + 16];
        byte[] guidBytes = Encoding.ASCII.GetBytes(text);
        Buffer.BlockCopy(guidBytes, 0, encBinary, 0, guidBytes.Length);
        Buffer.BlockCopy(encBytes, 0, encBinary, 16, encBytes.Length);

        return encBinary;
    }

    /// <summary>
    /// Mevius.App.Api.Encrypt
    /// </summary>
    public static string Encrypt(string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        bytes = Encrypt(bytes);
        value = Convert.ToBase64String(bytes);
        value = value.TrimEnd(new char[1]);
        return value;
    }

    /// <summary>
    /// Mevius.App.Api.Decrypt
    /// </summary>
    /// <param name="binary"></param>
    public static byte[] Decrypt(byte[] binary)
    {
        string iv = Encoding.ASCII.GetString(binary, 0, 16);

        using ICryptoTransform cryptoTransform = new RijndaelManaged
        {
            BlockSize = 128,
            KeySize = 128,
            IV = Encoding.UTF8.GetBytes(iv),
            Key = Encoding.UTF8.GetBytes(Program.AesKey), // Mevius.App.Api.AesKey
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        }.CreateDecryptor();

        byte[] decrypted = cryptoTransform.TransformFinalBlock(binary, 16, binary.Length - 16);
        return decrypted;
    }

    /// <summary>
    /// Mevius.App.Api.Decrypt
    /// </summary>
    // Mevius.App.AppManager.AppSetUp - "ZWQ5OGM5YjA0OTk2NDc1NkXg6sEHppPav9ixggrhnKeiXOZEBaoIy1NLXzfH+BdR" -> Managed/Assembly-CSharp.dll
    public static string Decrypt(string value)
    {
        byte[] array = Convert.FromBase64String(value);
        array = Decrypt(array);
        value = Encoding.ASCII.GetString(array, 0, array.Length);
        value = value.TrimEnd(new char[1]);
        return value;
    }
}
