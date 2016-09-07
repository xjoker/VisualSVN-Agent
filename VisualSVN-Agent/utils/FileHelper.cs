using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        /// 逐行读取文件 默认延时500ms 防止文件还在锁状态
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="startTag">起始标识符</param>
        /// <param name="sleep">启用延时</param>
        /// <returns>返回List类型</returns>
        public static List<string> ReadFileForLines(string filePath,string startTag="",bool sleep=true)
        {
            var lines = new List<string>();
            if (sleep) Thread.Sleep(500);
            using (FileStream fs=new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string input;
                    bool Tag = false;
                    while ((input = sr.ReadLine()) != null)
                    {
                        // 判断是否有起始标志
                        if (startTag != "")
                        {
                            if (Tag)
                            {
                                lines.Add(input);
                            }
                            if (input.Contains(startTag))
                            {
                                Tag = true;
                            }

                        }
                        // 没有则为默认起始
                        else
                        {
                            lines.Add(input);
                        }

                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 返回上层路径的文件夹名
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>如果返回失败则为空字符串</returns>
        public static string GetParentDirectoryName(string path)
        {
            try
            {
                DirectoryInfo dirInfo = Directory.GetParent(path);
                return Path.GetFileName(dirInfo.FullName);

            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// 返回当前路径上两层的文件夹名
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>如果返回失败则为空字符串</returns>
        public static string GetGrampaDirectoryName(string path)
        {
            try
            {
                DirectoryInfo dirInfo = Directory.GetParent(path);
                DirectoryInfo dirInfo1 = Directory.GetParent(dirInfo.FullName);
                return Path.GetFileName(dirInfo1.FullName);
            }
            catch
            {
                return "";
            }
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
