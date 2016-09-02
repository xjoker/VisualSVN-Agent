using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSVN_Agent.utils
{
    class FileWatcherHelper
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
        public void WatcherStrat(string path, string filter,NotifyFilters filters=NotifyFilters.LastWrite)
        {

            var watcher = new FileSystemWatcher
            {
                EnableRaisingEvents = false,
                Path = path,
                IncludeSubdirectories = true,
                NotifyFilter = filters,
                Filter = filter
            };

            watcher.Changed += OnProcess;
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
        }


        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // e.FullPath 是FileInfo类型 直接使用可输出完整路径
            var change=VirtualSVNHelper.SVNHelper.ReadRepositoriesFile(e.FullPath);
            // 将变化写入
            VirtualSVNHelper.SVNHelper.MergeDictionary(RepoDataSourcePermission.RepoPermissons, change);

            Console.WriteLine("写入完成");

        }
    }
}
