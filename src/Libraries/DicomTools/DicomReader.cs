using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
//from fo-Dicom
using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using Dicom.Media;
using Emgu.CV;
using Emgu.CV.Structure;

/*
 * 用于读取Dicom图片以及处理一些DICOM文件的相关操作
 */
namespace DicomTools
{
    public class DicomReader
    {
        /// <summary>
        /// 关闭合成构造方法
        /// </summary>
        private DicomReader(){}
        private DicomFile _file;
        /// <summary>
        /// 导出函数
        /// 
        /// 输入文件路径,返回Mat对象
        ///     前期采用方案1,后期换成方案3
        /// 方案1:固定窗宽窗位,全部动态范围
        /// 方案2:使用多输入值节点,用int给出窗宽窗位
        /// 方案3:使用自定义UI节点,通过自带控件直接给出窗宽窗位
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static Mat ReadDicomFromPath(string filepath)
        {
            DicomFile _file = DicomFile.Open(filepath);
            bool _grayscale;
            double _windowWidth;
            double _windowCenter;

            DicomImage _image = new DicomImage(_file.Dataset);
            _image.WindowWidth = 4096;
            
            _image.WindowCenter = 1024;
            _grayscale = !_image.PhotometricInterpretation.IsColor;
            if (_grayscale)
            {
                _windowWidth = _image.WindowWidth;
                _windowCenter = _image.WindowCenter;
            }
            Image<Bgr, Byte> currentFrame = new Image<Bgr, byte>((_image.RenderImage(0)).AsBitmap());
            Mat invert = new Mat();
            CvInvoke.BitwiseAnd(currentFrame, currentFrame, invert);
            return invert;

        }
    }
}
