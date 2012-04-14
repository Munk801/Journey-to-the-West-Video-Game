using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Engine;

namespace U5Designs {
	public class ProjectileProperties {
		public Vector3 scale, pbox, cbox;
		public bool existsIn2d, existsIn3d, gravity;
		public int damage;
		public float speed;
		public double staminaCost, duration;
		public SpriteSheet sprite;
		public Effect deathAnim;


		public ProjectileProperties(Vector3 scale, Vector3 pbox, Vector3 cbox, bool existsIn2d, bool existsIn3d, int damage,
										float speed, bool gravity, SpriteSheet sprite, Effect death = null, double staminaCost = 0.0, double duration = -1.0) {
			this.scale = scale;
			this.pbox = pbox;
			this.cbox = cbox;
			this.existsIn2d = existsIn2d;
			this.existsIn3d = existsIn3d;
			this.damage = damage;
			this.speed = speed;
			this.gravity = gravity;
			this.staminaCost = staminaCost;
			this.duration = duration;
			this.sprite = sprite;
			this.deathAnim = death;
		}
	}
}
