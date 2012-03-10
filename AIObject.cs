using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using OpenTK;

namespace U5Designs {
	public interface AIObject : PhysicsObject, CombatObject {
		void aiUpdate(double time, PlayState playstate, Vector3 playerposn, bool enable3d, List<PhysicsObject> physList);
	}
}
