using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using OpenTK;
using System.Drawing;
using Tao.DevIl;

using OpenTK.Graphics.OpenGL;

namespace U5Designs
{
    class Background : Decoration
    {
		private float speed;
        public float Speed {
			get { return speed; }
			set { speed = value; }
		}
		private int texID;

        public Background(Vector3 location, Vector3 scale, SpriteSheet sprite, float speed)
					: base(location, scale, true, true, Billboarding.Lock2d, sprite) {
			this.speed = speed;
			texID = GL.GenTexture();
		}

        public void UpdatePositionX(float deltax) {
            _location.X += deltax*speed;
        }

		public void UpdatePositionY(float deltay) {
			_location.Y += deltay * speed;
		}
    }
}
