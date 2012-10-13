using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Net;
using System.Text;
using Rolling;
using System.Diagnostics;

namespace Rolling
{
    class GameOver : GameScreen
    {
        SpriteBatch spriteBatch;
        SpriteFont font;

        GraphicsDevice device;
        ContentManager content;

        GamePadState XboxcurrentState;
        GamePadState XboxoldState;

        KeyboardState currentState;
        KeyboardState oldState;
        string name;
        public String Name
        {
            set { name = value; }
        }

        int level;
        public int Level
        {
            set { level = value; }
        }

        float[] MenuScale = { 1, 1, 1 };
        bool goUp = true;
        int Selected = 0;

        override public void Initialize()
        {
            content = ScreenManager.getManager();
            Sound.playSound("Sound\\gameOver", content);
            device = ScreenManager.getDevice();
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);
            XboxoldState = GamePad.GetState(PlayerIndex.One);
            currentState = Keyboard.GetState();
            oldState = Keyboard.GetState();
        }

        override public void Draw(GameTime time)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Game Over!", new Vector2(120, 100), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Start Over", new Vector2(120, 200), Color.White, 0, Vector2.Zero, MenuScale[0], SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Exit", new Vector2(120, 300), Color.White, 0, Vector2.Zero, MenuScale[1], SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(time);
        }

        override public void Update(GameTime time)
        {
            if (goUp) { MenuScale[Selected] += .01f; } else { MenuScale[Selected] -= .01f; }
            if (MenuScale[Selected] > 1.4 || MenuScale[Selected] < 1) { goUp = !goUp; }

            oldState = currentState;
            XboxoldState = XboxcurrentState;
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);
            currentState = Keyboard.GetState();

            if ((currentState.IsKeyDown(Keys.W) && !oldState.IsKeyDown(Keys.W)) || (currentState.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Up)) || (XboxcurrentState.DPad.Up == ButtonState.Pressed && XboxoldState.DPad.Up != ButtonState.Pressed))
            {
                Sound.playSoundOnce("Sound\\tap", content);
                MenuScale[Selected] = 1;
                Selected--;
                if (Selected < 0) { Selected = 1; }
                MenuScale[Selected] = 1;
                goUp = true;
            }
            if ((currentState.IsKeyDown(Keys.S) && !oldState.IsKeyDown(Keys.S)) || (currentState.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down)) || (XboxcurrentState.DPad.Down == ButtonState.Pressed && XboxoldState.DPad.Down != ButtonState.Pressed))
            {
                Sound.playSoundOnce("Sound\\tap", content);
                MenuScale[Selected] = 1;
                Selected++;
                if (Selected > 1) { Selected = 0; }
                MenuScale[Selected] = 1;
                goUp = true;
            }
            if ((currentState.IsKeyDown(Keys.Space) && !oldState.IsKeyDown(Keys.Space)) || (currentState.IsKeyDown(Keys.Enter) && !oldState.IsKeyDown(Keys.Enter)) || (XboxcurrentState.IsButtonDown(Buttons.A) && !XboxoldState.IsButtonDown(Buttons.A)))
            {
                //Enter was pressed on a menu Item.
                if (Selected == 0)
                {
                    ScreenManager.RemoveAll();
                    GamePlayScreen level = new GamePlayScreen();
                    level.Level = this.level;
                    level.Terrain = "Models\\" + this.level;
                    level.TerrainTexture = "Textures\\" + this.level;
                    level.Background = "Sound\\" + this.level;
                    level.Name = this.name;
                    ScreenManager.AddScreen(level);
                }
                else if (Selected == 1)
                {

                    ScreenManager.RemoveAll();
                    ScreenManager.AddScreen(new MainMenu());
                }
            }
        }

        override public void LoadContent()
        {
            spriteBatch = new SpriteBatch(device);
            font = content.Load<SpriteFont>("Menu");
        }

        override public void UnloadContent()
        {

        }
    }
}