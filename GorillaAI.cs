﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Engine;
namespace U5Designs {
    class GorillaAI : BossAI {

        bool active;
        Random rng;

        GorillaBossobject boss;
        bool barrelDelayed;
        double delayTimer, delayTime; // timer = running time, time = time we are waiting
		ProjectileProperties barrelProp;
		PlayState ps;

		List<Projectile> barrels; //keep this so we can delete barrels when they're offscreen

        public GorillaAI(Player player, PlayState ps) {
			this.ps = ps;
            rng = new Random(System.DateTime.Now.Millisecond);
            SpriteSheet gorillaSprite = LoadLevel.parseSpriteFile("gorilla_sprite.dat");
            barrelProp = LoadLevel.parseProjectileFile("barrel_projectile.dat", ps);
            Vector3 location = new Vector3(4370,162.5f,51);
            Vector3 scale = new Vector3(50.0f, 50.0f, 50.0f);
            Vector3 pbox = new Vector3(6, 6, 6);
            Vector3 cbox = new Vector3(6, 6, 6);
            boss = new GorillaBossobject(player, location, pbox, cbox, scale, true, true, 6, 2, 1, gorillaSprite);
            ps.objList.Add(boss);
            ps.physList.Add(boss);
            ps.collisionList.Add(boss);
            ps.renderList.Add(boss);
            ps.combatList.Add(boss);
			barrels = new List<Projectile>();

            delayTime = 3;
            delayTimer = 0;
            barrelDelayed = true;
        }

		public void makeActive() {
			active = true;
		}

        /*World cordinates for the boss area are 4150x to 4350x -50z to 150z
         * "barrel throw kill zone" is 4275 - 4310 */
        public void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d) {
            if (active) {
                if (barrelDelayed) {
                    delayTimer = delayTimer + time;
                    if (delayTimer > delayTime) {
                        delayTimer = 0;
                        barrelDelayed = false;
                    }
                }
                else {
                    //throw barrels
                    int inverseHP = (6 - boss.health) + 1; // used as a multiplier, the lower the bosses health gets, the higher this value becomes.
                    for (int i = 0; i < inverseHP + 2; i++) { // throw at least 3 barrels + more the weaker boss gets
                        Vector3 Target = new Vector3(rng.Next(4275, 4310), 75, rng.Next(-50, 150));
                        spawnProjectile(Target);
                    }

                    //set delaytime based on health
                    delayTime = 1 + (2 / inverseHP);
                    barrelDelayed = true;
                }
            }
			for(int i = barrels.Count - 1; i >= 0; i--) {
				Projectile cur = barrels[i];
				if(cur.location.X <= 4100.0f) {
					ps.objList.Remove(cur);
					ps.renderList.Remove(cur);
					ps.collisionList.Remove(cur);
					ps.physList.Remove(cur);
					ps.combatList.Remove(cur);
					barrels.RemoveAt(i);
				}
			}
        }

		public int gethealth() {
            return boss.health;
        }

        public void killBoss(PlayState ps) {
            //TODO: whatever happens when the boss dies
            ps.objList.Remove(boss);
            ps.physList.Remove(boss);
            ps.collisionList.Remove(boss);
            ps.renderList.Remove(boss);
            ps.combatList.Remove(boss); 
            active = false;
        }

        /// <summary>
        /// Fires a projectile at the specific point, which is a world cordinate
        /// </summary>
        /// <param name="point">The point at which to fire a projectile</param>
		public void spawnProjectile(Vector3 point) {
			//Move the calculation into the x plane for simplicity
			Vector3 projDir = new Vector3((float)Math.Sqrt(Math.Pow(point.X - boss.location.X, 2) + Math.Pow(point.Z - boss.location.Z, 2)),
											(point.Y - boss.location.Y), 0.0f);
			double gravity = GameObject.gravity;

			//Following is adapted from http://en.wikipedia.org/wiki/Trajectory_of_a_projectile#Angle_required_to_hit_coordinate_.28x.2Cy.29
			double velsquared = barrelProp.speed * barrelProp.speed;
			double sqrtPart = -Math.Sqrt(velsquared * velsquared - gravity * (gravity * projDir.X * projDir.X + 2 * projDir.Y * velsquared));
			double theta;
			if(sqrtPart == sqrtPart) { //false when sqrtPart == NaN
				theta = Math.Atan((velsquared - sqrtPart) / (gravity * projDir.X));
			} else {
				//Calculate how far in that direction we can get
				if(projDir.X == 0) { //Avoid divide by zero
					theta = Math.PI / 2;
				} else {
					double phi = Math.Atan(projDir.Y / projDir.X);
					double cosphi = Math.Cos(phi);
					double r = (gravity * velsquared * (1 - Math.Sin(phi))) / (gravity * gravity * cosphi * cosphi);
					r -= 0.01f; //This prevents a floating point roundoff bug
					projDir.X = (float)(r * cosphi);
					projDir.Y = (float)(r * Math.Sin(phi));
					sqrtPart = -Math.Sqrt(velsquared * velsquared - gravity * (gravity * projDir.X * projDir.X + 2 * projDir.Y * velsquared));
					theta = Math.Atan((velsquared - sqrtPart) / (gravity * projDir.X));
				}
			}

			projDir.X = (float)Math.Cos(theta);
			projDir.Y = (float)Math.Sin(theta);

			//Take the x coordinate and restore it to the proper x/z direction
			double phi2 = Math.Atan((point.X - boss.location.X) / (point.Z - boss.location.Z));
			if(point.Z < boss.location.Z) {
				phi2 += Math.PI;
			}
			projDir.Z = (float)(projDir.X * Math.Cos(phi2));
			projDir.X = (float)(projDir.X * Math.Sin(phi2));


			Vector3 projlocation = new Vector3(boss.location);

			//break the rendering tie between boss and projectile, or else they flicker
			projlocation.Z -= 0.001f;

			Projectile shot = new Projectile(projlocation, projDir, false, barrelProp, ps.player, (int)CombatType.barrel); // spawn the actual projectile
			if(ps.enable3d) {
				shot.cycleNumber = 1;
			}
			
			// add projectile to appropriate lists
			ps.objList.Add(shot);
			ps.renderList.Add(shot);
			ps.collisionList.Add(shot);
			ps.physList.Add(shot);
			ps.combatList.Add(shot);
		}
    }



    public class GorillaBossobject : GameObject, RenderObject, PhysicsObject, CombatObject, BossObject {

        //physics vars
        internal Vector3 velocity;
        internal Vector3 accel;
        private bool doesGravity; //true if gravity affects this object
        public bool moving; //true when the enemy is currently moving (used for animations)

        //timers
        public bool frozen;
        private double freezetimer;
        private double maxFreezeTime;



        public Player player;


        public GorillaBossobject(Player player, Vector3 location, Vector3 pbox, Vector3 cbox, Vector3 scale, bool existsIn2d, bool existsIn3d, int health, int damage, float speed,
            SpriteSheet sprite) {
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
            _hascbox = true;
            _type = 3;
            frozen = false;
            freezetimer = 0;
            maxFreezeTime = 0.7;
            this.player = player;

            _mesh = null;
            _texture = null;
            _sprite = sprite;
            _frameNum = 0;
            _is3dGeo = false;
            _animDirection = 1;

            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = false;
        }

        public void dodamage(int hit) {
            health = health - 1;
        }
        
        /// <summary>
        /// Does a physics update for this enemy if we are in 3d view
        /// </summary>
        /// <param name="time">Time elapsed since last update</param>
        /// <param name="physList">a pointer to physList, the list of all physics objects</param>
        public void physUpdate3d(double time, List<PhysicsObject> physList) {
            if (frozen) {
                freezetimer = freezetimer + time;
                if (freezetimer > maxFreezeTime) {
                    frozen = false;
                    freezetimer = 0;
                }
            }
            if (!frozen) {
                //save location to determine later if we moved
                Vector3 initLoc = new Vector3(_location);

                if (doesGravity) {
                    accel.Y -= (float)(gravity * time);
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
                        if (obj.collidesIn3d && obj != this && !alreadyCollidedList.Contains(obj)) {
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
                            switch (((CombatObject)collidingObj).type) {
                                case (int)CombatType.projectile: // obj is a projectile, despawn projectile do damage
                                    if (((Projectile)collidingObj).playerspawned) {
                                        time = 0.0;
                                        player.HitSound.Play();
                                        _health = _health - ((CombatObject)collidingObj).damage;
                                        //despawn the projectile
                                        ((CombatObject)collidingObj).health = 0;
                                    }
                                    break;

                                case (int)CombatType.player: //if the collidingObj is the player, do damage and knock the player back
                                    time = 0.0;
                                    player.HurtSound.Play();
                                    player.health = player.health - _damage;
                                    player.Invincible = true;
                                    player.Invincibletimer = 1.0;
                                    player.HasControl = false;
                                    frozen = true;

                                    player.knockback(true, this);
                                    break;
                            }
                        }
                    }
                }

                //Set moving flag
                if (_location.X != initLoc.X || _location.Y != initLoc.Y || _location.Z != initLoc.Z) {
                    moving = true;
                }
                else {
                    moving = false;
                }
            }
        }

        /// <summary>
        /// Does a physics update for this enemy if we are in 2d view
        /// </summary>
        /// <param name="time">Time elapsed since last update</param>
        /// <param name="physList">a pointer to physList, the list of all physics objects</param>
        public void physUpdate2d(double time, List<PhysicsObject> physList) {
            if (frozen) {
                freezetimer = freezetimer + time;
                if (freezetimer > maxFreezeTime) {
                    frozen = false;
                    freezetimer = 0;
                }
            }
            if (!frozen) {
                //save location to determine later if we moved
                Vector3 initLoc = new Vector3(_location);

                //first do gravity
                if (doesGravity) {
                    accel.Y -= (float)(gravity * time);
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
                        if (obj.collidesIn2d && obj != this && !alreadyCollidedList.Contains(obj)) {
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
                            switch (((CombatObject)collidingObj).type) {
                                case (int)CombatType.projectile: // obj is a projectile, despawn projectile do damage
                                    if (((Projectile)collidingObj).playerspawned) {
                                        time = 0.0;
                                        player.HitSound.Play();
                                        _health = _health - ((CombatObject)collidingObj).damage;
                                        //despawn the projectile
                                        ((CombatObject)collidingObj).health = 0;
                                    }
                                    break;

                                case (int)CombatType.player: //if the collidingObj is the player, do damage and knock the player back
                                    time = 0.0;
                                    player.HurtSound.Play();
                                    player.health = player.health - _damage;
                                    player.Invincible = true;
                                    player.Invincibletimer = 1.0;
                                    player.HasControl = false;
                                    frozen = true;

                                    player.knockback(false, this);
                                    break;
                            }
                        }
                    }
                }

                //Set moving flag
                if (_location.X != initLoc.X || _location.Y != initLoc.Y) {
                    moving = true;
                }
                else {
                    moving = false;
                }
            }
        }
        /*  The following are helper methods + getter/setters
        * 
        */

        //swaps physics box x and z coordinates (used for sprites that billboard)
        public void swapPBox() {
            float temp = _pbox.X;
            _pbox.X = _pbox.Z;
            _pbox.Z = temp;
        }

        //swaps combat box x and z coordinates (used for sprites that billboard)
        public void swapCBox() {
            float temp = _cbox.X;
            _cbox.X = _cbox.Z;
            _cbox.Z = temp;
        }
        private bool _is3dGeo;
        public bool is3dGeo {
            get { return _is3dGeo; }
        }

        public bool canSquish {
            get { return false; }
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

        private Effect _deathAnim;
        public Effect deathAnim {
            get { return _deathAnim; }
        }

        private int _animDirection;
        public int animDirection {
            get { return _animDirection; }
        }

        public bool hasTwoAnims {
            //Change to variable later if we add enemies with only 1 animation
            get { return true; }
        }

        public int ScreenRegion {
            get { return screenRegion; }
        }

        public bool drawWhenOffScreen {
            get { return false; }
        }

        public void doScaleTranslateAndTexture() {
            GL.PushMatrix();

            if (_is3dGeo) {
                _texture.doTexture();
            }

            GL.Translate(_location);
            GL.Scale(_scale);
        }
    }
}