using System.Drawing;
using Emgu.CV;
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
    }
}
