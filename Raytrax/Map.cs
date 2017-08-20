using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Raytrax
{
    public class Map
    {
        private readonly Block[,] map;

        private Map(Block[,] map)
        {
            this.map = map;
        }

        public int DimensionX => this.map.GetLength(0);
        public int DimensionY => this.map.GetLength(1);

        public RayTraceResult CastRay(Ray2D ray)
        {
            Vector2 rayDelta;
            float lengthMod;
            var halfW = this.DimensionX / 2f;
            var halfH = this.DimensionY / 2f;
            if (ray.Position.X < halfW && ray.Position.Y < halfH)
            {
                lengthMod =
                    (new Vector2(this.DimensionX + 1, this.DimensionY + 1) - new Vector2(ray.Position.X, ray.Position.Y))
                    .Length();
                rayDelta = lengthMod * ray.Direction;
            }
            else if (ray.Position.X > halfW && ray.Position.Y < halfH)
            {
                lengthMod =
                    (new Vector2(0, this.DimensionY + 1) - new Vector2(ray.Position.X, ray.Position.Y)).Length();
                rayDelta = lengthMod * ray.Direction;
            }
            else if (ray.Position.X < halfW && ray.Position.Y > halfH)
            {
                lengthMod =
                    (new Vector2(this.DimensionX + 1, 0) - new Vector2(ray.Position.X, ray.Position.Y)).Length();
                rayDelta = lengthMod * ray.Direction;
            }
            else
            {
                lengthMod = (new Vector2(0, 0) - new Vector2(ray.Position.X, ray.Position.Y)).Length();
                rayDelta = lengthMod * ray.Direction;
            }

            return this.castDeltaRay(ray.Position, rayDelta, 1f / lengthMod);
        }

        private RayTraceResult castDeltaRay(Vector2 position, Vector2 rayDelta, float rayLengthMod)
        {
            var dirX = rayDelta.X < 0 ? -1 : 1;
            var dirY = rayDelta.Y < 0 ? -1 : 1;

            var posX = (int) position.X;
            var posY = (int) position.Y;

            for (var x = posX; x < this.map.GetLength(0) && x >= 0; x += dirX)
            {
                for (var y = posY; y < this.map.GetLength(1) && y >= 0; y += dirY)
                {
                    if (this.map[x, y] != null)
                    {
                        var hit = this.rayCast(this.map[x, y].Rectangle.Position + new Vector2(0.5f, 0.5f), position,
                            rayDelta, out float texCoord);

                        if (hit.HasValue)
                        {
                            return new RayTraceResult(true, hit.Value, this.map[x, y],
                                new Ray2D(position, rayDelta * rayLengthMod), texCoord);
                        }
                    }
                }
            }

            return new RayTraceResult(false, -1, null, new Ray2D(), -1);
        }

        private float? rayCast(Vector2 aabbPos, Vector2 pos, Vector2 delta, out float texCoord)
        {
            texCoord = -1;

            var scaleX = 1.0f / delta.X;
            var scaleY = 1.0f / delta.Y;

            if (float.IsInfinity(scaleX))
            {
                scaleX = MathHelper.Max(this.DimensionX, this.DimensionY);
            }
            if (float.IsInfinity(scaleY))
            {
                scaleY = MathHelper.Max(this.DimensionX, this.DimensionY);
            }

            var signX = scaleX < 0 ? -1 : 1;
            var signY = scaleY < 0 ? -1 : 1;
            var nearTimeX = (aabbPos.X - signX * 0.5f - pos.X) * scaleX;
            var nearTimeY = (aabbPos.Y - signY * 0.5f - pos.Y) * scaleY;
            var farTimeX = (aabbPos.X + signX * 0.5f - pos.X) * scaleX;
            var farTimeY = (aabbPos.Y + signY * 0.5f - pos.Y) * scaleY;

            if (nearTimeX > farTimeY || nearTimeY > farTimeX)
            {
                return null;
            }

            var nearTime = nearTimeX > nearTimeY ? nearTimeX : nearTimeY;
            var farTime = farTimeX < farTimeY ? farTimeX : farTimeY;

            if (nearTime >= 1 || farTime <= 0)
            {
                return null;
            }

            var time = MathHelper.Clamp(nearTime, 0, 1);
            var dist = delta.Length() * time;

            var collisionPoint = pos + delta * time;
            if (nearTimeX > nearTimeY)
            {
                texCoord = (collisionPoint.Y - aabbPos.Y + 0.5f) * -signX;
            }
            else
            {
                texCoord = (collisionPoint.X - aabbPos.X + 0.5f) * signY;
            }

            if (texCoord < 0)
            {
                texCoord += 1;
            }

            return dist;
        }

        public Vector2 Collide(Vector2 origin, Vector2 delta)
        {
            var newPos = origin;

            var newPosX = newPos;
            newPosX.X += delta.X;
            var hitX = false;
            for (var x = 0; x < this.map.GetLength(0); x++)
            {
                for (var y = 0; y < this.map.GetLength(1); y++)
                {
                    if (this.map[x, y] != null && this.map[x, y].Rectangle.Expand(0.1f).Contains(new Point2(newPosX.X, newPosX.Y)))
                    {
                        hitX = true;
                        goto breakX;
                    }
                }
            }

            breakX:

            var newPosY = newPos;
            newPosY.Y += delta.Y;
            var hitY = false;
            for (var x = 0; x < this.map.GetLength(0); x++)
            {
                for (var y = 0; y < this.map.GetLength(1); y++)
                {
                    if (this.map[x, y] != null && this.map[x, y].Rectangle.Expand(0.1f).Contains(new Point2(newPosY.X, newPosY.Y)))
                    {
                        hitY = true;
                        goto breakY;
                    }
                }
            }

            breakY:
            if (!hitX)
            {
                newPos.X = newPosX.X;
            }
            if (!hitY)
            {
                newPos.Y = newPosY.Y;
            }

            return newPos;
        }

        public void DrawMiniMap(SpriteBatch spriteBatch, RectangleF rect, Color background, Vector2 position,
            Vector2 facing, List<RayTraceResult> traceResults = null)
        {
            spriteBatch.FillRectangle(rect, background * 0.6f);

            var offset = new Vector2(rect.X, rect.Y);

            var blockWidth = rect.Width / this.DimensionX;
            var blockHeight = rect.Height / this.DimensionY;
            var blockSize = new Size2(blockWidth, blockHeight);

            for (var x = 0; x < this.DimensionX; x++)
            {
                for (var y = 0; y < this.DimensionY; y++)
                {
                    if (this.map[x, y] != null)
                    {
                        if (this.map[x, y].Mode == Block.DrawMode.Color)
                        {
                            spriteBatch.FillRectangle(offset + new Vector2(x * blockWidth, y * blockHeight), blockSize,
                                this.map[x, y].Color * 0.5f);
                        }
                        else if (this.map[x, y].Mode == Block.DrawMode.Texture)
                        {
                            spriteBatch.Draw(this.map[x, y].Texture,
                                new RectangleF(offset + new Vector2(x * blockWidth, y * blockHeight), blockSize)
                                    .ToRectangle(), Color.White);
                        }
                    }
                }
            }

            if (traceResults != null)
            {
                foreach (var trace in traceResults)
                {
                    if (trace.Hit)
                    {
                        var start = new Vector2(trace.Ray.Position.X * blockWidth, trace.Ray.Position.Y * blockHeight) +
                                    offset;
                        var end = start +
                                  new Vector2(trace.Ray.Direction.X * blockWidth, trace.Ray.Direction.Y * blockHeight) *
                                  trace.Distance;
                        spriteBatch.DrawLine(start, end, Color.Yellow * 0.3f);
                    }
                }
            }

            const int RADIUS = 6;
            var drawPos = new Vector2(position.X * blockWidth, position.Y * blockHeight) + offset;
            var eyePos = drawPos + facing.NormalizedCopy() * RADIUS;
            spriteBatch.DrawCircle(drawPos, RADIUS, 16, Color.Red * 0.8f, RADIUS);
            spriteBatch.DrawCircle(eyePos, 3, 12, Color.Red * 0.8f, 3);
        }

        public static Map FromFile(string path, ContentManager content)
        {
            Block[,] map;

            using (var stream = new FileStream(path, FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    var dimString = reader.ReadLine();
                    dimString = dimString.Substring(4);
                    var dimSplit = dimString.Split(',');
                    var dimX = int.Parse(dimSplit[0]);
                    var dimY = int.Parse(dimSplit[1]);
                    map = new Block[dimX, dimY];

                    var rgbLookup = new Dictionary<char, Color>();
                    var texLookup = new Dictionary<char, Texture2D>();

                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (line.StartsWith("def"))
                        {
                            line = line.Substring(4);
                            var split = line.Split(' ');
                            if (split[1] == "rgb")
                            {
                                var colorSplit = split[2].Split(',');
                                var color = new Color(int.Parse(colorSplit[0]), int.Parse(colorSplit[1]),
                                    int.Parse(colorSplit[2]));
                                rgbLookup.Add(split[0][0], color);
                            }
                            else if (split[1] == "tex")
                            {
                                var tex = content.Load<Texture2D>(split[2]);
                                texLookup.Add(split[0][0], tex);
                            }
                        }
                        else if (line == "map")
                        {
                            break;
                        }
                    }

                    for (var i = 0; i < dimY; i++)
                    {
                        var line = reader.ReadLine();
                        for (var j = 0; j < dimY; j++)
                        {
                            if (line[j] != ' ')
                            {
                                if (rgbLookup.ContainsKey(line[j]))
                                {
                                    map[j, i] = new Block(new RectangleF(new Point2(j, i), new Size2(1, 1)),
                                        rgbLookup[line[j]]);
                                }
                                else if (texLookup.ContainsKey(line[j]))
                                {
                                    map[j, i] = new Block(new RectangleF(new Point2(j, i), new Size2(1, 1)),
                                        texLookup[line[j]]);
                                }
                            }
                        }
                    }
                }
            }

            return new Map(map);
        }
    }
}