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
#if WINDOWS
using System.Net.Sockets;
#endif
using System.Text;
using Rolling;
using System.Diagnostics;

namespace Rolling
{
    class GamePlayScreen : GameScreen
    {
        //test stuff
        float h;
        Vector3 norm;
        double a;
        double b;

        static bool debugging = true;
        GraphicsDevice device;
        ContentManager content;
    public ContentManager Content
    {
        get { return content; }
    }
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        ChaseCamera camera;
        Ball ball;
        Model ballModel;
        Model pitModel;
        public Texture2D pitTextureLava;
        public Texture2D pitTextureIce;
        public static float pitHeight = -1000;
        Model arrowModel;
        Model terrainModel;
        String terrain;
        public String Terrain
        { 
            set { terrain = value; }
        }
        Model exitModel;
        //Model exitModelText;
        Sky sky;
        public HeightMapInfo heightMap;
        Texture2D rockTexture;
        String terrainTexture;
        public static Texture2D scoreboardTexture;
        int level;
        public int Level{
            get { return level; }
        set { level = value; }
    }
        public String TerrainTexture
        {
            set { terrainTexture = value; }
        }
        String background;
        public String Background
        {
            set { background = value; }
        }
        String name;
        public String Name
        {
            set { name = value; }
        }

        /*Declare properties for different balls*/
        public BallProperties currentBallProperties;
        BallProperties beachBallProperties;
        BallProperties metalBallProperties;
        BallProperties airBallProperties;

        Texture2D ballTextureBeach;
        Texture2D ballTextureMetal;
        Texture2D ballTextureAir;

        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        MouseState lastMousState = new MouseState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        MouseState currentMouseState = new MouseState();
        Effect effect;

        Model boxModel;

        Vector3 ExitPosition;
        BoundingBox ExitBox;

        Texture2D coinTexture;
        List<Coin> coinList = new List<Coin>();
        Texture2D teleporterTexture;
        List<teleporter> teleporterList = new List<teleporter>();
        public Random random = new Random();

        Matrix ArrowRotation;
        Matrix ExitRotation;
        public long timeInMillis;
        float LevelTime = 50000.0f;

        ParticleEngine particleEngine;

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        override public void Initialize()
        {
            content = ScreenManager.getManager();
            device = ScreenManager.getDevice();
            //connect();
        }

#if WINDOWS
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
#endif

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        override public void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(device);
            spriteFont = content.Load<SpriteFont>("Arial");

            camera = new ChaseCamera(heightMap);
            // Set the camera offsets
            camera.DesiredPositionOffset = new Vector3(0.0f, 100.0f, 200.0f);
            camera.LookAtOffset = new Vector3(0f, 15.0f, 30.0f);

            ball = new Ball(this, device, "Models\\balltest");
            ball.scale = Vector3.One * 15f;

            camera.NearPlaneDistance = 10.0f;
            camera.FarPlaneDistance = 100000.0f;
            camera.AspectRatio = (float)device.Viewport.Width /
               device.Viewport.Height;
            camera.ChasePosition = ball.position;
            camera.ChaseDirection = ball.Direction;
            camera.Up = ball.Up;
            camera.Reset();

            ballModel = content.Load<Model>("Models\\balltest");
            pitModel = content.Load<Model>("Models\\ground");
            boxModel = content.Load<Model>("Models\\cube");
            arrowModel = content.Load<Model>("Models\\arrow");
            exitModel = content.Load<Model>("Models\\exit2");
            //exitModelText = content.Load<Model>("Models\\exitSign");
            ballTextureBeach = content.Load<Texture2D>("Textures\\beach");
            ballTextureMetal = content.Load<Texture2D>("Textures\\metal");
            ballTextureAir = content.Load<Texture2D>("Textures\\air");
            coinTexture = content.Load<Texture2D>("Textures\\coin");
            teleporterTexture = content.Load<Texture2D>("Textures\\teleporter");
            terrainModel = content.Load<Model>(terrain);
            rockTexture = content.Load<Texture2D>(terrainTexture);
            pitTextureLava = content.Load<Texture2D>("Textures\\lava");
            pitTextureIce = content.Load<Texture2D>("Textures\\3");
            scoreboardTexture = content.Load<Texture2D>("Textures\\highscores");
            heightMap = terrainModel.Tag as HeightMapInfo;
            if (heightMap == null)
            {
                string message = "The terrain model did not have a HeightMapInfo";
                throw new InvalidOperationException(message);
            }
            
            Sound.playSoundLoop(background, content);
            effect = content.Load<Effect>("Lighting/Effects");
            sky = content.Load<Sky>("Textures\\sky");

            /*Initialize properties for each ball*/
            beachBallProperties = new BallProperties(ballTextureBeach, 1.0f, 0.5f, 900f, 1f, new Vector3(0.97f, 1f, 0.97f), new Vector3(1f, 1f, 1f), "Sound\\jump", "Sound\\jump", "Sound\\changeBall");
            metalBallProperties = new BallProperties(ballTextureMetal, 3.0f, 0.15f, 750f, 3f, new Vector3(0.95f, 1f, 0.95f), new Vector3(1f, 1f, 1f), "Sound\\jump", "Sound\\jump", "Sound\\changeBall");
            airBallProperties = new BallProperties(ballTextureAir, 1f, 0.05f, 1200f, 0.8f, new Vector3(0.99f, 0.99f, 0.99f), new Vector3(0.99f, 0.99f, 0.99f), "Sound\\jump", "Sound\\jump", "Sound\\changeBall");
            currentBallProperties = beachBallProperties;

            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(Content.Load<Texture2D>("Textures\\circle"));
            textures.Add(Content.Load<Texture2D>("Textures\\star"));
            textures.Add(Content.Load<Texture2D>("Textures\\diamond"));
            particleEngine = new ParticleEngine(textures, new Vector2(100, 40));

            //ExitPosition = new Vector3(1405.1f, .90952f, -2367.78f);
            ExitPosition = new Vector3(2000, 0f, 2000);
            ExitPosition.Y = getHeight(ExitPosition)+50f;
            ExitBox = new BoundingBox(ExitPosition - new Vector3(50, 50, 50), ExitPosition + new Vector3(50, 50, 50));
            for (int x = 0; x < 50; x++)
            {
                Vector3 Pos = new Vector3(RandomNumber(-2000, 2000), 0, RandomNumber(-2000, 2000));
                //Vector3 Pos = new Vector3(0, 0, 0);
                Vector3 Normal;
                float Height;
                heightMap.GetHeightAndNormal(Pos, out Height, out Normal);
                Pos.Y = Height + 20;
                //System.Diagnostics.Debug.WriteLine(Pos);
                coinList.Add(new Coin(Pos, content));
            }
            for (int x = 0; x < 20; x++)
            {
                Vector3 Pos = new Vector3(RandomNumber(-2500, 3000), 0, RandomNumber(-2500, 3000));
                Vector3 Normal = new Vector3();
                float Height = 10;
                heightMap.GetHeightAndNormal(Pos, out Height, out Normal);
                Pos.Y = Height+50;
                //System.Diagnostics.Debug.WriteLine(Pos);
                teleporterList.Add(new teleporter(Pos, content));
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        override public void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        Matrix RotateToFace(Vector3 O, Vector3 P, Vector3 U)
        {
            Vector3 D = (O - P);
            Vector3 Right = Vector3.Cross(U, D);
            Vector3.Normalize(ref Right, out Right);
            Vector3 Backwards = Vector3.Cross(Right, U);
            Vector3.Normalize(ref Backwards, out Backwards);
            Vector3 Up = Vector3.Cross(Backwards, Right);
            Matrix rot = new Matrix(Right.X, Right.Y, Right.Z, 0, Up.X, Up.Y, Up.Z, 0, Backwards.X, Backwards.Y, Backwards.Z, 0, 0, 0, 0, 1);
            return rot;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        override public void Update(GameTime gameTime)
        {
            timeInMillis += gameTime.ElapsedGameTime.Milliseconds;
            LevelTime -= gameTime.ElapsedGameTime.Milliseconds;

            if (LevelTime < 0 || ball.position.Y <= pitHeight)
            {
                Sound.pauseSound("Sound\\"+this.level,content);
                GameOver startOver = new GameOver();
                startOver.Level = this.level;
                startOver.Name = this.name;
                ScreenManager.AddScreen(startOver);
            }

            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            lastMousState = currentMouseState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();

            Int32 middleX = device.Viewport.Bounds.Width / 2;
            Int32 middleY = device.Viewport.Bounds.Height / 2; ;

#if XBOX
            // Exit when the Escape key or Back button is pressed
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                ScreenManager.RemoveScreen(this);
            }

            if (!ball.onGround && currentGamePadState.IsButtonDown(Buttons.B) && !lastGamePadState.IsButtonDown(Buttons.B))
            {
                ball.dash();
            }
            else if (currentGamePadState.IsButtonDown(Buttons.A) && !lastGamePadState.IsButtonDown(Buttons.A))
                if (ball.onGround && !ball.jumpDelayTimer.IsRunning)
                {
                    //ball.dashDelayTimer.Start();
                    ball.jump();
                }

            if (currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                Vector3 offset = camera.DesiredPositionOffset;
                offset.Y = MathHelper.Clamp(offset.Y += 2, 20, 150);
                if (Math.Abs(offset.Y - camera.DesiredPositionOffset.Y) > 0.0001)
                    Sound.playSoundOnce("Sound\\rotateCam", content);
                camera.DesiredPositionOffset = offset;
            }
            else if (currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                Vector3 offset = camera.DesiredPositionOffset;
                offset.Y = MathHelper.Clamp(offset.Y -= 2, 20, 150);
                if (Math.Abs(offset.Y - camera.DesiredPositionOffset.Y) > 0.0001)
                    Sound.playSoundOnce("Sound\\rotateCam", content);
                camera.DesiredPositionOffset = offset;
            }

            if (currentGamePadState.IsButtonDown(Buttons.LeftShoulder) && !lastGamePadState.IsButtonDown(Buttons.LeftShoulder))
            {
                if (currentBallProperties.Equals(beachBallProperties))
                    currentBallProperties = airBallProperties;
                else if (currentBallProperties.Equals(airBallProperties))
                    currentBallProperties = metalBallProperties;
                else if (currentBallProperties.Equals(metalBallProperties))
                    currentBallProperties = beachBallProperties;
                Sound.playSoundOnce(currentBallProperties.ChangeToSound, content);
            }
            else if (currentGamePadState.IsButtonDown(Buttons.RightShoulder) && !lastGamePadState.IsButtonDown(Buttons.RightShoulder))
            {
                if (currentBallProperties.Equals(beachBallProperties))
                    currentBallProperties = metalBallProperties;
                else if (currentBallProperties.Equals(metalBallProperties))
                    currentBallProperties = airBallProperties;
                else if (currentBallProperties.Equals(airBallProperties))
                    currentBallProperties = beachBallProperties;
                Sound.playSoundOnce(currentBallProperties.ChangeToSound, content);
            }

            else if (currentGamePadState.IsButtonDown(Buttons.Back) && !lastGamePadState.IsButtonDown(Buttons.Back))
            {
                Sound.pauseSound(background, content);
                ScreenManager.AddScreen(new MainMenu());
            }

            if (currentGamePadState.IsButtonDown(Buttons.RightStick) && !lastGamePadState.IsButtonDown(Buttons.RightStick))
            {
                ball.Reset();
                camera.Reset();
            }
#endif
#if WINDOWS
            // Exit when the Escape key or Back button is pressed
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                ScreenManager.RemoveScreen(this);
            }

            if (!ball.onGround && !lastKeyboardState.IsKeyDown(Keys.Space) && currentKeyboardState.IsKeyDown(Keys.Space))
            {
                ball.dash();
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Space))
                if (ball.onGround && !ball.jumpDelayTimer.IsRunning)
                {
                    //ball.dashDelayTimer.Start();
                    ball.jump();
                }

            if (currentKeyboardState.IsKeyDown(Keys.I) || (currentMouseState.RightButton == ButtonState.Pressed) && (currentMouseState.Y>middleY))
            {
                Vector3 offset = camera.DesiredPositionOffset;
                offset.Y = MathHelper.Clamp(offset.Y += 2, 20, 150);
                if (Math.Abs(offset.Y - camera.DesiredPositionOffset.Y) > 0.0001)
                    Sound.playSoundOnce("Sound\\rotateCam", content);
                camera.DesiredPositionOffset = offset;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.K) || (currentMouseState.RightButton == ButtonState.Pressed) && (currentMouseState.Y <= middleY))
            {
                Vector3 offset = camera.DesiredPositionOffset;
                offset.Y = MathHelper.Clamp(offset.Y -= 2, 20, 150);
                if (Math.Abs(offset.Y - camera.DesiredPositionOffset.Y) > 0.0001)
                    Sound.playSoundOnce("Sound\\rotateCam", content);
                camera.DesiredPositionOffset = offset;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Q) && !lastKeyboardState.IsKeyDown(Keys.Q))
            {
                if (currentBallProperties.Equals(beachBallProperties))
                    currentBallProperties = airBallProperties;
                else if (currentBallProperties.Equals(airBallProperties))
                    currentBallProperties = metalBallProperties;
                else if (currentBallProperties.Equals(metalBallProperties))
                    currentBallProperties = beachBallProperties;
                Sound.playSoundOnce(currentBallProperties.ChangeToSound, content);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.E) && !lastKeyboardState.IsKeyDown(Keys.E))
            {
                if (currentBallProperties.Equals(beachBallProperties))
                    currentBallProperties = metalBallProperties;
                else if (currentBallProperties.Equals(metalBallProperties))
                    currentBallProperties = airBallProperties;
                else if (currentBallProperties.Equals(airBallProperties))
                    currentBallProperties = beachBallProperties;
                Sound.playSoundOnce(currentBallProperties.ChangeToSound, content);
            }

            else if (currentKeyboardState.IsKeyDown(Keys.M))
            {
                Sound.pauseSound(background, content);
                ScreenManager.AddScreen(new MainMenu());
            }

            if (currentKeyboardState.IsKeyDown(Keys.Z))
            {
                System.Diagnostics.Debug.WriteLine(ball.position);
            }

            bool touchTopLeft = currentMouseState.LeftButton == ButtonState.Pressed &&
                    lastMousState.LeftButton != ButtonState.Pressed &&
                    currentMouseState.X < device.Viewport.Width / 10 &&
                    currentMouseState.Y < device.Viewport.Height / 10;

            if (currentKeyboardState.IsKeyDown(Keys.R) ||
                currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                ball.Reset();
                camera.Reset();
            }
#endif
            ArrowRotation = RotateToFace(ball.position, ExitPosition, Vector3.Up);
            ExitRotation = RotateToFace(ExitPosition, ball.position, Vector3.Up);

            if (currentKeyboardState.IsKeyDown(Keys.P))
            {

                Sound.playSoundOnce("Sound\\victory", content);

                clearScore(1);
                clearScore(2);
                clearScore(3);
                clearScore(4);
                clearScore(5);
            }

            if (currentKeyboardState.IsKeyDown(Keys.O) || ExitBox.Contains(ball.position) == ContainmentType.Contains)
            {

                long nowTicks = DateTime.Now.Ticks;
#if WINDOWS
                sendScore(name, level, (int)(LevelTime), nowTicks);
#endif
                VictoryScreen victoryScreen = new VictoryScreen();
                victoryScreen.Level = this.level;
                victoryScreen.Name = this.name;
                victoryScreen.Ticks = nowTicks;
                ScreenManager.AddScreen(victoryScreen);
            }

            //Test stuff
            if (heightMap.IsOnHeightmap(ball.position))
            {
                heightMap.GetHeightAndNormal(ball.position, out h, out norm);
                norm.Normalize();
                h = Vector3.Dot(new Vector3(0, 1, 0), norm);
                a = Math.Acos(h);
                //a = getSignedAngle(new Vector3(0, 1, 0), norm);
                b = Math.Atan2(norm.Y, norm.X);
            }

            for (int x = 0; x < coinList.Count; x++ )
            {
                coinList[x].Update(gameTime);
                if (coinList[x].detectIntersection(ball.position))
                {
                    LevelTime += 5000f;
                    Sound.playSoundOnce("Sound\\ding", content);
                    coinList.RemoveAt(x);
                }
            }

            for (int x = 0; x < teleporterList.Count; x++)
            {
                teleporterList[x].Update(gameTime);
                if (teleporterList[x].detectIntersection(ball.position))
                {
                    Sound.playSoundOnce("Sound\\pop",content);
                    ball.position = new Vector3(RandomNumber(-2500, 3000), 0, RandomNumber(-2500, 3000)); 
                }
            }

            ball.properties = currentBallProperties;
            ball.Update(gameTime);
            camera.ChasePosition = ball.position;
            camera.ChaseDirection = ball.Direction;
            camera.Up = ball.Up;
            camera.Reset();

            particleEngine.EmitterLocation = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            particleEngine.Update();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        override public void Draw(GameTime gameTime)
        {
            device.Clear(Color.CornflowerBlue);
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.SamplerStates[0] = SamplerState.LinearWrap;

            sky.Draw(camera.View, camera.Projection);
            DrawModelWithEffect(terrainModel, Matrix.Identity, rockTexture);
            DrawModelWithEffect(ballModel, ball.World, currentBallProperties.Texture);
            DrawModel(arrowModel, Matrix.CreateScale(.5f) * ArrowRotation * Matrix.CreateTranslation(ball.position) * Matrix.CreateTranslation(0, 50f, 0), null);
            DrawModel(exitModel, Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(ExitPosition), null);
            DrawModel(pitModel, Matrix.CreateTranslation(new Vector3(0, pitHeight, 0)), terrainTexture.Equals("Textures\\2") ? pitTextureIce : pitTextureLava);

            foreach (Coin x in coinList)
            {
                DrawModel(x.getModel(), Matrix.CreateScale(.2f) * x.getWorld(), coinTexture);
            }

            foreach (teleporter x in teleporterList)
            {
                DrawModel(x.getModel(), Matrix.CreateScale(2f) * x.getWorld(), teleporterTexture);
            }

            particleEngine.Draw(spriteBatch);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, "Time Left: " + (LevelTime / 1000).ToString("#0.0"), new Vector2(device.Viewport.Bounds.Width - 175, device.Viewport.Bounds.Height - 50), Color.White);
            if (debugging)
            {
                spriteBatch.DrawString(spriteFont, "Arrow keys or WASD to move; I/K to move camera; E/Q to change ball; Space to jump", new Vector2(16, 32), Color.White);
                spriteBatch.DrawString(spriteFont, "Time: " + ((double)(timeInMillis / 100) / 10).ToString("#0.0"), new Vector2(32, 64), Color.White);
                spriteBatch.DrawString(spriteFont, "Gravity dot with normal: Angle(" + (a).ToString("0.00") + ") X(" + Math.Cos(a).ToString("0.00")
                    + ") Y(" + Math.Sin(a).ToString("0.00") + ") Z(" + Math.Cos(Math.PI / 2 - a).ToString("0.00") + ")", new Vector2(32, 96), Color.White);
                spriteBatch.DrawString(spriteFont, "Heights: ground(" + ball.getHeight(ball.position) + ") ball+rad(" + (ball.position.Y + ball.ballRadius) + ")", new Vector2(32, 128), Color.White);
                spriteBatch.DrawString(spriteFont, "Normal: X(" + norm.X.ToString("0.000") + ") Y(" + norm.Y.ToString("0.000")
                    + ") Z(" + norm.Z.ToString("0.000") + ")", new Vector2(32, 160), Color.White);
                spriteBatch.DrawString(spriteFont, "Acceleration: X(" + ball.acceleration.X.ToString("000.000") + ") Y(" + ball.acceleration.Y.ToString("000.000")
                    + ") Z(" + ball.acceleration.Z.ToString("000.000") + ")", new Vector2(32, 192), Color.White);
                spriteBatch.DrawString(spriteFont, "Velocity: X(" + Math.Abs(ball.velocity.X).ToString("000.000") + ") Y(" + Math.Abs(ball.velocity.Y).ToString("000.000")
                    + ") Z(" + Math.Abs(ball.velocity.Z).ToString("000.000") + ")", new Vector2(32, 224), Color.White);
                spriteBatch.DrawString(spriteFont, "Position: X(" + Math.Abs(ball.position.X).ToString("000.000") + ") Y(" + Math.Abs(ball.position.Y).ToString("000.000")
                    + ") Z(" + Math.Abs(ball.position.Z).ToString("000.000") + ")", new Vector2(32, 256), Color.White);
            }
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
                    effect.CurrentTechnique=effect.Techniques["Textured"];
                    effect.Parameters["xWorld"].SetValue(transforms[mesh.ParentBone.Index] * world);
                    effect.Parameters["xView"].SetValue(camera.View);
                    effect.Parameters["xProjection"].SetValue(camera.Projection);
                    effect.Parameters["xTexture"].SetValue(texture);
                    effect.Parameters["xEnableLighting"].SetValue(true);

                    //ambient light
                    effect.Parameters["xAmbient"].SetValue(0.3f);

                    //directional light
                    effect.Parameters["xLightDirection"].SetValue(new Vector3(0, 0, -1));
                    effect.Parameters["xDirLightIntensity"].SetValue(1f); 

                           //point light
                           effect.Parameters["xPointLight1"].SetValue(new Vector3(100.0f, 10.0f, -87.0f));
                           effect.Parameters["xPointIntensity1"].SetValue(10.0f);
                           effect.Parameters["xPointLight2"].SetValue(new Vector3(50.0f, 10.0f, -87.0f));
                           effect.Parameters["xPointIntensity2"].SetValue(10.0f);
                           effect.Parameters["xPointLight3"].SetValue(new Vector3(30.0f, 10.0f, -87.0f));
                           effect.Parameters["xPointIntensity3"].SetValue(10.0f);
                    
                    //spot light
                    effect.Parameters["xSpotPos"].SetValue(ball.position);
                    effect.Parameters["xSpotDir"].SetValue(ball.position + new Vector3(0f, 100.0f, 0f));
                    effect.Parameters["xSpotInnerCone"].SetValue(0.4f);
                    effect.Parameters["xSpotOuterCone"].SetValue(1.1f);
                    effect.Parameters["xSpotRange"].SetValue(15.0f);

                    //specular light
                    effect.Parameters["specularShininess"].SetValue(50000.0f);
                    effect.Parameters["specularColor"].SetValue(Color.White.ToVector4()); //set the specular light color
                    effect.Parameters["specularIntensity"].SetValue(2.0f); //set the specular light intensity
                }
                mesh.Draw();
            }
        }
#if WINDOWS
        public bool sendScore(String name, int level, int score, long ticks)
        {
            if (level > 5 || level < 0)
                throw new ArgumentOutOfRangeException("level must be between 0 and 5");
            if (name.Equals(""))
                name = "???";
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
                return false;
            }
            string command = "HIGHSCORE-INSERT-" + name + "-" + level + "-" + score + "-" + ticks + "\r\n";
            Console.WriteLine("Sending command \"" + command + "\".");
            server.Send(Encoding.ASCII.GetBytes(command));
            byte[] data = new byte[1024];
            Console.WriteLine("Sent command \"" + command + "\".");
            int receivedDataLength = server.Receive(data);
            Console.WriteLine("Received command \"" + command + "\".");
            string stringData = Encoding.ASCII.GetString(data, 0, receivedDataLength);
            Console.WriteLine("DATA FROM SERVER: " + stringData);
            server.Close();
            return true;
        }

        public static bool clearScore(int level)
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
                return false;
            }
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
            return true;
        }

        public static bool getScores(int level, out int numberOfEntries, out String[] names, out int[] scores, out long[] datesInMillis)
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
                return false;
            }
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
            return false;
        }

#endif
        public static double getSignedAngle(Vector3 originVector, Vector3 destVector)
        {
            Vector3 destVectorsRight = Vector3.Cross(destVector, Vector3.Up);
            originVector.Normalize();
            destVector.Normalize();
            destVectorsRight.Normalize();

            float forwardDot = Vector3.Dot(originVector, destVector);
            float rightDot = Vector3.Dot(originVector, destVectorsRight);

            // Keep dot in range to prevent rounding errors
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            double angleBetween = Math.Acos(forwardDot);

            if (rightDot < 0.0f)
                angleBetween *= -1.0f;

            return angleBetween;
        }

        private int RandomNumber(int min, int max)
        {
            return random.Next(min, max);
        }


        public float getHeight(Vector3 Position)
        {
            if (heightMap.IsOnHeightmap(Position))
            {
                float height;
                Vector3 normal;
                heightMap.GetHeightAndNormal(Position, out height, out normal);
                return height;
            }
            else return 0;
        }
    }
}