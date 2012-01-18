using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* Contains the sprite sheet for a particular in-game object which is to be rendered
 * in 2D.  Different in-game objects that share a sprite sheet may also share a single
 * SpriteSheet object, even if they are on different animation frames at a given time.
 * A SpriteSheet will contain all of the animation cycles for a given in-game object,
 * such as stand, walk, run, attack cycles.  The frame numbers for each cycle will be
 * designed to be separate, so that the frame number also indicates which cycle is being
 * drawn.  TODO: Need to figure out how the frame number gets reset to beginning of cycle.
 */

namespace Engine {
	class SpriteSheet {


		/*
		 * setGLTex - sets the OpenGL texture pointers to point to the current
		 *			  frame of the animation, given the frame number
		 */
		public void setGLTex(int frameNumber) {
			//TODO: IMPLEMENT ME
		}
	}
}
