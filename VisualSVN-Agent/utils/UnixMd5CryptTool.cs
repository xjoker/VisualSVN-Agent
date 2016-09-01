using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VisualSVN_Agent.utils
{
    class UnixMd5CryptTool
    {
        //** Password hash cryptMode */
        //private static String cryptMode = "$1$";

        /** Characters for base64 encoding */
        private static String itoa64 = "./0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// A function to concatenate bytes[]
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns>New adition array</returns>
        private static byte[] Concat(byte[] array1, byte[] array2)
        {
            byte[] concat = new byte[array1.Length + array2.Length];
            System.Buffer.BlockCopy(array1, 0, concat, 0, array1.Length);
            System.Buffer.BlockCopy(array2, 0, concat, array1.Length, array2.Length);
            return concat;
        }

        /// <summary>
        /// Another function to concatenate bytes[]
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <param name="max"></param>
        /// <returns>New adition array</returns>
        private static byte[] PartialConcat(byte[] array1, byte[] array2, int max)
        {
            byte[] concat = new byte[array1.Length + max];
            Buffer.BlockCopy(array1, 0, concat, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, concat, array1.Length, max);
            return concat;
        }

        /// <summary>
        /// Base64-Encode integer value
        /// </summary>
        /// <param name="value"> The value to encode</param>
        /// <param name="length"> Desired length of the result</param>
        /// <returns>@return Base64 encoded value</returns>
        private static string to64(int value, int length)
        {
            StringBuilder result;

            result = new StringBuilder();
            while (--length >= 0)
            {
                result.Append(itoa64.Substring(value & 0x3f, 1));
                value >>= 6;
            }
            return (result.ToString());
        }

        /// <summary>
        /// Unix-like Crypt-MD5 function
        /// </summary>
        /// <param name="password">用户密码</param>
        /// <param name="salt">盐</param>
        /// <param name="cryptMode">添加加密模式文字</param>
        /// <returns>a human readable string</returns>
        public static string crypt(string password, string salt, string cryptMode)
        {
            int saltEnd;
            int len;
            int value;
            int i;
            byte[] final;
            byte[] passwordBytes;
            byte[] saltBytes;
            byte[] ctx;

            StringBuilder result;
            HashAlgorithm x_hash_alg = HashAlgorithm.Create("MD5");

            // 如果 salt 以加密模式开头，则去除加密模式文字
            if (salt.StartsWith(cryptMode))
                salt = salt.Substring(cryptMode.Length);
            
            // 如果 salt 含有 Hash 后的密码也一并去除
            if ((saltEnd = salt.LastIndexOf('$')) != -1)
                salt = salt.Substring(0, saltEnd);

            // 如果 salt 超过8个字符则取前8个作为 salt
            if (salt.Length > 8)
                salt = salt.Substring(0, 8);

            // 将密码、加密模式、salt 组合
            ctx = Encoding.ASCII.GetBytes((password + cryptMode + salt));
            final = x_hash_alg.ComputeHash(Encoding.ASCII.GetBytes((password + salt + password)));


            // Add as many characters of ctx1 to ctx
            for (len = password.Length; len > 0; len -= 16)
            {
                if (len > 16)
                {
                    ctx = Concat(ctx, final);
                }
                else
                {
                    ctx = PartialConcat(ctx, final, len);
                }

                //System.Buffer.BlockCopy(final, 0, hash16, ctx.Length, len);
                //System.Buffer.BlockCopy(ctx, 0, hash16, 0, ctx.Length);

            }
            //ctx = hashM;

            // Then something really weird...
            passwordBytes = Encoding.ASCII.GetBytes(password);

            for (i = password.Length; i > 0; i >>= 1)
            {
                if ((i & 1) == 1)
                {
                    ctx = Concat(ctx, new byte[] { 0 });
                }
                else
                {
                    ctx = Concat(ctx, new byte[] { passwordBytes[0] });
                }
            }

            final = x_hash_alg.ComputeHash(ctx);

            byte[] ctx1;

            // Do additional mutations
            saltBytes = Encoding.ASCII.GetBytes(salt);  //.getBytes();
            for (i = 0; i < 1000; i++)
            {
                ctx1 = new byte[] { };

                if ((i & 1) == 1)
                {
                    ctx1 = Concat(ctx1, passwordBytes);
                }
                else
                {
                    ctx1 = Concat(ctx1, final);
                }
                if (i % 3 != 0)
                {
                    ctx1 = Concat(ctx1, saltBytes);
                }
                if (i % 7 != 0)
                {
                    ctx1 = Concat(ctx1, passwordBytes);
                }
                if ((i & 1) != 0)
                {
                    ctx1 = Concat(ctx1, final);
                }
                else
                {
                    ctx1 = Concat(ctx1, passwordBytes);
                }

                final = x_hash_alg.ComputeHash(ctx1);
            }

            result = new StringBuilder();

            // Add the password hash to the result string
            value = ((final[0] & 0xff) << 16) | ((final[6] & 0xff) << 8)
                    | (final[12] & 0xff);
            result.Append(to64(value, 4));
            value = ((final[1] & 0xff) << 16) | ((final[7] & 0xff) << 8)
                    | (final[13] & 0xff);
            result.Append(to64(value, 4));
            value = ((final[2] & 0xff) << 16) | ((final[8] & 0xff) << 8)
                    | (final[14] & 0xff);
            result.Append(to64(value, 4));
            value = ((final[3] & 0xff) << 16) | ((final[9] & 0xff) << 8)
                    | (final[15] & 0xff);
            result.Append(to64(value, 4));
            value = ((final[4] & 0xff) << 16) | ((final[10] & 0xff) << 8)
                    | (final[5] & 0xff);
            result.Append(to64(value, 4));
            value = final[11] & 0xff;
            result.Append(to64(value, 2));

            // Return result string
            return cryptMode + salt + "$" + result.ToString();
        }
    }
}
