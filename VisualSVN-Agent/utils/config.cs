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



    public class RootObject
    {
        public Repositories Repositories { get; set; }
    }

    public class Repositories
    {
        public string path { get; set; }
    }
}
