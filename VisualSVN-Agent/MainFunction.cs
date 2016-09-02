using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualSVN_Agent.utils;

namespace VisualSVN_Agent
{
    class MainFunction
    {
        public static void MainRunFunction()
        {   
            
            string[] file = FileHelper.GetFileList(ProgramSetting.Repositoriespath, "VisualSVN-SvnAuthz.ini", SearchOption.AllDirectories);
            VirtualSVNHelper.SVNHelper.ReadRepositories();
            var bbb = RepoDataSourcePermission.RepoPermissons;
            Console.WriteLine("...");

        }

        
    }
}
