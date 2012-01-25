using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

/*
 * This is the super-class of all in game objects.  It defines
 * things that are common for everything, like location
 */
namespace Engine {
	public class GameObject {
		//does this object exist when the game is
		bool existsIn2d() {

			return false;
		}

		//should this object be rendered when the view is 3d?
		bool existsIn3d() {

			return false;
		}

		Vector3 getLocation() {

			return new Vector3(0,0,0);
		}
	}
}
