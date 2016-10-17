using System;
using System.IO;
using System.ServiceProcess;
using VisualSVN_Agent.utils;
using Newtonsoft.Json;
using System.Threading;

namespace VisualSVN_Agent
{
    public partial class MainAgent : ServiceBase
    {
        public MainAgent()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }
        Thread mainThread = new Thread(new ThreadStart(MainFunction.MainRunFunction));

        protected override void OnStart(string[] args)
        {
            // 基本变量写入配置
            ProgramSetting.ProgramInPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            ProgramSetting.ConfigFilePath = FileHelper.Combine(ProgramSetting.ProgramInPath, "config.json");

            LogHelper.WriteLog("程序所在路径："+ ProgramSetting.ProgramInPath, LogHelper.Log4NetLevel.Info);
            // 读取配置文件
            if (File.Exists(ProgramSetting.ConfigFilePath))
            {
                string response=File.ReadAllText(ProgramSetting.ConfigFilePath);
                var responseJson=JsonConvert.DeserializeObject<RootObject>(response);
                ProgramSetting.Repositoriespath = responseJson.Repositories.path;
                ProgramSetting.APIurl = responseJson.APIConfig.APIurl;
                ProgramSetting.IV = responseJson.APIConfig.IV;
                ProgramSetting.SecretKey = responseJson.APIConfig.SecretKey;
                ProgramSetting.svnAccount = responseJson.SVNAccount.UserName;
                ProgramSetting.svnPassword = responseJson.SVNAccount.Password;
                ProgramSetting.mID = responseJson.ID;
                LogHelper.WriteLog("配置读取完成！", LogHelper.Log4NetLevel.Info);
#if DEBUG
                MainFunction.MainRunFunction();
#else
                mainThread.Start();
#endif
                
            }
            else
            {
                LogHelper.WriteLog(" 未找到配置文件 config.json", LogHelper.Log4NetLevel.Fatal);
            }
            

           
        }

        protected override void OnStop()
        {
            LogHelper.WriteLog("服务停止中...", LogHelper.Log4NetLevel.Info);
            mainThread.Abort();
        }
    }
}
