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

		bool collidesIn3d { get; }
		bool collidesIn2d { get; }

		//applies gravity/acceleration, then velocity, then collision detection
        void physUpdate2d(double time, List<PhysicsObject> physList);
        void physUpdate3d(double time, List<PhysicsObject> physList);

		//allows other objects to cause this one to accelerate
		//this could be AI accelerating itself, or the player accelerating projectiles, etc.
		void accelerate(Vector3 acceleration);

        // pbox is the size of the physics box
        Vector3 pbox {
            get;
        }


	}
}
