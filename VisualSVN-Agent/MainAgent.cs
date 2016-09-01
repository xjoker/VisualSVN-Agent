using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using VisualSVN_Agent.utils;

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
            
           
        }

        protected override void OnStop()
        {
        }
    }
}
