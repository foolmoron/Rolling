using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rolling
{
    class Entrance : GameScreen
    {
        static String AllowedKeys = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D background;
        GraphicsDevice device;
        ContentManager content;
        KeyboardState currentKeyboardState;
        KeyboardState oldKeyboardState;
        GamePadState XboxcurrentState;
        GamePadState XboxoldState;
        string textString;
        int level;
        public int Level
        {
            set { level = value; }
        }

        public override void Initialize()
        {
            content = ScreenManager.getManager();
            device = ScreenManager.getDevice();
            currentKeyboardState = Keyboard.GetState();
            oldKeyboardState = Keyboard.GetState();
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);
            XboxoldState = GamePad.GetState(PlayerIndex.One);
            textString = "";
            base.Initialize();
        }

        override public void LoadContent()
        {
            spriteBatch = new SpriteBatch(device);
            font = content.Load<SpriteFont>("Menu");
            background = content.Load<Texture2D>("Textures\\mainMenu");
        }

        override public void UnloadContent()
        {

        }

        override public void Draw(GameTime time)
        {
            device.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            Rectangle screenRectangle = new Rectangle(0, 0, device.Viewport.Bounds.Width, device.Viewport.Bounds.Height);
            spriteBatch.Draw(background, screenRectangle, Color.White);
            spriteBatch.DrawString(font, "Enter your name", new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(font, textString, new Vector2(50, 200), Color.White);
            spriteBatch.End();
            base.Draw(time);
        }

        override public void Update(GameTime time)
        {
            oldKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            XboxoldState = XboxcurrentState;
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);

            Keys[] pressedKeys;
            pressedKeys = currentKeyboardState.GetPressedKeys();

            if (XboxcurrentState.IsButtonDown(Buttons.A) && !XboxoldState.IsButtonDown(Buttons.A))
            {
                Sound.pauseSound("Sound\\menu", content);
                GamePlayScreen level = new GamePlayScreen();
                level.Level = this.level;
                level.Terrain = "Models\\" + this.level;
                level.TerrainTexture = "Textures\\" + this.level;
                level.Background = "Sound\\" + this.level;
                level.Name = textString;
                ScreenManager.AddScreen(level);
                ScreenManager.RemoveScreen(this);
            }

            foreach (Keys key in pressedKeys)
            {
                if (oldKeyboardState.IsKeyUp(key))
                {
                    if (key == Keys.Back)
                    { // overflows
                        if (textString.Length > 0)
                            textString = textString.Remove(textString.Length - 1, 1);
                    }
                    else
                    {
                        if (key == Keys.Enter || key == Keys.Space)
                        {
                            Sound.pauseSound("Sound\\menu", content);
                            GamePlayScreen level = new GamePlayScreen();
                            level.Level = this.level;
                            level.Terrain = "Models\\" + this.level;
                            level.TerrainTexture = "Textures\\" + this.level;
                            level.Background = "Sound\\" + this.level;
                            level.Name = textString;
                            ScreenManager.AddScreen(level);
                            ScreenManager.RemoveScreen(this);
                        }
                        else if (textString.Length > 5)
                            return;
                        else if (key == Keys.Space)
                            textString += " ";
                        else if (AllowedKeys.Contains(key.ToString()))
                            textString += key.ToString();
                    }
                }
            }
        }
    }
}
