using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

/*
 * RenderObject will be implemented by anything in the game which will be
 * rendered on screen in any way.  It includes methods for determining how
 * the object is rendered in 2D/3D, getting the object's geometry, textures,
 * and/or sprites, etc.
 */

namespace Engine {
	interface RenderObject {
		ObjMesh getMesh(); //null for sprites
		Bitmap getTexture(); //null for sprites
		SpriteSheet getSprite(); //null for 3d objects

		bool is3d(); //returns true if item has 3d geometry, false if sprite
		bool isAnimated(); //true if more than one frame (probably only applies to sprites)
		int getFrameNumber(); //returns index of the current animation frame
		void setFrameNumber(); //sets index of the current animation frame
		void doScaleTranslateAndTexture(); //pushes matrix, adds scale and translate to model view stack, and sets texture pointer
	}
}
