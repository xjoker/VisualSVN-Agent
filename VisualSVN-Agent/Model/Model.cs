using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSVN_Agent.Model
{
    /*
        {
            "Agent":"Server/Client",
            "Time":"UNIXtime",
            "DataType":"AllUserTable/NewRepo/DelRepo/AllAuthInfo/Other",
            "Data":"内容"
        }
    */


    /// <summary>
    /// 标准发送模型
    /// </summary>
    public class JsonPostModel
    {
        public string Agent { get; set; }
        private uint time;
        public uint Time {
            get
            {
                uint _nowTimestamp = (uint)((DateTime.UtcNow.Ticks - DateTime.Parse("1970-01-01 00:00:00").Ticks) / 10000000);
                return _nowTimestamp;
            }
            set
            {
                this.time = value;
            }
        }
        public string DataType { get; set; }
        public Object Data { get; set; }

    }

    public class JsonResponse
    {
        public string status { get; set; }

    }

    /// <summary>
    /// 删除repo时用到的属性
    /// </summary>
    public class DelRepo
    {
        public string RepoName { get; set; }
    }


    /// <summary>
    /// 消息类型
    /// </summary>
    public enum JsonPostModelDataType
    {
        AllUserTable,
        NewRepo,
        DelRepo,
        ChangeRepo,
        AllAuthInfo,
        Other
    }


    /// <summary>
    /// 客户端 or 服务端
    /// </summary>
    public enum JsonPostModelAgent
    {
        Server,
        Client
    }



}
