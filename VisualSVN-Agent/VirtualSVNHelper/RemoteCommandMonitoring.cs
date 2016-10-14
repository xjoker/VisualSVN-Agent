using System;
using System.Collections.Generic;
using System.Linq;
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
            string postJSON = "{\"Data\":{\"DataType\":\"requestCmd\"}}";
            var p = EncryptsAndDecryptsHelper.Encrypt(postJSON, ProgramSetting.SecretKey);
            var returnCommand = WebFunctionHelper.GetCmd(p, ProgramSetting.APIurl + "/cmd");


            // 如果没有值则直接置空
            returnCommand.name = returnCommand.name ?? "";
            returnCommand.repoName = returnCommand.repoName ?? "";
            returnCommand.groupName = returnCommand.groupName ?? "";
            returnCommand.password = returnCommand.password ?? "";
            returnCommand.permission = returnCommand.permission ?? 0;
            returnCommand.message = returnCommand.message ?? "";
            returnCommand.Folders = returnCommand.Folders ?? "";

            switch (returnCommand.commandType)
            {
                case Model.CommandType.SetRepositoryPermission:
                    if(returnCommand.permission>=0 || returnCommand.permission<=2)
                    {
                        VisualSVN_WMI_Api.SetRepositoryPermission(returnCommand.name, returnCommand.repoName, (int)returnCommand.permission);
                    }
                    else
                    {
                        VisualSVN_WMI_Api.SetRepositoryPermission(returnCommand.name, returnCommand.repoName);
                    }
                    break;
                case Model.CommandType.DelRepositoryPermission:
                    VisualSVN_WMI_Api.DelRepositoryPermission(returnCommand.name, returnCommand.repoName);
                    break;
                case Model.CommandType.CreatGroup:
                    if (!string.IsNullOrEmpty(returnCommand.name) && !returnCommand.name.Contains(','))
                    {
                        // 单用户
                        VisualSVN_WMI_Api.CreatGroup(returnCommand.groupName, new string[] { returnCommand.name });
                    }
                    else if (!string.IsNullOrEmpty(returnCommand.name) && returnCommand.name.Contains(','))
                    {
                        // 多用户
                        string[] userArr = returnCommand.name.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        VisualSVN_WMI_Api.CreatGroup(returnCommand.groupName, userArr);
                    }
                    else
                    {
                        // 仅创建用户组
                        VisualSVN_WMI_Api.CreatGroup(returnCommand.groupName);
                    }
                    break;
                case Model.CommandType.CreateUser:
                    VisualSVN_WMI_Api.CreateUser(returnCommand.name, returnCommand.password);
                    break;
                case Model.CommandType.CreateRepository:
                    VisualSVN_WMI_Api.CreateRepository(returnCommand.repoName);
                    break;
                case Model.CommandType.DeleteRepository:
                    VisualSVN_WMI_Api.DeleteRepository(returnCommand.repoName);
                    break;
                case Model.CommandType.CreateRepositoryFolders:
                    if (!string.IsNullOrEmpty(returnCommand.Folders))
                    {
                        string[] folderArr = returnCommand.Folders.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        VisualSVN_WMI_Api.CreateRepositoryFolders(returnCommand.repoName, folderArr, returnCommand.message);
                    }
                    break;
                case Model.CommandType.DeleteRepositoryFolders:
                    if (!string.IsNullOrEmpty(returnCommand.Folders))
                    {
                        string[] folderArr = returnCommand.Folders.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        VisualSVN_WMI_Api.DeleteRepositoryFolders(returnCommand.repoName, folderArr, returnCommand.message);
                    }
                    break;
                case Model.CommandType.AddMemberToGroup:
                    VisualSVN_WMI_Api.AddMemberToGroup(returnCommand.name, returnCommand.groupName);
                    break;
                case Model.CommandType.DelMemberOnGroup:
                    VisualSVN_WMI_Api.DelMemberOnGroup(returnCommand.name, returnCommand.groupName);
                    break;
                default:
                    break;
            }
        }
    }
}
