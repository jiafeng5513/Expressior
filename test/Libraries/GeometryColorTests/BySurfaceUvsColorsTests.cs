using System;
using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;
using TestServices;

namespace DisplayTests
{
    [TestFixture]
    public class BySurfaceUvsColorsTests : GeometricTestBase
    {

        private static Surface CreateOneSurface()
        {
            var rect = Rectangle.ByWidthLength(5, 5);
            var surface = Surface.ByPatch(rect);
            return surface;
        }

        private static Color[][] CreateOneRowOfColors()
        {
            var colors = new[]
            {
                new []
                {
                    Color.ByARGB(255, 255, 0, 0),
                    Color.ByARGB(255, 0, 255, 0),
                    Color.ByARGB(0, 0, 255)
                },
            };

            return colors;
        }

        private static Color[][] CreateTwoRowsOfColors()
        {
            var colors = new[]
            {
                new []
                {
                    Color.ByARGB(255, 255, 0, 0),
                    Color.ByARGB(255, 0, 255, 0),
                    Color.ByARGB(0, 0, 255)
                },
                new []
                {
                    Color.ByARGB(0, 0, 255),
                    Color.ByARGB(255, 0, 255, 0),
                    Color.ByARGB(255, 255, 0, 0),
                }
            };

            return colors;
        }

        private static Color[][] CreateJaggedArrayOfColors()
        {
            var colors = new[]
            {
                new []
                {
                    Color.ByARGB(255, 255, 0, 0),
                    Color.ByARGB(255, 0, 255, 0),
                    Color.ByARGB(0, 0, 255)
                },
                new []
                {
                    Color.ByARGB(0, 0, 255),
                    Color.ByARGB(255, 0, 255, 0),
                }
            };

            return colors;
        }
    }
}
