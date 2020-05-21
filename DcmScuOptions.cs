using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DcmUtil
{
    class DcmScuOptions
    {

        /// <summary>
        /// ip地址
        /// </summary>
        public string ip { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int port { get; set; }
        /// <summary>
        /// dcm路径
        /// </summary>
        public string dcmPath { get; set; }
    }
}
