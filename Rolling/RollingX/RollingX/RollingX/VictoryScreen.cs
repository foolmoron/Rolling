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
    class VictoryScreen : GameScreen
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
        public long Ticks;

        float[] MenuScale = { 0.5f, 0.5f, 0.5f };
        bool goUp = true;
        int Selected = 0;

        int numberOfEntries;
        String[] names;
        int[] scores;
        long[] datesInMillis;
        int yourScore = 0;

        override public void Initialize()
        {
            content = ScreenManager.getManager();
            device = ScreenManager.getDevice();
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);
            XboxoldState = GamePad.GetState(PlayerIndex.One);
            currentState = Keyboard.GetState();
            oldState = Keyboard.GetState();
            Sound.playSoundOnce("Sound\\victory", content);
            GamePlayScreen.getScores(level, out numberOfEntries, out names, out scores, out datesInMillis);
            int[] oldScores = new int[scores.Length];
            Array.Copy(scores, oldScores, scores.Length);
            Array.Sort<int, string>(scores, names, null);
            scores = oldScores;
            Array.Sort<int, long>(scores, datesInMillis, null);
            Array.Reverse(scores);
            Array.Reverse(names);
            Array.Reverse(datesInMillis);
            Console.WriteLine("NOW=" + Ticks);
            for (int i = 0; i < numberOfEntries; i++)
            {
                if (Math.Abs(datesInMillis[i] - Ticks) < 1000)
                {
                    yourScore = i;
                    Console.WriteLine(names[i] + " | " + scores[i] + " | " + datesInMillis[i] + "*");
                }
                else
                {
                    Console.WriteLine(names[i] + " | " + scores[i] + " | " + datesInMillis[i]);
                }
            }
        }

        override public void Draw(GameTime time)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(GamePlayScreen.scoreboardTexture, new Vector2(20, 20), Color.White);
            for (int i = 0; i < Math.Min(numberOfEntries, 5); i++)
            {
                Color textColor = Color.White;
                if (i == yourScore)
                    textColor = Color.Gold;
                spriteBatch.DrawString(font, "" + (i + 1) + ".", new Vector2(45, 88 + (i * 44)), textColor, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, names[i], new Vector2(123, 88 + (i * 44)), textColor, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "" + (((double)scores[i]) / 1000).ToString("#0.0"), new Vector2(240, 88 + (i * 44)), textColor, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "" + (new DateTime(datesInMillis[i])).ToString(("MM/dd hh:mm")), new Vector2(325, 88 + (i * 44)), textColor, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
            }
            if (yourScore > 5)
            {
                spriteBatch.DrawString(font, "" + (yourScore + 1) + ".", new Vector2(45, 308), Color.Gold, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, names[yourScore], new Vector2(123, 308), Color.Gold, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "" + (((double)scores[yourScore]) / 1000).ToString("#0.0"), new Vector2(240, 308), Color.Gold, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "" + (new DateTime(datesInMillis[yourScore])).ToString(("MM/dd hh:mm")), new Vector2(325, 308), Color.Gold, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
            }
            spriteBatch.DrawString(font, "You Beat Level " + level + "!", new Vector2(300, 30), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Replay", new Vector2(45, 360), Color.White, 0, Vector2.Zero, MenuScale[0], SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Next Level", new Vector2(45, 390), Color.White, 0, Vector2.Zero, MenuScale[1], SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Exit", new Vector2(45, 420), Color.White, 0, Vector2.Zero, MenuScale[2], SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(time);
        }

        override public void Update(GameTime time)
        {
            if (goUp) { MenuScale[Selected] += .005f; } else { MenuScale[Selected] -= .005f; }
            if (MenuScale[Selected] > 0.6 || MenuScale[Selected] < 0.5) { goUp = !goUp; }

            oldState = currentState;
            XboxoldState = XboxcurrentState;
            XboxcurrentState = GamePad.GetState(PlayerIndex.One);
            currentState = Keyboard.GetState();

            if ((currentState.IsKeyDown(Keys.Up) && !oldState.IsKeyDown(Keys.Up)) || (XboxcurrentState.DPad.Up == ButtonState.Pressed && XboxoldState.DPad.Up != ButtonState.Pressed))
            {
                Sound.playSoundOnce("Sound\\tap", content);
                MenuScale[Selected] = 0.5f;
                Selected--;
                if (Selected < 0) { Selected = 2; }
                MenuScale[Selected] = 0.5f;
                goUp = true;
            }
            if ((currentState.IsKeyDown(Keys.Down) && !oldState.IsKeyDown(Keys.Down)) || (XboxcurrentState.DPad.Down == ButtonState.Pressed && XboxoldState.DPad.Down != ButtonState.Pressed))
            {
                Sound.playSoundOnce("Sound\\tap", content);
                MenuScale[Selected] = 0.5f;
                Selected++;
                if (Selected > 2) { Selected = 0; }
                MenuScale[Selected] = 0.5f;
                goUp = true;
            }
            if ((currentState.IsKeyDown(Keys.Enter) && !oldState.IsKeyDown(Keys.Enter)) || (XboxcurrentState.IsButtonDown(Buttons.A) && !XboxoldState.IsButtonDown(Buttons.A)))
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
                    GamePlayScreen level = new GamePlayScreen();
                    this.level++;
                    if (this.level >= 5)
                        this.level = 0;
                    level.Level = this.level;
                    level.Terrain = "Models\\" + this.level;
                    level.TerrainTexture = "Textures\\" + this.level;
                    level.Background = "Sound\\" + this.level;
                    level.Name = this.name;
                    ScreenManager.AddScreen(level);
                }
                else if (Selected == 2)
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
