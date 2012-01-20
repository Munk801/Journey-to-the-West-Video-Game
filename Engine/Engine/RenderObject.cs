using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * RenderObject will be implemented by anything in the game which will be
 * rendered on screen in any way.  It includes methods for determining how
 * the object is rendered in 2D/3D, getting the object's geometry, textures,
 * and/or sprites, etc.
 */

namespace Engine {
	interface RenderObject {
	    bool isVisibleIn2d(); //should this object be rendered when the view is 2d?
		bool isVisibleIn3d(); //should this object be rendered when the view is 3d?
		bool is3d(); //returns true if item has 3d geometry, false if sprite
		Geometry3D getGeometry(); //returns geometry, or null for sprites
		SpriteSheet getSpriteSheet(); //returns sprite sheet, or null for 3d objects
		bool isAnimated(); //true if more than one frame (probably only applies to sprites)
		int getFrameNumber(); //returns index of the current animation frame
		void doScaleAndTranslate(); //pushes matrix, then adds scale and translate to model view stack
	}
}
