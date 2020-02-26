using System.Numerics;
using MyScript.IInk.Graphics;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class TransformExtensions
    {
        public static Transform ToNativeTransform(this Matrix3x2 source)
        {
            return new Transform(
                source.M11, source.M21, source.M31,
                source.M12, source.M22, source.M32);
        }

        public static Matrix3x2 ToPlatformTransform(this Transform source)
        {
            return new Matrix3x2(
                (float)source.XX, (float)source.XY,
                (float)source.YX, (float)source.YY,
                (float)source.TX, (float)source.TY);
        }
    }
}
