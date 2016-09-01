using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSVN_Agent.utils
{
    static class FileHelper
    {
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string ReadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 逐行读取文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>返回List类型</returns>
        public static List<string> ReadFileForLines(string filePath)
        {
            var lines = new List<string>();
            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string input;
                while ((input = sr.ReadLine()) != null)
                {
                    lines.Add(input);
                }
            }
            return lines;
        }

        /// <summary>
        /// 一次性读取文件所有内容
        /// </summary>字典
        /// <param name="filePath">文件路径</param>
        /// <returns>返回文件所有内容 MemoryStream </returns>
        public static MemoryStream ReadFileForAll(string filePath)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (Stream txt = File.OpenRead(filePath))
            {
                txt.CopyTo(memoryStream);
            }
            return memoryStream;
        }


        /// <summary>
        /// 写入文件 UTF-8
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">内容</param>
        public static void WriteFile(string filePath, string content)
        {
            WriteFile(filePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// 写入文件 (自定义编码)
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">内容</param>
        /// <param name="encoding">编码 Encoding类型</param>
        public static void WriteFile(string filePath, string content, Encoding encoding)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    Encoding encode = encoding;
                    //获得字节数组
                    byte[] data = encode.GetBytes(content);
                    //开始写入
                    stream.Write(data, 0, data.Length);
                    //清空缓冲区、关闭流
                    stream.Flush();
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(Convert.ToString(ex),LogHelper.Log4NetLevel.Error);
            }
        }

        /// <summary>
        /// 搜索路径下特定名称的文件并返回
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="se">搜索条件</param>
        /// <returns>string类型的路径数组</returns>
        public static string[] GetFileList(string folderPath,string fileName,SearchOption se)
        {
            string[] files = Directory.GetFiles(folderPath, fileName, se);

            return files;
        }

        /// <summary>
        /// 合并拼接路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        public static string Combine(this string path, string append)
        {
            return Path.Combine(path, append);
        }
    }
}
