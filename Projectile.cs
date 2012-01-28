using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Engine;
using OpenTK;

namespace U5Designs {
	class Projectile : GameObject, RenderObject, CombatObject, PhysicsObject {

		public Projectile(string name, Vector3 location, bool existsIn2d, bool existsIn3d, ObjMesh mesh = null, Bitmap texture = null, SpriteSheet sprite = null) {
			_obj_name_text = name;
			_location = location;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = mesh;
			_texture = texture;
			_sprite = sprite;
			_frameNum = 0;
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

		private int _frameNum; //index of the current animation frame
		int RenderObject.frameNumber {
			get { return _frameNum; }
			set { _frameNum = value; }
		}

		bool RenderObject.is3d() {
			throw new Exception("The method or operation is not implemented.");
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

		void PhysicsObject.accelerate(double acceleration) {
			throw new Exception("The method or operation is not implemented.");
		}

		void CombatObject.reset() {
			throw new NotImplementedException();
		}
	}
}
