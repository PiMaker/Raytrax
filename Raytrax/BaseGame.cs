using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Raytrax
{
    public class BaseGame : Game
    {
        public static int ScreenWidth, ScreenHeight;
        public static SpriteFont Font;
        private readonly GraphicsDeviceManager graphics;

        private Camera camera;
        private SpriteBatch spriteBatch;

        public BaseGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = false;

            this.graphics.PreparingDeviceSettings +=
                (sender, args) => { args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8; };
        }

        protected override void Initialize()
        {
            BaseGame.ScreenWidth = 1280;
            BaseGame.ScreenHeight = 720;

            this.graphics.PreferredBackBufferWidth = BaseGame.ScreenWidth;
            this.graphics.PreferredBackBufferHeight = BaseGame.ScreenHeight;
            this.graphics.IsFullScreen = false;
            this.graphics.ApplyChanges();

            InputManager.Init();
            this.camera = new Camera(Map.FromFile("example.map", this.Content));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            BaseGame.Font = this.Content.Load<SpriteFont>("font");

            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            if (InputManager.CurrentKeyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            this.camera.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.LightGray);

            this.spriteBatch.Begin();
            this.spriteBatch.FillRectangle(0, 0, BaseGame.ScreenWidth, BaseGame.ScreenHeight / 2, Color.Gray);
            this.camera.Draw(this.spriteBatch);
            this.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}