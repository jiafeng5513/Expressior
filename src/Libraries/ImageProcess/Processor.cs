using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Autodesk.DesignScript.Runtime;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Color = DSCore.Color;

/*
 * 内置节点API
 * 1.public函数可见，标记为 [IsVisibleInDynamoLibrary(false)]的不可见。
 * 2.非静态方法需要一个额外的参数this
 * 3.静态构造器在构造器组里面显示，图标是加号
 * 4.普通静态函数在普通方法中显示，图标是笔
 * 5.layoutSpecs.json引用路径：dll名.命名空间名.类名.方法名
 * 6.添加图标：对应的resx中，嵌入，
 *      命名规则：类名.方法名.Large/类名.方法名.Small
 *      大图标：96×96像素，位深32，Alpha,png
 *      小图标：32×32像素，位深32，Alpna,png
 */
namespace ImageProc
{
    /// <summary>
    /// Mat遍历辅助类,需要标记为不可见
    /// </summary>
    public static class MatExtension
    {
        [IsVisibleInDynamoLibrary(false)]
        public static dynamic GetValue(this Mat mat, int row, int col)
        {
            var value = CreateElement(mat.Depth);
            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
            return value[0];
        }
        [IsVisibleInDynamoLibrary(false)]
        public static void SetValue(this Mat mat, int row, int col, dynamic value)
        {
            var target = CreateElement(mat.Depth, value);
            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }
        private static dynamic CreateElement(DepthType depthType, dynamic value)
        {
            var element = CreateElement(depthType);
            element[0] = value;
            return element;
        }

        private static dynamic CreateElement(DepthType depthType)
        {
            if (depthType == DepthType.Cv8S)
            {
                return new sbyte[1];
            }
            if (depthType == DepthType.Cv8U)
            {
                return new byte[1];
            }
            if (depthType == DepthType.Cv16S)
            {
                return new short[1];
            }
            if (depthType == DepthType.Cv16U)
            {
                return new ushort[1];
            }
            if (depthType == DepthType.Cv32S)
            {
                return new int[1];
            }
            if (depthType == DepthType.Cv32F)
            {
                return new float[1];
            }
            if (depthType == DepthType.Cv64F)
            {
                return new double[1];
            }
            return new float[1];
        }
    }
    /// <summary>
    /// 轮廓对象
    /// </summary>
    struct ContourItem
    {
        [IsVisibleInDynamoLibrary(false)]
        public VectorOfPoint vp;//轮廓点集
        [IsVisibleInDynamoLibrary(false)]
        public double area;     //面积
    }
    public class Processor 
    {
        /// <summary>
        /// 关闭合成构造方法
        /// </summary>
        private Processor(){}
        /// <summary>
        /// sobel边缘检测
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Mat Sobel(Mat mat)
        {
            Mat outMat=new Mat();
            CvInvoke.Canny(mat, outMat, 90, 120, 3);
            return outMat;
        }
        /// <summary>
        /// 高斯模糊
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static Mat GaussianBlur(Mat mat)
        {
            Mat outMat = new Mat();
            CvInvoke.GaussianBlur(mat, outMat,new Size(3,3),0.5,0.5);
            return outMat;
        }
        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="inMat"></param>
        /// <param name="Threshold"></param>
        /// <returns></returns>
        public static Mat Binnary(Mat inMat,double Threshold,bool reverse=true)
        {
            Mat outMat =new Mat();
            if (reverse)
            {
                CvInvoke.Threshold(inMat, outMat, Threshold, 255.0, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            }
            else
            {
                CvInvoke.Threshold(inMat, outMat, Threshold, 255.0, Emgu.CV.CvEnum.ThresholdType.Binary);
            }
            return outMat;
        }


        /// <summary>
        /// 形态学:腐蚀
        /// </summary>
        /// <param name="inMat">输入图片:Mat</param>
        /// <param name="Size">内核尺寸:int</param>
        /// <returns></returns>
        public static Mat Erode(Mat inMat, int Size)
        {
            Mat outMat = new Mat();
            //构造内核
            int KernelSize = Math.Max(3, Size);
            int anchorPoint = (KernelSize % 2 == 0 ? KernelSize : KernelSize + 1) / 2;
            Mat StructingElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize),
                new Point(anchorPoint, anchorPoint));
            CvInvoke.Erode(inMat,
                outMat,
                StructingElement,
                new Point(-1, -1),
                1,
                BorderType.Default,
                new MCvScalar(0));
            return outMat;
        }

        /// <summary>
        /// 形态学:膨胀
        /// </summary>
        /// <param name="inMat">输入图片:Mat</param>
        /// <param name="Size">内核尺寸:int</param>
        /// <returns></returns>
        public static Mat Dilate(Mat inMat, int Size)
        {
            Mat outMat = new Mat();
            //构造内核
            int KernelSize = Math.Max(3, Size);
            int anchorPoint = (KernelSize % 2 == 0 ? KernelSize : KernelSize + 1) / 2;
            Mat StructingElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize),
                new Point(anchorPoint, anchorPoint));
            CvInvoke.Dilate(inMat, outMat, StructingElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

            return outMat;
        }


        /// <summary>
        /// 形态学:开
        /// </summary>
        /// <param name="inMat">输入图片:Mat</param>
        /// <param name="Size">内核尺寸:int</param>
        /// <returns></returns>
        public static Mat Open(Mat inMat, int Size)
        {
            Mat outMat = new Mat();
            Mat intermMat = new Mat();
            //构造内核
            int KernelSize = Math.Max(3, Size);
            int anchorPoint = (KernelSize % 2 == 0 ? KernelSize : KernelSize + 1) / 2;
            Mat StructingElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize),
                new Point(anchorPoint, anchorPoint));
            //先腐蚀
            CvInvoke.Erode(inMat, intermMat, StructingElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            //再膨胀        
            CvInvoke.Dilate(intermMat, outMat, StructingElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

            return outMat;
        }

        /// <summary>
        /// 形态学:闭
        /// </summary>
        /// <param name="inMat">输入图片:Mat</param>
        /// <param name="Size">内核尺寸:int</param>
        /// <returns></returns>
        public static Mat Close(Mat inMat, int Size)
        {
            Mat outMat = new Mat();
            Mat intermMat = new Mat();
            //构造内核
            int KernelSize = Math.Max(3, Size);
            int anchorPoint = (KernelSize % 2 == 0 ? KernelSize : KernelSize + 1) / 2;
            Mat StructingElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize),
                new Point(anchorPoint, anchorPoint));
            //先膨胀        
            CvInvoke.Dilate(inMat, intermMat, StructingElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            //再腐蚀
            CvInvoke.Erode(intermMat, outMat, StructingElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
            return outMat;
        }

        /// <summary>
        /// 保留最大联通
        /// </summary>
        /// <param name="inMat">输入图片:Mat</param>
        /// <param name="Num">要保留的最大区域数量</param>
        /// <returns></returns>
        public static Mat MaxRegion(Mat inMat, int Num)
        {
            Image<Bgr, byte> src = new Image<Bgr, byte>(inMat.Size);
            src = inMat.ToImage<Bgr, byte>();

            Image<Gray, byte> edges = new Image<Gray, byte>(src.Width, src.Height);
            Image<Gray, byte> hierarchy = new Image<Gray, byte>(src.Width, src.Height);

            CvInvoke.Canny(src, edges, 100, 60);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            //轮廓寻找
            CvInvoke.FindContours(edges, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxNone);
            //轮廓检出到数组,计算面积准备排序
            ContourItem[] ContourArray = new ContourItem[contours.Size];
            for (int i = 0; i < contours.Size; i++)
            {
                ContourArray[i].vp = contours[i];
                ContourArray[i].area = CvInvoke.ContourArea(ContourArray[i].vp);
            }
            //排序
            for (int i = 1; i < ContourArray.Length - 1; i++)
            {
                bool flag = true;
                for (int j = 0; j < ContourArray.Length - 1 - i; j++)
                {
                    if (ContourArray[j].area < ContourArray[j + 1].area)
                    {
                        ContourItem temp = ContourArray[j];
                        ContourArray[j] = ContourArray[j + 1];
                        ContourArray[j + 1] = temp;
                        flag = false;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            //保留前N个轮廓,把剩下的填充
            if (ContourArray.Length == contours.Size)
            {
                Console.WriteLine("相等" + contours.Size);
            }

            for (int i = Num; i < ContourArray.Length; i++)
            {
                //只填充面积>0的轮廓
                if (ContourArray[i].area > 0)
                {
                    VectorOfVectorOfPoint tempVectorOfPoint = new VectorOfVectorOfPoint();
                    tempVectorOfPoint.Push(ContourArray[i].vp);
                    CvInvoke.FillPoly(src, tempVectorOfPoint, new MCvScalar(255, 0, 255, 255));
                }
            }
            return src.Mat;
        }

        /// <summary>
        /// 移除与图像边缘联通的区域
        /// </summary>
        /// <param name="inMat">输入图片:Mat</param>
        /// <param name="color">填充颜色</param>
        /// <returns></returns>
        public static Mat ClearBorder(Mat inMat,Color color)
        {
            Image<Gray, byte> src = new Image<Gray, byte>(inMat.Size);
            src = inMat.ToImage<Gray, byte>();
            Image<Gray, byte> cannyOut = new Image<Gray, byte>(src.Size);
            CvInvoke.Canny(src, cannyOut, 5, 5 * 3);
            //边缘延拓
            Image<Gray, byte> mask = new Image<Gray, byte>(new Size(src.Width + 2, src.Height + 2));
            CvInvoke.cvSetImageROI(mask, new Rectangle(1, 1, src.Width, src.Height));
            cannyOut.CopyTo(mask);
            CvInvoke.cvResetImageROI(mask);

            Rectangle rect = new Rectangle();
            CvInvoke.FloodFill(
                src,                            //1 原图像
                mask,                           //2 掩码
                new Point(1, 1),               //3 种子点
                new MCvScalar(color.Blue, color.Green, color.Red),   //4 填充颜色值
                out rect,                       //5
                new MCvScalar(25, 25, 25),      //6
                new MCvScalar(0, 0, 0),         //7
                Connectivity.EightConnected,    //8 连通性设置
                FloodFillType.FixedRange        //9 
            );

            src = src - cannyOut;
            return src.Mat;
        }
        /*
         * dcm file name ->Mat
         * 二值化
         *
         * clear_border
         *
         * label
         *
         * 保留两个最大联通
         *
         * 半径为2的腐蚀
         *
         * 半径为10的闭
         *
         * 填充小洞
         *
         * 掩码提取
         */
        
    }//end of class
}//end of namespace
