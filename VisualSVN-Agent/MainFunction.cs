using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using VisualSVN_Agent.utils;
using VisualSVN_Agent.Model;
using VisualSVN_Agent.VirtualSVNHelper;

namespace VisualSVN_Agent
{
    class MainFunction
    {
        public static void MainRunFunction()
        {
            string encryptsJson = "";

            // 初始化 读取所有仓库的信息
            SVNHelper.ReadRepositoriesAll();

            JsonPostModel jpm = new JsonPostModel();
            jpm.Agent = JsonPostModelAgent.Client.ToString();
            jpm.DataType = JsonPostModelDataType.AllUserTable.ToString();
            jpm.Data = RepoDataSourcePermission.RepoPermissons;
            encryptsJson = EncryptsAndDecryptsHelper.Encrypt(JsonConvert.SerializeObject(jpm), ProgramSetting.SecretKey);
            if (!WebFunctionHelper.PostToAPI(encryptsJson, ProgramSetting.APIurl))
            {
                LogHelper.WriteLog("初始化时发送所有仓库信息出现错误", LogHelper.Log4NetLevel.Error);
            }


            // 初始化 读取所有用户的密码表
            SVNHelper.htpasswdRead();
            jpm.Agent = JsonPostModelAgent.Client.ToString();
            jpm.DataType = JsonPostModelDataType.AllAuthInfo.ToString();
            jpm.Data = htpasswdUserAndPassword.UsersTable;
            encryptsJson = EncryptsAndDecryptsHelper.Encrypt(JsonConvert.SerializeObject(jpm), ProgramSetting.SecretKey);
            if (!WebFunctionHelper.PostToAPI(encryptsJson, ProgramSetting.APIurl))
            {
                LogHelper.WriteLog("初始化时发送用户密码信息出现错误", LogHelper.Log4NetLevel.Error);
            }


            // 启动仓库权限文件监控
            FileWatcherHelper fwh = new FileWatcherHelper();
            fwh.WatcherStrat(@"C:\Repositories", "VisualSVN-SvnAuthz.ini");

            // 启动htpasswd文件监控
            htpasswdWatcher hw = new htpasswdWatcher();
            hw.WatcherStrat(@"C:\Repositories", "htpasswd");

            Console.WriteLine("程序启动");
            LogHelper.WriteLog("程序启动完成.", LogHelper.Log4NetLevel.Info);

        }

        
    }
}
