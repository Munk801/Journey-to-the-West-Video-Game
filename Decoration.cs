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
	public class Decoration : GameObject, RenderObject {

		public Decoration(Vector3 location, Vector3 scale, bool existsIn2d, bool existsIn3d, Billboarding bb, SpriteSheet sprite) : base() {
			_location = location;
			_scale = scale;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = mesh;
			_texture = texture;
			_sprite = sprite;
			_cycleNum = 0;
			_frameNum = 0;
			_is3dGeo = false;
            _hascbox = false;
			_billboards = bb;
			_animDirection = 1;
		}

		public Decoration(Vector3 location, Vector3 scale, bool existsIn2d, bool existsIn3d, ObjMesh mesh, MeshTexture texture) : base() {
			_location = location;
			_scale = scale;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = mesh;
			_texture = texture;
			_sprite = sprite;
			_cycleNum = 0;
			_frameNum = 0;
			_is3dGeo = true;
			_hascbox = false;
			_animDirection = 1;
		}

		//These four are only used by Player for his arms - they should not be called for other Decorations
		internal float locX {
			get { return _location.X; }
			set { _location.X = value; }
		}
		internal float locY {
			get { return _location.Y; }
			set { _location.Y = value; }
		}
		internal float locZ {
			get { return _location.Z; }
			set { _location.Z = value; }
		}
		internal float scaleX {
			get { return _scale.X; }
			set { _scale.X = value; }
		}

		private bool _is3dGeo;
		public bool is3dGeo {
			get { return _is3dGeo; }
		}

		private ObjMesh _mesh; //null for sprites
		public ObjMesh mesh {
			get { return _mesh; }
		}

		private MeshTexture _texture; //null for sprites
		public MeshTexture texture {
			get { return _texture; }
		}

		private SpriteSheet _sprite; //null for 3d objects
		public SpriteSheet sprite {
			get { return _sprite; }
		}

        private Vector3 _scale;
		public Vector3 scale {
			get { return _scale; }
			set { _scale = value; }
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

		private Billboarding _billboards;
		public Billboarding billboards {
			get { return _billboards; }
		}

		private int _animDirection;
		public int animDirection {
			get { return _animDirection; }
			set { _animDirection = value; }
		}

		public bool hasTwoAnims {
			get { return false; } //Change later if we want to add this functionality
		}

		public int ScreenRegion {
			get { return screenRegion; }
		}

		public bool drawWhenOffScreen {
			get { return true; }
		}

		public void doScaleTranslateAndTexture() {
			GL.PushMatrix();
			if(_is3dGeo) {
				_texture.doTexture();
			}

			GL.Translate(_location);
			GL.Scale(_scale);
		}
	}
}
