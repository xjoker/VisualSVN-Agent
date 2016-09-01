
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VisualSVN_Agent.utils;

namespace VisualSVN_Agent.VirtualSVNHelper
{
    public static class SVNHelper
    {
        /// <summary>
        /// 获取SVN所有的用户组以及用户
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// 将htpasswd内的用户名密码读入
        /// </summary>
        public static void htpasswdRead()
        {
            string SVNGroupConfigPath = FileHelper.Combine(ProgramSetting.Repositoriespath, "htpasswd");

            if (File.Exists(SVNGroupConfigPath))
            {
                // 按行读取文件，确认文件至少药有一行
                var passwdFile = FileHelper.ReadFileForLines(SVNGroupConfigPath);
                if (passwdFile.Count>1)
                {
                    foreach (var item in passwdFile)
                    {
                        // 按行读取文件，确认是否有数据
                        string[] line = item.Split(':');
                        if (line.Length==2)
                        {
                            // 切分密码字符串
                            string[] pw = line[1].Split('$');
                            // 构建密码字符串类型
                            if (pw.Length==3)
                            {
                                htpasswdPassword hp = new htpasswdPassword();
                                hp.cryptMode = pw[0];
                                hp.salt = pw[1];
                                hp.passwordHash = pw[2];

                                
                                htpasswdUserAndPassword.UsersTable.Add(line[0], hp);
                            }
                        }
                    }
                }
                else
                {
                    LogHelper.WriteLog("htpasswd 文件为空", LogHelper.Log4NetLevel.Error);
                }
            }
            else
            {
                LogHelper.WriteLog("htpasswd 文件不存在", LogHelper.Log4NetLevel.Error);
            }
        }



        /// <summary>
        /// 密码校验
        /// </summary>
        /// <param name="username">SVN内存在的用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static bool passwordCheck(string username,string password)
        {
            if (htpasswdUserAndPassword.UsersTable.ContainsKey(username))
            {
                var pd = htpasswdUserAndPassword.UsersTable[username];
                var result = UnixMd5CryptTool.crypt(password, pd.salt, pd.cryptMode);
                if (result==("$"+pd.cryptMode+"$"+pd.salt+"$"+pd.passwordHash))
                {
                    return true;
                }
            }


            return false;
        }
    }
}
