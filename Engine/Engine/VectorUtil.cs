using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Engine {
	public class VectorUtil {
		public static Vector3 abs(Vector3 v) {
			v.X = Math.Abs(v.X);
			v.Y = Math.Abs(v.Y);
			v.Z = Math.Abs(v.Z);
			return v;
		}

		public static Vector3 div(Vector3 dividend, Vector3 divisor) {
			dividend.X /= divisor.X;
			dividend.Y /= divisor.Y;
			dividend.Z /= divisor.Z;
			return dividend;
		}

		public static float minVal(Vector3 v) {
			if(v.X <= v.Y && v.X <= v.Z) {
				return v.X;
			} else {
				return (v.Y < v.Z ? v.Y : v.Z);
			}
		}

		public static float maxVal(Vector3 v) {
			if(v.X >= v.Y && v.X >= v.Z) {
				return v.X;
			} else {
				return (v.Y > v.Z ? v.Y : v.Z);
			}
		}

		public static int maxIndex(Vector3 v) {
			if(v.X >= v.Y && v.X >= v.Z) {
				return 0; //x = 0
			} else {
				return (v.Y > v.Z ? 1 : 2); //y = 1, z = 2
			}
		}

		public static float minVal(Vector2 v) {
			return (v.X < v.Y ? v.X : v.Y);
		}

		public static float maxVal(Vector2 v) {
			return (v.X > v.Y ? v.X : v.Y);
		}

		public static int maxIndex(Vector2 v) {
			return (v.X > v.Y ? 0 : 1); //x = 0, y = 1
		}

		public static void sort(ref Vector3 mins, ref Vector3 maxes) {
			if(maxes.X < mins.X) {
				float tmp = maxes.X;
				maxes.X = mins.X;
				mins.X = tmp;
			}
			if(maxes.Y < mins.Y) {
				float tmp = maxes.Y;
				maxes.Y = mins.Y;
				mins.Y = tmp;
			}
			if(maxes.Z < mins.Z) {
				float tmp = maxes.Z;
				maxes.Z = mins.Z;
				mins.Z = tmp;
			}
		}

		public static float dist(Vector3 v1, Vector3 v2) {
			Vector3 tmp = new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
			return tmp.LengthFast;
		}

		public static Vector3 getdir(Vector3 player, Vector3 enemy) {
			Vector3 tmp = new Vector3(player.X - enemy.X, player.Y - enemy.Y, player.Z - enemy.Z);
			tmp.Normalize();
			return tmp;
		}
	}
}
