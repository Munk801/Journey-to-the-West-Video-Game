using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using OpenTK;
using System.Drawing;
using Tao.DevIl;

namespace U5Designs
{
    class Background : GameObject, RenderObject
    {
        public float Speed { get; set; }
        public Vector3 Direction { get; set; }
        public Bitmap Image { get; set; }


        public Background(Bitmap image, float speed, Vector3 direction)
        {
            _texture = image;
            this.Speed = speed;
			this.Direction = direction;

        }

        public void Update(FrameEventArgs e)
        {
            // Update Speed

        }

        public void Draw()
        {
            // Draw background
        }

        private bool _is3dGeo;
		public bool is3dGeo
        {
            get { return _is3dGeo; }
        }

        private ObjMesh _mesh; //null for sprites
		public ObjMesh mesh
        {
            get { return _mesh; }
        }

        private Bitmap _texture; //null for sprites
		public Bitmap texture
        {
            get { return _texture; }
        }

        private SpriteSheet _sprite; //null for 3d objects
		public SpriteSheet sprite
        {
            get { return _sprite; }
        }

        private Vector3 _scale;
		public Vector3 scale
        {
            get { return _scale; }
        }

		private int _cycleNum;
		public int cycleNumber {
			get { return _cycleNum; }
			set { _cycleNum = value; }
		}

        private double _frameNum; //index of the current animation frame
		public double frameNumber
        {
            get { return _frameNum; }
            set { _frameNum = value; }
        }

		public bool isAnimated()
        {
            throw new Exception("The method or operation is not implemented.");
        }

		public void doScaleTranslateAndTexture()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
