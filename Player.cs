using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using Engine.Input;
using OpenTK.Input;

namespace U5Designs
{
    public class Player : GameObject, RenderObject, PhysicsObject, CombatObject
    {
		private const int fallDamage = 1;

        //Knockback physics constants:
        private Vector3 kbspeed = new Vector3(70, 100, 70);

        public PlayerState p_state;
		public Vector3 velocity;
		private Vector3 accel;
        internal bool Invincible, HasControl;
        private bool ClickDown;

		private Vector3 lastPosOnGround;

        private double Invincibletimer, NoControlTimer, projectileTimer;
		public double fallTimer, viewSwitchJumpTimer;
        SpriteSheet banana;

		public float deltax; //used for updating position of camera, etc.
		public Camera cam; //pointer to the camera, used for informing of y position changes
		private bool enable3d; //track current world state (used in spawning projectiles)
		public bool onGround; //true when on ground, or when we just switched to 3d and were in midair

        // SOUND FILES
        static string jumpSoundFile = "../../Resources/Sound/jump_sound.ogg";
        AudioFile jumpSound = new AudioFile(jumpSoundFile);

        public Player(SpriteSheet sprite, SpriteSheet banana) : base(Int32.MaxValue) //player always has largest ID for rendering purposes
        {
            p_state = new PlayerState("TEST player");
            //p_state.setSpeed(130);
			_speed = 75;
			_location = new Vector3(25, 12.5f, 50);
            _scale = new Vector3(16.25f, 25f, 16.25f);
            _pbox = new Vector3(5f, 12.5f, 5f);
            _cbox = new Vector3(4f, 11.5f, 4f);
			velocity = new Vector3(0, 0, 0);
			accel = new Vector3(0, 0, 0);
			_cycleNum = 0;
			_frameNum = 0;
			_sprite = sprite;
            this.banana = banana;
            _hascbox = true;
            _type = 0; //type 0 is the player and only the player
			_existsIn2d = true;
			_existsIn3d = true;
			onGround = true;
            _damage = 1;
            _health = 5;
            Invincible = false;
            HasControl = true;
            Invincibletimer = 0.0;
            NoControlTimer = 0.0;
			fallTimer = 0.0;
			viewSwitchJumpTimer = 0.0;
			projectileTimer = 0.0;
			lastPosOnGround = new Vector3(_location);
			_animDirection = 1;
        }

        /**
         * Sets the PlayerState elements to the current Player values.  Call this method every update or simply when the state changes.  This will be used to store
         * the Players State when saving the game.
		 * 
		 * Returns the movement of the player to be used in updating camera, etc.
         * */
        bool spaceDown;
        internal void updateState(bool enable3d, bool a, bool s, bool d, bool w, bool c, bool x, bool space, bool ekey, FrameEventArgs e, PlayState playstate) {
			this.enable3d = enable3d;

			if(projectileTimer > 0.0) {
				projectileTimer -= e.Time;
			}

            if (Invincible)
                Invincibletimer = Invincibletimer - e.Time;
            if (Invincibletimer <= 0) { // invincible is gone
                Invincibletimer = 0;
                Invincible = false;
            }

            if (!HasControl)
                NoControlTimer = NoControlTimer + e.Time;
            if (NoControlTimer >= 0.5) {
                NoControlTimer = 0;
                HasControl = true;
            }

			if(viewSwitchJumpTimer > 0.0) {
				viewSwitchJumpTimer -= e.Time;
			}

            if (HasControl) {
				if(enable3d) {
					Vector2 newVel = new Vector2(0);
					if(w) { newVel.X++; }
					if(s) { newVel.X--; }
					if(a) { newVel.Y--; }
					if(d) { newVel.Y++; }

					newVel.NormalizeFast();
					if(newVel.X != 0 || newVel.Y != 0) {
						viewSwitchJumpTimer = 0.0;
					}

					velocity.X = newVel.X*speed;
					velocity.Z = newVel.Y*speed;

					_animDirection = (velocity.X >= 0 ? 1 : -1);
					if(velocity.X == 0) {
						_cycleNum = (enable3d ? 1 : 0);
					} else {
						_cycleNum = (enable3d ? 3 : 2);
					}
                } else {
                    if ((d && a) || (!d && !a)) {
                        velocity.X = 0f;
                    } else if(d) {
                        velocity.X = _speed;
					} else { //a
						velocity.X = -_speed;
					}

					_animDirection = (velocity.X >= 0 ? 1 : -1);
					if(velocity.X == 0) {
						_cycleNum = (enable3d ? 1 : 0);
					} else {
						_cycleNum = (enable3d ? 3 : 2);
					}
                }

                //TMP PHYSICS TEST BUTTON and suicide button and projectile button
				if(c) {
					velocity.Y = _speed;
					fallTimer = 0.0;
				}
                if(x)
                    _health = 0;

                MouseInput(playstate);

                //if (ekey && !edown)
                //{
                //    spawnProjectile(playstate);
                //    edown = true;
                //}
                //else if (!ekey)
                //    edown = false;


                //********************** space
                if (space && !spaceDown) {
					if(onGround) {
						accelerate(Vector3.UnitY * 230);
						onGround = false;
						viewSwitchJumpTimer = 0.0;
						//jumpSound.Play();
					}
                    spaceDown = true;
                }
                else if (!space) {
                    spaceDown = false;
                }
            }
        }
		
        /// <summary>
        /// Provides all the mouse input for the player.  Will currently check for a left click and shoot a projectile in the direction
        /// </summary>
        /// <param name="playstate"></param>
        private void MouseInput(PlayState playstate)
        {
            if (playstate.eng.ThisMouse.LeftPressed() && projectileTimer <= 0.0)
            {
                // Grab Mouse coord.  Since Y goes down, just subtract from height to get correct orientation
                Vector3d mousecoord = new Vector3d((double)playstate.eng.Mouse.X, (double)(playstate.eng.Height - playstate.eng.Mouse.Y), 1.0);

                // Pull the projection and model view matrix from the camera.   
                Matrix4d project = playstate.camera.GetProjectionMatrix();
                Matrix4d model = playstate.camera.GetModelViewMatrix();
                
                // Unproject the coordinates to convert from mouse to world coordinates
                Vector3d mouseWorld = GameMouse.UnProject(mousecoord, model, project, playstate.camera.getViewport());

                // Since Z is 50 in 2d, we just change it here if in 2d
				if(!playstate.enable3d) {
				    double vel = 250.0;

				    Vector3 projDir = new Vector3((float)mouseWorld.X, (float)mouseWorld.Y, _location.Z);
				    projDir -= _location;
					projDir.X = Math.Abs(projDir.X);

				    //Following is adapted from http://en.wikipedia.org/wiki/Trajectory_of_a_projectile#Angle_required_to_hit_coordinate_.28x.2Cy.29
				    double velsquared = vel * vel;
					double sqrtPart = Math.Sqrt(velsquared * velsquared - gravity * (gravity * projDir.X * projDir.X + 2 * projDir.Y * velsquared));
					double theta;
					if(sqrtPart == sqrtPart) { //false when sqrtPart == NaN
						theta = Math.Atan((velsquared - sqrtPart) / (gravity * projDir.X));
					} else {
						//TODO: Come up with a better looking solution than this.
						theta = Math.PI / 4; //can't reach that point, go as far as we can
					}

				    projDir.X = (float)Math.Cos(theta);
				    projDir.Y = (float)Math.Sin(theta);
				    projDir.Z = 0.0f;
					if(mouseWorld.X < _location.X) {
						projDir.X = -projDir.X;
					}

				    spawnProjectile(playstate, projDir, (float)vel);
				} else {

					// Cannot implicitly typecast a vector3d to vector3
					Vector3 projDir = new Vector3((float)mouseWorld.X, (float)mouseWorld.Y, (float)mouseWorld.Z);
					projDir -= _location;

					// Must normalize or else the direction is wrong.  Using fast but may  need to user the slower one
					projDir.Normalize();
					spawnProjectile(playstate, projDir, 250);
				}
                
				projectileTimer = 0.25;
            }
        }

        private void spawnProjectile(PlayState playstate, Vector3 direction, float speed) {
            // make new projectile object
            //TODO: determine if banana or fireball or w/e
            Vector3 projlocation = new Vector3(location);

			if(!enable3d) {
				projlocation.Z += 0.001f; //break the rendering tie between player and projectile, or else they flicker
			}
            //Vector3 projdirection = new Vector3(1, 0, 1); //TODO: get the direction vector based on where the mouse is.
            //Vector3 projdirection = new Vector3((float)playstate.mouseWorld.X, (float)playstate.mouseWorld.Y, (float)1.0f);
            //direction.NormalizeFast();
            Projectile shot = new Projectile(projlocation, direction, new Vector3(9f, 9f, 9f), new Vector3(4.5f, 4.5f, 4.5f), new Vector3(3.5f, 3.5f, 3.5f), true, true, playstate.enable3d, damage, speed, true, true, banana);
			
			shot.accelerate(new Vector3(velocity.X, 0.0f, velocity.Z)); //account for player's current velocity

            // add projectile to appropriate lists
            playstate.objList.Add(shot);
            playstate.renderList.Add(shot);
            playstate.colisionList.Add(shot);
            playstate.physList.Add(shot);
            playstate.combatList.Add(shot);
        }

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

		//public void draw(bool viewIs3d, double time)
		//{
		//    doScaleTranslateAndTexture();
		//    frameNumber = sprite.draw(viewIs3d, cycleNumber, frameNumber + time);
		//}

		public Vector3 setLocation {
			set { _location = value; }
		}

		public bool is3dGeo {
			get { return false; }
		}

		public ObjMesh mesh {
			get { return null; }
		}

		public MeshTexture texture {
			get { return null; }
		}

		private SpriteSheet _sprite;
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
            set { _type = value; }
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

		private int _animDirection;
		public int animDirection {
			get { return _animDirection; }
		}

        public void doScaleTranslateAndTexture() {
			GL.PushMatrix();

			GL.Translate(_location);
			GL.Scale(_scale);
        }

        public void physUpdate3d(double time, List<PhysicsObject> physList) {
			double origTime = time;
			bool collidedWithGround = false;

			//first deal with gravity
			if(viewSwitchJumpTimer <= 0.0) {
				accel.Y -= (float)(400*time); //TODO: turn this into a constant somewhere
			}

			//now do acceleration
			velocity += accel;
			accel.X = 0;
			accel.Y = 0;
			accel.Z = 0;

			//now check for collisions and move
			deltax = 0.0f;
			List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

			while(time > 0.0) {
				PhysicsObject collidingObj = null;
				float collidingT = 1.0f / 0.0f; //pos infinity
				int collidingAxis = -1;

                foreach (PhysicsObject obj in physList) {
					// don't do collision physics to yourself, or on things you already hit this frame
					if(obj.collidesIn3d && obj != this && !alreadyCollidedList.Contains(obj)) {
						Vector3 mybox, objbox;
						if(!Invincible && obj.hascbox) {
							mybox = _cbox;
							objbox = ((CombatObject)obj).cbox;
						} else {
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
					deltax += _location.X - startLoc.X; //update this value for camera offset
					if(viewSwitchJumpTimer <= 0.0 && !collidedWithGround) {
						onGround = false;
					}
				} else {
					alreadyCollidedList.Add(collidingObj);
					if(Invincible || !collidingObj.hascbox) { //if this is a normal physics collision
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
								deltax += _location.X - startLoc.X; //update this value for camera offset
								velocity.X = 0;
								break;
							case 1: //y
								if(_location.Y < collidingObj.location.Y) {
									_location.Y = collidingObj.location.Y - (pbox.Y + collidingObj.pbox.Y) - 0.0001f;
								} else {
									_location.Y = collidingObj.location.Y + pbox.Y + collidingObj.pbox.Y + 0.0001f;
									//Special case for landing on platforms
									fallTimer = 0.0;
									onGround = true;
									collidedWithGround = true;
									lastPosOnGround = new Vector3(_location);
									cam.moveToYPos(_location.Y);
								}
								if(velocity.Y != 0) {//should always be true, but just in case...
									double deltaTime = (location.Y - startLoc.Y) / velocity.Y;
									_location.X += (float)(velocity.X * deltaTime);
									_location.Z += (float)(velocity.Z * deltaTime);
									time -= deltaTime;
								}
								deltax += _location.X - startLoc.X; //update this value for camera offset
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
								deltax += _location.X - startLoc.X; //update this value for camera offset
								velocity.Z = 0;
								break;
						}
					} else { //this is a combat collision
						if(((CombatObject)collidingObj).type == 1) {// obj is an enemy, do damage, knock player back
							time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
							_health = _health - ((CombatObject)collidingObj).damage;
							Invincible = true;
							HasControl = false;
							((Enemy)collidingObj).frozen = true;

                            knockback(true, collidingObj);
						}
						if(((CombatObject)collidingObj).type == 2) { // obj is a projectile, despawn projectile do damage
							//if projectile was not spawned by the player, deal with it. Ignore all player spawned projectiles
							if(!((Projectile)collidingObj).playerspawned) {
                                _health = _health - ((CombatObject)collidingObj).damage;
                                Invincible = true;
                                HasControl = false;
                                //despawn the projectile
                                ((CombatObject)collidingObj).health = 0;
                            }
						}
					}
				}
			}

			//Now that everything is done, move the camera if we're below the bottom of the screen
			if(fallTimer > 0.0 || !cam.playerIsAboveScreenBottom()) {
				fallTimer += origTime;
				cam.trackPlayer();
			}

			if(fallTimer > 1.5) {
				_health -= fallDamage;
				_location = lastPosOnGround;
				Invincible = true;
				Invincibletimer = 2.0;
			}
		}

        public void physUpdate2d(double time, List<PhysicsObject> physList) {
			double origTime = time;
			bool collidedWithGround = false;
			
			//first do gravity
			if(viewSwitchJumpTimer <= 0.0) {
				accel.Y -= (float)(400 * time); //TODO: turn this into a constant somewhere
			}
			
			//now deal with acceleration
			velocity += accel;
			accel.X = 0;
			accel.Y = 0;
			accel.Z = 0;

			velocity.Z = 0; //special case for 2d

			//now check for collisions and move
			deltax = 0.0f;
			List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

			while(time > 0.0) {
				PhysicsObject collidingObj = null;
				float collidingT = 1.0f / 0.0f; //pos infinity
				int collidingAxis = -1;

                foreach (PhysicsObject obj in physList) {
					// don't do collision physics to yourself, or on things you already hit this frame
					if(obj.collidesIn2d && obj != this && !alreadyCollidedList.Contains(obj)) {
						Vector3 mybox, objbox;
						if(!Invincible && obj.hascbox) {
							mybox = _cbox;
							objbox = ((CombatObject)obj).cbox;
						} else {
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
					}
				}

				Vector3 startLoc = new Vector3(_location.X, _location.Y, _location.Z);
				if(collidingAxis == -1) { //no collision
					_location += velocity * (float)time;
					time = 0.0;
					deltax += _location.X - startLoc.X; //update this value for camera offset
					if(viewSwitchJumpTimer <= 0.0 && !collidedWithGround) {
						onGround = false;
					}
				} else {
					alreadyCollidedList.Add(collidingObj);
					if(Invincible || !collidingObj.hascbox) { //if this is a normal physics collision
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
								deltax += _location.X - startLoc.X; //update this value for camera offset
								velocity.X = 0;
								break;
							case 1: //y
								if(_location.Y < collidingObj.location.Y) {
									_location.Y = collidingObj.location.Y - (pbox.Y + collidingObj.pbox.Y) - 0.0001f;
								} else {
									_location.Y = collidingObj.location.Y + pbox.Y + collidingObj.pbox.Y + 0.0001f;
									//Special case for landing on platforms
									fallTimer = 0.0;
									onGround = true;
									collidedWithGround = true;
									lastPosOnGround = new Vector3(_location);
									cam.moveToYPos(_location.Y);
								}
								if(velocity.Y != 0) {//should always be true, but just in case...
									double deltaTime = (location.Y - startLoc.Y) / velocity.Y;
									_location.X += (float)(velocity.X * deltaTime);
									time -= deltaTime;
								}
								deltax += _location.X - startLoc.X; //update this value for camera offset
								velocity.Y = 0;
								break;
						}
					} else { //this is a combat collision
						if(((CombatObject)collidingObj).type == 1) {// obj is an enemy, do damage, knock player back
							time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
							_health = _health - ((CombatObject)collidingObj).damage;
							Invincible = true;
							Invincibletimer = 0.5;
							HasControl = false;
							((Enemy)collidingObj).frozen = true;

                            knockback(false, collidingObj);


						}
						if(((CombatObject)collidingObj).type == 2) { // obj is a projectile, despawn projectile do damage
                            //if projectile was not spawned by the player, deal with it. Ignore all player spawned projectiles
							if(!((Projectile)collidingObj).playerspawned) {
								_health = _health - ((CombatObject)collidingObj).damage;
								Invincible = true;
								Invincibletimer = 0.5;
								HasControl = false;
								//despawn the projectile
                                ((CombatObject)collidingObj).health = 0;
							}
						}
					}
				}
			}

			//Now that everything is done, move the camera if we're below the bottom of the screen
			if(fallTimer > 0.0 || !cam.playerIsAboveScreenBottom()) {
				fallTimer += origTime;
				cam.trackPlayer();
			}

			if(fallTimer > 1.5) {
				_health -= fallDamage;
				_location = lastPosOnGround;
				Invincible = true;
				Invincibletimer = 2.0;
			}
		}

        public void knockback(bool is3d, PhysicsObject collidingObj) {

            if (is3d) {
                // direction we need to be knocked back in.
                Vector3 direction = new Vector3(location.X - collidingObj.location.X, 0, location.Z - collidingObj.location.Z);
                direction.Normalize();

                location = new Vector3(collidingObj.location.X + (collidingObj.pbox.X * direction.X), collidingObj.location.Y + collidingObj.pbox.Y, collidingObj.location.Z + (collidingObj.pbox.Z * direction.Z));
                velocity = new Vector3(0, 0, 0);
                accel = new Vector3(kbspeed.X * direction.X, kbspeed.Y, kbspeed.Z * direction.Z);
            } else {
                Vector3 direction = new Vector3(location.X - collidingObj.location.X, 0, 0);
                direction.Normalize();

                float origX = location.X;

                location = new Vector3(collidingObj.location.X + (collidingObj.pbox.X * direction.X), collidingObj.location.Y + collidingObj.pbox.Y, location.Z);
                deltax = location.X - origX;
                velocity = new Vector3(0, 0, 0);
                accel = new Vector3(kbspeed.X * direction.X, kbspeed.Y, 0);
            }
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

		//TODO: Don't know if reset really applies to player or not...
		public void reset() {
			throw new NotImplementedException();
		}
	}
}
