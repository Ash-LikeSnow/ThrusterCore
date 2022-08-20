using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace ThrusterCore
{
    internal class Draw
    {
        internal static readonly MyStringId _square = MyStringId.GetOrCompute("Square");

        internal static void DrawBox(MyOrientedBoundingBoxD obb, Color color)
        {
            var box = new BoundingBoxD(-obb.HalfExtent, obb.HalfExtent);
            var wm = MatrixD.CreateFromTransformScale(obb.Orientation, obb.Center, Vector3D.One);
            MySimpleObjectDraw.DrawTransparentBox(ref wm, ref box, ref color, MySimpleObjectRasterizer.Solid, 1);
        }

        internal static void DrawScaledPoint(Vector3D pos, double radius, Color color, int divideRatio = 20, bool solid = true, float lineWidth = 0.5f)
        {
            var posMatCenterScaled = MatrixD.CreateTranslation(pos);
            var posMatScaler = MatrixD.Rescale(posMatCenterScaled, radius);
            var material = _square;
            MySimpleObjectDraw.DrawTransparentSphere(ref posMatScaler, 1f, ref color, solid ? MySimpleObjectRasterizer.Solid : MySimpleObjectRasterizer.Wireframe, divideRatio, null, material, lineWidth);
        }

        internal static void DrawLine(Vector3D start, Vector3D end, Vector4 color, float width)
        {
            var c = color;
            MySimpleObjectDraw.DrawLine(start, end, _square, ref c, width);
        }

        internal static void DrawLine(Vector3D start, Vector3D dir, Vector4 color, float width, float length)
        {
            var c = color;
            MySimpleObjectDraw.DrawLine(start, start + (dir * length), _square, ref c, width);
        }
    }
}
