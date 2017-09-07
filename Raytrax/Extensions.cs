using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Raytrax
{
    public static class Extensions
    {
        public static RectangleF Expand(this RectangleF rect, float amount)
        {
            return new RectangleF(rect.X - amount / 2f, rect.Y - amount / 2f, rect.Width + amount, rect.Height + amount);
        }

        public static Point ToPoint(this Point2 point)
        {
            return new Point((int) point.X, (int) point.Y);
        }
    }
}