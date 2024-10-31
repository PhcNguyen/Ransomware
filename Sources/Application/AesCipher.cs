// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using System.Security.Cryptography;
using System.Text;


namespace Ransomware.Sources.Application;
public class AesCipher
{
    public byte[] key;
    public byte[] iv;

    public AesCipher(int keySize = 256)
    {
        if (keySize != 128 && keySize != 192 && keySize != 256)
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");
        // Tạo khóa ngẫu nhiên
        using (var rng = RandomNumberGenerator.Create())
        {
            this.key = new byte[keySize / 8]; // Chia cho 8 để chuyển đổi từ bit sang byte
            rng.GetBytes(this.key);
        }

        // Tạo vector khởi tạo (IV) ngẫu nhiên
        this.iv = new byte[16]; // 128 bit cho IV
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(this.iv);
        }
    }

    // Hàm mã hóa
    public byte[] Encrypt(string plaintext)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = this.key; 
            aes.IV = this.iv; 

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var ms = new System.IO.MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new System.IO.StreamWriter(cs))
                    {
                        sw.Write(plaintext);
                    }
                }
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

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var ms = new System.IO.MemoryStream(cipherText))
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var sr = new System.IO.StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}

public class AesGcmCipher
{
    private readonly byte[] key;

    // Tag size is set to 128 bits (16 bytes)
    private const int TagSize = 16; // 16 bytes for 128-bit tag

    public AesGcmCipher(int keySize = 256)
    {
        if (keySize != 128 && keySize != 192 && keySize != 256)
            throw new ArgumentException("Key size must be 128, 192, or 256 bits.");

        key = new byte[keySize / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
    }

    public (byte[] Ciphertext, byte[] Tag, byte[] IV) Encrypt(string plaintext)
    {
        using (AesGcm aesGcm = new AesGcm(key, TagSize))
        {
            byte[] iv = new byte[AesGcm.NonceByteSizes.MaxSize]; // You can specify a size, e.g., 12 bytes for GCM
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[TagSize]; // Create tag array of specified size

            aesGcm.Encrypt(iv, Encoding.UTF8.GetBytes(plaintext), ciphertext, tag);

            return (ciphertext, tag, iv);
        }
    }

    public string Decrypt(byte[] ciphertext, byte[] tag, byte[] iv)
    {
        using (AesGcm aesGcm = new AesGcm(key, TagSize))
        {
            byte[] plaintext = new byte[ciphertext.Length];
            aesGcm.Decrypt(iv, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}