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
		private Vector3 startLoc, theoretical;

		private float speed;
        public float Speed {
			get { return speed; }
			set { speed = value; }
		}

        // Added by Seth for Level Designer because we need access to the sprite path string for the level XML file
        private String _sprite_path;
        public String Path
        {
            get { return _sprite_path; }
        }

        public Background(Vector3 location, Vector3 scale, SpriteSheet sprite, float speed, String sp)
					: base(location, scale, true, true, Billboarding.Lock2d, sprite) {
			this.speed = speed;
            this._sprite_path = sp;
			startLoc = new Vector3(_location);
			theoretical = new Vector3(_location);
		}

        public void UpdatePositionX(float deltax) {
            _location.X += deltax*speed;
        }

		public void UpdatePositionY(float deltay) {
/*			_location.Y += deltay * 0.85f;*/
			theoretical.Y += deltay * 0.85f;
			float offset = theoretical.Y - startLoc.Y;
			if(offset > 40.0f) {
				_location.Y = startLoc.Y + 40.0f;
			} else if(offset < -40.0f) {
				_location.Y = startLoc.Y - 40.0f;
			} else {
				_location.Y = theoretical.Y;
			}
		}
    }
}
