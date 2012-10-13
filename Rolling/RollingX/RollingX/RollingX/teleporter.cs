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
    class teleporter
    {
        Vector3 position;
        Model pitModel;
        Matrix World;
        BoundingBox bBox;
        float RotateSpeed;

        public teleporter(Vector3 pos, ContentManager content)
        {
            position = pos;
            Vector3 bBoxWidth = new Vector3(50, 50, 50);
            bBox = new BoundingBox(pos - bBoxWidth, pos + bBoxWidth);
            RotateSpeed = 1f;
            pitModel = content.Load<Model>("Models\\teleporter");
        }

        public void Update(GameTime time)
        {
            RotateSpeed += .05f * time.ElapsedGameTime.Milliseconds;
            World = Matrix.CreateFromYawPitchRoll(0,this.RotateSpeed, 0) * Matrix.CreateTranslation(this.position);
        }

        public Model getModel()
        {
            return pitModel;
        }

        public Matrix getWorld()
        {
            return World;
        }

        public bool detectIntersection(Vector3 pos)
        {
            return (bBox.Contains(pos) == ContainmentType.Contains);
        }
    }
}