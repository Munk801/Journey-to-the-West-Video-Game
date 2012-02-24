using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Engine {
	public interface PhysicsObject {
		//duplicated from GameObject
		Vector3 location { get; }
		bool hascbox { get; }
		bool existsIn3d { get; }
		bool existsIn2d { get; }

		/* The following are variables that should exist in any
		 * class implementing PhysicsObject
		private Vector3 velocity;
		private Vector3 accel;
		private bool doesGravity; //true if gravity affects this object
		*/

		//applies gravity/acceleration, then velocity, then collision detection
        void physUpdate2d(double time, List<GameObject> objList, List<RenderObject> renderList, List<PhysicsObject> colisionList, List<PhysicsObject> physList, List<CombatObject> combatList);
        void physUpdate3d(double time, List<GameObject> objList, List<RenderObject> renderList, List<PhysicsObject> colisionList, List<PhysicsObject> physList, List<CombatObject> combatList);

		//allows other objects to cause this one to accelerate
		//this could be AI accelerating itself, or the player accelerating projectiles, etc.
		void accelerate(Vector3 acceleration);

        // pbox is the size of the physics box
        Vector3 pbox {
            get;
        }


	}
}
