
using System.Security.Cryptography;

namespace Ransomware.Sources.Application;
public class AesCipher
{
    private readonly byte[] key;
    private readonly byte[] iv;

    public AesCipher(int keySize = 256)
    {
        if (keySize != 128 && keySize != 192 && keySize != 256)
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

        using (var rng = RandomNumberGenerator.Create())
        {
            this.key = new byte[keySize / 8];
            rng.GetBytes(this.key);

            this.iv = new byte[16];
            rng.GetBytes(this.iv);
        }
    }

    public byte[] Key => (byte[])key.Clone();
    public byte[] IV => (byte[])iv.Clone();

    // Hàm mã hóa
    public byte[] Encrypt(string plaintext)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = this.key;
            aes.IV = this.iv;

            using (var ms = new System.IO.MemoryStream())
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new System.IO.StreamWriter(cs))
            {
                sw.Write(plaintext);
                sw.Flush();
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }
    }

    public string Decrypt(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = this.key;
            aes.IV = this.iv;

            using (var ms = new System.IO.MemoryStream(cipherText))
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (var sr = new System.IO.StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
