﻿using System;
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
		//duplicated from GameObject
		Vector3 location { get; set;  }
		bool hascbox { get; }
		bool existsIn3d { get; }
		bool existsIn2d { get; }
		int ScreenRegion { get; }
		int getID();

		//true if this thing should always draw; false if it should only draw when in ON_SCREEN region
		bool drawWhenOffScreen { get; }

		//null for sprites
		ObjMesh mesh { get; }

		//null for sprites
		MeshTexture texture { get; }

		//null for 3d objects
		SpriteSheet sprite { get; }

		Vector3 scale { get; set; }

		//index of animation for current action
		int cycleNumber { get; set; }

		//index of the current animation frame
		double frameNumber { get; set; }

        bool is3dGeo { get; }
		Billboarding billboards { get; }

		//+1 when sprite animation should run forward, -1 when backward
		int animDirection { get; }

		//true if sprite has separate 2D and 3D animations for every cycle
		//false if same animation should be reused in both 2D and 3D
		bool hasTwoAnims { get; }

		void doScaleTranslateAndTexture(); //pushes matrix, adds scale and translate to model view stack, and sets texture pointer
	}
}
