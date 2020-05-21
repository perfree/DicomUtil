using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DcmUtil
{
    class Dcm2JpgOptions
    {

        /// <summary>
        /// dcm路径
        /// </summary>
        public string dcmPath { get; set; }
        /// <summary>
        /// jpg图片输出目录
        /// </summary>
        public string outJpgDir { get; set; }
    }
}
