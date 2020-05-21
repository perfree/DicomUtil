using Dicom;
using Dicom.Imaging;
using Dicom.IO.Buffer;
using Dicom.Network.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DcmUtil
{
    class Program
    {
        [Obsolete]
        static async Task Main(string[] args)
        {
            // 如果未传参数，则直接返回
            if (args.Length <= 1) {
                Console.WriteLine("请传入参数，如：");
                Console.WriteLine("img2dcm \"{patientName:'xxx',patientID:'123',images:['1.jpg','2.jpg'],outDcm:'1.dcm'}\"");
                Console.WriteLine("dcm2jpg \"{'dcmPath':'E:/test/images/3.dcm','outJpgDir':'E:/test/images/outJpg'}\"");
                Console.WriteLine("dcmScu \"{'ip':'127.0.0.1','port':4242,'dcmPath':'E:/test/images/3.dcm'}\"");
                Console.ReadKey();
                return;
            }
            Console.WriteLine(args[1]);
            // 解析命令行参数


            // 图片转dcm
            if (args[0].Equals("img2dcm"))
            { 
                Img2DcmOptions options = JsonConvert.DeserializeObject<Img2DcmOptions>(args[1]);
                Image2dcm(options);
            }

            // dcm转图片
            if (args[0].Equals("dcm2jpg"))
            { 
                Dcm2JpgOptions options = JsonConvert.DeserializeObject<Dcm2JpgOptions>(args[1]);
                Dcm2Jpg(options);
            }

            // dcm转图片
            if (args[0].Equals("dcmScu"))
            {
                DcmScuOptions options = JsonConvert.DeserializeObject<DcmScuOptions>(args[1]);
                await DcmScu(options);
            }
        }

        /// <summary>
        /// 获取图片字节数组
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] GetPixels(Bitmap bitmap){
            // 将jpg转换为bmp，如直接是bmp，则不需要进行该步骤
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            bitmap = new Bitmap(bitmap.Width, bitmap.Height, data.Stride, PixelFormat.Format16bppArgb1555, data.Scan0);


            byte[] bytes = new byte[bitmap.Width * bitmap.Height * 3];
            int wide = bitmap.Width;
            int i = 0;
            int height = bitmap.Height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < wide; x++)
                {
                    var srcColor = bitmap.GetPixel(x, y);
                    bytes[i] = srcColor.R;
                    i++;
                    bytes[i] = srcColor.G;
                    i++;
                    bytes[i] = srcColor.B;
                    i++;
                }
            }
            return bytes;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// 图片转换dcm
        /// </summary>
        /// <param name="file"></param>
        public static void Image2dcm(Img2DcmOptions options)
        {
            if (options.images.Count <= 0) {
                Console.WriteLine("请传入图片路径");
                return;
            }
            // 取第一张图片作为基准图片
            Bitmap bitmap = new Bitmap(@options.images[0]);
            byte[] pixels = GetPixels(bitmap);
            MemoryByteBuffer buffer = new MemoryByteBuffer(pixels);
            DicomDataset dataset = new DicomDataset();
            dataset.Add(DicomTag.SpecificCharacterSet, "GB18030");
            // 写入tag数据
            dataset.Add(DicomTag.PhotometricInterpretation, PhotometricInterpretation.Rgb.Value);
            dataset.Add(DicomTag.Rows, (ushort)bitmap.Height);
            dataset.Add(DicomTag.Columns, (ushort)bitmap.Width);
            dataset.Add(DicomTag.BitsAllocated, (ushort)8);
            dataset.Add(DicomTag.SOPClassUID, "1.2.840.10008.5.1.4.1.1.2");
            dataset.Add(DicomTag.SOPInstanceUID, "1.2.840.10008.5.1.4.1.1.2." + GetTimeStamp());
            dataset.Add(DicomTag.PatientName, Encoding.Default, string.IsNullOrEmpty(options.patientName) ? "test" : options.patientName);
            dataset.Add(DicomTag.PatientID, string.IsNullOrEmpty(options.patientID) ? Guid.NewGuid().ToString("N") : options.patientID);
            dataset.Add(DicomTag.StudyInstanceUID, "1.2.3.4.5.6.7.8.9.11." + GetTimeStamp());
            dataset.Add(DicomTag.StudyDate, DateTime.Now.ToString("yyyyMMdd"));
            dataset.Add(DicomTag.StudyTime, DateTime.Now.ToString("HHmmss"));
            dataset.Add(DicomTag.StudyID, GetTimeStamp());
            dataset.Add(DicomTag.Modality, "CT");
            dataset.Add(DicomTag.SeriesInstanceUID, "1.2.3.4.5.6.7.8.9.11." + GetTimeStamp());
            dataset.Add(DicomTag.InstanceNumber, "1000");

            DicomPixelData pixelData = DicomPixelData.Create(dataset, true);
            pixelData.BitsStored = 8;
            pixelData.SamplesPerPixel = 3;
            pixelData.HighBit = 7;
            pixelData.PixelRepresentation = 0;
            pixelData.PlanarConfiguration = 0;
            pixelData.AddFrame(buffer);

            // 如果图片大于等于两张，则继续追加
            if (options.images.Count >= 2) {
                for (var i = 1; i < options.images.Count; i++) {
                    Bitmap addBit = new Bitmap(@options.images[i]);
                    byte[] addPixels = GetPixels(addBit);
                    MemoryByteBuffer addBuffer = new MemoryByteBuffer(addPixels);
                    pixelData.AddFrame(addBuffer);
                }
            }
           
            // 保存dcm文件
            DicomFile dicomfile = new DicomFile(dataset);
            dicomfile.Save(@options.outDcm);
            Console.WriteLine("success：jpg转dcm成功");
        }

        /// <summary>
        /// dcm转图片
        /// </summary>
        [Obsolete]
        public static void Dcm2Jpg(Dcm2JpgOptions options) {
            var file = DicomFile.Open(@options.dcmPath);
            var image = new DicomImage(file.Dataset);
            var patientid = file.Dataset.Get<string>(DicomTag.PatientID);
            if (string.IsNullOrEmpty(patientid)) {
                patientid = GetTimeStamp();
            }
            int x = file.Dataset.Get<int>(DicomTag.NumberOfFrames);
            for (var i = 0; i < x; i++) {
                var fileName = options.outJpgDir + "/" + patientid + "_" + (i + 1) + ".jpg";
                image.RenderImage(i).AsBitmap().Save(@fileName);
            }
            Console.WriteLine("success: dcm转jpg成功");
        }

        /// <summary>
        /// dcm上传
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [Obsolete]
        async public static 
        Task
        DcmScu(DcmScuOptions options) {
            var client = new DicomClient(options.ip, options.port, false, "SCU", "ANY-SCP");
            await client.AddRequestAsync(new Dicom.Network.DicomCStoreRequest(@options.dcmPath));
            await client.SendAsync();
            Console.WriteLine("success: dcm文件上传成功");
        }
    }
}
