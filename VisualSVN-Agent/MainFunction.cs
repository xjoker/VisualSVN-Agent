using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using VisualSVN_Agent.utils;
using VisualSVN_Agent.Model;

namespace VisualSVN_Agent
{
    class MainFunction
    {
        public static void MainRunFunction()
        {   
            
            string[] file = FileHelper.GetFileList(ProgramSetting.Repositoriespath, "VisualSVN-SvnAuthz.ini", SearchOption.AllDirectories);
            VirtualSVNHelper.SVNHelper.ReadRepositoriesAll();
            var bbb = RepoDataSourcePermission.RepoPermissons;
            FileWatcherHelper fwh = new FileWatcherHelper();
            fwh.WatcherStrat(@"C:\Repositories", "VisualSVN-SvnAuthz.ini");
            var requestJson = JsonConvert.SerializeObject(bbb);

            JsonPostModel jpm = new JsonPostModel();
            jpm.Agent = JsonPostModelAgent.Client.ToString();
            jpm.DataType = JsonPostModelDataType.AllUserTable.ToString();
            jpm.Data = bbb;
            string output = JsonConvert.SerializeObject(jpm);

            var aada = EncryptsAndDecryptsHelper.Encrypt(output, ProgramSetting.SecretKey);

            var dfdfd = EncryptsAndDecryptsHelper.Decrypt(aada, ProgramSetting.SecretKey);

            var basdf=WebFunctionHelper.PostToAPI(aada, ProgramSetting.APIurl);

            Console.WriteLine("...");

        }

        
    }
}
