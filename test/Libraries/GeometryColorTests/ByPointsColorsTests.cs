using Autodesk.DesignScript.Geometry;
using DSCore;
using NUnit.Framework;
using System;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using TestServices;

namespace GeometryColorTests
{
    [TestFixture]
    public class ByPointsColorsTests : GeometricTestBase
    {
        private static Point[] TestVerticesInALine()
        {
            var a = Point.ByCoordinates(0, 0, 0);
            var b = Point.ByCoordinates(1, 0, 0);
            var c = Point.ByCoordinates(2, 0, 0);

            return new[] { a, b, c};
        }

        private static Point[] TestVerticesTwoInSamePlace()
        {
            var a = Point.ByCoordinates(0, 0, 0);
            var b = Point.ByCoordinates(0, 0, 0);
            var c = Point.ByCoordinates(2, 0, 0);

            return new[] { a, b, c };
        }

        private static Point[] TestVertices()
        {
            var a = Point.ByCoordinates(0, 0, 0);
            var b = Point.ByCoordinates(1, 0, 0);
            var c = Point.ByCoordinates(1, 1, 0);
            var d = Point.ByCoordinates(0, 1, 1);

            return new[] { a, b, c, a, c, d };
        }

        private static Color[] TestColors()
        {
            var a = Color.ByARGB(255, 0, 0, 255);
            var b = Color.ByARGB(255, 0, 255, 255);
            var c = Color.ByARGB(255, 255, 255, 255);
            var d = Color.ByARGB(255, 255, 0, 0);

            return new[] { a, b, c, a,c, d };
        }

    }
}
