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
    /*  Sizes for the boss and his boxes
     * each box must be 75x75
     * pbox = 37.5
     * scale = 37.5
     * 
     * floor is at 125
     * */

    public class ZookeeperAI {
        Boss bossobject;
        int maxHeight = 225;
        internal FallingBox[] boxes;
        internal int currentBossIndex;
        Random rng;

        public ZookeeperAI(Player player, PlayState ps) {
			//Load all resources required for all subclasses here so there is only one copy of each in memory
			SpriteSheet zookeeperSprite = LoadLevel.parseSpriteFile("zoo_keeper_sprite.dat");
			SpriteSheet ropeSprite = LoadLevel.parseSpriteFile("zoo_keeper_rope.dat"); //TODO: Change to rope when available

			ProjectileProperties invisProj = LoadLevel.parseProjectileFile("invisible_projectile.dat", ps);
			invisProj.cbox = new Vector3(24.8f, 0.1f, 24.8f);
			invisProj.pbox = new Vector3(24.8f, 0.1f, 24.8f);
			invisProj.speed = 200;

			List<Vector3> locs = new List<Vector3>();
			for( int i = 0; i < 4; i++){
                for (int k = 0; k < 4; k++) {
					locs.Add(new Vector3(4175 + (50 * i), maxHeight, (-25 + (50 * k))));
				}
			}
			List<Obstacle> boxObstacles = LoadLevel.parseSingleObstacleFile("zoo_keeper_crate.dat", locs);
			List<Obstacle> groundObstacles = LoadLevel.parseSingleObstacleFile("zookeeper_boss_ground.dat", locs);
            List<Obstacle> ropeObstacles = LoadLevel.parseSingleObstacleFile("zookeeper_boss_rope.dat", locs);

			rng = new Random();
            //initialize arrays of crates + boss(FallingBox)
            boxes = new FallingBox[16];
            currentBossIndex = 9;
            /* looking at it from the top down
             * 0  4  8 12
             * 1  5  9 13
             * 2  6 10 14
             * 3  7 11 15
             * */
			for(int i=0; i<16; i++) {
				Obstacle box = boxObstacles[i];
				FallingBox newcrate;
				if(i != currentBossIndex) {
                    newcrate = new Crate(player, ps, new Vector3(box.location), maxHeight, invisProj, box, groundObstacles[i], ropeObstacles[i]);
				} else {
                    newcrate = bossobject = new Boss(player, ps, new Vector3(box.location), maxHeight, invisProj, box, groundObstacles[i], ropeObstacles[i], zookeeperSprite);
					ps.combatList.Add(bossobject);
                    ps.renderList.Add(bossobject);
				}
				boxes[i] = newcrate;
				ps.objList.Add(newcrate);
				ps.physList.Add(newcrate);
			    ps.collisionList.Add(newcrate);
				//ps.renderList.Add(newcrate);
			}
        }

        double switchdelaytimer;
        double switchtime = 1.5;
        public void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d) {
            //do Ai code, update the list of crates
            //tmp: pass update to the just the lone boss object for now
            if (bossobject.idle && switchdelaytimer > switchtime) {
                //time to swap the boss with something else and make him fall
				int index = rng.Next(0, 16);
				while(index == currentBossIndex || !boxes[index].idle) {
					index = rng.Next(0, 16);
				}
				//Swap boss/box code
                Vector3 temp = new Vector3(boxes[index].location.X, maxHeight, boxes[index].location.Z);
                boxes[index].setPosition(new Vector3(bossobject.centerLoc.X, maxHeight, bossobject.centerLoc.Z));
                bossobject.setPosition(temp);
                boxes[currentBossIndex] = boxes[index];
                currentBossIndex = index;
                boxes[index] = bossobject;
                Console.WriteLine(currentBossIndex);
                switchdelaytimer = 0;
                //end swap code

				//Choose random facing for boss
				bossobject.setFacing(rng.Next(2) == 1);

                if (gethealth() == 5) {
                    //first stage, only drop the boss
                    bossobject.fall();
                }
                if (gethealth() == 4) {
                    // second stage, drop boss + either his row or column of boxes
                    int rows = rng.Next(0, 2);
                    Console.WriteLine("rows :" + rows);
                    if (rows == 0) {
                        //row
                        for (int i = 0; i < 4; i++) {
                            if (currentBossIndex == 0 + i) {
                                boxes[4 + i].fall();
                                boxes[8 + i].fall();
                                boxes[12 + i].fall();
                            }
                            if (currentBossIndex == 4 + i) {
                                boxes[0 + i].fall();
                                boxes[8 + i].fall();
                                boxes[12 + i].fall();
                            }
                            if (currentBossIndex == 8 + i) {
                                boxes[0 + i].fall();
                                boxes[4 + i].fall();
                                boxes[12 + i].fall();
                            }
                            if (currentBossIndex == 12 + i) {
                                boxes[0 + i].fall();
                                boxes[4 + i].fall();
                                boxes[8 + i].fall();
                            }
                        }
                    }
                    else {
                        //column
                        for (int i = 0; i < 4; i++) {
                            if (currentBossIndex >= i * 4 && currentBossIndex <= (i * 4) + 3) {
                                for (int k = 0; k < 4; k++) {
                                    if ((i*4) + k != currentBossIndex)
                                        boxes[(i*4) + k].fall();
                                }
                            }
                        }
                    }
                    bossobject.fall();
                }
                if (gethealth() == 3) {
                    //third, drop both his row and column
                    //row
                    for (int i = 0; i < 4; i++) {
                        if (currentBossIndex == 0 + i) {
                            boxes[4 + i].fall();
                            boxes[8 + i].fall();
                            boxes[12 + i].fall();
                        }
                        if (currentBossIndex == 4 + i) {
                            boxes[0 + i].fall();
                            boxes[8 + i].fall();
                            boxes[12 + i].fall();
                        }
                        if (currentBossIndex == 8 + i) {
                            boxes[0 + i].fall();
                            boxes[4 + i].fall();
                            boxes[12 + i].fall();
                        }
                        if (currentBossIndex == 12 + i) {
                            boxes[0 + i].fall();
                            boxes[4 + i].fall();
                            boxes[8 + i].fall();
                        }
                    }
                    //column
                    for (int i = 0; i < 4; i++) {
                        if (currentBossIndex >= i * 4 && currentBossIndex <= (i * 4) + 3) {
                            for (int k = 0; k < 4; k++) {
                                if ((i * 4) + k != currentBossIndex)
                                    boxes[(i * 4) + k].fall();
                            }
                        }
                    }
                    bossobject.fall();
                }
                if (gethealth() == 2) {
                    //specific pattern
                    int pattern = rng.Next(0, 3);
                    if (pattern == 0) { //checkers pattern
                        if (currentBossIndex == 0 || currentBossIndex == 2 || currentBossIndex == 5 || currentBossIndex == 7 ||
                            currentBossIndex == 8 || currentBossIndex == 10 || currentBossIndex == 13 || currentBossIndex == 15) {
                            boxes[0].fall(); boxes[2].fall();
                            boxes[5].fall(); boxes[7].fall();
                            boxes[8].fall(); boxes[10].fall();
                            boxes[13].fall(); boxes[15].fall();
                        }
                        else {
                            boxes[1].fall(); boxes[3].fall();
                            boxes[4].fall(); boxes[6].fall();
                            boxes[9].fall(); boxes[11].fall();
                            boxes[12].fall(); boxes[14].fall();
                        }
                        bossobject.fall();
                    }
                    if (pattern == 1) {// cross pattern
                            boxes[0].fall(); boxes[12].fall();
                            boxes[5].fall(); boxes[9].fall();
                            boxes[6].fall(); boxes[10].fall();
                            boxes[3].fall(); boxes[15].fall();
                            bossobject.fall();
                    }
                    if (pattern == 2) {//box pattern
                        boxes[0].fall(); boxes[1].fall();
                        boxes[2].fall(); boxes[3].fall();
                        boxes[4].fall(); boxes[7].fall();
                        boxes[8].fall(); boxes[11].fall();
                        boxes[12].fall(); boxes[13].fall();
                        boxes[14].fall(); boxes[15].fall();
                        bossobject.fall();
                    }
                }
                if (gethealth() == 1) {
                    //all but one box
                    int stayup = rng.Next(0, 16);
					while(stayup == currentBossIndex) {
						stayup = rng.Next(0, 16);
					}
                    for (int i = 0; i < 16; i++) {
                        if (i != stayup)
                            boxes[i].fall();
                    }
                }

            } else if(bossobject.idle) {
				switchdelaytimer = switchdelaytimer + time;
			}


            for (int i = 0; i < 16; i++) {
                if (i != currentBossIndex) {
                    boxes[i].update(time, playstate, playerposn, enable3d);
                }
                else {
                    bossobject.update(time, playstate, playerposn, enable3d);// always pass control to update
                }
            }
        }

        public int gethealth() {
            return bossobject.health;
        }

        public void killBoss(PlayState ps) {
            //TODO: whatever happens when the boss dies
            ps.objList.Remove(bossobject);
            ps.physList.Remove(bossobject);
            ps.collisionList.Remove(bossobject);
            ps.renderList.Remove(bossobject);
            ps.combatList.Remove(bossobject);
        }
    }

    //the boss and the crates must both conform to this update
    internal abstract class FallingBox : GameObject, RenderObject, PhysicsObject {
		public bool falling, rising, prefalling, idle; // true when currently doing an animation
        protected Obstacle mybox, myGround, myRope;

		//physics vars
		internal Vector3 velocity;
		internal Vector3 accel;
		protected bool doesGravity; //true if gravity affects this object
		protected int minHeight, maxHeight, preHeight;

		//public Player player;
		internal ProjectileProperties projectile;

        internal float ropeOffset = 25;

		internal FallingBox(Player player, PlayState ps, Vector3 location, int maxheight, ProjectileProperties invisProj, Obstacle crate, Obstacle ground, Obstacle rope)
			: base() {
            //physics stuff
            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = false;
            _scale = new Vector3(30.0f, 33.84f, 30.0f);
            _pbox = new Vector3(15.0f, 16.92f, 0.0f);
            _existsIn3d = true;
            _existsIn2d = true;
            maxHeight = maxheight;
            falling = false;
            rising = false;
            prefalling = false;
            idle = true;

            //animation
			_sprite = null;
            _cycleNum = 0;
            _frameNum = 0;
            _animDirection = 1;

			//timers
			downtime = 3;
			pretime = 1.5;
			shadowtimer = 0;

            //setup the sub-objects of the boss
			projectile = invisProj;
            
            mybox = crate;
            ps.objList.Add(mybox);
            ps.physList.Add(mybox);
            ps.renderList.Add(mybox);

			myGround = ground;
			ps.objList.Add(myGround);
			ps.physList.Add(myGround);
			ps.renderList.Add(myGround);

            myRope = rope;
            ps.objList.Add(myRope);
            ps.renderList.Add(myRope);

            //initialize everything's location based on the seed location
            mybox.canSquish = true;
            mybox.location = location + new Vector3(0, mybox.pbox.Y, 0);
            _location = location + new Vector3(0, (mybox.pbox.Y * 2) + pbox.Y, 0); 
            myRope.location = location + new Vector3(0, (mybox.pbox.Y * 2) + pbox.Y + ropeOffset, 0);//rope sprite
			myGround.location = new Vector3(myGround.location.X, 100.0f, myGround.location.Z);
            minHeight = 125;
            preHeight = 210;
		}

		protected double downtimer, pretimer;
		protected double downtime;
		protected double pretime;
		protected double shadowtimer;

		public virtual void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d) {
			if(prefalling) {
				if((mybox.location.Y - mybox.pbox.Y) <= preHeight) {
					setPosition(new Vector3(mybox.location.X, preHeight, mybox.location.Z));
					velocity = mybox.velocity = myRope.velocity = new Vector3(0, 0, 0);
					pretimer = pretimer + time;
					if(pretimer >= pretime) {
						pretimer = 0;
						falling = true;
						prefalling = false;
						velocity = mybox.velocity = myRope.velocity = new Vector3(0, -200, 0);
						Projectile shot = new Projectile(new Vector3(mybox.location.X, preHeight - 0.12f, mybox.location.Z), new Vector3(0, -1, 0), false, projectile, playstate.player); // spawn the actual projectile		
						shot.type = (int)CombatType.squish;
						// add projectile to appropriate lists
						playstate.objList.Add(shot);
						playstate.renderList.Add(shot);
						playstate.collisionList.Add(shot);
						playstate.physList.Add(shot);
						playstate.combatList.Add(shot);
					}
				}
			}
			if(falling) {
				if(mybox.velocity.Y == 0.0f) { //Wait for physics to stop the crate
					setPosition(new Vector3(mybox.location.X, minHeight, mybox.location.Z));
                    velocity = mybox.velocity = myRope.velocity = new Vector3(0, 0, 0);

					downtimer = downtimer + time;
					if(downtimer >= downtime) {
						downtimer = 0;
						falling = false;
						rising = true;
                        velocity = mybox.velocity = myRope.velocity = new Vector3(0, 80, 0);
						shadowtimer = 1.25;
					}
				}
			}
			if(rising) {
				if(mybox.location.Y - mybox.pbox.Y >= maxHeight) {
					setPosition(new Vector3(mybox.location.X, maxHeight, mybox.location.Z));
                    velocity = mybox.velocity = myRope.velocity = new Vector3(0, 0, 0);
					rising = false;
					idle = true;
				}
			}

			//Update shadow
			if(prefalling || falling) { //Shadow increasing
				shadowtimer += time;
				myGround.frame3d = Math.Min((int)(shadowtimer * 22.0), 11);
			} else if(rising) { //Shadow decreasing
				shadowtimer -= time;
				myGround.frame3d = Math.Min((int)(shadowtimer * 22.0), 11);
			} else { //No shadow
				myGround.frame3d = 0;
			}
		}

		public virtual void setPosition(Vector3 newposn) {
            _location = newposn + new Vector3(0, (mybox.pbox.Y * 2) + pbox.Y, 0); //boss sprite(or nothing if crate)
            myRope.location = newposn + new Vector3(0, (mybox.pbox.Y * 2) + pbox.Y + ropeOffset, 0); //rope sprite
			mybox.location = newposn + new Vector3(0, mybox.pbox.Y, 0);
			myGround.location = new Vector3(newposn.X, 100.0f, newposn.Z);
		}

		public virtual void fall() {
			if(idle) {
				idle = false;
				prefalling = true;
				velocity = mybox.velocity = new Vector3(0, -30, 0);
				shadowtimer = 0.0;
			}
		}

		public void physUpdate3d(double time, List<PhysicsObject> physList) {
			mybox.physUpdate3d(time, physList);
            myRope.physUpdate3d(time, physList);
            _location.Y = mybox.location.Y + mybox.pbox.Y + pbox.Y;
		}

		public void physUpdate2d(double time, List<PhysicsObject> physList) {
			mybox.physUpdate2d(time, physList);
            myRope.physUpdate2d(time, physList);
            _location.Y = mybox.location.Y + mybox.pbox.Y + pbox.Y;
		}

		/*  The following are helper methods + getter/setters */

		//swaps physics box x and z coordinates (used for sprites that billboard)
		public void swapPBox() {
			float temp = _pbox.X;
			_pbox.X = _pbox.Z;
			_pbox.Z = temp;
		}

		public bool canSquish {
			get { return false; }
		}

		public bool is3dGeo {
			get { return false; }
		}

		public ObjMesh mesh {
			get { return null; } //null for 3d objects
		}

		public MeshTexture texture {
			get { return null; } //null for sprites
		}

		internal SpriteSheet _sprite; //null for 3d objects
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

		internal int _cycleNum;
		public int cycleNumber {
			get { return _cycleNum; }
			set { _cycleNum = value; }
		}

		internal double _frameNum; //index of the current animation frame
		public double frameNumber {
			get { return _frameNum; }
			set { _frameNum = value; }
		}

		public virtual Billboarding billboards {
			get { return Billboarding.Yes; }
		}

		public virtual bool collidesIn3d {
			get { return false; }
		}

		public virtual bool collidesIn2d {
			get { return false; }
		}

		public void reset() {
			throw new NotImplementedException();
		}

		public void accelerate(Vector3 acceleration) {
			accel += acceleration;
		}

		internal int _animDirection;
		public int animDirection {
			get { return _animDirection; }
		}

		public bool hasTwoAnims {
			get { return false; }
		}

		public int ScreenRegion {
			get { return screenRegion; }
		}

		public virtual void doScaleTranslateAndTexture() {
			GL.PushMatrix();

			GL.Translate(_location);
			GL.Scale(_scale);
		}
    }

	//Boss, the crate he's standing on, and the ground under him
	internal class Boss : FallingBox, CombatObject {

		public bool invincible;
		public Vector3 centerLoc;
		private bool in3d;

		internal Boss(Player player, PlayState ps, Vector3 location, int maxheight, ProjectileProperties invisProj, Obstacle crate, Obstacle ground, Obstacle rope, SpriteSheet bossSprite)
            : base(player, ps, location, maxheight, invisProj, crate, ground, rope) {

            //animation
            _sprite = bossSprite;
            _cycleNum = 0;
            _frameNum = 0;
            _animDirection = 1;

			//combat stuff
			_cbox = new Vector3(6, 6, 6);
            _health = 5;
            _damage = 1;
            _speed = 1;
            _alive = true;
            _hascbox = true;
            _type = 3; //Type 3 means this is the boss

			//Offset boss location
			centerLoc = new Vector3(_location);
			_location.X += 8.0f;
			_location.Z -= 0.01f;
			in3d = false;
			bb = Billboarding.Lock2d;

            invincible = false;
        }

		public override void setPosition(Vector3 newposn) {
			centerLoc = newposn + new Vector3(0, (mybox.pbox.Y * 2) + pbox.Y, 0); //boss sprite(or nothing if crate)
			myRope.location = newposn + new Vector3(0, (mybox.pbox.Y * 2) + pbox.Y + ropeOffset, 0); //rope sprite
			mybox.location = newposn + new Vector3(0, mybox.pbox.Y, 0);
			myGround.location = new Vector3(newposn.X, 100.0f, newposn.Z);

			_location = new Vector3(centerLoc);
			if(in3d) {
				_location.Z += 8.0f;
				_location.X += 0.01f; //break rendering tie
			} else { //2D
				_location.X += 8.0f;
				_location.Z -= 0.01f; //break rendering tie
			}
		}

		public override void fall() {
			if(idle) {
				invincible = false;
				cycleNumber = 0;
				base.fall();
			}
		}

        public void dodamage(int hit) {
            if (!invincible) {
                health = health - 1;
				cycleNumber = 1;
                invincible = true;
            }
        }

		//Sets if boss should face for 2d or 3d view
		public void setFacing(bool to3d) {
			if(!in3d && to3d) { // 2D -> 3D
				_location.Z = centerLoc.Z + 8.0f;
				_location.X = centerLoc.X + 0.01f; //break rendering tie
				bb = Billboarding.Lock3d;
				in3d = true;
			} else if(in3d && !to3d) { // 3D -> 2D
				_location.X = centerLoc.X + 8.0f;
				_location.Z -= 0.01f; //break rendering tie
				bb = Billboarding.Lock2d;
				in3d = false;
			}
		}

		//Overrides
		private Billboarding bb;
		public override Billboarding billboards {
			get { return bb; }
		}

		public override bool collidesIn3d {
			get { return in3d; }
		}

		public override bool collidesIn2d {
			get { return !in3d; }
		}

		public override void doScaleTranslateAndTexture() {
			base.doScaleTranslateAndTexture();
			if(frameNumber*_sprite.framesPerSecond >= 1.0) {
				cycleNumber = 2;
				frameNumber = 0;
			}
		}

		/*  The following are helper methods + getter/setters */
		private Vector3 _cbox;
		public Vector3 cbox {
			get { return _cbox; }
		}

		//swaps combat box x and z coordinates (used for sprites that billboard)
		public void swapCBox() {
			float temp = _cbox.X;
			_cbox.X = _cbox.Z;
			_cbox.Z = temp;
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

		private int _type;
		public int type {
			get { return _type; }
		}

		public Effect deathAnim {
			//Change if we give him a death animation later
			get { return null; }
		}
    }

	//Crate without boss and the ground under it
	internal class Crate : FallingBox {

		internal Crate(Player player, PlayState ps, Vector3 location, int maxheight, ProjectileProperties invisProj, Obstacle crate, Obstacle ground, Obstacle rope)
			: base(player, ps, location, maxheight, invisProj, crate, ground, rope) {
		}
    }
}
