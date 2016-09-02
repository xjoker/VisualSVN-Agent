using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public static bool PostToAPI(string jsonTxt,string url)
        {
            bool is_success = false;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string postJSON = "{\"Agent\":\"test\"," +"\"data\":\""+ jsonTxt + "\"}";

                streamWriter.Write(postJSON);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (result == "True") is_success = true;
            }

            return is_success;
        }
    }
}
