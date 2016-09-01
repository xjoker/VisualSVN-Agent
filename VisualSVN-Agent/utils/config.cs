using System.Collections.Generic;
namespace VisualSVN_Agent.utils
{
    //程序的配置存储
    public static class ProgramSetting
    {
        // VisualSVN软件仓库位置
        public static string Repositoriespath { get; set; }
        // 程序所在的路径
        public static string ProgramInPath { get; set; }
        // 配置文件位置
        public static string ConfigFilePath { get; set; }

    }

    /// <summary>
    /// 用户账号密码表
    /// </summary>
    public static class htpasswdUserAndPassword
    {
        public static Dictionary<string, htpasswdPassword> UsersTable { get; set; }
    }

    /// <summary>
    /// 用户的密码详细，包含加密模式、盐、密码hash
    /// </summary>
    public class htpasswdPassword
    {
        public string cryptMode { get; set; }
        public string salt { get; set; }
        public string passwordHash { get; set; }
    }


    /// <summary>
    /// 读取配置文件的方法
    /// </summary>
    public class RootObject
    {
        public Repositories Repositories { get; set; }
    }

    /// <summary>
    /// 配置文件内的库位置参数
    /// </summary>
    public class Repositories
    {
        public string path { get; set; }
    }
}
