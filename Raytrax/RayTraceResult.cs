using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Raytrax
{
    public class RayTraceResult
    {
        public bool Hit { get; private set; }
        public float Distance { get; private set; }
        public Block Block { get; private set; }
        public Ray2D Ray { get; private set; }
        public float TexCoord { get; private set; }

        public RayTraceResult(bool hit, float distance, Block block, Ray2D ray, float texCoord)
        {
            this.Hit = hit;
            this.Distance = distance;
            this.Block = block;
            this.Ray = ray;
            this.TexCoord = texCoord;
        }
    }
}