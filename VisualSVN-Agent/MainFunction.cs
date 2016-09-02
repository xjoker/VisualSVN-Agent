using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using VisualSVN_Agent.utils;

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
            var responseJson = JsonConvert.SerializeObject(bbb);

            var aada = EncryptsAndDecryptsHelper.Encrypt(responseJson,ProgramSetting.SecretKey,ProgramSetting.AccessKey);

            var dfdfd = EncryptsAndDecryptsHelper.Decrypt(Convert.ToString(aada), ProgramSetting.SecretKey, ProgramSetting.AccessKey);

            Console.WriteLine("...");

        }

        
    }
}
