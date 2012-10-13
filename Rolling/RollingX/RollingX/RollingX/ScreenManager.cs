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
using System.Text;
using Rolling;
using System.Diagnostics;

namespace Rolling
{
    static class ScreenManager
    {
        static private List<GameScreen> ScreenStack = new List<GameScreen>();
        static private ContentManager content;
        static private GraphicsDevice device;

        public static void Init(ContentManager mcontent, GraphicsDevice mdevice)
        {
            content = mcontent;
            device = mdevice;
        }

        public static ContentManager getManager()
        {
            return content;
        }

        public static GraphicsDevice getDevice()
        {
            return device;
        }

        public static void AddScreen(GameScreen toAdd)
        {
            toAdd.Initialize();
            toAdd.LoadContent();
            ScreenStack.Add(toAdd);
        }

        public static void RemoveScreen(GameScreen toRemove)
        {
            toRemove.UnloadContent();
            ScreenStack.Remove(toRemove);
        }

        public static void RemoveAll()
        {
            ScreenStack.Clear();
        }

        public static void Update(GameTime time)
        {
            if (ScreenStack.Count > 0)
            {
                ScreenStack[ScreenStack.Count - 1].Update(time);
            }
        }

        public static void Draw(GameTime time)
        {
            foreach (GameScreen temp in ScreenStack)
            {
                temp.Draw(time);
            }
        }

        public static int getScreenCount()
        {
            return ScreenStack.Count;
        }
    }
}
