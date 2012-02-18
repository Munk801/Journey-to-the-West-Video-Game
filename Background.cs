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

        
        public static SpriteSheet Sprite;

        private static bool is3D;

        private Bitmap image;
        private Obstacle dimensions;
        private int backgroundWidth;
        private int backgroundHeight;

        public Vector3 Location;

        private int[] cycleStarts = { 0 };
        private int[] cycleLengths = { 1 };

        public Background(Vector3 location, string fileName, float speed, bool _is3D)
            : base(location, true, false, false, null, null, Sprite)
        {
            Sprite = sprite;
			this.speed = speed;
            Speed = speed;
            image = new Bitmap(fileName);
            backgroundWidth = image.Width;
            backgroundHeight = image.Height;
            is3D = _is3D;
            Location = location;
            Sprite = new SpriteSheet(image, cycleStarts, cycleLengths, backgroundWidth, backgroundHeight);
            dimensions = new Obstacle(Location, new Vector3(320, 100, 1), new Vector3(0, 0, 0), true, true, Sprite);
			texID = GL.GenTexture();
		}

        public void Draw(bool viewIn3D)
        {

            if (is3D == viewIn3D)
            {
                //dimensions = new Obstacle(Location, new Vector3(320, 100, 1), new Vector3(0, 0, 0), true, true, Sprite);
                this.dimensions.doScaleTranslateAndTexture();
                this.dimensions.sprite.draw(false);
                
            }
        }



        public void UpdatePosition(float deltax)
        {
            this.dimensions.UpdateLocationX(deltax * Speed);
        }

        public int GetBackgroundWidth(Background b)
        {
            return image.Width;

        }

        public int GetBackgroundHeight(Background b)
        {
            return image.Height;
        }
    }
}
