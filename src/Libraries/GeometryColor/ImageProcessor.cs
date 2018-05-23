using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Analysis;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using GeometryColor.Properties;
using Dynamo.Graph.Nodes;
using Emgu.CV;
using Emgu.CV.Structure;
using Color = DSCore.Color;
using Math = DSCore.Math;
using Point = Autodesk.DesignScript.Geometry.Point;
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
namespace Modifiers
{
    public class ImageProcessor :  IGraphicItem
    {
        #region private members

        private readonly Point[] vertices;
        private readonly Geometry geometry;
        private readonly Color singleColor;
        private readonly Color[][] colorMap;
        private readonly Color[] meshVertexColors;

        #endregion

        #region private constructors

        private ImageProcessor(Geometry geometry, Color color)
        {
            this.geometry = geometry;
            this.singleColor = color;
        }

        private ImageProcessor(Surface surface, Color[][] colors)
        {
            geometry = surface;

            // Transpose the colors array. This is required
            // to correctly align the colors on the surface with
            // the UV space of the surface.

            var rows = colors.GetLength(0);
            var columns = colors[0].Count();

            colorMap = new Color[columns][];
            for (var c = 0; c < columns; c++)
            {
                colorMap[c] = new Color[rows];
            }

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    colorMap[j][i] = colors[i][j];
                }
            } 
        }

        private ImageProcessor(Point[] vertices, Color[] colors)
        {
            this.vertices = vertices;
            meshVertexColors = colors;
        }

        #endregion

        #region static constructors

        /// <summary>
        /// Display geometry using a color.
        /// 参考价值：如何加载属于这个模块的图标
        /// </summary>
        /// <param name="geometry">The geometry to which you would like to apply color.</param>
        /// <param name="color">The color.</param>
        /// <returns>A Display object.</returns>
        public static ImageProcessor ByGeometryColor([KeepReferenceAttribute]Geometry geometry, Color color)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (color == null)
            {
                throw new ArgumentNullException("color");
            }

            return new ImageProcessor(geometry, color);
        }

        public static ImageProcessor Test(Point[] vertices, Color[] colors)
        {
            return new ImageProcessor(vertices, colors);
        }

        /// <summary>
        /// Display color values on a surface.
        /// 参考价值：[KeepReferenceAttribute]
        ///           [DefaultArgument]
        /// The colors provided are converted internally to an image texture which is
        /// mapped to the surface. 
        /// </summary>
        /// <param name="surface">The surface on which to apply the colors.
        /// </param>
        /// <param name="colors">A two dimensional list of Colors.
        /// 
        /// The list of colors must be square. Attempting to pass a jagged array
        /// will result in an exception. </param>
        /// <returns>A Display object.</returns>
        public static ImageProcessor BySurfaceColors([KeepReferenceAttribute]Surface surface,
            [DefaultArgument("{{Color.ByARGB(255,255,0,0),Color.ByARGB(255,255,255,0)},{Color.ByARGB(255,0,255,255),Color.ByARGB(255,0,0,255)}};")] Color[][] colors)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (colors == null)
            {
                throw new ArgumentNullException("colors");
            }

            if (!colors.Any())
            {
                throw new ArgumentException(Resources.NoColorsExceptionMessage);
            }

            if (colors.Length == 1)
            {
                throw new ArgumentException(Resources.TwoDimensionalListExceptionMessage);
            }

            var size = colors[0].Count();
            foreach (var list in colors)
            {
                if (list.Count() != size)
                {
                    throw new ArgumentException(Resources.JaggedListExceptionMessage);
                }
            }

            return new ImageProcessor(surface, colors);
        }

        /// <summary>
        /// 利用Emgu的Image转换构造函数将bitmap转换为mat
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Mat Bitmap2Mat(Bitmap bitmap)
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
        public static Bitmap Mat2Bitmap(Mat mat)
        {
            return mat.Bitmap;
        }
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

        #endregion

        #region public methods

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            if(vertices != null)
            {
                CreateVertexColoredMesh(vertices, meshVertexColors, package, parameters);
                return;
            }

            if (singleColor != null)
            {
                CreateGeometryRenderData(singleColor, package, parameters);
                return;
            }

            if (colorMap != null)
            {
                if (!colorMap.Any())
                {
                    return;
                }

                CreateColorMapOnSurface(colorMap, package, parameters);
                return;
            }
        }

        public override string ToString()
        {
            return string.Format("ImageProcessor" + "(Geometry = {0}, Appearance = {1})", geometry, singleColor != null ? singleColor.ToString() : "Multiple colors.");
        }

        #endregion

        #region private helper methods

        private void CreateColorMapOnSurface(Color[][] colorMap , IRenderPackage package, TessellationParameters parameters)
        {
            const byte gray = 80;
 
            geometry.Tessellate(package, parameters);

            var colorBytes = new List<byte>();

            foreach (var colorArr in colorMap)
            {
                foreach (var c in colorArr)
                {
                    colorBytes.Add(c.Blue);
                    colorBytes.Add(c.Green);
                    colorBytes.Add(c.Red);
                    colorBytes.Add(c.Alpha);
                }
            }

            package.SetColors(colorBytes.ToArray());
            package.ColorsStride = colorMap.First().Length * 4;

            TessellateEdges(package, parameters);

            if (package.LineVertexCount > 0)
            {
                package.ApplyLineVertexColors(CreateColorByteArrayOfSize(package.LineVertexCount, gray, gray,
                    gray, 255));
            }
        }

        private void CreateGeometryRenderData(Color color, IRenderPackage package, TessellationParameters parameters)
        {
            package.RequiresPerVertexColoration = true;

            // As you add more data to the render package, you need
            // to keep track of the index where this coloration will 
            // start from.

            geometry.Tessellate(package, parameters);

            TessellateEdges(package, parameters);

            if (package.LineVertexCount > 0)
            {
                package.ApplyLineVertexColors(CreateColorByteArrayOfSize(package.LineVertexCount, color.Red, color.Green,
                    color.Blue, color.Alpha));
            }

            if (package.PointVertexCount > 0)
            {
                package.ApplyPointVertexColors(CreateColorByteArrayOfSize(package.PointVertexCount, color.Red, color.Green,
                    color.Blue, color.Alpha));
            }

            if (package.MeshVertexCount > 0)
            {
                package.ApplyMeshVertexColors(CreateColorByteArrayOfSize(package.MeshVertexCount, color.Red, color.Green,
                    color.Blue, color.Alpha));
            }
        }

        private void TessellateEdges(IRenderPackage package, TessellationParameters parameters)
        {
            if (!parameters.ShowEdges) return;

            var surf = geometry as Surface;
            if (surf != null)
            {
                foreach (var curve in surf.PerimeterCurves())
                {
                    curve.Tessellate(package, parameters);
                    curve.Dispose();
                }
            }

            var solid = geometry as Solid;
            if (solid != null)
            {
                foreach (var geom in solid.Edges.Select(edge => edge.CurveGeometry))
                {
                    geom.Tessellate(package, parameters);
                    geom.Dispose();
                }
            }
        }

        private static byte[] CreateColorByteArrayOfSize(int size, byte red, byte green, byte blue, byte alpha)
        {
            var arr = new byte[size * 4];
            for (var i = 0; i < arr.Count(); i+=4)
            {
                arr[i] = red;
                arr[i + 1] = green;
                arr[i + 2] = blue;
                arr[i + 3] = alpha;
            }
            return arr;
        }

        private static void CreateVertexColoredMesh(Point[] vertices, Color[] colors, IRenderPackage package, TessellationParameters parameters)
        {
            package.RequiresPerVertexColoration = true;

            for (var i = 0; i <= vertices.Count()-3; i+=3)
            {
                var ptA = vertices[i];
                var ptB = vertices[i+1];
                var ptC = vertices[i+2];

                if (ptA.IsAlmostEqualTo(ptB) ||
                    ptB.IsAlmostEqualTo(ptC) ||
                    ptA.IsAlmostEqualTo(ptC))
                {
                    continue;
                }

                var alongLine = false;
                using (var l = Line.ByStartPointEndPoint(ptA, ptC))
                {
                    alongLine = ptB.DistanceTo(l) < 0.00001;
                }
                if (alongLine)
                {
                    continue;
                }

                var cA = colors[i];
                var cB = colors[i+1];
                var cC = colors[i+2];

                var s1 = ptB.AsVector().Subtract(ptA.AsVector()).Normalized();
                var s2 = ptC.AsVector().Subtract(ptA.AsVector()).Normalized();
                var n = s1.Cross(s2);

                package.AddTriangleVertex(ptA.X, ptA.Y, ptA.Z);
                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
                package.AddTriangleVertexColor(cA.Red, cA.Green, cA.Blue, cA.Alpha);
                package.AddTriangleVertexUV(0, 0);

                package.AddTriangleVertex(ptB.X, ptB.Y, ptB.Z);
                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
                package.AddTriangleVertexColor(cB.Red, cB.Green, cB.Blue, cB.Alpha);
                package.AddTriangleVertexUV(0, 0);

                package.AddTriangleVertex(ptC.X, ptC.Y, ptC.Z);
                package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
                package.AddTriangleVertexColor(cC.Red, cC.Green, cC.Blue, cC.Alpha);
                package.AddTriangleVertexUV(0, 0);
            }
        }

        #endregion
    }
}
