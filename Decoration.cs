using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;

using Engine;

namespace U5Designs {
	class Decoration : GameObject, RenderObject {
		public Decoration(Vector3 location, bool existsIn2d, bool existsIn3d, bool is3dGeo, ObjMesh mesh = null, Bitmap texture = null, SpriteSheet sprite = null) {
			_location = location;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = mesh;
			_texture = texture;
			_sprite = sprite;
			_frameNum = 0;
			_is3dGeo = is3dGeo;
		}

		private bool _is3dGeo;
		bool RenderObject.is3dGeo {
			get { return _is3dGeo; }
		}

		private ObjMesh _mesh; //null for sprites
		ObjMesh RenderObject.mesh {
			get { return _mesh; }
		}

		private Bitmap _texture; //null for sprites
		Bitmap RenderObject.texture {
			get { return _texture; }
		}

		private SpriteSheet _sprite; //null for 3d objects
		SpriteSheet RenderObject.sprite {
			get { return _sprite; }
		}

        private Vector3 _scale;
        Vector3 RenderObject.scale
        {
            get { return _scale; }
        }

		private int _frameNum; //index of the current animation frame
		int RenderObject.frameNumber {
			get { return _frameNum; }
			set { _frameNum = value; }
		}

		bool RenderObject.isAnimated() {
			throw new Exception("The method or operation is not implemented.");
		}

		void RenderObject.doScaleTranslateAndTexture() {
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
