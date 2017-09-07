// File: Camera.cs
// Created: 07.09.2017
// 
// See <summary> tags for more information.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Raytrax
{
    public class Camera
    {
        private readonly float fov = MathHelper.ToRadians(70);
        private readonly Map map;
        private Vector2 facing;

        private Vector2 position;

        public Camera(Map map)
        {
            this.map = map;
            this.position = new Vector2(map.DimensionX / 2f, map.DimensionY / 2f);
            this.facing = -Vector2.UnitY;
        }

        public void Update(GameTime gameTime)
        {
            var movement = Vector2.Zero;

            if (InputManager.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                movement += this.facing * (float) gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (InputManager.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                movement -= this.facing * (float) gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (InputManager.CurrentKeyboardState.IsKeyDown(Keys.D))
            {
                movement += this.facing.PerpendicularCounterClockwise() *
                            (float) gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (InputManager.CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                movement += this.facing.PerpendicularClockwise() * (float) gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (movement != Vector2.Zero)
            {
                movement.Normalize();
                this.position = this.map.Collide(this.position, movement * 0.0325f);
            }

            this.facing =
                this.facing.Rotate(InputManager.MouseDelta.X *
                                   (float) gameTime.ElapsedGameTime.TotalSeconds * 0.3f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var angle = -this.fov / 2f;
            var angleMod = this.fov / BaseGame.ScreenWidth;
            var forward = this.facing.Rotate(angle);
            var halfScreenHeight = BaseGame.ScreenHeight / 2f;
            //var heightFov = this.fov / (BaseGame.ScreenWidth / (float) BaseGame.ScreenHeight);
            var fullHeightDist = MathHelper.PiOver4 / (this.fov / 2f);

            var traceResults = new List<RayTraceResult>();

            for (var i = 0; i < BaseGame.ScreenWidth; i++)
            {
                this.DrawTrace(spriteBatch, i, angle, forward, halfScreenHeight, fullHeightDist, traceResults);

                forward = forward.Rotate(angleMod);
                angle += angleMod;
            }

            // Mini map
            this.map.DrawMiniMap(spriteBatch,
                new RectangleF(BaseGame.ScreenWidth - 200, BaseGame.ScreenHeight - 200, 200, 200), Color.Black,
                this.position, this.facing,
                traceResults);
        }

        private void DrawTrace(SpriteBatch spriteBatch, int xpos, float angle, Vector2 forward, float halfScreenHeight,
            float fullHeightDist, List<RayTraceResult> traceResults, List<Point> ignored = null)
        {
            var ray = new Ray2D(this.position, forward);
            var result = this.map.CastRay(ray, ignored);
            traceResults.Add(result);

            if (result.Hit && result.Distance > 0)
            {
                var dist = Camera.AntiFisheye(result.Distance, angle);
                var height = halfScreenHeight / (dist / fullHeightDist);

                if (result.Block.Mode == Block.DrawMode.Color)
                {
                    Camera.DrawColorLine(spriteBatch, xpos, halfScreenHeight, height, result.Block.Color);
                }
                else if (result.Block.Mode == Block.DrawMode.Texture)
                {
                    Camera.DrawTextureLine(spriteBatch, result.Block.Texture, result.TexCoord, xpos, halfScreenHeight,
                        height);
                }
                else if (result.Block.Mode == Block.DrawMode.ColorAlpha)
                {
                    var ig = ignored ?? new List<Point>();
                    ig.Add(result.Block.Rectangle.Position.ToPoint());
                    this.DrawTrace(spriteBatch, xpos, angle, forward, halfScreenHeight, fullHeightDist, traceResults,
                        ig);

                    if (ignored == null)
                    {
                        Camera.DrawColorLine(spriteBatch, xpos, halfScreenHeight, height, result.Block.Color);
                    }
                }
            }
        }

        private static void DrawTextureLine(SpriteBatch spriteBatch, Texture2D texture, float texCoord, int xpos,
            float halfScreenHeight,
            float height)
        {
            spriteBatch.Draw(texture,
                new Rectangle(xpos, (int) (halfScreenHeight - height), 1, (int) (height * 2)),
                new Rectangle((int) (texture.Width * texCoord), 0, 1,
                    texture.Height), Color.White);
        }

        private static void DrawColorLine(SpriteBatch spriteBatch, int xpos, float halfScreenHeight, float height,
            Color color)
        {
            spriteBatch.DrawLine(xpos, halfScreenHeight - height, xpos, halfScreenHeight + height,
                color);
        }

        private static float AntiFisheye(float dist, float angle)
        {
            angle = Math.Abs(angle);
            if (angle > MathHelper.PiOver4)
            {
                angle -= MathHelper.PiOver4;
            }
            return dist * (float) Math.Cos(angle);
        }
    }
}