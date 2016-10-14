using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace VisualSVN_Agent.utils
{
    class VisualSVN_WMI_Api
    {
        /// <summary>
        /// 权限代码
        /// </summary>
        public enum AccessLevel : uint
        {
            NoAccess = 0,
            ReadOnly,
            ReadWrite
        }

        /// <summary>
        /// 检测 VisualSVN 的WMI是否可用
        /// </summary>
        /// <returns></returns>
        public static bool CheckVisualSNVWMI()
        {
            ManagementClass userClass = new ManagementClass("root\\VisualSVN", "VisualSVN_User", null);
            if (userClass == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取仓库实体
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static ManagementObject GetRepositoryObject(string repoName)
        {
            return new ManagementObject("root\\VisualSVN", string.Format("VisualSVN_Repository.Name='{0}'", repoName), null);
        }

        /// <summary>
        /// 获取权限字典
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IDictionary<string, AccessLevel> GetPermissions(string repositoryName, string path)
        {
            ManagementObject repository = GetRepositoryObject(repositoryName);
            ManagementBaseObject inParameters = repository.GetMethodParameters("GetSecurity");
            inParameters["Path"] = path;
            ManagementBaseObject outParameters = repository.InvokeMethod("GetSecurity", inParameters, null);

            var permissions = new Dictionary<string, AccessLevel>();

            if (outParameters != null)
                foreach (ManagementBaseObject p in (ManagementBaseObject[])outParameters["Permissions"])
                {
                    var account = (ManagementBaseObject)p["Account"];
                    if (((ManagementBaseObject)p.Properties["Account"].Value).ClassPath.ClassName == "VisualSVN_Group")
                    {
                        var userName = "@" + (string)account["Name"];
                        var accessLevel = (AccessLevel)p["AccessLevel"];
                        permissions[userName] = accessLevel;
                    }
                    else
                    {
                        var userName = (string)account["Name"];
                        var accessLevel = (AccessLevel)p["AccessLevel"];
                        permissions[userName] = accessLevel;
                    }
                }

            return permissions;
        }

        /// <summary>  
        /// 添加仓库权限（给用户和用户组授权）  
        /// </summary>  
        /// <param name="Name">用户名或者组名，组名前添加"@"修饰符做区分</param>  
        /// <param name="repository">SVN仓库</param>  
        /// <param name="permission">权限码：0拒绝，1只读，2读写</param> 
        public static bool SetRepositoryPermission(string Name, string repository, int permission = 2)
        {
            return SetRepositoryPermission(new string[] { Name }, repository, permission);
        }

        /// <summary>
        /// 删除仓库权限（给用户和用户组授权） 
        /// </summary>
        /// <param name="uName">用户名或者组名，组名前添加"@"修饰符做区分</param>
        /// <param name="repository">SVN仓库</param>
        /// <returns></returns>
        public static bool DelRepositoryPermission(string Name, string repository)
        {
            return SetRepositoryPermission(new string[] { Name }, repository, 2, true);
        }


        /// <summary>  
        ///  设置仓库权限（给用户和用户组授权）  
        /// </summary>  
        /// <param name="userName">用户名或者组名，组名前添加"@"修饰符做区分</param>  
        /// <param name="repository">SVN仓库</param>  
        /// <param name="permission"> 权限码：0拒绝，1只读，2读写</param>  
        /// <param name="isDelete"> 删除标记，不为true时执行增加操作</param>  
        public static bool SetRepositoryPermission(string[] userName, string repository, int permission = 0, bool isDelete = false)
        {
            try
            {
                IDictionary<string, AccessLevel> permissions = GetPermissions(repository, "/");
                foreach (string s in userName)
                {
                    if (isDelete)
                    {
                        if (permissions.ContainsKey(s))
                        {
                            permissions.Remove(s);
                        }
                    }
                    else
                    {
                        if (permissions.ContainsKey(s))
                        {
                            // 修改权限
                            if (permissions[s] != (AccessLevel)permission)
                            {
                                permissions[s] = (AccessLevel)permission;
                            }
                        }
                        else
                        {
                            permissions.Add(s, (AccessLevel)permission);
                        }
                    }
                }
                SetPermissions(repository, "/", permissions);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>  
        /// 设置仓库权限  
        /// </summary>  
        /// <param name="repositoryName"></param>  
        /// <param name="path"></param>  
        /// <param name="permissions"></param> 
        private static void SetPermissions(string repositoryName, string path, IEnumerable<KeyValuePair<string, AccessLevel>> permissions)
        {
            ManagementObject repository = GetRepositoryObject(repositoryName);
            ManagementBaseObject inParameters = repository.GetMethodParameters("SetSecurity");
            inParameters["Path"] = path;
            IEnumerable<ManagementObject> permissionObjects = permissions.Select(p => GetPermissionObject(p.Key, p.Value));

            inParameters["Permissions"] = permissionObjects.ToArray();
            repository.InvokeMethod("SetSecurity", inParameters, null);
        }

        /// <summary>
        /// 读取权限实体
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="accessLevel"></param>
        /// <returns></returns>
        private static ManagementObject GetPermissionObject(string Name, AccessLevel accessLevel)
        {
            var accountClass = new ManagementClass("root\\VisualSVN",
                                                   "VisualSVN_User", null);
            var groupClass = new ManagementClass("root\\VisualSVN",
                                                   "VisualSVN_Group", null);
            var entryClass = new ManagementClass("root\\VisualSVN",
                                                 "VisualSVN_PermissionEntry", null);

            ManagementObject account = accountClass.CreateInstance();

            ManagementObject group = groupClass.CreateInstance();

            ManagementObject entry = entryClass.CreateInstance();

            // 是否为用户组的判断
            if (Name.IndexOf('@') == 0)
            {
                if (group != null) group["Name"] = Name.Replace("@", "");
                if (entry != null)
                {
                    entry["AccessLevel"] = accessLevel;
                    entry["Account"] = group;
                    return entry;
                }
            }
            else
            {
                if (account != null) account["Name"] = Name;
                if (entry != null)
                {
                    entry["AccessLevel"] = accessLevel;
                    entry["Account"] = account;
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// 创建用户组
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="user">用户列表</param>
        /// <returns></returns>
        public static bool CreatGroup(string groupName, string[] user = null)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_Group", null);
                ManagementBaseObject @params = svn.GetMethodParameters("Create");

                if (user != null)
                {
                    List<ManagementObject> member = new List<ManagementObject>();
                    foreach (var item in user)
                    {
                        member.Add(new ManagementObject("root\\VisualSVN", string.Format("VisualSVN_User.Name='{0}'", item), null));
                    }

                    @params["Name"] = groupName.Trim();
                    @params["Members"] = member.ToArray();
                }
                else
                {
                    @params["Name"] = groupName.Trim();
                    @params["Members"] = new object[] { };
                }

                svn.InvokeMethod("Create", @params, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>  
        /// 创建用户  
        /// </summary>  
        /// <param name="userName"></param>  
        /// <param name="password"></param>  
        /// <returns></returns>  
        public static bool CreateUser(string userName, string password)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_User", null);
                ManagementBaseObject @params = svn.GetMethodParameters("Create");
                @params["Name"] = userName.Trim();
                @params["Password"] = password.Trim();
                svn.InvokeMethod("Create", @params, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>  
        /// 创建仓库  
        /// </summary>  
        /// <param name="repoName"></param>  
        /// <returns></returns>  
        public static bool CreateRepository(string repoName)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_Repository", null);
                ManagementBaseObject @params = svn.GetMethodParameters("Create");
                @params["Name"] = repoName.Trim();
                svn.InvokeMethod("Create", @params, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 删除仓库
        /// </summary>
        /// <param name="repoName"></param>
        /// <returns></returns>
        public static bool DeleteRepository(string repoName)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_Repository", null);
                ManagementBaseObject @params = svn.GetMethodParameters("Delete");
                @params["Name"] = repoName.Trim();
                svn.InvokeMethod("Delete", @params, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>  
        /// 创建仓库内目录  
        /// </summary>  
        /// <param name="repositories"> </param>  
        /// <param name="name"></param>  
        /// <returns></returns>  
        public static bool CreateRepositoryFolders(string repositories, string[] name, string message = "")
        {
            try
            {
                var repository = new ManagementClass("root\\VisualSVN", "VisualSVN_Repository", null);
                ManagementObject repoObject = repository.CreateInstance();
                if (repoObject != null)
                {
                    repoObject.SetPropertyValue("Name", repositories);
                    ManagementBaseObject inParams = repository.GetMethodParameters("CreateFolders");
                    inParams["Folders"] = name;
                    inParams["Message"] = message;
                    repoObject.InvokeMethod("CreateFolders", inParams, null);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 删除仓库内目录
        /// </summary>
        /// <param name="repositories"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool DeleteRepositoryFolders(string repositories, string[] name, string message = "")
        {
            try
            {
                var repository = new ManagementClass("root\\VisualSVN", "VisualSVN_Repository", null);
                ManagementObject repoObject = repository.CreateInstance();
                if (repoObject != null)
                {
                    repoObject.SetPropertyValue("Name", repositories);
                    ManagementBaseObject inParams = repository.GetMethodParameters("DeleteFolders");
                    inParams["Folders"] = name;
                    inParams["Message"] = message;
                    repoObject.InvokeMethod("DeleteFolders", inParams, null);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 设置用户隶属用户组
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static bool AddMemberToGroup(string userName, string groupName)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_Group", null);
                ManagementObject instance = svn.CreateInstance();

                instance.SetPropertyValue("Name", groupName); //  过滤组名
                ManagementBaseObject setMember = instance.GetMethodParameters("SetMembers"); //获取 SetMembers 方法

                List<ManagementObject> Member = GetGroupUsersArr(groupName);

                // 添加本次新用户
                Member.Add(new ManagementObject("root\\VisualSVN", string.Format("VisualSVN_User.Name='{0}'", userName), null));

                setMember["Members"] = Member.ToArray();

                instance.InvokeMethod("SetMembers", setMember, null);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 从用户组内删除特定用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static bool DelMemberOnGroup(string userName, string groupName)
        {
            try
            {
                var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_Group", null);
                ManagementObject instance = svn.CreateInstance();

                instance.SetPropertyValue("Name", groupName); //  过滤组名
                ManagementBaseObject setMember = instance.GetMethodParameters("SetMembers"); //获取 SetMembers 方法

                List<ManagementObject> Member = GetGroupUsersArr(groupName, userName);


                setMember["Members"] = Member.ToArray();

                instance.InvokeMethod("SetMembers", setMember, null);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获得用户组内用户
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="excludedUser"></param>
        /// <returns></returns>
        public static List<ManagementObject> GetGroupUsersArr(string groupName, string excludedUser = "")
        {
            var svn = new ManagementClass("root\\VisualSVN", "VisualSVN_Group", null);

            ManagementObject instance = svn.CreateInstance();

            instance.SetPropertyValue("Name", groupName); //  过滤组名

            var nowGroupMembers = instance.InvokeMethod("GetMembers", null, null); //获得组的成员

            // 拉取已存在的组内用户信息
            List<ManagementObject> Member = new List<ManagementObject>();
            if (nowGroupMembers != null)
            {
                var members = nowGroupMembers["Members"] as ManagementBaseObject[];
                if (members != null)
                {
                    foreach (var member in members)
                    {
                        if (string.IsNullOrEmpty(excludedUser))
                        {
                            Member.Add(new ManagementObject("root\\VisualSVN", string.Format("VisualSVN_User.Name='{0}'", member["Name"]), null));
                        }
                        else
                        {
                            if ((string)member["Name"] != excludedUser)
                            {
                                Member.Add(new ManagementObject("root\\VisualSVN", string.Format("VisualSVN_User.Name='{0}'", member["Name"]), null));
                            }
                        }
                    }
                }
            }

            return Member;
        }
    }
}
