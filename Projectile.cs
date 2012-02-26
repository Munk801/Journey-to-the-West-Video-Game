using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Engine;

namespace U5Designs {
    /*
     * projectiles belong on the following lists:
     * objList
     * renderList
     * colisionList
     * physList
     * combatList
     * */
    class Projectile : GameObject, RenderObject, CombatObject, PhysicsObject {

        internal Vector3 velocity;
        internal Vector3 accel;
        internal Vector3 direction;
        internal bool doesGravity, playerspawned; //true if gravity affects this object\
 

        public Projectile(Vector3 location, Vector3 direction, Vector3 scale, Vector3 pbox, Vector3 cbox, bool existsIn2d, bool existsIn3d, bool in3d, int damage, float speed, bool gravity, bool PlayerSpawned, SpriteSheet sprite) {
			_location = location;
            this.direction = direction;
			_scale = scale;
            _pbox = pbox;
            _cbox = cbox;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
            _health = 1; // health 1 = active, health 0 = despawning, waiting for cleanup in PlayState
            _damage = damage;
            _speed = speed;
            _alive = true;
            _hascbox = true;
            _type = 2; // type 2 means this is a projectile

			_mesh = null;
			_texture = null;
			_sprite = sprite;
			_frameNum = 0;
			_is3dGeo = false;
            playerspawned = PlayerSpawned;

            velocity = new Vector3(0, 0, 0);
            velocity.X = (float)(speed * direction.X);
            velocity.Y = (float)(speed * direction.Y);
            velocity.Z = (float)(speed * direction.Z);
            if (!in3d)
                velocity.Z = 0;
            accel = new Vector3(0, 0, 0);
            doesGravity = gravity;
		}

		private bool _is3dGeo;
		public bool is3dGeo {
			get { return _is3dGeo; }
		}

		private int _health;
		public int health {
			get { return _health; }
			set { _health = value; }
		}

		private int _damage;
		public int damage {
			get { return _damage; }
		}

        private float _speed;
		public float speed {
            get { return _speed; }
            set { _speed = value; }
        }
		private bool _alive;
		public bool alive {
			get { return _alive; }
			set { _alive = value; }
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
        }

        private Vector3 _pbox;
		public Vector3 pbox {
            get { return _pbox; }
        }

        private Vector3 _cbox;
		public Vector3 cbox {
            get { return _cbox; }
		}

        private int _type;
        public int type {
            get { return _type; }
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


        public void accelerate(Vector3 acceleration) {
            accel += acceleration;
        }

		public void doScaleTranslateAndTexture() {
            GL.PushMatrix();

            if (_is3dGeo) {
                _texture.doTexture();
            }

            GL.Translate(_location);
            GL.Scale(_scale);
		}

        public void physUpdate3d(double time, List<GameObject> objList, List<RenderObject> renderList, List<PhysicsObject> colisionList, List<PhysicsObject> physList, List<CombatObject> combatList) {
            if (doesGravity) {
                accel.Y -= (float)(400 * time); //TODO: turn this into a constant somewhere
            }
            //now do acceleration
            velocity += accel;
            accel.X = 0;
            accel.Y = 0;
            accel.Z = 0;

            //now check for collisions and move
            List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

            while (time > 0.0) {
                PhysicsObject collidingObj = null;
                float collidingT = 1.0f / 0.0f; //pos infinity
                int collidingAxis = -1;

                foreach (PhysicsObject obj in physList) {
                    // don't do collision physics to yourself, or on things you already hit this frame
                    if (obj != this && !alreadyCollidedList.Contains(obj)) {
                        Vector3 mybox, objbox;
                        if (obj.hascbox) {
                            mybox = _cbox;
                            objbox = ((CombatObject)obj).cbox;
                        }
                        else {
                            mybox = _pbox;
                            objbox = obj.pbox;
                        }
                        //find possible ranges of values for which a collision may have occurred
                        Vector3 maxTvals = VectorUtil.div(obj.location + objbox - _location + mybox, velocity);
                        Vector3 minTvals = VectorUtil.div(obj.location - objbox - _location - mybox, velocity);
                        VectorUtil.sort(ref minTvals, ref maxTvals);

                        float minT = VectorUtil.maxVal(minTvals);
                        float maxT = VectorUtil.minVal(maxTvals);

                        // all three axes? || forward? || within range?
                        if (minT > maxT || minT < 0.0f || minT > time) {
                            continue; //no intersection
                        }

                        //if we're here, there's a collision
                        //see if this collision is the first to occur
                        if (minT < collidingT) {
                            collidingT = minT;
                            collidingObj = obj;
                            collidingAxis = VectorUtil.maxIndex(minTvals);
                        }
                    }
                }

                Vector3 startLoc = new Vector3(_location.X, _location.Y, _location.Z);
                if (collidingAxis == -1) { //no collision
                    _location += velocity * (float)time;
                    time = 0.0;
                }
                else {
                    alreadyCollidedList.Add(collidingObj);
                    if (!collidingObj.hascbox) { //if this is a normal physics collision
                        health = 0;
                    }
                    else { //this is a combat collision
                        //time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
                        if (((CombatObject)collidingObj).type == 0) { //hit the player
                            if (!playerspawned) {
                                time = 0.0;
                                ((CombatObject)collidingObj).health = ((CombatObject)collidingObj).health - this.damage;
                                health = 0;
                            }
							//else {
							//    _location += velocity * (float)time;
							//    time = 0.0;
							//}
                        }
                        if (((CombatObject)collidingObj).type == 1) { //hit an enemy
                            if (playerspawned) {
                                time = 0.0;
                                ((CombatObject)collidingObj).health = ((CombatObject)collidingObj).health - this.damage;
                                health = 0;
                            }
							//else {
							//    _location += velocity * (float)time;
							//    time = 0.0;
							//}
                        }
                    }
                }
            }
        }
		

        public void physUpdate2d(double time, List<GameObject> objList, List<RenderObject> renderList, List<PhysicsObject> colisionList, List<PhysicsObject> physList, List<CombatObject> combatList) {
            //first do gravity
            if (doesGravity) {
                accel.Y -= (float)(400 * time); //TODO: turn this into a constant somewhere
            }
            
            //now deal with acceleration
            velocity += accel;
            accel.X = 0;
            accel.Y = 0;
            accel.Z = 0;

            //now check for collisions and move
            List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

            while (time > 0.0) {
                PhysicsObject collidingObj = null;
                float collidingT = 1.0f / 0.0f; //pos infinity
                int collidingAxis = -1;

                foreach (PhysicsObject obj in physList) {
                    // don't do collision physics to yourself, or on things you already hit this frame
                    if (obj != this && !alreadyCollidedList.Contains(obj)) {
                        Vector3 mybox, objbox;
                        if (obj.hascbox) {
                            mybox = _cbox;
                            objbox = ((CombatObject)obj).cbox;
                        }
                        else {
                            mybox = _pbox;
                            objbox = obj.pbox;
                        }
                        //find possible ranges of values for which a collision may have occurred
                        Vector3 maxTvals = VectorUtil.div(obj.location + objbox - _location + mybox, velocity);
                        Vector3 minTvals = VectorUtil.div(obj.location - objbox - _location - mybox, velocity);
                        VectorUtil.sort(ref minTvals, ref maxTvals);

                        float minT = VectorUtil.maxVal(minTvals.Xy);
                        float maxT = VectorUtil.minVal(maxTvals.Xy);

                        // both axes? || forward? || within range?
                        if (minT > maxT || minT < 0.0f || minT > time) {
                            continue; //no intersection
                        }

                        //if we're here, there's a collision
                        //see if this collision is the first to occur
                        if (minT < collidingT) {
                            collidingT = minT;
                            collidingObj = obj;
                            collidingAxis = VectorUtil.maxIndex(minTvals.Xy);
                        }
                    }
                }

                Vector3 startLoc = new Vector3(_location.X, _location.Y, _location.Z);
                if (collidingAxis == -1) { //no collision
                    _location += velocity * (float)time;
                    time = 0.0;
                }
                else {
                    alreadyCollidedList.Add(collidingObj);
                    if (!collidingObj.hascbox) { //if this is a normal physics collision
                        health = 0;
                    }
                    else { //this is a combat collision
                        if (((CombatObject)collidingObj).type == 0) { //hit the player
                            if (!playerspawned) {
                                time = 0.0;
                                ((CombatObject)collidingObj).health = ((CombatObject)collidingObj).health - this.damage;
                                health = 0;
                            }
                        }
                        if (((CombatObject)collidingObj).type == 1) { //hit an enemy
                            if (playerspawned) {
                                time = 0.0;
                                ((CombatObject)collidingObj).health = ((CombatObject)collidingObj).health - this.damage;
                                health = 0;
                            }
                        }
                    }
                }
            }
        }

		public void reset() {
			throw new NotImplementedException();
		}
	}
}
