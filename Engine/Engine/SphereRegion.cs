using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Engine {
	public class SphereRegion {
		private Vector3 center;
		private float radius;
		public SphereRegion(Vector3 center, float radius) {
			this.center = center;
			this.radius = radius;
		}

		public bool contains(Vector3 p) {
			return (p - center).LengthFast <= radius;
		}
	}
}
