using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Raytrax
{
    public static class InputManager
    {
        private static Vector2 MouseCenter;
        public static MouseState CurrentMouseState { get; private set; }
        public static MouseState OldMouseState { get; private set; }
        public static Vector2 MouseDelta { get; private set; }

        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static KeyboardState OldKeyboardState { get; private set; }

        public static void Init()
        {
            InputManager.CurrentMouseState = InputManager.OldMouseState = Mouse.GetState();
            InputManager.CurrentKeyboardState = InputManager.OldKeyboardState = Keyboard.GetState();
            InputManager.MouseCenter = new Vector2(BaseGame.ScreenWidth/2, BaseGame.ScreenHeight/2);
        }

        public static void Update()
        {
            InputManager.OldMouseState = InputManager.CurrentMouseState;
            InputManager.OldKeyboardState = InputManager.CurrentKeyboardState;

            InputManager.CurrentMouseState = Mouse.GetState();
            InputManager.CurrentKeyboardState = Keyboard.GetState();

            InputManager.MouseDelta = new Vector2(InputManager.CurrentMouseState.X, InputManager.CurrentMouseState.Y) -
                                      InputManager.MouseCenter;
            Mouse.SetPosition((int)InputManager.MouseCenter.X, (int)InputManager.MouseCenter.Y);
        }
    }
}