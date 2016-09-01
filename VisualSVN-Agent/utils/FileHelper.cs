﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSVN_Agent.utils
{
    class FileHelper
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
                String input;
                while ((input = sr.ReadLine()) != null)
                {
                    lines.Add(input);
                }
            }
            return lines;
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
    }
}