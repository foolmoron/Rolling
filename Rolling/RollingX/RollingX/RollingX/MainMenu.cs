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
    class MainMenu : GameScreen
    {
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D background;

        GraphicsDevice device;
        ContentManager content;

        GamePadState XboxcurrentState;
        GamePadState XboxoldState;

        KeyboardState currentState;
        KeyboardState oldState;

        float[] MenuScale = {1,1,1,1};
        bool goUp = true;
        int Selected = 0;

        override public void Initialize()
        {
            content = ScreenManager.getManager();
            device = ScreenManager.getDevice();
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);
            XboxoldState = GamePad.GetState(PlayerIndex.One);
            currentState = Keyboard.GetState();
            oldState = Keyboard.GetState();
            Sound.playSoundLoop("Sound\\menu", content);
        }

        override public void Draw(GameTime time)
        {
            device.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            Rectangle screenRectangle = new Rectangle(0, 0, device.Viewport.Bounds.Width, device.Viewport.Bounds.Height);
            spriteBatch.Draw(background, screenRectangle, Color.White);
            spriteBatch.DrawString(font, "New Game", new Vector2(120, 80), Color.White, 0, Vector2.Zero, MenuScale[0], SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Load Level", new Vector2(120, 160), Color.White, 0, Vector2.Zero, MenuScale[1], SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Help", new Vector2(120, 240), Color.White, 0, Vector2.Zero, MenuScale[2], SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Exit", new Vector2(120, 320), Color.White, 0, Vector2.Zero, MenuScale[3], SpriteEffects.None, 0);
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

            if ((currentState.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Up)) || (XboxcurrentState.DPad.Up == ButtonState.Pressed && XboxoldState.DPad.Up != ButtonState.Pressed))
            {
                Sound.playSoundOnce("Sound\\tap", content);
                MenuScale[Selected] = 1;
                Selected--;
                if (Selected < 0) { Selected = 2; }
                MenuScale[Selected] = 1;
                goUp = true;
            }
            if ((currentState.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down)) || (XboxcurrentState.DPad.Down == ButtonState.Pressed && XboxoldState.DPad.Down != ButtonState.Pressed))
            {
                Sound.playSoundOnce("Sound\\tap", content);
                MenuScale[Selected] = 1;
                Selected++;
                if (Selected > 3) { Selected = 0; }
                MenuScale[Selected] = 1;
                goUp = true;
            }
            if ((currentState.IsKeyDown(Keys.Enter) && !oldState.IsKeyDown(Keys.Enter)) || (XboxcurrentState.IsButtonDown(Buttons.A) && !XboxoldState.IsButtonDown(Buttons.A)))
            {
                //Enter was pressed on a menu Item.
                if (Selected == 0)
                {
                    Entrance newGame = new Entrance();
                    newGame.Level = 1;
                    ScreenManager.AddScreen(newGame);
                }
                else if (Selected == 1)
                {
                    ScreenManager.AddScreen(new LoadLevelScreen());
                }
                else if (Selected == 2) 
                {
                    ScreenManager.AddScreen(new HelpScreen());
                }
                else if (Selected == 3)
                {
                    ScreenManager.RemoveAll();
                }
            }
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
    }
}
