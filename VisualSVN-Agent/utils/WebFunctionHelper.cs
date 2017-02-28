using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VisualSVN_Agent.Model;

namespace VisualSVN_Agent.utils
{
    class WebFunctionHelper
    {




        /// <summary>
        /// 发送数据到服务器的方法
        /// </summary>
        /// <param name="jsonTxt">传入 Json 文本</param>
        /// <param name="url">API 地址</param>
        /// <returns>返回是否成功</returns>
        public static bool PostToAPI(string jsonTxt, string url, string SecretKey,string IV)
        {
            bool is_success = false;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string postJSON = $"{{\"Data\":\"{jsonTxt}\"}}";
                    streamWriter.Write(postJSON);
                    streamWriter.Flush();
                    streamWriter.Close();
                }


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var resp = EncryptsAndDecryptsHelper.Decrypt(result,SecretKey,IV);
                    try
                    {
                        var reader = JsonConvert.DeserializeObject<JsonResponse>(resp);
                        if (reader!=null)
                        {
                            is_success = Convert.ToBoolean(reader.status);
                            return is_success;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("服务端返回状态有误，可能是密钥不对:\n" + ex.ToString(), LogHelper.Log4NetLevel.Error);
                    }

                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif

                LogHelper.WriteLog("连接API出现错误:" + ex.ToString(), LogHelper.Log4NetLevel.Error);
            }



            return is_success;
        }



        /// <summary>
        /// 接收命令
        /// </summary>
        /// <param name="jsonTxt">传入 Json 文本</param>
        /// <param name="url">API 地址</param>
        /// <returns>返回值</returns>
        public static List<RemoteCommand> GetCmd(string jsonTxt, string url, string SecretKey,string IV)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            try
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string postJSON = $"{{\"Data\":\"{jsonTxt}\"}}";
                    streamWriter.Write(postJSON);
                    streamWriter.Flush();
                    streamWriter.Close();
                }


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var resp = EncryptsAndDecryptsHelper.Decrypt(result, SecretKey,IV);

                    if (resp!= "{'CommandType':'NoCommand'}")
                    {
                        LogHelper.WriteLog(resp);
                        try
                        {
                            string[] r = resp.Split('|');

                            List<RemoteCommand> rm = new List<RemoteCommand>();
                            foreach (var item in r)
                            {
                                if (!string.IsNullOrEmpty(item))
                                {
                                    rm.Add(JsonConvert.DeserializeObject<RemoteCommand>(item));
                                }
                            }
                            if (rm != null)
                            {
                                return rm;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog("服务端返回有误，可能是密钥不对:\n" + ex.ToString(), LogHelper.Log4NetLevel.Error);
                        }
                    }
                    
                    

                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif

                LogHelper.WriteLog("连接API出现错误:" + ex.ToString(), LogHelper.Log4NetLevel.Error);
            }



            return null;
        }
    }
}
