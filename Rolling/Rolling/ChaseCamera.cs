using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rolling
{
    public class ChaseCamera
    {
        public Vector3 ChasePosition
        {
            get { return chasePosition; }
            set { chasePosition = value; }
        }
        private Vector3 chasePosition;

        public Vector3 ChaseDirection
        {
            get { return chaseDirection; }
            set { chaseDirection = value; }
        }
        private Vector3 chaseDirection;

        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }
        private Vector3 up = Vector3.Up;

        public Vector3 DesiredPositionOffset
        {
            get { return desiredPositionOffset; }
            set { desiredPositionOffset = value; }
        }
        private Vector3 desiredPositionOffset = new Vector3(0, 2.0f, 2.0f);

        private Vector3 desiredPosition;

        public Vector3 LookAtOffset
        {
            get { return lookAtOffset; }
            set { lookAtOffset = value; }
        }
        private Vector3 lookAtOffset = new Vector3(0, 2.8f, 0);

        public Vector3 LookAt
        {
            get
            {
                UpdateWorldPositions();
                return lookAt;
            }
        }
        private Vector3 lookAt;

        public Vector3 Position
        {
            get { return position; }
        }
        private Vector3 position;

        public Vector3 Velocity
        {
            get { return velocity; }
        }
        private Vector3 velocity;

        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; }
        }
        private float aspectRatio = 4.0f / 3.0f;

        public float FieldOfView
        {
            get { return fieldOfView; }
            set { fieldOfView = value; }
        }
        private float fieldOfView = MathHelper.ToRadians(45.0f);

        public float NearPlaneDistance
        {
            get { return nearPlaneDistance; }
            set { nearPlaneDistance = value; }
        }
        private float nearPlaneDistance = 1.0f;

        public float FarPlaneDistance
        {
            get { return farPlaneDistance; }
            set { farPlaneDistance = value; }
        }
        private float farPlaneDistance = 100000.0f;

        public Matrix View
        {
            get { return view; }
        }
        private Matrix view;

        public Matrix Projection
        {
            get { return projection; }
        }
        private Matrix projection;

        public HeightMapInfo heightMap;

        public ChaseCamera(HeightMapInfo heightmap)
        {
            this.heightMap = heightmap;
        }

        private void UpdateWorldPositions()
        {
            Matrix transform = Matrix.Identity;
            transform.Forward = ChaseDirection;
            transform.Up = Up;
            transform.Right = Vector3.Cross(Up, ChaseDirection);

            // Calculate desired camera properties in world space
            desiredPosition = ChasePosition +
                Vector3.TransformNormal(DesiredPositionOffset, transform);
            /*if (heightMap.IsOnHeightmap(desiredPosition))
            {
                float minimumHeight =
                    heightMap.GetHeight(desiredPosition);
                if (desiredPosition.Y < minimumHeight)
                {
                    desiredPosition.Y = minimumHeight;
                }
            }*/

            lookAt = ChasePosition +
                Vector3.TransformNormal(LookAtOffset, transform);
        }

        private void UpdateMatrices()
        {
            view = Matrix.CreateLookAt(this.Position, this.LookAt, this.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView,
                AspectRatio, NearPlaneDistance, FarPlaneDistance);
        }

        public void Reset()
        {
            UpdateWorldPositions();
            velocity = Vector3.Zero;
            position = desiredPosition;
            UpdateMatrices();
        }

        public void Update(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            UpdateWorldPositions();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 stretch = position - desiredPosition;
            Vector3 force = (-1800.0f) * stretch - 600.0f * velocity;
            Vector3 acceleration = force / 50.0f;
            velocity += acceleration * elapsed;
            position += velocity * elapsed;
            UpdateMatrices();
        }
    }
}