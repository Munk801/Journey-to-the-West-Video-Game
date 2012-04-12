using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Engine;

namespace U5Designs {

	/// <summary>
	/// Effects are Decorations which play their animation a single time and then delete themselves
	/// </summary>
	public class Effect : Decoration {
		PlayState playstate;
		double timeElapsed;

		public Effect(PlayState ps, Vector3 location, Vector3 scale, bool existsIn2d, bool existsIn3d, Billboarding bb, SpriteSheet sprite)
			: base(location, scale, existsIn2d, existsIn3d, bb, sprite) {
				this.playstate = ps;
				timeElapsed = 0.0;
		}

		public Effect(Vector3 location, Effect template)
			: this(template.playstate, location, template.scale, template.existsIn2d, template.existsIn3d, template.billboards, template.sprite) { }

		public void update(double time) {
			timeElapsed += time;
			if(timeElapsed >= sprite.frameCount / sprite.framesPerSecond) {
				playstate.objList.Remove(this);
				playstate.renderList.Remove(this);
				playstate.effectsList.Remove(this);
			}
		}
	}
}
