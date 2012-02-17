using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Engine;

namespace U5Designs {
	class Obstacle : GameObject, RenderObject, PhysicsObject{
		private int texID;

		public Obstacle(Vector3 location, Vector3 scale, Vector3 pbox, bool existsIn2d, bool existsIn3d, ObjMesh mesh, Bitmap texture) {
			_location = location;
            _scale = scale;
            _pbox = pbox;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = mesh;
			_texture = texture;
			_sprite = null;
			_cycleNum = 0;
			_frameNum = 0;
			_is3dGeo = true;
            texID = GL.GenTexture();
            _hascbox = false;
		}


		public Obstacle(Vector3 location, Vector3 scale, Vector3 pbox, bool existsIn2d, bool existsIn3d, SpriteSheet sprite) {
			_location = location;
			_scale = scale;
            _pbox = pbox;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = null;
			_texture = null;
			_sprite = sprite;
			_frameNum = 0;
			_is3dGeo = false;
			texID = GL.GenTexture();
            _hascbox = false;
		}

		private bool _is3dGeo;
		public bool is3dGeo {
			get { return _is3dGeo; }
		}

		private ObjMesh _mesh; //null for sprites
		public ObjMesh mesh {
			get { return _mesh; }
		}

		private Bitmap _texture; //null for sprites
		public Bitmap texture {
			get { return _texture; }
		}

		private SpriteSheet _sprite; //null for 3d objects
		public SpriteSheet sprite {
			get { return _sprite; }
		}

        //private Vector3 _location;
        //public void _move_Location(Vector3 v)
        //{
            //_location.X += x;
            //_location.Y += y;
            //_location.Z += z;
        //    _location += v;
        //}
        private Vector3 _scale;
		public Vector3 scale {
            get { return _scale; }
        }

        private Vector3 _pbox;
		public Vector3 pbox {
            get { return _pbox; }
        }

		private int _cycleNum;
		public int cycleNumber {
			get { return _cycleNum; }
			set { _cycleNum = value; }
		}

		private double _frameNum; //index of the current animation frame
		public double frameNumber {
			get { return _frameNum; }
			set { _frameNum = value; }
		}

		public bool isAnimated() {
			throw new Exception("The method or operation is not implemented.");
		}

		public void doScaleTranslateAndTexture() {
			GL.PushMatrix();
			if(_is3dGeo) {
				GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
				GL.BindTexture(TextureTarget.Texture2D, texID);
				BitmapData bmp_data = _texture.LockBits(new Rectangle(0, 0, _texture.Width, _texture.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
				_texture.UnlockBits(bmp_data);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			}

            GL.Translate(_location);
            GL.Scale(_scale);
		}

		public void physUpdate3d(FrameEventArgs e, List<PhysicsObject> objlist) {
			//obstacles don't move (for now) so they don't need an update
			return;
		}

		public void physUpdate2d(FrameEventArgs e, List<PhysicsObject> objlist) {
			//obstacles don't move (for now) so they don't need an update
			return;
		}

		public void accelerate(Vector3 acceleration) {
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
