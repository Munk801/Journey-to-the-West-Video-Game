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

		public Obstacle(Vector3 location, Vector3 scale, Vector3 pbox, bool existsIn2d, bool existsIn3d, bool collidesIn2d, bool collidesIn3d, ObjMesh mesh, MeshTexture texture) {
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
            _hascbox = false;
			_collidesIn2d = collidesIn2d;
			_collidesIn3d = collidesIn3d;
		}


		public Obstacle(Vector3 location, Vector3 scale, Vector3 pbox, bool existsIn2d, bool existsIn3d, bool collidesIn2d, bool collidesIn3d, Billboarding bb, SpriteSheet sprite) {
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
			_hascbox = false;
			_collidesIn2d = collidesIn2d;
			_collidesIn3d = collidesIn3d;
			_billboards = bb;
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

		private Billboarding _billboards;
		public Billboarding billboards {
			get { return _billboards; }
		}

		private bool _collidesIn3d;
		public bool collidesIn3d {
			get { return _collidesIn3d; }
		}

		private bool _collidesIn2d;
		public bool collidesIn2d {
			get { return _collidesIn2d; }
		}

		public void doScaleTranslateAndTexture() {
			GL.PushMatrix();
			if(_is3dGeo) {
				_texture.doTexture();
			}

            GL.Translate(_location);
            GL.Scale(_scale);
		}

        public void DoRotate(float rotate, Vector3d axis)
        {
            GL.Rotate(rotate, axis);
        }

        public void physUpdate3d(double time, List<PhysicsObject> physList) {
			//obstacles don't move (for now) so they don't need an update
			return;
		}

        public void physUpdate2d(double time, List<PhysicsObject> physList) {
			//obstacles don't move (for now) so they don't need an update
			return;
		}

		public void accelerate(Vector3 acceleration) {
			//obstacles don't move (for now) so they don't need an accelerate
		}
	}
}
