using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace TerrainSandBox
{
    public class Camera
    {
        protected Game1 game;

        protected Matrix view;

        protected Matrix projection;

        public Vector3 position;

        protected Vector3 target;

        protected Vector3 up;

        //accessors and mutators
        public Matrix View
        {
            get
            {
                return view;
            }
            protected set
            {
                view = value;
            }
        }

        public Matrix Projection
        {
            get
            {
                return projection;
            }
            protected set
            {
                projection = value;
            }
        }

        public Vector3 Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }

        public Vector3 Up
        {
            get
            {
                return up;
            }
            set
            {
                up = value;
            }
        }


        public Camera(Game1 game, Vector3 position, Vector3 target, Vector3 up)
        {
            this.game = game;
            this.position = position;
            this.target = target;
            this.up = up;

            view = Matrix.CreateLookAt(position, target, up);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
            (float)game.Window.ClientBounds.Width / (float)game.Window.ClientBounds.Height,
            1, 3000);

        }

        public virtual void Update(GameTime gameTime)
        {
            view = Matrix.CreateLookAt(position, target, up);
        }

    }
}
