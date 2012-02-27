using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Engine;

namespace U5Designs
{
    public class Enemy : GameObject, AIObject, RenderObject, PhysicsObject, CombatObject
    {
		private int AItype; //NOTE: to see what number corresponds to which AI look at the comments for InitlizeAI() (its below the 2 physics methods)
        internal Vector3 velocity;
        internal Vector3 accel;
        private bool doesGravity; //true if gravity affects this object
        public bool frozen;
        private double freezetimer;
        internal SpriteSheet projectileSprite;

        //attack delay stuff
        internal int attackspeed;
        internal bool attackdelayed;
        internal double attacktimer;

        Stack<Airoutine> CurrentAI; // This is the stack holding the AI routines, functions identical to the state stack in GameEngine

        public Player player;

		public Enemy(Vector3 location, Vector3 scale, Vector3 pbox, Vector3 cbox, bool existsIn2d, bool existsIn3d, int health, int damage, float speed, int AItype, ObjMesh mesh, MeshTexture texture) {
			_location = location;
            _scale = scale;
            _pbox = pbox;
            _cbox = cbox;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
            _health = health;
            _damage = damage;
            _speed = speed;
            _alive = true;
            this.AItype = AItype;
            _hascbox = true;
            _type = 1;
            frozen = false;
            freezetimer = 0;
            attackspeed = 1;
            attacktimer = 0;
            

			_mesh = mesh;
			_texture = texture;
			_sprite = null;
			_cycleNum = 0;
			_frameNum = 0;
			_is3dGeo = true;

            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = true;

            CurrentAI = new Stack<Airoutine>();
            InitilizeAI();
		}


        public Enemy(Vector3 location, Vector3 scale, Vector3 pbox, Vector3 cbox, bool existsIn2d, bool existsIn3d, int health, int damage, float speed, int AItype, SpriteSheet sprite, SpriteSheet projectileSprite)
        {
			_location = location;
			_scale = scale;
            _pbox = pbox;
            _cbox = cbox;
			_existsIn3d = existsIn3d;
			_existsIn2d = existsIn2d;
            _health = health;
            _damage = damage;
            _speed = speed;
            _alive = true;
            this.AItype = AItype;
            _hascbox = true;
            _type = 1; // type one means this is an enemy
            frozen = false;
            freezetimer = 0;
            attackspeed = 1;
            attacktimer = 0;

			_mesh = null;
			_texture = null;
			_sprite = sprite;
            this.projectileSprite = projectileSprite;
			_frameNum = 0;
			_is3dGeo = false;

            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = true;

            CurrentAI = new Stack<Airoutine>();
            InitilizeAI();
		}


        /** Like the Player Status update call this every time you need to update an Enemies State before saving **/
        public void updateState()
        {
            // Add in the other State elements that will need to be maintained here..
		}

        // Pushes a new state onto the stack, calls the states Init method, deletes old state(ie launch game, nuke menu)
        internal void ChangeState(Airoutine ai) {
            if (CurrentAI.Count != 0) {
                CurrentAI.Pop();
            }

            CurrentAI.Push(ai);
        }
        // same as changestate but doesnt delete old state(ie pause game, bringup menu)
        internal void PushState(Airoutine ai) {
            CurrentAI.Push(ai);
        }
        // pops the current state off and lets the next state have control(menu nukes self, resumes game)
        public void PopState() {
            if (CurrentAI.Count > 0) {
                CurrentAI.Pop();
            }
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
		public Vector3 scale
        {
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

		public Billboarding billboards {
			get { return Billboarding.Yes; }
		}

		public bool collidesIn3d {
			get { return true; }
		}

		public bool collidesIn2d {
			get { return true; }
		}

        public void reset() {
            throw new NotImplementedException();
        }

        public void accelerate(Vector3 acceleration) {
            accel += acceleration;
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

		public void doScaleTranslateAndTexture() {
            GL.PushMatrix();

			if(_is3dGeo) {
				_texture.doTexture();
			}

            GL.Translate(_location);
            GL.Scale(_scale);
		}

        public void physUpdate3d(double time, List<PhysicsObject> physList) {
            if (frozen)
                freezetimer = freezetimer + time;
            if (freezetimer > 1) {
                frozen = false;
                freezetimer = 0;
            }
            if (!frozen) {
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
						if(obj.collidesIn3d && obj != this && !alreadyCollidedList.Contains(obj)) {
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
                            switch (collidingAxis) {
                                case 0: //x
                                    if (_location.X < collidingObj.location.X) {
                                        _location.X = collidingObj.location.X - (pbox.X + collidingObj.pbox.X) - 0.0001f;
                                    }
                                    else {
                                        _location.X = collidingObj.location.X + pbox.X + collidingObj.pbox.X + 0.0001f;
                                    }
                                    if (velocity.X != 0) {//should always be true, but just in case...
                                        double deltaTime = (location.X - startLoc.X) / velocity.X;
                                        _location.Y += (float)(velocity.Y * deltaTime);
                                        _location.Z += (float)(velocity.Z * deltaTime);
                                        time -= deltaTime;
                                    }
                                    velocity.X = 0;
                                    break;
                                case 1: //y
                                    if (_location.Y < collidingObj.location.Y) {
                                        _location.Y = collidingObj.location.Y - (pbox.Y + collidingObj.pbox.Y) - 0.0001f;
                                    }
                                    else {
                                        _location.Y = collidingObj.location.Y + pbox.Y + collidingObj.pbox.Y + 0.0001f;
                                    }
                                    if (velocity.Y != 0) {//should always be true, but just in case...
                                        double deltaTime = (location.Y - startLoc.Y) / velocity.Y;
                                        _location.X += (float)(velocity.X * deltaTime);
                                        _location.Z += (float)(velocity.Z * deltaTime);
                                        time -= deltaTime;
                                    }
                                    velocity.Y = 0;
                                    break;
                                case 2: //z
                                    if (_location.Z < collidingObj.location.Z) {
                                        _location.Z = collidingObj.location.Z - (pbox.Z + collidingObj.pbox.Z) - 0.0001f;
                                    }
                                    else {
                                        _location.Z = collidingObj.location.Z + pbox.Z + collidingObj.pbox.Z + 0.0001f;
                                    }
                                    if (velocity.Z != 0) {//should always be true, but just in case...
                                        double deltaTime = (location.Z - startLoc.Z) / velocity.Z;
                                        _location.X += (float)(velocity.X * deltaTime);
                                        _location.Y += (float)(velocity.Y * deltaTime);
                                        time -= deltaTime;
                                    }
                                    velocity.Z = 0;
                                    break;
                            }
                        }
                        else { //this is a combat collision
                            if (((CombatObject)collidingObj).type == 2) { // obj is a projectile, despawn projectile do damage
                                if (((Projectile)collidingObj).playerspawned) {
                                    time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
                                    _health = _health - ((CombatObject)collidingObj).damage;
                                    //despawn the projectile
                                    ((CombatObject)collidingObj).health = 0;
                                }
                            }

                            //if the collidingObj is the player and the player is in some kill enemy state, hurt the enemy else hurt player and kb player
                            if (((CombatObject)collidingObj).type == 0) {
                                time = 0.0;
                                player.health = player.health - _damage;
                                player.Invincible = true;
                                player.HasControl = false;
                                frozen = true;

                                player.knockback(true, this);
                            }
                        }
                    }
                }

                
            }
        }

        public void physUpdate2d(double time, List<PhysicsObject> physList) {
            if (frozen)
                freezetimer = freezetimer + time;
            if (freezetimer > 1) {
                frozen = false;
                freezetimer = 0;
            }
            if (!frozen) {
                //first do gravity
                if (doesGravity) {
                    accel.Y -= (float)(400 * time); //TODO: turn this into a constant somewhere
                }
                //now deal with acceleration
                velocity += accel;
                accel.X = 0;
                accel.Y = 0;
                accel.Z = 0;

                velocity.Z = 0; //special case for 2d

                //now check for collisions and move
                List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

                while (time > 0.0) {
                    PhysicsObject collidingObj = null;
                    float collidingT = 1.0f / 0.0f; //pos infinity
                    int collidingAxis = -1;

                    foreach (PhysicsObject obj in physList) {
                        // don't do collision physics to yourself, or on things you already hit this frame
						if(obj.collidesIn2d && obj != this && !alreadyCollidedList.Contains(obj)) {
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
                            switch (collidingAxis) {
                                case 0: //x
                                    if (_location.X < collidingObj.location.X) {
                                        _location.X = collidingObj.location.X - (pbox.X + collidingObj.pbox.X) - 0.0001f;
                                    }
                                    else {
                                        _location.X = collidingObj.location.X + pbox.X + collidingObj.pbox.X + 0.0001f;
                                    }
                                    if (velocity.X != 0) {//should always be true, but just in case...
                                        double deltaTime = (location.X - startLoc.X) / velocity.X;
                                        _location.Y += (float)(velocity.Y * deltaTime);
                                        time -= deltaTime;
                                    }
                                    velocity.X = 0;
                                    break;
                                case 1: //y
                                    if (_location.Y < collidingObj.location.Y) {
                                        _location.Y = collidingObj.location.Y - (pbox.Y + collidingObj.pbox.Y) - 0.0001f;
                                    }
                                    else {
                                        _location.Y = collidingObj.location.Y + pbox.Y + collidingObj.pbox.Y + 0.0001f;
                                    }
                                    if (velocity.Y != 0) {//should always be true, but just in case...
                                        double deltaTime = (location.Y - startLoc.Y) / velocity.Y;
                                        _location.X += (float)(velocity.X * deltaTime);
                                        time -= deltaTime;
                                    }
                                    velocity.Y = 0;
                                    break;
                            }
                        }
                        else { //this is a combat collision
                            time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead      
                            if (((CombatObject)collidingObj).type == 2) { // obj is a projectile, despawn projectile do damage
                                if (((Projectile)collidingObj).playerspawned) {
                                    time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
                                    _health = _health - ((CombatObject)collidingObj).damage;
                                    //despawn the projectile
                                    ((CombatObject)collidingObj).health = 0;
                                }

                            }

                            //if the collidingObj is the player and the player is in some kill enemy state, hurt the enemy else hurt player and kb player
                            if (((CombatObject)collidingObj).type == 0) {
                                time = 0.0;
                                player.health = player.health - _damage;
                                player.Invincible = true;
                                player.HasControl = false;
                                frozen = true;

                                player.knockback(false, this);
                            }
                        }
                    }
                }
            }
        }

        /* Hardcoded stuff time.
         * AItype:
         * 1 = icecream throwing kid. walks to the player up to a set distance
         *      once close enough, he throws his projectile at the player
         *      
         * 2 = flying bird, flyes to the player, when close enough will swoop down on player
         * 
         * 
         */
        private void InitilizeAI() {
            if (AItype == 1)
                CurrentAI.Push(new Kidmoveto());
            if (AItype == 2) {
                doesGravity = false;
                CurrentAI.Push(new Birdmoveto());
            }
        }

		internal void aiUpdate(FrameEventArgs e, PlayState playstate, Vector3 playerposn, bool enable3d) {

            CurrentAI.Peek().update(e, playstate, playerposn, this, enable3d);
        }

        // calculates the literal distance between 2 points
        double dist(Vector3 v1, Vector3 v2) {
            Vector3 tmp = new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
            return Math.Sqrt((tmp.X*tmp.X)+(tmp.Y*tmp.Y)+(tmp.Z*tmp.Z));
        }

        // returns a vector 3 containing the direction from this enemy to the player
        Vector3 getdir(Vector3 player, Vector3 enemy) {
            Vector3 tmp = new Vector3(player.X - enemy.X, player.Y - enemy.Y, player.Z - enemy.Z);
            tmp.Normalize();
            return tmp;
        }


	}
}
