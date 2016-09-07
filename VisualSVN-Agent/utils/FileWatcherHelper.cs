using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualSVN_Agent.Model;
using VisualSVN_Agent.VirtualSVNHelper;

namespace VisualSVN_Agent.utils
{

    /// <summary>
    /// 用于监视 VisualSVN-SvnAuthz.ini
    /// </summary>
    class SVNFileWatcherHelper
    {
        // 用于过滤触发两次事件
        private class FileChange
        {
            public DateTime Time { get; set; }
            public string FileName { get; set; }
        }

        private static FileChange _lastFileChange = new FileChange();

        /// <summary>
        /// 文件变化监控方法
        /// </summary>
        /// <param name="path">监控路径</param>
        /// <param name="filter">监控过滤条件</param>
        /// <param name="filters">监控文件的事件方式</param>
        public void WatcherStrat(
            string path,
            string filter,
            NotifyFilters filters = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            )
        {

            var watcher = new FileSystemWatcher
            {
                EnableRaisingEvents = false,
                Path = path,
                IncludeSubdirectories = true,
                NotifyFilter = filters,
                Filter = filter
            };

            watcher.Changed += new FileSystemEventHandler(OnProcess);
            watcher.Created += new FileSystemEventHandler(OnProcess);
            watcher.Deleted += new FileSystemEventHandler(OnProcess);
            watcher.EnableRaisingEvents = true;
        }


        private static void OnProcess(object source, FileSystemEventArgs e)
        {
            // 用于修正触发两次的问题
            if (_lastFileChange != null && e.FullPath == _lastFileChange.FileName && _lastFileChange.Time.AddSeconds(1) > DateTime.Now)
                return;

            _lastFileChange = new FileChange { Time = DateTime.Now, FileName = e.FullPath };

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                OnChanged(source, e);
            }
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                OnDeleted(source, e);
            }
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                OnCreate(source, e);
            }
        }

        // 文件/文件夹改变处理
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // e.FullPath 是FileInfo类型 直接使用可输出完整路径
            var change = VirtualSVNHelper.SVNHelper.ReadRepositoriesFile(e.FullPath);
            // 将变化写入
            VirtualSVNHelper.SVNHelper.MergeRepoDictionary(RepoDataSourcePermission.RepoPermissons, change);
            LogHelper.WriteLog(e.FullPath + "\n 文件发生变化", LogHelper.Log4NetLevel.Debug);

            bool is_Success = false;
            int reTryCount = 3;
            if (is_Success == false && reTryCount > 0)
            {

                // 加密发送消息
                JsonPostModel jpm = new JsonPostModel();
                jpm.Agent = JsonPostModelAgent.Client.ToString();
                jpm.DataType = JsonPostModelDataType.ChangeRepo.ToString();
                jpm.Data = change;
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

        // 文件/文件夹删除处理
        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            // 如果是目录被删除则为Repo被删除
            var dirName = FileHelper.GetGrampaDirectoryName(e.FullPath);
            if (File.GetAttributes(e.FullPath.Replace(e.Name, "")) == FileAttributes.Directory)
            {
                //删除这个Repo
                RepoDataSourcePermission.RepoPermissons.Remove(dirName);
                LogHelper.WriteLog(e.FullPath + "\n repo 被删除", LogHelper.Log4NetLevel.Debug);

                bool is_Success = false;
                int reTryCount = 3;
                if (is_Success == false && reTryCount > 0)
                {
                    // 删除repo
                    DelRepo dr = new DelRepo();
                    dr.RepoName = dirName;
                    // 加密发送消息
                    JsonPostModel jpm = new JsonPostModel();
                    jpm.Agent = JsonPostModelAgent.Client.ToString();
                    jpm.DataType = JsonPostModelDataType.DelRepo.ToString();
                    jpm.Data = dr;
                    is_Success = WebFunctionHelper.PostToAPI(EncryptsAndDecryptsHelper.Encrypt(JsonConvert.SerializeObject(jpm), ProgramSetting.SecretKey), ProgramSetting.APIurl);
                    if (is_Success)
                    {
                        is_Success = true;
                    }
                    else
                    {
                        LogHelper.WriteLog("发送删除Repo信息出现错误", LogHelper.Log4NetLevel.Error);
                        reTryCount--;
                        Thread.Sleep(3000);
                    }

                }
                else
                {
                    LogHelper.WriteLog("发送删除Repo信息重试3次之后依旧出错！", LogHelper.Log4NetLevel.Error);
                }



            }
            else
            {
                Console.WriteLine("文件被删除");
            }

        }

        // 文件/文件夹创建处理
        private static void OnCreate(object source, FileSystemEventArgs e)
        {

            var change = VirtualSVNHelper.SVNHelper.ReadRepositoriesFile(e.FullPath);
            VirtualSVNHelper.SVNHelper.MergeRepoDictionary(RepoDataSourcePermission.RepoPermissons, change);
            LogHelper.WriteLog(e.FullPath + "\n 新的Repo已经创建", LogHelper.Log4NetLevel.Debug);
            bool is_Success = false;
            int reTryCount = 3;
            if (is_Success == false && reTryCount > 0)
            {
                // 加密发送消息
                JsonPostModel jpm = new JsonPostModel();
                jpm.Agent = JsonPostModelAgent.Client.ToString();
                jpm.DataType = JsonPostModelDataType.NewRepo.ToString();
                jpm.Data = change;
                is_Success = WebFunctionHelper.PostToAPI(EncryptsAndDecryptsHelper.Encrypt(JsonConvert.SerializeObject(jpm), ProgramSetting.SecretKey), ProgramSetting.APIurl);
                if (is_Success)
                {
                    is_Success = true;
                }
                else
                {
                    LogHelper.WriteLog("发送新Repo信息出现错误", LogHelper.Log4NetLevel.Error);
                    reTryCount--;
                    Thread.Sleep(3000);
                }

            }
            else
            {
                LogHelper.WriteLog("发送新Repo信息重试3次之后依旧出错！", LogHelper.Log4NetLevel.Error);
            }

        }
    }


    /// <summary>
    /// 用于监视htpasswd
    /// </summary>
    class htpasswdWatcher
    {
        // 用于过滤触发两次事件
        private class FileChange
        {
            public DateTime Time { get; set; }
            public string FileName { get; set; }
        }
        private static FileChange _lastFileChange = new FileChange();

        public void WatcherStrat(string path, string filter, NotifyFilters filters = NotifyFilters.LastWrite)
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
    
    /// <summary>
    /// 用于监视group.conf
    /// </summary>
    class groupFileWatcher
    {
        // 用于过滤触发两次事件
        private class FileChange
        {
            public DateTime Time { get; set; }
            public string FileName { get; set; }
        }
        private static FileChange _lastFileChange = new FileChange();

        public void WatcherStrat(string path, string filter, NotifyFilters filters = NotifyFilters.LastWrite)
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
            // 刷新用户组
            groupList.userGroup = SVNHelper.GetAllUserAndGroup();

            // 刷新用户表
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
