using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Engine {
	public interface AIObject : PhysicsObject, CombatObject {
		void aiUpdate(FrameEventArgs e, Vector3 playerposn);
	}
}
