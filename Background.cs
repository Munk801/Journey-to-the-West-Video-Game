﻿using System;
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

        public Background(Vector3 location, SpriteSheet sprite, float speed)
					: base(location, true, false, false, null, null, sprite) {
			this.speed = speed;
			texID = GL.GenTexture();
		}

        public void UpdatePosition(float deltax)
        {
            _location.X += deltax*speed;
        }
    }
}