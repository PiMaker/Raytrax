using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Raytrax
{
    public class Block
    {
        public enum DrawMode
        {
            Color,
            Texture
        }

        public Block(RectangleF rectangle, Color color)
        {
            this.Rectangle = rectangle;
            this.Color = color;
            this.Mode = DrawMode.Color;
        }

        public Block(RectangleF rectangle, Texture2D texture)
        {
            this.Rectangle = rectangle;
            this.Texture = texture;
            this.Mode = DrawMode.Texture;
        }

        public RectangleF Rectangle { get; private set; }
        public DrawMode Mode { get; private set; }
        public Color Color { get; private set; }
        public Texture2D Texture { get; private set; }
    }
}