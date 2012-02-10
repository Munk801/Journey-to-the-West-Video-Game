using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;

/*
 * RenderObject will be implemented by anything in the game which will be
 * rendered on screen in any way.  It includes methods for determining how
 * the object is rendered in 2D/3D, getting the object's geometry, textures,
 * and/or sprites, etc.
 */

namespace Engine {
	public interface RenderObject {
		ObjMesh mesh { //null for sprites
			get;
		}

		Bitmap texture { //null for sprites
			get;
		}

		SpriteSheet sprite { //null for 3d objects
			get;
		}

        Vector3 scale {
            get;
        }

		int frameNumber { //index of the current animation frame
			get;
			set;
		}

        bool is3dGeo {
            get;
        }

		bool isAnimated(); //true if more than one frame (probably only applies to sprites)
		void doScaleTranslateAndTexture(); //pushes matrix, adds scale and translate to model view stack, and sets texture pointer
	}
}
