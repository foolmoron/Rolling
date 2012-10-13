using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Diagnostics;

namespace Rolling
{
    class Ball
    {
        public GamePlayScreen main;
        private GraphicsDevice graphicsDevice;

        public Vector3 acceleration;
        public Vector3 velocity;
        public Vector3 position;
        public Vector3 Direction;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector3 Up;
        private Vector3 right;
        public Vector3 rotationRate;
        public Vector3 modelRotation;
        public Vector3 DragFactor;
        public Vector3 DashDrag = new Vector3(0.95f, 1f, 0.95f);
        public float heightOfGround;
        public Vector3 normalToGround;
        public bool onGround;
        public float oldVelocityNormal = 0;
        public float newVelocityNormal = 0;

        public Model model;
        
        Matrix[] transforms;
        private Matrix world;
        string path; 
        public Vector3 Right
        {
            get { return right; }
        }
        public Matrix World
        {
            get { return world; }
        }
        public const int jumpDelayMillis = 0;
        public Stopwatch jumpDelayTimer = new Stopwatch();
        public const int dashDelayMillis = 250;
        public Stopwatch dashDelayTimer = new Stopwatch();
        public bool canDash = true;
        public bool canJump = true;

        public BallProperties properties;
        public float RotationRate = 1.5f;
        public float ballRadius = 15f;

        public Ball(GamePlayScreen main, GraphicsDevice device, string path)
        {
            this.main = main;
            this.path = path;
            graphicsDevice = device;
            position = new Vector3(-2500, 10.0f, -2500);
            Direction = Vector3.Forward;
            Up = Vector3.Up;
            right = Vector3.Right;
            rotation = Vector3.Zero;
            modelRotation = Vector3.Zero;
            scale = Vector3.One;
            acceleration = Vector3.Zero;
            velocity = Vector3.Zero;
            properties = main.currentBallProperties;
        }

        public void Reset()
        {
            position = new Vector3(-2000, 10.0f, -2000);
            Direction = Vector3.Forward;
            Up = Vector3.Up;
            right = Vector3.Right;
            rotation = Vector3.Zero;
            modelRotation = Vector3.Zero;
            scale = Vector3.One;
            acceleration = Vector3.Zero;
            velocity = Vector3.Zero;
            properties = main.currentBallProperties;
        }

        public void Load(ContentManager content)
        {
            model = content.Load<Model>(path);
            transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            MouseState mouseState = Mouse.GetState();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 rotationAmount = -gamePadState.ThumbSticks.Left;

            Int32 middleX = graphicsDevice.Viewport.Bounds.Width / 2;
            Int32 middleY = graphicsDevice.Viewport.Bounds.Height / 2; ;

            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A) || (mouseState.RightButton == ButtonState.Pressed) && (mouseState.X <= middleX))
            {
                rotationRate.X = 1.5f;
                rotationAmount.X = 1.0f;
            }
            else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D) || (mouseState.RightButton == ButtonState.Pressed) && (mouseState.X > middleX))
            {
                rotationRate.X = -1.5f;
                rotationAmount.X = -1.0f;
            }
            else
            {
                rotationRate.X = 0f;
            }

            acceleration = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W) || mouseState.LeftButton == ButtonState.Pressed)
            {
                rotationRate.Y = MathHelper.Clamp(rotationRate.Y + -0.5f, -10, 10);
                if (velocity.Length() < 2000f && onGround)
                {
                    acceleration = Direction * 800f;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                rotationRate.Y = MathHelper.Clamp(rotationRate.Y + 0.5f, -10, 10);
                if (velocity.Length() < 2000f && onGround) 
                {
                    acceleration = -Direction * 800f;
                }
            }
            else
            {
                rotationRate.Y *= DragFactor.X;
            }
            Vector3 nextPosition = position;
            getHeightAndNormal(position, out heightOfGround, out normalToGround);

            acceleration.Y = -50000f * properties.Gravity * elapsed;

            velocity += acceleration * elapsed;
            nextPosition += velocity * elapsed;

            float elasticity = (keyboardState.IsKeyDown(Keys.Space)) ? (1 + properties.Elasticity) / 2 : properties.Elasticity;
            if (nextPosition.Y <= heightOfGround + ballRadius)
            {
                if (dashDelayTimer.ElapsedMilliseconds > dashDelayMillis)
                    dashDelayTimer.Reset();
                canDash = true;
                canJump = true;
                position.Y = heightOfGround + ballRadius;
                DragFactor = properties.DragGround;
                getHeightAndNormal(position, out heightOfGround, out normalToGround);
                Vector3 normVel;
                Vector3.Normalize(ref velocity, out normVel);
                velocity = Vector3.Reflect(velocity, normalToGround);
                float dot = Vector3.Dot(normalToGround, normVel);
                oldVelocityNormal = newVelocityNormal;
                newVelocityNormal = dot*velocity.Length();
                Console.WriteLine(newVelocityNormal - oldVelocityNormal);
                if (!onGround)
                {
                    if (Math.Abs(newVelocityNormal - oldVelocityNormal) < 5)
                        velocity += dot * velocity.Length() * normalToGround;
                    else
                        velocity += (1 - elasticity) * dot * velocity.Length() * normalToGround;
                    Console.WriteLine("reflect " + velocity.Y);
                    canJump = false;
                }
                else
                {
                    if (Math.Abs(newVelocityNormal - oldVelocityNormal) < 5)
                        velocity += dot * velocity.Length() * normalToGround;
                    else
                        velocity += (1 - properties.Elasticity) * dot * velocity.Length() * normalToGround;
                }
                position += velocity * elapsed;
                onGround = true;
            }
            else
            {
                position = nextPosition;
                onGround = false;
                if (!canDash)
                    DragFactor = DashDrag;
                else
                    DragFactor = properties.DragAir;
            }
            velocity *= DragFactor;

            rotationAmount = rotationAmount * RotationRate * elapsed;
            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(velocity, rotationAmount.Y) *
                Matrix.CreateRotationY(rotationAmount.X);

            Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            Up = Vector3.TransformNormal(Up, rotationMatrix);
            Direction.Normalize();
            Up.Normalize();
            right = Vector3.Cross(Direction, Up);
            Up = Vector3.Cross(Right, Direction);

            modelRotation += rotationRate * elapsed;
            world = Matrix.CreateFromYawPitchRoll(modelRotation.X, modelRotation.Y, modelRotation.Z) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
        }

        public void jump()
        {
            if (!canJump || jumpDelayTimer.IsRunning)
                return;
            Vector3 jumpVector = Vector3.UnitY/2;
            jumpVector += normalToGround;
            Console.WriteLine("Jump: X(" + jumpVector.X.ToString("000.000") + ") Y(" + jumpVector.Y.ToString("000.000")
                + ") Z(" + jumpVector.Z.ToString("000.000") + ")");
            jumpVector.Normalize();
            velocity += jumpVector * 600;
            Sound.playSoundOnce(properties.JumpSound, main.Content);
        }

        public void dash()
        {
            if (!canDash || dashDelayTimer.IsRunning)
                return;
            dashDelayTimer.Start();
            canDash = false;
            acceleration.Y = 0;
            velocity.Y = 0;
            float heightDifference = MathHelper.Clamp(position.Y - Math.Max(heightOfGround, -1000f), 50f, 400f);
            velocity += Direction * properties.DashSpeed * heightDifference/200f;
            Console.WriteLine("Dash: " + properties.DashSpeed * heightDifference / 200f);
            Sound.playSoundOnce("Sound\\dash", main.Content);
        }

        public void applyForce(Vector3 force)
        {
            acceleration += force / properties.Mass;
        }

        public float getHeight(Vector3 Position)
        {
            if (main.heightMap.IsOnHeightmap(Position))
            {
                float height;
                Vector3 normal;
                main.heightMap.GetHeightAndNormal(Position, out height, out normal);
                return height;
            }
            else return 0;
        }

        private void getHeightAndNormal(Vector3 Position, out float height, out Vector3 normal)
        {
            if (main.heightMap.IsOnHeightmap(Position))
            {
                main.heightMap.GetHeightAndNormal(Position, out height, out normal);
            }
            else
            {
                height = 0;
                normal = new Vector3(0, 1, 0); 
                Sound.playSoundOnce("Sound\\pop", main.Content);
                position = new Vector3(main.random.Next(-2500, 3000), 0, main.random.Next(-2500, 3000)); 
            }
        }
    }
}