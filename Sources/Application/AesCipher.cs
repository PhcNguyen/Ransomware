// Copyright(C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

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

            this.iv = new byte[16]; // IV size for AES is always 16 bytes
            rng.GetBytes(this.iv);
        }
    }

    public byte[] Key => (byte[])key.Clone();
    public byte[] IV => (byte[])iv.Clone();

    // Hàm mã hóa
    public byte[] Encrypt(byte[] plaintext)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = this.key;
            aes.IV = this.iv;
            aes.Mode = CipherMode.CBC; // Đảm bảo bạn thiết lập chế độ mã hóa
            aes.Padding = PaddingMode.PKCS7; // Đảm bảo bạn thiết lập padding

            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plaintext, 0, plaintext.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }
    }

    public byte[] Decrypt(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = this.key;
            aes.IV = this.iv;
            aes.Mode = CipherMode.CBC; // Đảm bảo bạn thiết lập chế độ mã hóa
            aes.Padding = PaddingMode.PKCS7; // Đảm bảo bạn thiết lập padding

            using (var ms = new MemoryStream(cipherText))
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                using (var resultStream = new MemoryStream())
                {
                    cs.CopyTo(resultStream);
                    return resultStream.ToArray();
                }
            }
        }
    }
}
