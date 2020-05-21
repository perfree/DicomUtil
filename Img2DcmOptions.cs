using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DcmUtil
{
    class Img2DcmOptions
    {
        /// <summary>
        /// 患者名
        /// </summary>
        public string patientName { get; set; }
        /// <summary>
        /// 患者ID
        /// </summary>
        public string patientID { get; set; }
        /// <summary>
        /// dcm输出文件路径及名称
        /// </summary>
        public string outDcm { get; set; }
        /// <summary>
        /// 图片集合
        /// </summary>
        public List<String> images { get; set; }
    }
}
