using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Rolling
{
    class GameObject
    {
        Model model;
        Matrix[] transforms;
        Matrix worldMatrix;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector3 velocity;
        public Vector3 rotationRate;
        string path;

        public GameObject(string path)
        {
            this.path = path;
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            scale = Vector3.One;
            velocity = Vector3.Zero;
            rotationRate = Vector3.Zero;
            worldMatrix = new Matrix();
        }

        public void Load(ContentManager content)
        {
            this.model = content.Load<Model>(path);
            // Copy any parent transforms.
            transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
        }

        public void Update(GameTime time)
        {
            position += velocity * time.ElapsedGameTime.Milliseconds;
            rotation += rotationRate * time.ElapsedGameTime.Milliseconds;
            worldMatrix = Matrix.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) *
                        Matrix.CreateScale(scale) *
                        Matrix.CreateTranslation(position);
        }

        public void Draw(GameTime time, Matrix View, Matrix Projection)
        {
            // Draw the model. A model can have multiple meshes, so loop.
            foreach (ModelMesh mesh in model.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
        }
    }
}