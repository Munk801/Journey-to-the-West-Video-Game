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

		//applies gravity/acceleration, then velocity, then collision detection
		void physUpdate(FrameEventArgs e, List<PhysicsObject> objlist);

		//allows other objects to cause this one to accelerate
		//this could be AI accelerating itself, or the player accelerating projectiles, etc.
		void accelerate(double acceleration);
	}
}
