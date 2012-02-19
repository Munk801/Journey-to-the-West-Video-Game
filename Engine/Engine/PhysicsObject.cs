using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Engine {
	public interface PhysicsObject {
		/* The following are variables that should exist in any
		 * class implementing PhysicsObject
		private Vector3 velocity;
		private Vector3 accel;
		private bool doesGravity; //true if gravity affects this object
		private Vector3 boundOrigin; //corner of bounding box at min x,y,z corner
		private Vector3 boundExtent; //width, length, height of bounding box
		*/

        //Vector3 collidingBBOrigin
        //{
        //    get;
        //}

        //Vector3 collidingBBMax
        //{
        //    get;
        //}
        
		//applies gravity/acceleration, then velocity, then collision detection
		void physUpdate2d(double time, List<PhysicsObject> objlist);
		void physUpdate3d(double time, List<PhysicsObject> objlist);

		//allows other objects to cause this one to accelerate
		//this could be AI accelerating itself, or the player accelerating projectiles, etc.
		void accelerate(Vector3 acceleration);

        // pbox is the size of the physics box
        Vector3 pbox {
            get;
        }


	}
}
