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
    class HelpScreen : GameScreen
    {
        SpriteBatch spriteBatch;
        SpriteFont font;

        GraphicsDevice device;
        ContentManager content;

        Texture2D background;
        GamePadState XboxcurrentState;
        GamePadState XboxoldState;

        KeyboardState currentState;
        KeyboardState oldState;

        override public void Initialize()
        {
            content = ScreenManager.getManager();
            device = ScreenManager.getDevice();
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);
            XboxoldState = GamePad.GetState(PlayerIndex.One);
            currentState = Keyboard.GetState();
            oldState = Keyboard.GetState();
        }

        override public void Draw(GameTime time)
        {
            device.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            Rectangle screenRectangle = new Rectangle(0, 0, device.Viewport.Bounds.Width, device.Viewport.Bounds.Height);
            spriteBatch.Draw(background, screenRectangle, Color.White);
            spriteBatch.DrawString(font, "Plot", new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(font, "You, a charismatic smooth talking ladies man, were", new Vector2(40, 20), Color.White);
            spriteBatch.DrawString(font, "turned into a ball by a wizard who then kidnapped", new Vector2(40, 40), Color.White);
            spriteBatch.DrawString(font, "and threw you into his twisted labyrinth of evil.", new Vector2(40, 60), Color.White);
            spriteBatch.DrawString(font, "Forced to play his evil game to escape, find the", new Vector2(40,80), Color.White);
            spriteBatch.DrawString(font, "exit for each level to find a way to transform back", new Vector2(40, 100), Color.White);
            spriteBatch.DrawString(font, "to your original form.", new Vector2(40,120), Color.White);
            spriteBatch.DrawString(font, "Keyboard Inputs", new Vector2(0, 180), Color.White);
            spriteBatch.DrawString(font, "Left/right arrows or A/D to rotate the camera left/right", new Vector2(40, 200), Color.White);
            spriteBatch.DrawString(font, "Up/down arrows or W/S to move", new Vector2(40, 220), Color.White);
            spriteBatch.DrawString(font, "I/K to rotate the camera up/down", new Vector2(40, 240), Color.White);
            spriteBatch.DrawString(font, "Space to jump", new Vector2(40, 260), Color.White);
            spriteBatch.DrawString(font, "E/Q to change the ball", new Vector2(40, 280), Color.White);
            spriteBatch.DrawString(font, "Mouse Inputs", new Vector2(0, 340), Color.White);
            spriteBatch.DrawString(font, "Left click to move forward", new Vector2(40, 360), Color.White);
            spriteBatch.DrawString(font, "Right click to rotate the camera", new Vector2(40, 380), Color.White);
            spriteBatch.End();

            base.Draw(time);
        }

        override public void Update(GameTime time)
        {
            oldState = currentState;
            XboxoldState = XboxcurrentState;
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);
            currentState = Keyboard.GetState();

            if ((currentState.IsKeyDown(Keys.Space) && !oldState.IsKeyDown(Keys.Space)) || (currentState.IsKeyDown(Keys.Enter) && !oldState.IsKeyDown(Keys.Enter)) || (currentState.IsKeyDown(Keys.Left) && !oldState.IsKeyDown(Keys.Left)) || (XboxcurrentState.IsButtonDown(Buttons.Back) && !XboxoldState.IsButtonDown(Buttons.Back)))
            {
                ScreenManager.AddScreen(new MainMenu());
            }
        }

        override public void LoadContent()
        {
            spriteBatch = new SpriteBatch(device);
            font = content.Load<SpriteFont>("Arial");
            background = content.Load<Texture2D>("Textures\\mainMenu");
        }

        override public void UnloadContent()
        {

        }
    }
}