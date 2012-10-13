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
    class Coin
    {
        Vector3 position;
        Model coinModel;
        Matrix World;
        BoundingBox bBox;
        float RotateSpeed;

        public Coin(Vector3 pos, ContentManager content)
        {
            position = pos;
            Vector3 bBoxWidth = new Vector3(20, 20, 20);
            bBox = new BoundingBox(pos - bBoxWidth, pos + bBoxWidth);
            RotateSpeed = .5f;
            coinModel = content.Load<Model>("coin");
        }

        public void Update(GameTime time)
        {
            RotateSpeed += .003f * time.ElapsedGameTime.Milliseconds;
            World = Matrix.CreateFromYawPitchRoll(this.RotateSpeed, 0, 0) *Matrix.CreateTranslation(this.position);
        }

        public Model getModel()
        {
            return coinModel;
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
