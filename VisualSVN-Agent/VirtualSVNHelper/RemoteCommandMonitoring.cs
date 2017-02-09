using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using VisualSVN_Agent.utils;

namespace VisualSVN_Agent.VirtualSVNHelper
{
    static class RemoteCommandMonitoring
    {
        public static void CheckRemoteCommand(int timeInterval = 10000)
        {
            Timer t = new Timer(timeInterval);
            t.Elapsed += new ElapsedEventHandler(TimingCheckAPI);
            t.Start();
        }

        public static void TimingCheckAPI(object source, ElapsedEventArgs e)
        {
            // https://xJoker-Office-PC:8443/svn/t/
            string postJSON = $"{{\"Data\":{{\"DataType\":\"requestCmd\",\"SVNPrefix\":\"{ProgramSetting.SitePrefix }\",\"mID\":\"{ProgramSetting.mID}\"}}}}";
            var p = EncryptsAndDecryptsHelper.Encrypt(postJSON, ProgramSetting.SecretKey);
            var returnCommand = WebFunctionHelper.GetCmd(p, ProgramSetting.CMDurl);

            if (returnCommand!=null)
            {
                foreach (var item in returnCommand)
                {
                    if (item.commandType != 0)
                    {
                        LogHelper.WriteLog(item.commandType.ToString());
                        // 如果没有值则直接置空
                        item.name = item.name ?? "";
                        item.repoName = item.repoName ?? "";
                        item.groupName = item.groupName ?? "";
                        item.password = item.password ?? "";
                        item.permission = item.permission ?? 0;
                        item.message = item.message ?? "";
                        item.Folders = item.Folders ?? "";
                        string svnAccount = ProgramSetting.svnAccount;
                        string svnPassword = ProgramSetting.svnPassword;

                        switch (item.commandType)
                        {
                            case Model.CommandType.SetRepositoryPermission:
                                if (item.permission >= 0 || item.permission <= 2)
                                {
                                    VisualSVN_WMI_Api.SetRepositoryPermission(item.name, item.repoName, (int)item.permission);
                                }
                                else
                                {
                                    VisualSVN_WMI_Api.SetRepositoryPermission(item.name, item.repoName);
                                }
                                break;
                            case Model.CommandType.DelRepositoryPermission:
                                VisualSVN_WMI_Api.DelRepositoryPermission(item.name, item.repoName);
                                break;
                            case Model.CommandType.CreatGroup:
                                if (!string.IsNullOrEmpty(item.name) && !item.name.Contains(','))
                                {
                                    // 单用户
                                    VisualSVN_WMI_Api.CreatGroup(item.groupName, new string[] { item.name });
                                }
                                else if (!string.IsNullOrEmpty(item.name) && item.name.Contains(','))
                                {
                                    // 多用户
                                    string[] userArr = item.name.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                    VisualSVN_WMI_Api.CreatGroup(item.groupName, userArr);
                                }
                                else
                                {
                                    // 仅创建用户组
                                    VisualSVN_WMI_Api.CreatGroup(item.groupName);
                                }
                                break;
                            case Model.CommandType.CreateUser:
                                VisualSVN_WMI_Api.CreateUser(item.name, item.password);
                                break;
                            case Model.CommandType.CreateRepository:
                                VisualSVN_WMI_Api.CreateRepository(item.repoName);
                                break;
                            case Model.CommandType.DeleteRepository:
                                VisualSVN_WMI_Api.DeleteRepository(item.repoName);
                                break;
                            case Model.CommandType.CreateRepositoryFolders:
                                if (!string.IsNullOrEmpty(item.Folders))
                                {
                                    string[] folderArr = item.Folders.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                    VisualSVN_WMI_Api.CreateRepositoryFolders(item.repoName, folderArr, item.message);
                                }
                                break;
                            case Model.CommandType.DeleteRepositoryFolders:
                                if (!string.IsNullOrEmpty(item.Folders))
                                {
                                    string[] folderArr = item.Folders.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                    VisualSVN_WMI_Api.DeleteRepositoryFolders(item.repoName, folderArr, item.message);
                                }
                                break;
                            case Model.CommandType.AddMemberToGroup:
                                VisualSVN_WMI_Api.AddMemberToGroup(item.name, item.groupName);
                                break;
                            case Model.CommandType.DelMemberOnGroup:
                                VisualSVN_WMI_Api.DelMemberOnGroup(item.name, item.groupName);
                                break;
                            case Model.CommandType.CheckOut:
                                
                                if (!string.IsNullOrEmpty(item.svnAccount) && !string.IsNullOrEmpty(item.svnPassword))
                                {
                                    svnAccount = item.svnAccount;
                                    svnPassword = item.svnPassword;
                                }
                                if (!SVNHelper.CheckOut(svnAccount, svnPassword, item.svnRepoUrl, item.svnLocalPath))
                                {
                                    LogHelper.WriteLog("SVN检出错误！",LogHelper.Log4NetLevel.Error);
                                }
                                break;
                            case Model.CommandType.Update:
                                if (!string.IsNullOrEmpty(item.svnAccount) && !string.IsNullOrEmpty(item.svnPassword))
                                {
                                    svnAccount = item.svnAccount;
                                    svnPassword = item.svnPassword;
                                }
                                SVNHelper.Update(svnAccount, svnPassword, item.svnLocalPath);
                                break;
                            case Model.CommandType.SetDirectoryAccessRule:
                                FileHelper.SetDirectoryAccessRule(item.name, item.Folders);
                                break;
                            case Model.CommandType.SetDirectoryShare:
                                // 创建共享目录
                                if (!Taxi.FileHelper.FileHelper.DirectoryExists(item.svnLocalPath))
                                {
                                    Taxi.FileHelper.FileHelper.CreateDirectory(item.svnLocalPath);
                                }

                                var acl = new Taxi.Network.Win32_Ace().SecurityDescriptor(ProgramSetting.WindowsShareUsername,ACL:Taxi.Network.Win32ShareType.ShareAccessMask.Read);
                                Taxi.Network.WindowsShareFolder.ShareFolder(item.svnLocalPath, item.Folders, item.Folders,acl);
                                break;
                            default:
                                break;
                        }
                    }
                }

            }

            
        }
    }
}
