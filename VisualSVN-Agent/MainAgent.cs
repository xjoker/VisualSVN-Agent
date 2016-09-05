using System;
using System.IO;
using System.ServiceProcess;
using VisualSVN_Agent.utils;
using Newtonsoft.Json;

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


        protected override void OnStart(string[] args)
        {
            // 基本变量写入配置
            ProgramSetting.ProgramInPath = Environment.CurrentDirectory;
            ProgramSetting.ConfigFilePath = FileHelper.Combine(Environment.CurrentDirectory, "config.json");

            // 读取配置文件
            if (File.Exists(ProgramSetting.ConfigFilePath))
            {
                string response=File.ReadAllText(ProgramSetting.ConfigFilePath);
                var responseJson=JsonConvert.DeserializeObject<RootObject>(response);
                ProgramSetting.Repositoriespath = responseJson.Repositories.path;
                ProgramSetting.APIurl = responseJson.APIConfig.APIurl;
                ProgramSetting.IV = responseJson.APIConfig.IV;
                ProgramSetting.SecretKey = responseJson.APIConfig.SecretKey;
                MainFunction.MainRunFunction();
            }
            else
            {
                LogHelper.WriteLog(" 未找到配置文件 config.json", LogHelper.Log4NetLevel.Fatal);
            }
            

           
        }

        protected override void OnStop()
        {
            
        }
    }
}
