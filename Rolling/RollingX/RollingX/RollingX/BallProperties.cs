using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Rolling
{
    class BallProperties
    {
        public Texture2D Texture
        {
            get { return texture; }
        }
        private Texture2D texture;

        public float Elasticity
        {
            get { return elasticity; }
        }
        private float elasticity;

        public float Mass
        {
            get { return mass; }
        }
        private float mass;

        public float DashSpeed
        {
            get { return dashSpeed; }
        }
        private float dashSpeed;

        public float Gravity
        {
            get { return gravity; }
        }
        private float gravity;

        public Vector3 DragGround
        {
            get { return dragGround; }
        }
        private Vector3 dragGround;

        public Vector3 DragAir
        {
            get { return dragAir; }
        }
        private Vector3 dragAir;

        public String JumpSound
        {
            get { return jumpSound; }
        }
        private String jumpSound;

        public String BounceSound
        {
            get { return bounceSound; }
        }
        private String bounceSound;

        public String ChangeToSound
        {
            get { return changeToSound; }
        }
        private String changeToSound;

        public BallProperties(Texture2D texture, float mass, float elasticity, float dashSpeed, float gravity, Vector3 dragGround, Vector3 dragAir, String jumpSound, String bounceSound, String changeToSound)
        {
            if (texture == null || gravity == null || dragGround == null || dragAir == null)
                throw new ArgumentNullException("Cannot have null ball properties.");
            if (elasticity < 0 || elasticity > 1)
                throw new ArgumentOutOfRangeException("Elasticity must be between 0 and 1");
            if (mass < 0)
                throw new ArgumentOutOfRangeException("Mass must be greater than 0");
            this.texture = texture;
            this.mass = mass;
            this.elasticity = elasticity;
            this.dashSpeed = dashSpeed;
            this.gravity = gravity;
            this.dragGround = dragGround;
            this.dragAir = dragAir;
            this.jumpSound = jumpSound;
            this.bounceSound = bounceSound;
            this.changeToSound = changeToSound;
        }
    }
}
