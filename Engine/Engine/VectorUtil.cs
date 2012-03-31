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
		
		//Test if an object is entirely over the ground
		//Used for saving safe player locations to respawn, and keeping enemies from jumping to death
		public static bool overGround2d(PhysicsObject obj, List<PhysicsObject> physList) {
			bool left = false, right = false;
			float leftEdge = obj.location.X - obj.pbox.X;
			float rightEdge = obj.location.X + obj.pbox.X;

			foreach(PhysicsObject po in physList) {
				if(po.hascbox) {
					//Anything with a cbox is not a valid ground object
					continue;
				}
				if(obj.location.Y - obj.pbox.Y >= po.location.Y + po.pbox.Y) {
					if(leftEdge >= po.location.X - po.pbox.X && leftEdge <= po.location.X + po.pbox.X) {
						left = true;
					}
					if(rightEdge >= po.location.X - po.pbox.X && rightEdge <= po.location.X + po.pbox.X) {
						right = true;
					}
					//Note: Edges have to be separate like this to correctly handle seams between ground planes
					if(left && right) {
						return true;
					}
				}
			}
			//No match
			return false;
		}

		//Helper method for overGround3d
		private static bool inBox(Vector2 point, PhysicsObject po) {
			return point.X < po.location.X + po.pbox.X && point.X > po.location.X - po.pbox.X
					&& point.Y < po.location.Z + po.pbox.Z && point.Y > po.location.Z - po.pbox.Z;
		}

		/// <summary>
		/// Test if an object is over the ground
		/// Used for saving safe player locations to respawn, and keeping enemies from jumping to death
		/// 
		/// The strict version requires that all four corners be over the ground; see also overGround3dLoose
		/// </summary>
		/// <param name="obj">Object to be tested</param>
		/// <param name="physList">List of all other objects for collision</param>
		/// <returns></returns>
		public static bool overGround3dStrict(PhysicsObject obj, List<PhysicsObject> physList) {
			Vector2 loc = new Vector2(obj.location.X, obj.location.Z);
			Vector2 box = new Vector2(obj.pbox.X, obj.pbox.Z);
			Vector2 frontRight = new Vector2(obj.location.X + obj.pbox.X, obj.location.Z + obj.pbox.Z);
			Vector2 backLeft = new Vector2(obj.location.X - obj.pbox.X, obj.location.Z - obj.pbox.Z);
			Vector2 frontLeft = new Vector2(backLeft.X, frontRight.Y);
			Vector2 backRight = new Vector2(frontRight.X, backLeft.Y);
			bool fl = false, fr = false, bl = false, br = false;

			foreach(PhysicsObject po in physList) {
				if(po.hascbox) {
					//Anything with a cbox is not a valid ground object
					continue;
				}
				if(obj.location.Y - obj.pbox.Y >= po.location.Y + po.pbox.Y) {
					if(inBox(frontLeft, po)) { fl = true; }
					if(inBox(frontRight, po)) { fr = true; }
					if(inBox(backLeft, po)) { bl = true; }
					if(inBox(backRight, po)) { br = true; }

					//Note: Edges have to be separate like this to correctly handle seams between ground planes
					if(fl && fr && bl && br) {
						return true;
					}
				}
			}
			//No match
			return false;
		}

		/// <summary>
		/// Test if an object is over the ground
		/// Used for saving safe player locations to respawn, and keeping enemies from jumping to death
		/// 
		/// The loose version requires that one corner be over the ground; see also overGround3dStrict
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="physList"></param>
		/// <returns></returns>
		public static bool overGround3dLoose(PhysicsObject obj, List<PhysicsObject> physList) {
			Vector2 loc = new Vector2(obj.location.X, obj.location.Z);
			Vector2 box = new Vector2(obj.pbox.X, obj.pbox.Z);
			Vector2 frontRight = new Vector2(obj.location.X + obj.pbox.X, obj.location.Z + obj.pbox.Z);
			Vector2 backLeft = new Vector2(obj.location.X - obj.pbox.X, obj.location.Z - obj.pbox.Z);
			Vector2 frontLeft = new Vector2(backLeft.X, frontRight.Y);
			Vector2 backRight = new Vector2(frontRight.X, backLeft.Y);
			bool fl = false, fr = false, bl = false, br = false;

			foreach(PhysicsObject po in physList) {
				if(po.hascbox) {
					//Anything with a cbox is not a valid ground object
					continue;
				}
				if(obj.location.Y - obj.pbox.Y >= po.location.Y + po.pbox.Y) {
					if(inBox(frontLeft, po)) { return true; }
					if(inBox(frontRight, po)) { return true; }
					if(inBox(backLeft, po)) { return true; }
					if(inBox(backRight, po)) { return true; }
				}
			}
			//No match
			return false;
		}
	}
}
