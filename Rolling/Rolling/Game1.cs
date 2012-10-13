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
using System.Net.Sockets;
using System.Text;
using Rolling;

namespace Rolling
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        ChaseCamera camera;
        Ball ball;
        Model ballModel;
        Model groundModel;
        Model arrowModel;
        Model terrainModel;
        Model exitModel;
        Texture2D ballTexture;
        Texture2D ballTextureBeach;
        Texture2D ballTextureMetal;
        Texture2D rockTexture;
        Sky sky;
        public HeightMap heightMap;

        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        MouseState lastMousState = new MouseState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        MouseState currentMouseState = new MouseState();
        Effect effect;

        Model boxModel;
        GameObject box;
        public long timeInMillis;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            Content.RootDirectory = "Content";
            camera = new ChaseCamera(heightMap);
            IsMouseVisible = true;
            // Set the camera offsets
            camera.DesiredPositionOffset = new Vector3(0.0f, 100.0f, 200.0f);
            camera.LookAtOffset = new Vector3(0f, 15.0f, 30.0f);

            camera.NearPlaneDistance = 10.0f;
            camera.FarPlaneDistance = 100000.0f;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            ball = new Ball(this, GraphicsDevice, "Models\\balltest");
            ball.scale = Vector3.One * 15f;
            ballTexture = ballTextureBeach;
            camera.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
               graphics.GraphicsDevice.Viewport.Height;
            camera.ChasePosition = ball.Position;
            camera.ChaseDirection = ball.Direction;
            camera.Up = ball.Up;
            camera.Reset();
            connect();
        }

        protected void connect()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("128.143.69.241"), 9050);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ip);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Unable to connect to server.");
                return;
            }
            server.Send(Encoding.ASCII.GetBytes("GetLogCount\r\n"));
            byte[] data = new byte[1024];
            int receivedDataLength = server.Receive(data);
            string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
            Console.WriteLine("DATA FROM SERVER: " + stringData);
            //server.Close();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Arial");

            ballModel = Content.Load<Model>("Models\\balltest");
            groundModel = Content.Load<Model>("Models\\ground");
            boxModel = Content.Load<Model>("Models\\cube");
            arrowModel = Content.Load<Model>("Models\\arrow");
            exitModel = Content.Load<Model>("Models\\exit");
            ballTextureBeach = Content.Load<Texture2D>("Textures\\ball");
            ballTextureMetal = Content.Load<Texture2D>("Models\\tile");
            terrainModel = Content.Load<Model>("Models\\terrain");
            rockTexture = Content.Load<Texture2D>("Models\\rocks");
            heightMap = terrainModel.Tag as HeightMap;
            if (heightMap == null)
            {
                string message = "The terrain model did not have a HeightMapInfo";
                throw new InvalidOperationException(message);
            }
            Sound.playSoundLoop("Sound\\background", Content);
            effect = Content.Load<Effect>("Lighting/Effects");
            sky=Content.load<Sky>("Textures\\sky");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            timeInMillis += gameTime.ElapsedGameTime.Milliseconds;

            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            lastMousState = currentMouseState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();

            // Exit when the Escape key or Back button is pressed
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            if (currentKeyboardState.IsKeyDown(Keys.Space))
                if (ball.onGround && !ball.jumpDelayTimer.IsRunning)
                {
                    ball.ApplyForce(new Vector3(0, 300, 0));
                    Sound.playSoundOnce("Sound\\jump", Content);
                }

            if (currentKeyboardState.IsKeyDown(Keys.I))
            {
                Vector3 offset = camera.DesiredPositionOffset;
                offset.Y = MathHelper.Clamp(offset.Y += 2, 20, 150);
                camera.DesiredPositionOffset = offset;
                Sound.playSoundOnce("Sound\\rotateCam", Content);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.K))
            {
                Vector3 offset = camera.DesiredPositionOffset;
                offset.Y = MathHelper.Clamp(offset.Y -= 2, 20, 150);
                camera.DesiredPositionOffset = offset;
                Sound.playSoundOnce("Sound\\rotateCam", Content);
            }

            if (currentKeyboardState.IsKeyDown(Keys.Q) && !lastKeyboardState.IsKeyDown(Keys.Q))
            {
                if (ballTexture.Equals(ballTextureBeach))
                    ballTexture = ballTextureMetal;
                else if (ballTexture.Equals(ballTextureMetal))
                    ballTexture = ballTextureBeach;
                Sound.playSoundOnce("Sound\\changeBall", Content);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.E) && !lastKeyboardState.IsKeyDown(Keys.E))
            {
                if (ballTexture.Equals(ballTextureBeach))
                    ballTexture = ballTextureMetal;
                else if (ballTexture.Equals(ballTextureMetal))
                    ballTexture = ballTextureBeach;
                Sound.playSoundOnce("Sound\\changeBall", Content);
            }
            if (currentKeyboardState.IsKeyDown(Keys.N))
            {
                Sound.pauseSound("Sound\\background", Content);
                Sound.playSoundLoop("Sound\\background2", Content);
            }
            if (currentKeyboardState.IsKeyDown(Keys.M))
            {
                Sound.pauseSound("Sound\\background2", Content);
                Sound.playSoundLoop("Sound\\background", Content);
            }

            bool touchTopLeft = currentMouseState.LeftButton == ButtonState.Pressed &&
                    lastMousState.LeftButton != ButtonState.Pressed &&
                    currentMouseState.X < GraphicsDevice.Viewport.Width / 10 &&
                    currentMouseState.Y < GraphicsDevice.Viewport.Height / 10;

            if (currentKeyboardState.IsKeyDown(Keys.R) ||
                currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                ball.Reset();
                camera.Reset();
            }

            //if (currentKeyboardState.IsKeyDown(Keys.Z))
            //{
            //    IPEndPoint ip = new IPEndPoint(IPAddress.Parse("128.143.69.241"), 9050);
            //    Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //    try
            //    {
            //        server.Connect(ip);
            //    }
            //    catch (SocketException e)
            //    {
            //        Console.WriteLine("Unable to connect to server.");
            //        return;
            //    }
            //    server.Send(Encoding.ASCII.GetBytes("HIGHSCORE-INSERT-1-5000"));
            //    byte[] data = new byte[1024];
            //    int receivedDataLength = server.Receive(data);
            //    string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
            //    Console.WriteLine("DATA FROM SERVER: " + stringData);
            //}

            ball.Update(gameTime);
            camera.ChasePosition = ball.Position;
            camera.ChaseDirection = ball.Direction;
            camera.Up = ball.Up;
            camera.Reset();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;
            device.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            sky.Draw(camera.View, camera.Projection);
            DrawModelWithEffect(terrainModel, Matrix.Identity, rockTexture);
            DrawModelWithEffect(ballModel, ball.World, ballTexture);
            DrawModel(arrowModel, Matrix.CreateScale(0.03f) * ball.World * Matrix.CreateTranslation(0, 50f, 0), null);
            //DrawModelWithEffect(exitModel, Matrix.CreateScale(4f) * Matrix.Identity*Matrix.CreateTranslation(950f, -560f, -1020), null);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, "Arrow keys or WASD to move; I/K to move camera; E/Q to change ball; Space to jump, N/M to change music", new Vector2(16, 32), Color.White);
            spriteBatch.DrawString(spriteFont, "Time: " + ((double)(timeInMillis / 100) / 10).ToString("#0.0"), new Vector2(32, 64), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawModel(Model model, Matrix world, Texture2D texture)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.CurrentTechnique = effect.Techniques["Textured"];
                    effect.Parameters["xWorld"].SetValue(transforms[mesh.ParentBone.Index] * world);
                    effect.Parameters["xView"].SetValue(camera.View);
                    effect.Parameters["xProjection"].SetValue(camera.Projection);
                    effect.Parameters["xTexture"].SetValue(texture);
                    effect.Parameters["xEnableLighting"].SetValue(false);
                    //ambient light
                    effect.Parameters["xAmbient"].SetValue(0.3f);
                    //direcitonal light
                    effect.Parameters["xLightDirection"].SetValue(new Vector3(-1.0f, -0.5f, 1.0f));
                    effect.Parameters["xDirLightIntensity"].SetValue(0.2f);
                }
                mesh.Draw();
            }
        }

        private void DrawModelWithEffect(Model model, Matrix world, Texture2D texture)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.CurrentTechnique = effect.Techniques["Textured"];
                    effect.Parameters["xWorld"].SetValue(transforms[mesh.ParentBone.Index] * world);
                    effect.Parameters["xView"].SetValue(camera.View);
                    effect.Parameters["xProjection"].SetValue(camera.Projection);
                    effect.Parameters["xTexture"].SetValue(texture);
                    effect.Parameters["xEnableLighting"].SetValue(true);

                    //ambient light
                    effect.Parameters["xAmbient"].SetValue(1f);

                    //directional light
                    effect.Parameters["xLightDirection"].SetValue(new Vector3(0, 0, -1));
                    effect.Parameters["xDirLightIntensity"].SetValue(0.5f);

                    //point light
                    effect.Parameters["xPointLight1"].SetValue(new Vector3(100.0f, 10.0f, -87.0f));
                    effect.Parameters["xPointIntensity1"].SetValue(0.2f);
                    effect.Parameters["xPointLight2"].SetValue(new Vector3(50.0f, 10.0f, -87.0f));
                    effect.Parameters["xPointIntensity2"].SetValue(0.2f);
                    effect.Parameters["xPointLight3"].SetValue(new Vector3(30.0f, 10.0f, -87.0f));
                    effect.Parameters["xPointIntensity3"].SetValue(0.1f);

                    //spot light
                    effect.Parameters["xSpotPos"].SetValue(ball.Position);
                    effect.Parameters["xSpotDir"].SetValue(ball.Position + new Vector3(0f, 100.0f, 0f));
                    effect.Parameters["xSpotInnerCone"].SetValue(0.4f);
                    effect.Parameters["xSpotOuterCone"].SetValue(1.1f);
                    effect.Parameters["xSpotRange"].SetValue(15.0f);

                    //specular light
                    effect.Parameters["specularShininess"].SetValue(50000.0f);
                    effect.Parameters["specularColor"].SetValue(Color.White.ToVector4()); //set the specular light color
                    effect.Parameters["specularIntensity"].SetValue(0.3f); //set the specular light intensity
                }
                mesh.Draw();
            }
        }


        public void sendScore(String name, int level, int score)
        {
            if (level > 5 || level < 0)
                throw new ArgumentOutOfRangeException("level must be between 0 and 5");
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("128.143.69.241"), 9050);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Console.WriteLine("Trying connection to server.");
                server.Connect(ip);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Unable to connect to server.");
                return;
            }
            double rando = new System.Random().NextDouble();
            string command = "HIGHSCORE-INSERT-" + name + "-" + level + "-" + score + "\r\n";
            Console.WriteLine("Sending command \"" + command + "\".");
            server.Send(Encoding.ASCII.GetBytes(command));
            byte[] data = new byte[1024];
            Console.WriteLine("Sent command \"" + command + "\".");
            int receivedDataLength = server.Receive(data);
            Console.WriteLine("Received command \"" + command + "\".");
            string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
            Console.WriteLine("DATA FROM SERVER: " + stringData);
            server.Close();
        }

        public void clearScore(int level)
        {
            if (level > 5 || level < 0)
                throw new ArgumentOutOfRangeException("level must be between 0 and 5");
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("128.143.69.241"), 9050);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Console.WriteLine("Trying connection to server.");
                server.Connect(ip);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Unable to connect to server.");
                return;
            }
            double rando = new System.Random().NextDouble();
            string command = "HIGHSCORE-CLEAR-" + level + "\r\n";
            Console.WriteLine("Sending command \"" + command + "\".");
            server.Send(Encoding.ASCII.GetBytes(command));
            byte[] data = new byte[1024];
            Console.WriteLine("Sent command \"" + command + "\".");
            int receivedDataLength = server.Receive(data);
            Console.WriteLine("Received command \"" + command + "\".");
            string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
            Console.WriteLine("DATA FROM SERVER: " + stringData);
            server.Close();
        }

        public void getScores(int level, out int numberOfEntries, out String[] names, out int[] scores, out long[] datesInMillis)
        {
            if (level > 5 || level < 0)
                throw new ArgumentOutOfRangeException("level must be between 0 and 5");
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("128.143.69.241"), 9050);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Console.WriteLine("Trying connection to server.");
                server.Connect(ip);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Unable to connect to server.");
                numberOfEntries = 0;
                names = null;
                scores = null;
                datesInMillis = null;
                return;
            }
            double rando = new System.Random().NextDouble();
            string command = "HIGHSCORE-GET-" + level + "\r\n";
            Console.WriteLine("Sending command \"" + command + "\".");
            server.Send(Encoding.ASCII.GetBytes(command));
            byte[] data = new byte[1024];
            Console.WriteLine("Sent command \"" + command + "\".");
            int receivedDataLength = server.Receive(data);
            Console.WriteLine("Received command \"" + command + "\".");
            string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
            Console.WriteLine("DATA FROM SERVER: " + stringData);
            //process returned data
            String[] splits = stringData.Split('|');
            numberOfEntries = 0;
            foreach (String s in splits)
            {
                if (!(s == null))
                    if (!s.Equals("") && !s.Equals("\n"))
                    {
                        numberOfEntries++;
                    }
            }
            String[] entries = new String[numberOfEntries];
            int index = 0;
            foreach (String s in splits)
            {
                if (!(s == null))
                    if (!s.Equals("") && !s.Equals("\n"))
                    {
                        entries[index] = s;
                        index++;
                        //Console.WriteLine("<<" + s + ">>");
                    }
            }
            names = new String[numberOfEntries];
            scores = new int[numberOfEntries];
            datesInMillis = new long[numberOfEntries];
            //Console.WriteLine("entries = " + numberOfEntries);
            for (int i = 0; i < numberOfEntries; i++)
            {
                //Console.WriteLine(entries[i]);
                String[] split = entries[i].Split('-');
                names[i] = split[0];
                scores[i] = Convert.ToInt32(split[1]);
                datesInMillis[i] = Convert.ToInt64(split[2]);
            }
            server.Close();
        }
    }
}