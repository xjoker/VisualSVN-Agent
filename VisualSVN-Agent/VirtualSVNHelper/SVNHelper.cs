using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VisualSVN_Agent.utils;

namespace VisualSVN_Agent.VirtualSVNHelper
{
    public static class SVNHelper
    {
        public static Dictionary<string, List<string>> GetAllUserAndGroup()
        {
            Dictionary<string, List<string>> GroupAndUsers = new Dictionary<string, List<string>>();
            string SVNGroupConfigPath = FileHelper.Combine(ProgramSetting.Repositoriespath, "groups.conf");

            if (File.Exists(SVNGroupConfigPath))
            {
                string groupConf = FileHelper.ReadFile(SVNGroupConfigPath);
                if (!(string.IsNullOrEmpty(groupConf)))
                {
                    // 确定文件内是否有 [groups] 这一项配置
                    if (groupConf.Contains("[groups]"))
                    {
                        // 截取从 [groups] 起始到文件结束
                        groupConf = groupConf.Substring(groupConf.IndexOf("[groups]", StringComparison.Ordinal));

                        // 遍历所有用户与组的关系
                        var usersRegex = new Regex("(?i)(?<groupName>\\w+)=(?<users>[\\w,]+)", RegexOptions.CultureInvariant | RegexOptions.Compiled);

                        foreach (Match item in usersRegex.Matches(groupConf))
                        {
                            string key = item.Groups["groupName"].ToString().Trim();
                            string val = item.Groups["users"].ToString().Trim();
                            // 根据 "," 切分隶属用户组的用户
                            string[] users = val.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                            // 未创建组key的先创建
                            if (!GroupAndUsers.ContainsKey(key))
                            {
                                GroupAndUsers.Add(key, new List<string>());
                            }

                            GroupAndUsers[key] = users.ToList();
                        }

                        // 正常返回的位置
                        return GroupAndUsers;
                    }
                    else
                    {
                        LogHelper.WriteLog("配置文件内未发现 [groups] 节点", LogHelper.Log4NetLevel.Error);
                    }
                }
                else
                {
                    LogHelper.WriteLog("groups.conf 内容为空或者读取失败", LogHelper.Log4NetLevel.Error);
                }
            }
            else
            {
                LogHelper.WriteLog("未找到 groups.conf 文件", LogHelper.Log4NetLevel.Error);
            }
            return null;
        }
    }
}
