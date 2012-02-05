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
		public Obstacle(Vector3 location, Vector3 scale, bool existsIn2d, bool existsIn3d, ObjMesh mesh, Bitmap texture) {
			_location = location;
            _scale = scale;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = mesh;
			_texture = texture;
			_sprite = null;
			_frameNum = 0;
			_is3dGeo = true;
            texID = GL.GenTexture();
		}


		public Obstacle(Vector3 location, Vector3 scale, bool existsIn2d, bool existsIn3d, SpriteSheet sprite) {
			_location = location;
			_scale = scale;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = null;
			_texture = null;
			_sprite = sprite;
			_frameNum = 0;
			_is3dGeo = false;
			texID = GL.GenTexture();
		}


		private int texID;

		private bool _is3dGeo;
		bool RenderObject.is3dGeo {
			get { return _is3dGeo; }
		}

		private ObjMesh _mesh; //null for sprites
		ObjMesh RenderObject.mesh {
			get { return _mesh; }
		}

		private Bitmap _texture; //null for sprites
		Bitmap RenderObject.texture {
			get { return _texture; }
		}

		private SpriteSheet _sprite; //null for 3d objects
		SpriteSheet RenderObject.sprite {
			get { return _sprite; }
		}

        private Vector3 _scale;
        Vector3 RenderObject.scale
        {
            get { return _scale; }
        }

		private int _frameNum; //index of the current animation frame
		int RenderObject.frameNumber {
			get { return _frameNum; }
			set { _frameNum = value; }
		}

		bool RenderObject.isAnimated() {
			throw new Exception("The method or operation is not implemented.");
		}

		void RenderObject.doScaleTranslateAndTexture() {
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

		void PhysicsObject.physUpdate(FrameEventArgs e, List<PhysicsObject> objlist) {
			//throw new Exception("The method or operation is not implemented.");
		}

		void PhysicsObject.accelerate(Vector3 acceleration) {
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
