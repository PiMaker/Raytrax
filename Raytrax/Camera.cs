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
                var ray = new Ray2D(this.position, forward);
                var result = this.map.CastRay(ray);
                traceResults.Add(result);

                if (result.Hit && result.Distance > 0)
                {
                    var dist = Camera.antiFisheye(result.Distance, angle);
                    var height = halfScreenHeight / (dist / fullHeightDist);

                    if (result.Block.Mode == Block.DrawMode.Color)
                    {
                        spriteBatch.DrawLine(i, halfScreenHeight - height, i, halfScreenHeight + height,
                            result.Block.Color);
                    }
                    else if (result.Block.Mode == Block.DrawMode.Texture)
                    {
                        spriteBatch.Draw(result.Block.Texture,
                            new Rectangle(i, (int) (halfScreenHeight - height), 1, (int) (height * 2)),
                            new Rectangle((int) (result.Block.Texture.Width * result.TexCoord), 0, 1,
                                result.Block.Texture.Height), Color.White);
                    }
                }

                forward = forward.Rotate(angleMod);
                angle += angleMod;
            }

            // Mini map
            this.map.DrawMiniMap(spriteBatch,
                new RectangleF(BaseGame.ScreenWidth - 200, BaseGame.ScreenHeight - 200, 200, 200), Color.Black,
                this.position, this.facing,
                traceResults);
        }

        private static float antiFisheye(float dist, float angle)
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