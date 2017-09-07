using System;
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
            Texture,
            ColorAlpha
        }

        public Block(RectangleF rectangle, Color color, DrawMode mode)
        {
            if (mode != DrawMode.Color && mode != DrawMode.ColorAlpha)
            {
                throw new ArgumentException("Mode has to be Color or ColorAlpha for this constructor.", nameof(mode));
            }

            this.Rectangle = rectangle;
            this.Color = color;
            this.Mode = mode;
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