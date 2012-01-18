using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* Contains the geometry, textures, etc. for a particular in-game object which is to be
 * rendered in 3D.  Different in-game objects that share geometry may also share a single
 * Geometry3D object.
 */

namespace Engine {
	class Geometry3D {

		/*
		 * draw - Uses OpenGL to render this geometry.  Requires that
		 *		  the modelview matrix has been properly set up previously
		 *		  to translate to proper location
		 */
		public void draw() {
			//TODO: IMPLEMENT ME
		}
	}
}
