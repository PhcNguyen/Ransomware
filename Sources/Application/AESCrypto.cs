// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using System.Security.Cryptography;


namespace Ransomware.Sources.Application;
public class AESCrypto
{
    public byte[] key;
    public byte[] iv;

    public AESCrypto(int keySize = 256)
    {
        if (keySize != 128 && keySize != 192 && keySize != 256)
            throw new ArgumentException("Kích thước khóa phải là 128, 192 hoặc 256 bit.");
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