using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VisualSVN_Agent.utils
{
    class EncryptsAndDecryptsHelper
    {
        /// <summary>
        /// AES 加密模块
        /// </summary>
        /// <param name="text">明文</param>
        /// <param name="SecretKey">Key</param>
        /// <param name="AccessKey">IV</param>
        /// <returns>Base64编码的String</returns>
        public static string Encrypt(string text,string SecretKey,string IV)
        {
            byte[] plaintextbytes = Encoding.UTF8.GetBytes(text);
            
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.Key = Encoding.UTF8.GetBytes(SecretKey);
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            ICryptoTransform crypto = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encrypted = crypto.TransformFinalBlock(plaintextbytes, 0, plaintextbytes.Length);
            crypto.Dispose();
            return Convert.ToBase64String(encrypted.ToArray());
        }

        /// <summary>
        /// AES 解密模块
        /// </summary>
        /// <param name="encrypted">密文</param>
        /// <param name="SecretKey">Key</param>
        /// <param name="AccessKey">IV</param>
        /// <returns>UTF-8编码的String</returns>
        public static string Decrypt(string encrypted,string SecretKey, string IV)
        {
            byte[] encryptedbytes = Convert.FromBase64String(encrypted);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.Key = Encoding.UTF8.GetBytes(SecretKey);
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            ICryptoTransform crypto = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] secret = crypto.TransformFinalBlock(encryptedbytes, 0, encryptedbytes.Length);
            crypto.Dispose();
            return Encoding.UTF8.GetString(secret);

        }



    }
}

