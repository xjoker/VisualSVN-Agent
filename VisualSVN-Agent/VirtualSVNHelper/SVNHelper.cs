
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            htpasswdUserAndPassword.UsersTable.Clear();

            if (File.Exists(SVNGroupConfigPath))
            {
                // 按行读取文件，确认文件至少药有一行
                var passwdFile = FileHelper.ReadFileForLines(SVNGroupConfigPath);
                if (passwdFile.Count > 1)
                {
                    foreach (var item in passwdFile)
                    {
                        // 按行读取文件，确认是否有数据
                        string[] line = item.Split(':');
                        if (line.Length == 2)
                        {
                            // 切分密码字符串 并去除空切分
                            string[] pw = line[1].Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                            // 构建密码字符串类型
                            if (pw.Length == 3)
                            {
                                htpasswdPassword hp = new htpasswdPassword();
                                hp.cryptMode = pw[0];
                                hp.salt = pw[1];
                                hp.passwordHash = pw[2];

                                foreach (var g in groupList.userGroup)
                                {
                                    if (g.Value.Contains(line[0]))
                                    {
                                        if (string.IsNullOrEmpty(hp.userGroup))
                                        {
                                            hp.userGroup = g.Key;
                                        }
                                        else
                                        {
                                            hp.userGroup = hp.userGroup + "," + g.Key;
                                        }
                                        
                                    }
                                }



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
        public static bool passwordCheck(string username, string password)
        {
            if (htpasswdUserAndPassword.UsersTable.ContainsKey(username))
            {
                var pd = htpasswdUserAndPassword.UsersTable[username];
                var result = UnixMd5CryptTool.crypt(password, pd.salt, pd.cryptMode);
                if (result == ("$" + pd.cryptMode + "$" + pd.salt + "$" + pd.passwordHash))
                {
                    return true;
                }
            }


            return false;
        }


        /// <summary>
        /// 读取 VisualSVN-SvnAuthz.ini 文件的方法
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>仓库的详细权限表</returns>
        public static Dictionary<string, RepoAccessPermisson> ReadRepositoriesFile(string filePath)
        {

            try
            {
                if (File.Exists(filePath))
                {
                    // 解析配置文件
                    var SvnAuthzFile = FileHelper.ReadFileForLines(filePath, "[/");
                    string Reponame = FileHelper.GetGrampaDirectoryName(filePath);
                    RepoAccessPermisson rap = new RepoAccessPermisson();

                    //按行解析用户
                    foreach (var a in SvnAuthzFile)
                    {
                        if (!string.IsNullOrWhiteSpace(a))
                        {
                            string[] userAndPermissions = a.Split('=');
                            string Username = userAndPermissions[0];

                            // 权限
                            RepoAccessPermissionDetails rapd = new RepoAccessPermissionDetails();

                            // Read Only
                            if (userAndPermissions[1].Contains('r'))
                            {
                                rapd.Read = true;
                            }
                            // Read / Write
                            if (userAndPermissions[1].Contains("rw"))
                            {
                                rapd.Write = true;
                            }

                            // No Access
                            if (!(userAndPermissions[1].Contains('r') || userAndPermissions[1].Contains('w')))
                            {
                                rapd.NoAccess = true;
                            }

                            // is Group?
                            if (userAndPermissions[0].StartsWith("@"))
                            {
                                rapd.IsGroup = true;
                                Username = Username.Replace("@", "##");
                            }

                            rap.Permission.Add(Username, rapd);

                        }

                    }
                    Dictionary<string, RepoAccessPermisson> rdsp = new Dictionary<string, RepoAccessPermisson>();
                    rdsp.Add(Reponame, rap);
                    return rdsp;
                }
                else
                {
                    LogHelper.WriteLog("指定路径不存在 VisualSVN-SvnAuthz.ini 文件", LogHelper.Log4NetLevel.Error);
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.StackTrace);
                LogHelper.WriteLog("读取仓库权限出现异常 可能是SVN详细设定的目录权限导致");
                return null;
            }
        }

        /// <summary>
        /// 读取所有仓库文件夹内的权限
        /// </summary>
        public static void ReadRepositoriesAll()
        {
            var SvnAuthzList = FileHelper.GetFileList(ProgramSetting.Repositoriespath, "VisualSVN-SvnAuthz.ini", SearchOption.AllDirectories);
            Dictionary<string, RepoAccessPermisson> bbb = new Dictionary<string, RepoAccessPermisson>();
            foreach (var svnFile in SvnAuthzList)
            {
                var res = ReadRepositoriesFile(svnFile);
                if (res!=null)
                {
                    bbb = MergeRepoDictionary(bbb, res);
                }
            }

            RepoDataSourcePermission.RepoPermissons = bbb;
        }

        /// <summary>
        /// 合并仓库权限表
        /// </summary>
        /// <param name="first">字典合并的目标</param>
        /// <param name="second">被合并的目标</param>
        /// <returns></returns>
        public static Dictionary<string, RepoAccessPermisson> MergeRepoDictionary(Dictionary<string, RepoAccessPermisson> first, Dictionary<string, RepoAccessPermisson> second)
        {
            if (first == null) first = new Dictionary<string, RepoAccessPermisson>();
            if (second == null) return first;

            foreach (string key in second.Keys)
            {
                if (!first.ContainsKey(key))
                    first.Add(key, second[key]);
            }
            return first;
        }

        /// <summary>
        /// SVN 检出
        /// </summary>
        /// <param name="svnUsername"></param>
        /// <param name="svnPassword"></param>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckOut(string svnUsername,string svnPassword,string url,string path)
        {
            try
            {

                Process proc = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.RedirectStandardOutput = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "svn.exe";
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.Arguments = string.Format("checkout {0} {1} --username {2} --password {3}", url, path, svnUsername, svnPassword); 

                proc.StartInfo = startInfo;
                if (proc.Start())
                {
                    proc.WaitForExit();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("SVN 检出错误:" + ex);
                return false;
            }

        }

        /// <summary>
        /// SVN 更新
        /// </summary>
        /// <param name="svnUsername"></param>
        /// <param name="svnPassword"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Update(string svnUsername, string svnPassword,string path)
        {
            try
            {
                Process proc = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.RedirectStandardOutput = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "svn.exe";
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.Arguments = string.Format("up  --username {0} --password {1} {2}", svnUsername, svnPassword, path);

                proc.StartInfo = startInfo;
                if (proc.Start())
                {
                    proc.WaitForExit();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("SVN 更新错误"+ex);
                return false;
            }
        }

    }
}
