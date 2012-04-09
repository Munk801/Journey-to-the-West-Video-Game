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
			SpriteSheet ropeSprite = LoadLevel.parseSpriteFile("zoo_keeper_sprite.dat"); //TODO: Change to rope when available

			ProjectileProperties invisProj = LoadLevel.parseProjectileFile("invisible_projectile.dat");
			invisProj.cbox = new Vector3(24.8f, 0.1f, 24.8f);
			invisProj.pbox = new Vector3(24.8f, 0.1f, 24.8f);
			invisProj.speed = 200;

			List<Vector3> locs = new List<Vector3>();
			for( int i = 0; i < 4; i++){
                for (int k = 0; k < 4; k++) {
					locs.Add(new Vector3(4075 + (50 * i), maxHeight, (-25 + (50 * k))));
				}
			}
			List<Obstacle> boxObstacles = LoadLevel.parseSingleObstacleFile("zoo_keeper_crate.dat", locs);
			List<Obstacle> groundObstacles = LoadLevel.parseSingleObstacleFile("zookeeper_boss_ground.dat", locs);

			//Cycle through all the ground textures once to get them in memory - prevent pop-in on first run
			Obstacle aGround = groundObstacles[0];
			for(int i = 0; i < 11; i++) {
				aGround.frame3d = i;
				aGround.doScaleTranslateAndTexture();
				GL.PopMatrix();
			}
			aGround.frame3d = 0;

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
                    newcrate = new Crate(player, ps, new Vector3(box.location), maxHeight, ropeSprite, invisProj, box, groundObstacles[i]);
				} else {
					newcrate = bossobject = new Boss(player, ps, new Vector3(box.location), maxHeight, zookeeperSprite, invisProj, box, groundObstacles[i]);
					ps.combatList.Add(bossobject);
				}
				boxes[i] = newcrate;
				ps.objList.Add(newcrate);
				ps.physList.Add(newcrate);
				ps.colisionList.Add(newcrate);
				ps.renderList.Add(newcrate);
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
				//Do the swap
                Vector3 temp = new Vector3(boxes[index].location.X, maxHeight, boxes[index].location.Z);
                boxes[index].setPosition(new Vector3(bossobject.location.X, maxHeight, bossobject.location.Z));
                bossobject.setPosition(temp);
                bossobject.fall();
                switchdelaytimer = 0;
            } else if(bossobject.idle) {
				switchdelaytimer = switchdelaytimer + time;
			}

            bossobject.update(time, playstate, playerposn, enable3d);// always pass control to update
        }

        public int gethealth() {
            return bossobject.health;
        }

        public void killBoss(PlayState ps) {
            //TODO: whatever happens when the boss dies
            ps.objList.Remove(bossobject);
            ps.physList.Remove(bossobject);
            ps.colisionList.Remove(bossobject);
            ps.renderList.Remove(bossobject);
            ps.combatList.Remove(bossobject);
        }
    }

    //the boss and the crates must both conform to this update
    internal abstract class FallingBox : GameObject, RenderObject, PhysicsObject {
		public bool falling, rising, prefalling, idle; // true when currently doing an animation
		protected Obstacle mybox, myGround;

		//physics vars
		internal Vector3 velocity;
		internal Vector3 accel;
		protected bool doesGravity; //true if gravity affects this object
		protected int minHeight, maxHeight, preHeight;
		protected bool pbox2d, pbox3d; //these control if normal physics happens

		//public Player player;
		internal ProjectileProperties projectile;

		internal FallingBox(Player player, PlayState ps, Vector3 location, int maxheight, SpriteSheet mySprite, ProjectileProperties invisProj, Obstacle crate, Obstacle ground)
			: base() {
            //physics stuff
            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = false;
            _scale = new Vector3(12,12,12);
            _pbox = new Vector3(6,6,6);
            _existsIn3d = true;
            _existsIn2d = true;
            maxHeight = maxheight;
            falling = false;
            rising = false;
            prefalling = false;
            idle = true;

            //animation
			_sprite = mySprite;
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

            //initialize everything's location based on the seed location
            mybox.canSquish = true;
            mybox.location = location + new Vector3(0, mybox.pbox.Y, 0);
            _location = location + new Vector3(0, (mybox.pbox.Y *2) + pbox.Y, 0); //boss sprite
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
					velocity = mybox.velocity = new Vector3(0, 0, 0);
					pretimer = pretimer + time;
					if(pretimer >= pretime) {
						pretimer = 0;
						falling = true;
						prefalling = false;
						velocity = mybox.velocity = new Vector3(0, -200, 0);
						Projectile shot = new Projectile(new Vector3(mybox.location.X, preHeight - 0.12f, mybox.location.Z), new Vector3(0, -1, 0), false, projectile, playstate.player); // spawn the actual projectile		
						shot.type = (int)CombatType.squish;
						// add projectile to appropriate lists
						playstate.objList.Add(shot);
						playstate.renderList.Add(shot);
						playstate.colisionList.Add(shot);
						playstate.physList.Add(shot);
						playstate.combatList.Add(shot);
					}
				}
			}
			if(falling) {
				if(mybox.velocity.Y == 0.0f) { //Wait for physics to stop the crate
					setPosition(new Vector3(mybox.location.X, minHeight, mybox.location.Z));
					velocity = mybox.velocity = new Vector3(0, 0, 0);

					downtimer = downtimer + time;
					if(downtimer >= downtime) {
						downtimer = 0;
						falling = false;
						rising = true;
						velocity = mybox.velocity = new Vector3(0, 80, 0);
						shadowtimer = 1.25;
					}
				}
			}
			if(rising) {
				if(mybox.location.Y - mybox.pbox.Y >= maxHeight) {
					setPosition(new Vector3(mybox.location.X, maxHeight, mybox.location.Z));
					velocity = mybox.velocity = new Vector3(0, 0, 0);
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

		public void setPosition(Vector3 newposn) {
			_location = newposn + new Vector3(0, (mybox.pbox.Y * 2) + pbox.Y, 0); //boss sprite
			mybox.location = newposn + new Vector3(0, mybox.pbox.Y, 0);
			myGround.location = new Vector3(newposn.X, 100.0f, newposn.Z);
		}

		public void fall() {
			if(idle) {
				idle = false;
				prefalling = true;
				velocity = mybox.velocity = new Vector3(0, -30, 0);
				shadowtimer = 0.0;
			}
		}

		public void physUpdate3d(double time, List<PhysicsObject> physList) {
			mybox.physUpdate3d(time, physList);
			_location.Y = mybox.location.Y + mybox.pbox.Y + pbox.Y;
		}

		public void physUpdate2d(double time, List<PhysicsObject> physList) {
			mybox.physUpdate2d(time, physList);
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
			get { return pbox3d; }
		}

		public bool collidesIn2d {
			get { return pbox2d; }
		}

		public void reset() {
			throw new NotImplementedException();
		}

		public void accelerate(Vector3 acceleration) {
			accel += acceleration;
		}

		private int _animDirection;
		public int animDirection {
			get { return _animDirection; }
		}

		public int ScreenRegion {
			get { return screenRegion; }
		}

		public void doScaleTranslateAndTexture() {
			GL.PushMatrix();

			GL.Translate(_location);
			GL.Scale(_scale);
		}
    }

	//Boss, the crate he's standing on, and the ground under him
    internal class Boss : FallingBox, CombatObject {
		internal Boss(Player player, PlayState ps, Vector3 location, int maxheight, SpriteSheet bossSprite, ProjectileProperties invisProj, Obstacle crate, Obstacle ground)
			: base(player, ps, location, maxheight, bossSprite, invisProj, crate, ground) {
			pbox2d = true;
			pbox3d = true;

			//combat stuff
			_cbox = new Vector3(6, 6, 6);
            _health = 5;
            _damage = 1;
            _speed = 1;
            _alive = true;
            _hascbox = true;
            _type = 3; //Type 3 means this is the boss

            invincible = false;
            invintimer = 0;
        }

        double invintimer;
        double invintime = 1.5;
        public bool invincible;

        public override void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d) {
            if (invincible) {
                invintimer = invintimer + time;
                if (invintimer >= invintime) {
                    invintimer = 0;
                    invincible = false;
                }
            }

			base.update(time, playstate, playerposn, enable3d);
        }

        public void dodamage(int hit) {
            if (!invincible) {
                health = health - 1;
                //TODO: play pain animation or w/e
                downtimer = downtime - 0.2;// make him come up after .2 seconds
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
    }

	//Crate without boss and the ground under it
	internal class Crate : FallingBox {

		internal Crate(Player player, PlayState ps, Vector3 location, int maxheight, SpriteSheet ropeSprite, ProjectileProperties invisProj, Obstacle crate, Obstacle ground)
			: base(player, ps, location, maxheight, ropeSprite, invisProj, crate, ground) {
			//This is just the rope, so let the player pass through freely
			pbox2d = false;
			pbox3d = false;
		}
    }
}
