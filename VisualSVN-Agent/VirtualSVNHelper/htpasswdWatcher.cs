using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualSVN_Agent.Model;
using VisualSVN_Agent.utils;

namespace VisualSVN_Agent.VirtualSVNHelper
{
    class htpasswdWatcher
    {
        // 用于过滤触发两次事件
        private class FileChange
        {
            public DateTime Time { get; set; }
            public string FileName { get; set; }
        }
        private static FileChange _lastFileChange = new FileChange();

        public void WatcherStrat(string path,string filter, NotifyFilters filters = NotifyFilters.LastWrite)
        {

            var watcher = new FileSystemWatcher
            {
                EnableRaisingEvents = false,
                Path = path,
                IncludeSubdirectories = false,
                NotifyFilter = filters,
                Filter = filter
            };

            watcher.Changed += new FileSystemEventHandler(OnProcess);
            watcher.EnableRaisingEvents = true;
        }

        private static void OnProcess(object sender, FileSystemEventArgs e)
        {
            // 用于修正触发两次的问题
            if (_lastFileChange != null && e.FullPath == _lastFileChange.FileName && _lastFileChange.Time.AddSeconds(1) > DateTime.Now)
                return;

            _lastFileChange = new FileChange { Time = DateTime.Now, FileName = e.FullPath };

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                OnChanged(sender, e);
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            SVNHelper.htpasswdRead();

            bool is_Success = false;
            int reTryCount = 3;
            if (is_Success == false && reTryCount > 0)
            {

                // 加密发送消息
                JsonPostModel jpm = new JsonPostModel();
                jpm.Agent = JsonPostModelAgent.Client.ToString();
                jpm.DataType = JsonPostModelDataType.AllAuthInfo.ToString();
                jpm.Data = htpasswdUserAndPassword.UsersTable;
                is_Success = WebFunctionHelper.PostToAPI(EncryptsAndDecryptsHelper.Encrypt(JsonConvert.SerializeObject(jpm), ProgramSetting.SecretKey), ProgramSetting.APIurl);
                if (is_Success)
                {
                    is_Success = true;
                }
                else
                {
                    LogHelper.WriteLog("发送Repo更新信息出现错误", LogHelper.Log4NetLevel.Error);
                    reTryCount--;
                    Thread.Sleep(3000);
                }

            }
            else
            {
                LogHelper.WriteLog("发送Repo更新信息重试3次之后依旧出错！", LogHelper.Log4NetLevel.Error);
            }
        }
    }
}
