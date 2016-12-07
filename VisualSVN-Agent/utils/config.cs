using System.Collections.Generic;
namespace VisualSVN_Agent.utils
{

    //程序的配置存储
    public static class ProgramSetting
    {
        public static string  mID { get; set; }
        // VisualSVN软件仓库位置
        public static string Repositoriespath { get; set; }
        // 程序所在的路径
        public static string ProgramInPath { get; set; }
        // 配置文件位置
        public static string ConfigFilePath { get; set; }
        // API地址
        public static string APIurl { get; set; }
        // cmd地址
        public static string CMDurl { get; set; }
        // API 加密密钥
        public static string IV { get; set; }
        public static string SecretKey { get; set; }
        public static string svnAccount { get; set; }

        public static string svnPassword { get; set; }
        public static string SitePrefix { get; set; }
    }



    /// <summary>
    /// 用于存储用户和用户组的关系
    /// </summary>
    public class groupList
    {
        static groupList()
        {
            userGroup = new Dictionary<string, List<string>>();
        }
        public static Dictionary<string, List<string>> userGroup;
    }


    /// <summary>
    /// 用户账号密码表
    /// </summary>userGroup
    public class htpasswdUserAndPassword
    {
        static htpasswdUserAndPassword()
        {
            UsersTable = new Dictionary<string, htpasswdPassword>();
        }
        public static Dictionary<string, htpasswdPassword> UsersTable { get; set; }
    }

    /// <summary>
    /// 用户的密码详细，包含加密模式、盐、密码hash
    /// </summary>
    public class htpasswdPassword
    {
        public string userGroup { get; set; }
        public string cryptMode { get; set; }
        public string salt { get; set; }
        public string passwordHash { get; set; }
    }


    /// <summary>
    /// 存储仓库名称与对应的权限表
    /// </summary>
    public class RepoDataSourcePermission
    {

        static RepoDataSourcePermission()
        {
            RepoPermissons = new Dictionary<string, RepoAccessPermisson>();
        }
        public static Dictionary<string, RepoAccessPermisson> RepoPermissons { get; set; }

    }

    /// <summary>
    /// 存储仓库内用户/用户组对应的权限
    /// </summary>
    public class RepoAccessPermisson
    {
        public RepoAccessPermisson()
        {
            Permission = new Dictionary<string, RepoAccessPermissionDetails>();
        }
        public Dictionary<string, RepoAccessPermissionDetails> Permission { get; set; }
    }

    /// <summary>
    /// 每一个用户/用户组对应的权限详细
    /// </summary>
    public class RepoAccessPermissionDetails
    {
        public bool IsGroup { get; set; }
        public bool NoAccess { get; set; }
        public bool Read { get; set; }
        public bool Write { get; set; }
    }

    // config.json 配置文件
    /////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 读取配置文件的方法
    /// </summary>
    public class RootObject
    {
        public Repositories Repositories { get; set; }
        public APIConfig APIConfig { get; set; }
        public string ID { get; set; }
        public SVNAccount SVNAccount { get; set; }
    }

    /// <summary>
    /// 配置文件内的库位置参数
    /// </summary>
    public class Repositories
    {
        public string path { get; set; }
        public string SitePrefix { get; set; }
    }

    public class APIConfig
    {
        public string APIurl { get; set; }
        public string CMDurl { get; set; }
        public string IV { get; set; }
        public string SecretKey { get; set; }
    }

    public class SVNAccount
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }




    /////////////////////////////////////////////////////////////////////////////////////////////////
}
