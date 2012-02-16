using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Engine;
using OpenTK;

namespace U5Designs {
	class Projectile : GameObject, RenderObject, CombatObject, PhysicsObject {

		public Projectile(Vector3 location, bool existsIn2d, bool existsIn3d, bool is3dGeo, ObjMesh mesh = null, Bitmap texture = null, SpriteSheet sprite = null) {
			_location = location;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = mesh;
			_texture = texture;
			_sprite = sprite;
			_cycleNum = 0;
			_frameNum = 0;
			_is3dGeo = is3dGeo;
            _hascbox = true;
            _type = 2;
		}

		private bool _is3dGeo;
		public bool is3dGeo {
			get { return _is3dGeo; }
		}

		private int _health;
		public int health {
			get { return _health; }
			set { _health = value; }
		}

		private int _damage;
		public int damage {
			get { return _damage; }
		}

        private float _speed;
		public float speed {
            get { return _speed; }
            set { _speed = value; }
        }
		private bool _alive;
		public bool alive {
			get { return _alive; }
			set { _alive = value; }
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
        private Vector3 _scale;
		public Vector3 scale {
            get { return _scale; }
        }

        private Vector3 _pbox;
		public Vector3 pbox {
            get { return _pbox; }
        }

        private Vector3 _cbox;
		public Vector3 cbox {
            get { return _cbox; }
		}

        private int _type;
        public int type {
            get { return _type; }
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
			throw new Exception("The method or operation is not implemented.");
		}

		public void physUpdate3d(FrameEventArgs e, List<PhysicsObject> objlist) {
			throw new Exception("The method or operation is not implemented.");
		}

		public void physUpdate2d(FrameEventArgs e, List<PhysicsObject> objlist) {
			throw new Exception("The method or operation is not implemented.");
		}

		public void accelerate(Vector3 acceleration) {
			throw new Exception("The method or operation is not implemented.");
		}

		public void reset() {
			throw new NotImplementedException();
		}
	}
}
