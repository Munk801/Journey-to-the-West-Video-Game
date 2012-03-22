﻿using System;
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
using OpenTK.Input;
using System.Reflection;
using System.Runtime.InteropServices;

namespace U5Designs
{
    public class Player : GameObject, RenderObject, PhysicsObject, CombatObject
    {
		private const int fallDamage = 1;

        //Knockback physics constants:
        private Vector3 kbspeed;
		private float jumpspeed;

        public PlayerState p_state;
		public Vector3 velocity;
		private Vector3 accel;
        internal bool Invincible, HasControl;
        static Assembly assembly_new = Assembly.GetExecutingAssembly();
		private Vector3 lastPosOnGround; // used for falling off detection

        //timers
        private double Invincibletimer, NoControlTimer, projectileTimer;
		public double fallTimer, viewSwitchJumpTimer;

        //projectile managment
		List<ProjectileProperties> projectiles;
		public ProjectileProperties curProjectile;
		public SpriteSheet marker;
		public List<Decoration> markerList;


		public float deltax; //used for updating position of camera, etc.
		public Camera cam; //pointer to the camera, used for informing of y position changes
		private bool enable3d; //track current world state (used in spawning projectiles)
		public bool onGround; //true when on ground, or when we just switched to 3d and were in midair

        //spin attack
        public bool spinning; //true when spin attack is in progress
        public double spinSize; // the distance from the center of the player the spin attack extends
        public int spinDamage;

		private bool spaceDown;
        
        // SOUND FILES
        static string jumpSoundFile = "../../Resources/Sound/jump_sound.ogg";
        AudioFile jumpSound = new AudioFile(jumpSoundFile);
        AudioFile bananaSound = new AudioFile(assembly_new.GetManifestResourceStream("U5Designs.Resources.Sound.banana2.ogg"));
        AudioFile hurtSound = new AudioFile(assembly_new.GetManifestResourceStream("U5Designs.Resources.Sound.hurt.ogg"));

        public Player(SpriteSheet sprite, List<ProjectileProperties> projectiles) : base(Int32.MaxValue) //player always has largest ID for rendering purposes
        {
            p_state = new PlayerState("TEST player");
            //p_state.setSpeed(130);
			_speed = 75;
			_location = new Vector3(25, 12.5f, 50);
            _scale = new Vector3(16.25f, 25f, 16.25f);
            _pbox = new Vector3(5f, 12.5f, 5f);
            _cbox = new Vector3(4f, 11.5f, 4f);
            spinSize = 12; //?? experiment with this, TODO: change it to match the sprites size
			velocity = new Vector3(0, 0, 0);
			accel = new Vector3(0, 0, 0);
			kbspeed = new Vector3(70, 100, 70);
			jumpspeed = 230.0f;
			_cycleNum = 0;
			_frameNum = 0;
			_sprite = sprite;
            _hascbox = true;
            _type = 0; //type 0 is the player and only the player
			_existsIn2d = true;
			_existsIn3d = true;
			onGround = true;
            //combat things

            _damage = 1;
            spinDamage = 2;
            _health = 7;

            Invincible = false;
            HasControl = true;
            Invincibletimer = 0.0;
            NoControlTimer = 0.0;
			fallTimer = 0.0;
			viewSwitchJumpTimer = 0.0;
			projectileTimer = 0.0;
			lastPosOnGround = new Vector3(_location);
			_animDirection = 1;
			this.projectiles = projectiles;
			curProjectile = projectiles[0];
			markerList = new List<Decoration>();
        }

		/// <summary>
		/// Adds indicators to show where the players projectile is going to go
		/// Used for gravity based projectiles (specifically the grenade)
		/// </summary>
		/// <param name="ps">Pointer to the current PlayState, used to calculate directions</param>
		public void addMarkers(PlayState ps) {
			if(curProjectile.gravity) {
				markerList.Clear();

				Vector3 projDir = calcProjDir(ps) * curProjectile.speed;
				//projDir -= new Vector3(velocity.X, 0.0f, velocity.Z);
				Vector3 pos = new Vector3(_location);

				for(int i = 0; i < 40; i++) {
					projDir.Y -= 20.0f;
					pos += (projDir / 20.0f);
					markerList.Add(new Decoration(pos, new Vector3(5, 5, 5), true, true, Billboarding.Yes, marker));
				}
			}
		}

        /// <summary>
        ///  Updates the player specific state. Gets called every update frame
        /// </summary>
        /// <param name="enable3d"> true if we are in 3d view</param>
        /// <param name="keyboard"> the keyboard object, for handling input</param>
        /// <param name="time">time elapsed since last update</param>
        /// <param name="playstate">a pointer to play state, for accessing the obj lists in playstate</param>
        internal void updateState(bool enable3d, KeyboardDevice keyboard, double time, PlayState playstate) {
			this.enable3d = enable3d;

			//Update timers
			if(projectileTimer > 0.0) {
				projectileTimer -= time;
			}

            if (Invincible)
                Invincibletimer = Invincibletimer - time;
            if (Invincibletimer <= 0) { // invincible is gone
                Invincibletimer = 0;
                Invincible = false;
            }

            if (!HasControl && NoControlTimer > 0.0)
                NoControlTimer = NoControlTimer - time;
            if (NoControlTimer <= 0.0) {
                NoControlTimer = 0.0;
                HasControl = true;
            }

			if(viewSwitchJumpTimer > 0.0) {
				viewSwitchJumpTimer -= time;
			}
			
            if (HasControl) {
				//Keyboard
				if(enable3d) {
					Vector2 newVel = new Vector2(0);
					if(keyboard[Key.W]) { newVel.X++; }
					if(keyboard[Key.S]) { newVel.X--; }
					if(keyboard[Key.A]) { newVel.Y--; }
					if(keyboard[Key.D]) { newVel.Y++; }

					newVel.NormalizeFast();
					if(viewSwitchJumpTimer > 0.0 && (newVel.X != 0 || newVel.Y != 0)) {
						viewSwitchJumpTimer = Math.Min(viewSwitchJumpTimer, 0.025);
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
                    if (keyboard[Key.A] == keyboard[Key.D]) {
                        velocity.X = 0f;
                    } else if(keyboard[Key.D]) {
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

                //TMP PHYSICS TEST BUTTON float button
				if(keyboard[Key.C]) {
					velocity.Y = _speed;
					fallTimer = 0.0;
				}

				//TMP PHYSICS TEST BUTTON suicide button
				if(keyboard[Key.X]) {
					_health = 0;
				}

                if (keyboard[Key.Space] && !spaceDown) {
					if(onGround) {
						accelerate(Vector3.UnitY * jumpspeed);
						onGround = false;
						viewSwitchJumpTimer = 0.0;
						//jumpSound.Play();
					}
                    spaceDown = true;
                }
                else if (!keyboard[Key.Space]) {
                    spaceDown = false;
                }

				//Keep this last
				handleMouseInput(playstate);
            }
        }

		private Vector3 calcProjDir(PlayState playstate) {
			// Grab Mouse coord.  Since Y goes down, just subtract from height to get correct orientation
			Vector3d mousecoord = new Vector3d((double)playstate.eng.Mouse.X, (double)(playstate.eng.Height - playstate.eng.Mouse.Y), 1.0);
			Vector3d mouseWorld;

			//mousecoord.X -= 400.0;

			//If 3d view, get z coordinate of mouse
			if(enable3d) {
				//float[] z = new float[1];
				//GL.ReadPixels((int)mousecoord.X, (int)mousecoord.Y, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, OpenTK.Graphics.OpenGL.PixelType.Float, z);
				//mousecoord.Z = z[0];

				// Pull the projection and model view matrix from the camera.   
				Matrix4d project = playstate.camera.GetProjectionMatrix();
				Matrix4d model = playstate.camera.GetModelViewMatrix();

				// Unproject the coordinates to convert from mouse to world coordinates
				mousecoord.Z = 0.0;
				Vector3d near = GameMouse.UnProject(mousecoord, model, project, playstate.camera.getViewport());
				mousecoord.Z = 1.0;
				Vector3d far = GameMouse.UnProject(mousecoord, model, project, playstate.camera.getViewport());

				//Interpolate to find coordinates just in front of player
				double t = (_location.X + 100.0f - near.X) / (far.X - near.X);
				mouseWorld = new Vector3d(_location.X + 100.0, near.Y + (far.Y - near.Y) * t, near.Z + (far.Z - near.Z) * t);

			} else {
				// Pull the projection and model view matrix from the camera.   
				Matrix4d project = playstate.camera.GetProjectionMatrix();
				Matrix4d model = playstate.camera.GetModelViewMatrix();

				// Unproject the coordinates to convert from mouse to world coordinates
				mouseWorld = GameMouse.UnProject(mousecoord, model, project, playstate.camera.getViewport());
			}

			Vector3 projDir;
			if(curProjectile.gravity) {
				if(!playstate.enable3d) {
					//Console.WriteLine(mouseWorld.Y.ToString());
					projDir = new Vector3((float)mouseWorld.X, (float)mouseWorld.Y, _location.Z);
					projDir -= _location;
					projDir.X = Math.Abs(projDir.X); //do this to simplify equation - will undo at end
				} else {
					projDir = new Vector3((float)(Math.Sqrt(Math.Pow(mouseWorld.X - _location.X, 2) + Math.Pow(mouseWorld.Z - _location.Z, 2))),
											(float)(mouseWorld.Y - _location.Y), 0.0f);
				}

				//Following is adapted from http://en.wikipedia.org/wiki/Trajectory_of_a_projectile#Angle_required_to_hit_coordinate_.28x.2Cy.29
				double velsquared = curProjectile.speed * curProjectile.speed;
				double sqrtPart = Math.Sqrt(velsquared * velsquared - gravity * (gravity * projDir.X * projDir.X + 2 * projDir.Y * velsquared));
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
						sqrtPart = Math.Sqrt(velsquared * velsquared - gravity * (gravity * projDir.X * projDir.X + 2 * projDir.Y * velsquared));
						theta = Math.Atan((velsquared - sqrtPart) / (gravity * projDir.X));
					}
				}

				projDir.X = (float)Math.Cos(theta);
				projDir.Y = (float)Math.Sin(theta);

				if(enable3d) {
					double phi = Math.Atan((mouseWorld.X - _location.X) / (mouseWorld.Z - _location.Z));
					if(mouseWorld.Z < _location.Z) {
						phi += Math.PI;
					}
					projDir.Z = (float)(projDir.X * Math.Cos(phi));
					projDir.X = (float)(projDir.X * Math.Sin(phi));
				} else {
					projDir.Z = 0.0f;
					if(mouseWorld.X < _location.X) {
						projDir.X = -projDir.X;
					}
				}
			} else { //projectile doesn't do gravity
				projDir = new Vector3((float)mouseWorld.X, (float)mouseWorld.Y, (float)mouseWorld.Z);
				projDir -= _location;
				if(!enable3d) {
					projDir.Z = 0.0f;
				}
				projDir.NormalizeFast();
			}
			return projDir;
		}
		
        /// <summary>
        /// Provides all the mouse input for the player.  Will currently check for a left click and shoot a projectile in the direction
        /// </summary>
        /// <param name="playstate">a pointer to play state, for accessing the obj lists in playstate</param>
        private void handleMouseInput(PlayState playstate)
        {
            if (playstate.eng.ThisMouse.LeftPressed() && projectileTimer <= 0.0) {
				Vector3 projDir = calcProjDir(playstate);

				bananaSound.Play();
				spawnProjectile(playstate, projDir);

				velocity.X = 0.0f;
				velocity.Z = 0.0f;

				projectileTimer = 0.25;
            }

            if (playstate.eng.ThisMouse.RightPressed()) {
                //TODO: impliment stamina constraints on right click/spin attack
                spinning = true;
            }
        }

        /// <summary>
        /// Spawns the players projectile in the param direction
        /// </summary>
        /// <param name="playstate">a pointer to play state, for accessing the obj lists in playstate</param>
        /// <param name="direction">The direction to fire the projectile in</param>
        private void spawnProjectile(PlayState playstate, Vector3 direction) {
            Vector3 projlocation = new Vector3(location);

            //break the rendering tie between player and projectile, or else they flicker
			if(!enable3d) {
				projlocation.Z += 0.001f;
			}
            
			Projectile shot = new Projectile(projlocation, direction, true, curProjectile, playstate.player); // spawn the actual projectile		
			//shot.accelerate(new Vector3(velocity.X, 0.0f, velocity.Z)); //account for player's current velocity

            // add projectile to appropriate lists
            playstate.objList.Add(shot);
            playstate.renderList.Add(shot);
            playstate.colisionList.Add(shot);
            playstate.physList.Add(shot);
            playstate.combatList.Add(shot);
        }

        /// <summary>
        /// Does a physics update for the player if we are in 3d view
        /// </summary>
        /// <param name="time">Time elapsed since last update</param>
        /// <param name="physList">a pointer to physList, the list of all physics objects</param>
        public void physUpdate3d(double time, List<PhysicsObject> physList) {
			double origTime = time;
			bool collidedWithGround = false;

			//first deal with gravity
			if(viewSwitchJumpTimer <= 0.0) {
				accel.Y -= (float)(gravity * time);
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
                bool endspin = false; // flag to end spinn

                foreach (PhysicsObject obj in physList) {
                    //if we are spinning, do a simple non-physics collision test against combat objects
                    //If we hit a Combat obj, damage it, despwan it w/e, then flag the spin attack to end
                    if (spinning && obj.collidesIn2d && obj.hascbox && obj != this && !alreadyCollidedList.Contains(obj)) {
                        //note: we cant do collisions the 'good' way like below with a cbox or pbox
                        // because we may start spinning with an enemy already inside of our box. Thus we must just simply check whether an enemy
                        // is inside our 'zone'(location + spinSize in x and z)
                        //Note: if we impliment an enemy with a not square cbox this may look... odd. until then, doing zones is very fast/easy
                        double enemyZone = (((CombatObject)obj).cbox.X + ((CombatObject)obj).cbox.Z) / 2; // avg(in case z and x are not equal)
                        double xdist = Math.Abs(obj.location.X - location.X);
                        double zdist = Math.Abs(obj.location.Z - location.Z);
                        double ydist = Math.Abs(obj.location.Y - location.Y);
                        if ((xdist < (enemyZone + spinSize)) && (zdist < (enemyZone + spinSize)) && ydist < ((CombatObject)obj).cbox.Y + cbox.Y) {
                            if (((CombatObject)obj).type == 1) {// obj is an enemy, hurt it
                                ((CombatObject)obj).health = ((CombatObject)obj).health - spinDamage;
                            }
                            if (((CombatObject)obj).type == 2) { // obj is a projectile, despawn projectile do damage
                                //if projectile was not spawned by the player, deal with it. Ignore all player spawned projectiles
                                if (!((Projectile)obj).playerspawned) {
                                    //despawn the projectile
                                    ((CombatObject)obj).health = 0;
                                }
                            }
                            endspin = true;
                        }
                    }
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
                if (endspin)
                    spinning = false;

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
									if(VectorUtil.overGround3d(this, physList)) {
										lastPosOnGround = new Vector3(_location);
									}
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
							NoControlTimer = 0.5;
							((Enemy)collidingObj).frozen = true;

                            knockback(true, collidingObj);

                            
						}
						if(((CombatObject)collidingObj).type == 2) { // obj is a projectile, despawn projectile do damage
                            //if projectile was not spawned by the player, deal with it. Ignore all player spawned projectiles
							if(!((Projectile)collidingObj).playerspawned) {
                                
                                _health = _health - ((CombatObject)collidingObj).damage;
                                Invincible = true;
								HasControl = false;
								NoControlTimer = 0.5;
                                //despawn the projectile
                                ((CombatObject)collidingObj).health = 0;

                                knockback(true, collidingObj);
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
				velocity = Vector3.Zero;
				Invincible = true;
				Invincibletimer = 2.0;
			}
		}

        /// <summary>
        /// Does a physics update for the player if we are in 2d view
        /// </summary>
        /// <param name="time">Time elapsed since last update</param>
        /// <param name="physList">a pointer to physList, the list of all physics objects</param>
        public void physUpdate2d(double time, List<PhysicsObject> physList) {
			double origTime = time;
			bool collidedWithGround = false;
			
			//first do gravity
			if(viewSwitchJumpTimer <= 0.0) {
				accel.Y -= (float)(gravity * time);
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
                bool endspin = false; // flag to end spinn

                foreach (PhysicsObject obj in physList) {
                    //if we are spinning, do a simple non-physics collision test against combat objects
                    //If we hit a Combat obj, damage it, despwan it w/e, then flag the spin attack to end
                    if (spinning && obj.collidesIn2d && obj.hascbox && obj != this && !alreadyCollidedList.Contains(obj)) {
                        //note: we cant do collisions the 'good' way like below with a cbox or pbox
                        // because we may start spinning with an enemy already inside of our box. Thus we must just simply check whether an enemy
                        // is inside our 'zone'(location + spinSize in x and z)
                        //Note: if we impliment an enemy with a not square cbox this may look... odd. until then, doing zones is very fast/easy
                        double enemyZone = (((CombatObject)obj).cbox.X + ((CombatObject)obj).cbox.Z) / 2; // avg(in case z and x are not equal)
                        double xdist = Math.Abs(obj.location.X - location.X);//only do x dist in 2d
                        double ydist = Math.Abs(obj.location.Y - location.Y);
                        if ((xdist < (enemyZone + spinSize)) && ydist < ((CombatObject)obj).cbox.Y + cbox.Y) {

                            if (((CombatObject)obj).type == 1) {// obj is an enemy, hurt it
                                ((CombatObject)obj).health = ((CombatObject)obj).health - spinDamage;
                            }
                            if (((CombatObject)obj).type == 2) { // obj is a projectile, despawn projectile do damage
                                //if projectile was not spawned by the player, deal with it. Ignore all player spawned projectiles
                                if (!((Projectile)obj).playerspawned) {
                                    //despawn the projectile
                                    ((CombatObject)obj).health = 0;
                                }
                            }
                            endspin = true;
                        }
                    }
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
                if (endspin)
                    spinning = false;

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

									//Using 3D instead of 2D is intentional here - don't save position if switching views could make you fall
									if(VectorUtil.overGround3d(this, physList)) {
										lastPosOnGround = new Vector3(_location);
									}
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
							NoControlTimer = 0.5;
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
								NoControlTimer = 0.5;
								//despawn the projectile
                                ((CombatObject)collidingObj).health = 0;
                                knockback(false, collidingObj);
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
				velocity = Vector3.Zero;
				Invincible = true;
				Invincibletimer = 2.0;
			}
		}

        /// <summary>
        /// Knocks the player back relative to the param collidingObj ( can be any physics object)
        /// </summary>
        /// <param name="is3d"> True if we are in 3d</param>
        /// <param name="collidingObj">The physics object we are colliding with</param>
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

        public int ScreenRegion {
            get { return screenRegion; }
        }

        public void doScaleTranslateAndTexture() {
            GL.PushMatrix();

            GL.Translate(_location);
            GL.Scale(_scale);
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

        double dist2d(Vector3 v1, Vector3 v2) { // does a dist using only x and z
            Vector2 tmp = new Vector2(v1.X - v2.X, v1.Z - v2.Z);
            return Math.Sqrt((tmp.X * tmp.X) + (tmp.Y * tmp.Y));
        }

		//TODO: Don't know if reset really applies to player or not...
		public void reset() {
			throw new NotImplementedException();
		}
	}
}
