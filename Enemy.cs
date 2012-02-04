using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;

using Engine;

namespace U5Designs
{
    class Enemy : GameObject, AIObject, RenderObject
    {
        /** This struct will contain the Players Status **/
        struct EnemyState
        {
            public int e_health;
            public int e_damage;
            public double e_speed;
            public string e_equip_ability;
            public int e_current_zone;

            // texture...

        };

        int health, damage;
        double speed;
        // texture = .... ;

        EnemyState e_state;

        public Enemy(int hp, int dam, double spd)
        {
            health = hp;
            damage = dam;
            speed = spd;
            // type = ... ;
        }


        /** Like the Player Status update call this every time you need to update an Enemies State before saving **/
        public void updateState()
        {
            e_state.e_health = health;
            e_state.e_damage = damage;
            e_state.e_speed = speed;

            // Add in the other State elements that will need to be maintained here..
		}

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
			throw new NotImplementedException();
		}

		void PhysicsObject.accelerate(double acceleration) {
			throw new NotImplementedException();
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

		void AIObject.aiUpdate(FrameEventArgs e) {
			throw new NotImplementedException();
		}

		void CombatObject.reset() {
			throw new NotImplementedException();
		}
	}
}
