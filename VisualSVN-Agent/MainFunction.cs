using Newtonsoft.Json;
using System;
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
            try
            {
                SVNHelper.ReadRepositoriesAll();
                LogHelper.WriteLog("仓库信息初始化完成", LogHelper.Log4NetLevel.Info);
            }
            catch (Exception ex)
            {

                LogHelper.WriteLog("初始化仓库出现错误\n"+ex, LogHelper.Log4NetLevel.Error);
            }

            foreach (var item in ProgramSetting.APIConfig)
            {
                JsonPostModel jpm = new JsonPostModel();
                jpm.Agent = JsonPostModelAgent.Client.ToString();
                jpm.DataType = JsonPostModelDataType.AllUserTable.ToString();
                jpm.Data = RepoDataSourcePermission.RepoPermissons;
                encryptsJson = EncryptsAndDecryptsHelper.Encrypt(JsonConvert.SerializeObject(jpm), item.SecretKey,item.IV);
                if (!WebFunctionHelper.PostToAPI(encryptsJson, item.APIurl, item.SecretKey, item.IV))
                {
                    LogHelper.WriteLog("初始化时发送所有仓库信息出现错误", LogHelper.Log4NetLevel.Error);
                }

                // 初始化 读取用户组信息
                try
                {
                    groupList.userGroup = SVNHelper.GetAllUserAndGroup();
                    LogHelper.WriteLog("用户与用户组信息初始化完成", LogHelper.Log4NetLevel.Info);
                }
                catch (Exception ex)
                {

                    LogHelper.WriteLog("初始化用户信息出现错误\n" + ex, LogHelper.Log4NetLevel.Error);
                }

                // 初始化 读取所有用户的密码表
                try
                {
                    SVNHelper.htpasswdRead();
                    LogHelper.WriteLog("用户密码表信息初始化完成", LogHelper.Log4NetLevel.Info);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("初始化用户密码表信息出现错误\n" + ex, LogHelper.Log4NetLevel.Error);
                }

                jpm.Agent = JsonPostModelAgent.Client.ToString();
                jpm.DataType = JsonPostModelDataType.AllAuthInfo.ToString();
                jpm.Data = htpasswdUserAndPassword.UsersTable;
                encryptsJson = EncryptsAndDecryptsHelper.Encrypt(JsonConvert.SerializeObject(jpm), item.SecretKey, item.IV);
                if (!WebFunctionHelper.PostToAPI(encryptsJson, item.APIurl, item.SecretKey, item.IV))
                {
                    LogHelper.WriteLog("初始化时发送用户密码信息出现错误", LogHelper.Log4NetLevel.Error);
                }

            }



            LogHelper.WriteLog("启动文件监控", LogHelper.Log4NetLevel.Info);

            // 启动仓库权限文件监控
            try
            {
                SVNFileWatcherHelper fwh = new SVNFileWatcherHelper();
                fwh.WatcherStrat(ProgramSetting.Repositoriespath, "VisualSVN-SvnAuthz.ini");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("启动 VisualSVN-SvnAuthz.ini 监控出现错误： " + ex.ToString(), LogHelper.Log4NetLevel.Error);
                throw;
            }

            // 启动htpasswd文件监控
            try
            {
                htpasswdWatcher hw = new htpasswdWatcher();
                hw.WatcherStrat(ProgramSetting.Repositoriespath, "htpasswd");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("启动 htpasswd 监控出现错误： " + ex.ToString(), LogHelper.Log4NetLevel.Error);
                throw;
            }

            // 启动用户组文件监控
            try
            {
                groupFileWatcher gfw = new groupFileWatcher();
                gfw.WatcherStrat(ProgramSetting.Repositoriespath, "groups.conf");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("启动 groups.conf 监控出现错误： " + ex.ToString(), LogHelper.Log4NetLevel.Error);
                throw;
            }

            RemoteCommandMonitoring.CheckRemoteCommand();


            Console.WriteLine("程序启动");
            LogHelper.WriteLog("程序启动完成.", LogHelper.Log4NetLevel.Info);

        }

        
    }
}
