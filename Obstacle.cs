using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Engine;

namespace U5Designs {
	class Obstacle : GameObject, RenderObject, PhysicsObject {
		internal Vector3 velocity;
		internal Vector3 accel;

		public Obstacle(Vector3 location, Vector3 scale, Vector3 pbox, bool existsIn2d, bool existsIn3d, bool collidesIn2d, bool collidesIn3d,
							ObjMesh mesh, MeshTexture texture) : base() {
			_location = location;
            _scale = scale;
            _pbox = pbox;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = mesh;
			_texture = texture;
			_sprite = null;
			_cycleNum = 0;
			_frameNum = 0;
			_frame3d = 0;
			_is3dGeo = true;
            _hascbox = false;
            canSquish = false;
			_collidesIn2d = collidesIn2d;
			_collidesIn3d = collidesIn3d;
			_animDirection = 1;
		}


		public Obstacle(Vector3 location, Vector3 scale, Vector3 pbox, bool existsIn2d, bool existsIn3d, bool collidesIn2d, bool collidesIn3d,
							Billboarding bb, SpriteSheet sprite) : base() {
			_location = location;
			_scale = scale;
            _pbox = pbox;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
			_mesh = null;
			_texture = null;
			_sprite = sprite;
			_cycleNum = 0;
			_frameNum = 0;
			_frame3d = 0;
			_is3dGeo = false;
			_hascbox = false;
            canSquish = false;
			_collidesIn2d = collidesIn2d;
			_collidesIn3d = collidesIn3d;
			_billboards = bb;
			_animDirection = 1;
		}

		private bool _is3dGeo;
		public bool is3dGeo {
			get { return _is3dGeo; }
		}

		private ObjMesh _mesh; //null for sprites
		public ObjMesh mesh {
			get { return _mesh; }
		}

		private MeshTexture _texture; //null for sprites
		public MeshTexture texture {
			get { return _texture; }
		}

		private SpriteSheet _sprite; //null for 3d objects
		public SpriteSheet sprite {
			get { return _sprite; }
		}

        private Vector3 _scale;
		public Vector3 scale {
			get { return _scale; }
			set { _scale = value; }
        }

        private Vector3 _pbox;
		public Vector3 pbox {
            get { return _pbox; }
        }

        private bool _canSquish;
        public bool canSquish {
            get { return _canSquish; }
            set { _canSquish = value; }
        }

		private int _cycleNum;
		public int cycleNumber {
			get { return _cycleNum; }
			set { _cycleNum = value; }
		}

		private double _frameNum; //index of the current animation frame
		public double frameNumber {
			get { return _frameNum; }
			set { _frameNum = value; }
		}

		private Billboarding _billboards;
		public Billboarding billboards {
			get { return _billboards; }
		}

		private bool _collidesIn3d;
		public bool collidesIn3d {
			get { return _collidesIn3d; }
		}

		private bool _collidesIn2d;
		public bool collidesIn2d {
			get { return _collidesIn2d; }
		}

		private int _animDirection;
		public int animDirection {
			get { return _animDirection; }
		}

		public bool hasTwoAnims {
			get { return false; } //Change later if we want to add this functionality
		}

		private int _frame3d; //Only used for frame number of 3d geometry obstacles
		public int frame3d {
			get { return _frame3d; }
			set {
				Debug.Assert(_frame3d >= 0);
				_frame3d = value;
			}
		}

		public void doScaleTranslateAndTexture() {
			GL.PushMatrix();
			if(_is3dGeo) {
				_texture.doTexture(_frame3d);
			}

            GL.Translate(_location);
            GL.Scale(_scale);
		}

        public void DoRotate(float rotate, Vector3d axis)
        {
            GL.Rotate(rotate, axis);
        }

		//NOTE: Most obstacles will never have physUpdate called because they are not on the collision list

		/// <summary>
		/// Does a physics update for this enemy if we are in 3d view
		/// </summary>
		/// <param name="time">Time elapsed since last update</param>
		/// <param name="physList">a pointer to physList, the list of all physics objects</param>
		public void physUpdate3d(double time, List<PhysicsObject> physList) {
			//For now, assume obstacles don't have acceleration or gravity

			//now check for collisions and move
			List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

			while(time > 0.0) {
				PhysicsObject collidingObj = null;
				float collidingT = 1.0f / 0.0f; //pos infinity
				int collidingAxis = -1;

				foreach(PhysicsObject obj in physList) {
					// don't do collision physics to yourself, or on things you already hit this frame
					if(obj.collidesIn3d && obj != this && !alreadyCollidedList.Contains(obj)) {
						//find possible ranges of values for which a collision may have occurred
						Vector3 maxTvals = VectorUtil.div(obj.location + obj.pbox - _location + _pbox, velocity);
						Vector3 minTvals = VectorUtil.div(obj.location - obj.pbox - _location - _pbox, velocity);
						VectorUtil.sort(ref minTvals, ref maxTvals);

						float minT = VectorUtil.maxVal(minTvals);
						float maxT = VectorUtil.minVal(maxTvals);

						// all three axes? || forward? || within range?
						if(minT > maxT || minT < 0.0f || minT > time) {
							continue; //no intersection
						}

						//if we're here, there's a collision
						//see if this collision is the first to occur
						if(minT < collidingT) {
							collidingT = minT;
							collidingObj = obj;
							collidingAxis = VectorUtil.maxIndex(minTvals);
						}
					}
				}

				Vector3 startLoc = new Vector3(_location.X, _location.Y, _location.Z);
				if(collidingAxis == -1) { //no collision
					_location += velocity * (float)time;
					time = 0.0;
				} else {
					alreadyCollidedList.Add(collidingObj);
					if(!collidingObj.hascbox) { //if this is a normal physics collision
						switch(collidingAxis) {
							case 0: //x
								if(_location.X < collidingObj.location.X) {
									_location.X = collidingObj.location.X - (pbox.X + collidingObj.pbox.X) - 0.0001f;
								} else {
									_location.X = collidingObj.location.X + pbox.X + collidingObj.pbox.X + 0.0001f;
								}
								if(velocity.X != 0) {//should always be true, but just in case...
									double deltaTime = (location.X - startLoc.X) / velocity.X;
									_location.Y += (float)(velocity.Y * deltaTime);
									_location.Z += (float)(velocity.Z * deltaTime);
									time -= deltaTime;
								}
								velocity.X = 0;
								break;
							case 1: //y
								if(_location.Y < collidingObj.location.Y) {
									_location.Y = collidingObj.location.Y - (pbox.Y + collidingObj.pbox.Y) - 0.0001f;
								} else {
									_location.Y = collidingObj.location.Y + pbox.Y + collidingObj.pbox.Y + 0.0001f;
								}
								if(velocity.Y != 0) {//should always be true, but just in case...
									double deltaTime = (location.Y - startLoc.Y) / velocity.Y;
									_location.X += (float)(velocity.X * deltaTime);
									_location.Z += (float)(velocity.Z * deltaTime);
									time -= deltaTime;
								}
								velocity.Y = 0;
								break;
							case 2: //z
								if(_location.Z < collidingObj.location.Z) {
									_location.Z = collidingObj.location.Z - (pbox.Z + collidingObj.pbox.Z) - 0.0001f;
								} else {
									_location.Z = collidingObj.location.Z + pbox.Z + collidingObj.pbox.Z + 0.0001f;
								}
								if(velocity.Z != 0) {//should always be true, but just in case...
									double deltaTime = (location.Z - startLoc.Z) / velocity.Z;
									_location.X += (float)(velocity.X * deltaTime);
									_location.Y += (float)(velocity.Y * deltaTime);
									time -= deltaTime;
								}
								velocity.Z = 0;
								break;
						}
					} else { //this is a combat collision
						switch(((CombatObject)collidingObj).type) {
							case (int)CombatType.projectile:
								//Just despawn the projectile and move on
								((CombatObject)collidingObj).health = 0;
								break;
							case (int)CombatType.boss:
								//For crates in the boss battle, we need to temporarily ignore the boss
								//The boss object will move itself however we move
								break;
							case (int)CombatType.grenade:
								//TODO: Implement!!
								break;
							default:
								//For now do nothing, add other cases as needed
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Does a physics update for this enemy if we are in 2d view
		/// </summary>
		/// <param name="time">Time elapsed since last update</param>
		/// <param name="physList">a pointer to physList, the list of all physics objects</param>
		public void physUpdate2d(double time, List<PhysicsObject> physList) {
			//For now, assume obstacles don't have acceleration or gravity

			velocity.Z = 0; //special case for 2d

			//now check for collisions and move
			List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

			while(time > 0.0) {
				PhysicsObject collidingObj = null;
				float collidingT = 1.0f / 0.0f; //pos infinity
				int collidingAxis = -1;

				foreach(PhysicsObject obj in physList) {
					// don't do collision physics to yourself, or on things you already hit this frame
					if(obj.collidesIn2d && obj != this && !alreadyCollidedList.Contains(obj)) {
						if(obj.hascbox) {
							//find possible ranges of values for which a collision may have occurred
							Vector3 maxTvals = VectorUtil.div(obj.location + obj.pbox - _location + _pbox, velocity);
							Vector3 minTvals = VectorUtil.div(obj.location - obj.pbox - _location - _pbox, velocity);
							VectorUtil.sort(ref minTvals, ref maxTvals);

							float minT = VectorUtil.maxVal(minTvals.Xy);
							float maxT = VectorUtil.minVal(maxTvals.Xy);

							// both axes? || forward? || within range?
							if(minT > maxT || minT < 0.0f || minT > time) {
								continue; //no intersection
							}

							//if we're here, there's a collision
							//see if this collision is the first to occur
							if(minT < collidingT) {
								collidingT = minT;
								collidingObj = obj;
								collidingAxis = VectorUtil.maxIndex(minTvals.Xy);
							}
						} else { //Obstacles do 3D collision with other obstacles - allows moving platforms to ignore 2D structure
							//find possible ranges of values for which a collision may have occurred
							Vector3 maxTvals = VectorUtil.div(obj.location + obj.pbox - _location + _pbox, velocity);
							Vector3 minTvals = VectorUtil.div(obj.location - obj.pbox - _location - _pbox, velocity);
							VectorUtil.sort(ref minTvals, ref maxTvals);

							float minT = VectorUtil.maxVal(minTvals);
							float maxT = VectorUtil.minVal(maxTvals);

							// all three axes? || forward? || within range?
							if(minT > maxT || minT < 0.0f || minT > time) {
								continue; //no intersection
							}

							//if we're here, there's a collision
							//see if this collision is the first to occur
							if(minT < collidingT) {
								collidingT = minT;
								collidingObj = obj;
								collidingAxis = VectorUtil.maxIndex(minTvals);
							}
						}
					}
				}

				Vector3 startLoc = new Vector3(_location.X, _location.Y, _location.Z);
				if(collidingAxis == -1) { //no collision
					_location += velocity * (float)time;
					time = 0.0;
				} else {
					alreadyCollidedList.Add(collidingObj);
					if(!collidingObj.hascbox) { //if this is a normal physics collision
						switch(collidingAxis) {
							case 0: //x
								if(_location.X < collidingObj.location.X) {
									_location.X = collidingObj.location.X - (pbox.X + collidingObj.pbox.X) - 0.0001f;
								} else {
									_location.X = collidingObj.location.X + pbox.X + collidingObj.pbox.X + 0.0001f;
								}
								if(velocity.X != 0) {//should always be true, but just in case...
									double deltaTime = (location.X - startLoc.X) / velocity.X;
									_location.Y += (float)(velocity.Y * deltaTime);
									time -= deltaTime;
								}
								velocity.X = 0;
								break;
							case 1: //y
								if(_location.Y < collidingObj.location.Y) {
									_location.Y = collidingObj.location.Y - (pbox.Y + collidingObj.pbox.Y) - 0.0001f;
								} else {
									_location.Y = collidingObj.location.Y + pbox.Y + collidingObj.pbox.Y + 0.0001f;
								}
								if(velocity.Y != 0) {//should always be true, but just in case...
									double deltaTime = (location.Y - startLoc.Y) / velocity.Y;
									_location.X += (float)(velocity.X * deltaTime);
									time -= deltaTime;
								}
								velocity.Y = 0;
								break;
							//NOTE: Still don't need to check for z (even though some collisions are done 3D)
							//      because our z velocity is still 0, so we should never collide in z
						}
					} else { //this is a combat collision
						switch(((CombatObject)collidingObj).type) {
							case (int)CombatType.projectile:
								//Just despawn the projectile and move on
								((CombatObject)collidingObj).health = 0;
								break;
							case (int)CombatType.boss:
								//For crates in the boss battle, we need to temporarily ignore the boss
								//The boss object will move itself however we move
								break;
							case (int)CombatType.grenade:
								//TODO: Implement!!
								break;
							default:
								//For now do nothing, add other cases as needed
								break;
						}
					}
				}
			}
		}

		public void accelerate(Vector3 acceleration) {
			//obstacles don't use this for now
		}

		//swaps physics box x and z coordinates (used for sprites that billboard)
		public void swapPBox() {
			float temp = _pbox.X;
			_pbox.X = _pbox.Z;
			_pbox.Z = temp;
		}
	}
}
