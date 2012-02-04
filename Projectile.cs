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
			_frameNum = 0;
			_is3dGeo = is3dGeo;
		}

		private bool _is3dGeo;
		bool RenderObject.is3dGeo {
			get { return _is3dGeo; }
		}

		private float _health;
		float CombatObject.health {
			get { return _health; }
			set { _health = value; }
		}

		private float _damage;
		float CombatObject.damage {
			get { return _damage; }
		}

        private float _speed;
        float CombatObject.speed {
            get { return _speed; }
            set { _speed = value; }
        }
		private bool _alive;
		bool CombatObject.alive {
			get { return _alive; }
			set { _alive = value; }
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
        Vector3 RenderObject.scale {
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
			throw new Exception("The method or operation is not implemented.");
		}

		void PhysicsObject.physUpdate(FrameEventArgs e, List<PhysicsObject> objlist) {
			throw new Exception("The method or operation is not implemented.");
		}

		void PhysicsObject.accelerate(Vector3 acceleration) {
			throw new Exception("The method or operation is not implemented.");
		}

		void CombatObject.reset() {
			throw new NotImplementedException();
		}
	}
}
