using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Emgu.CV;
using Emgu.CV.Structure;
using Point = Autodesk.DesignScript.Geometry.Point;

/*
 * Mat-Bitmap转换器
 */
namespace ImageProc
{
    public class Converter 
    {
        private Converter(){}

        /// <summary>
        /// 利用Emgu的Image转换构造函数将bitmap转换为mat
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Mat BitmapToMat(Bitmap bitmap)
        {
            Image<Bgr, Byte> currentFrame = new Image<Bgr, Byte>(bitmap);
            Mat invert = new Mat();
            CvInvoke.BitwiseAnd(currentFrame, currentFrame, invert);
            return invert;
        }

        /// <summary>
        /// Mat转换为BitMap
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Bitmap MatToBitmap(Mat mat)
        {
            return mat.Bitmap;
        }


    }
}
