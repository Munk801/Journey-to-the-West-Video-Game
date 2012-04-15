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
using OpenTK.Input;
using System.Reflection;
using System.Runtime.InteropServices;

namespace U5Designs {
	public enum PlayerAnim { stand2d=0, stand3d=1, walk2d=2, walk3d=3, spin2d=4, spin3d=5 };

    public class Player : GameObject, RenderObject, PhysicsObject, CombatObject
    {
		private const int fallDamage = 1;
        private const int runSpeed = 125;
		private const int spinSpeed = 175;

		private PlayState playstate; //Keep a reference since we'll need this regularly

        //Knockback physics constants:
        private Vector3 kbspeed;
		private float jumpspeed;

		//Player state
        public PlayerState p_state;
		public Vector3 velocity;
		private Vector3 accel;
		public double stamina, maxStamina;
		public int maxHealth;
        internal bool Invincible, HasControl, isMobile;
        private Vector3 lastPosOnGround; // used for falling off detection

        //timers
        private double NoControlTimer, projectileTimer, spinTimer;
		public double Invincibletimer, fallTimer, viewSwitchJumpTimer, lookDownTimer, squishTimer;

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

		private bool spaceDown, eDown, pDown;

        public bool inLevelDesignMode = false; // THIS IS ONLY USED FOR LEVEL DESIGNER
        
        // SOUND FILES
        AudioFile jumpSound, bananaSound, hurtSound;

        public Player(SpriteSheet sprite, List<ProjectileProperties> projectiles, PlayState ps) : base(Int32.MaxValue) //player always has largest ID for rendering purposes
        {
            p_state = new PlayerState("TEST player");
            //p_state.setSpeed(130);
			this.playstate = ps;
            _speed = runSpeed;
			_location = new Vector3(25, 12.5f, 50);
            _scale = new Vector3(16.25f, 25f, 16.25f);
            _pbox = new Vector3(5f, 12.5f, 5f);
            _cbox = new Vector3(4f, 11.5f, 4f);
            spinSize = 12; //?? experiment with this, TODO: change it to match the sprites size
			velocity = new Vector3(0, 0, 0);
			accel = new Vector3(0, 0, 0);
			kbspeed = new Vector3(70, 100, 70);
			jumpspeed = 250.0f;
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
            maxHealth = _health = 7;
			stamina = 5.0;
			maxStamina = 5.0;

			spaceDown = false;
			eDown = false;
            pDown = false;
			isMobile = true;

            Invincible = false;
            HasControl = true;
            Invincibletimer = 0.0;
            NoControlTimer = 0.0;
			fallTimer = 0.0;
			viewSwitchJumpTimer = 0.0;
			projectileTimer = 0.0;
			spinTimer = 0.0;
			lookDownTimer = -1.0;
			squishTimer = -1.0;
			lastPosOnGround = new Vector3(_location);
			_animDirection = 1;
			this.projectiles = projectiles;
			curProjectile = projectiles[0];
            curProjectile.damage = _damage;
			markerList = new List<Decoration>();

			Assembly assembly = Assembly.GetExecutingAssembly();
			jumpSound = new AudioFile(assembly.GetManifestResourceStream("U5Designs.Resources.Sound.jump_sound.ogg"));
			bananaSound = new AudioFile(assembly.GetManifestResourceStream("U5Designs.Resources.Sound.banana2.ogg"));
			hurtSound = new AudioFile(assembly.GetManifestResourceStream("U5Designs.Resources.Sound.hurt.ogg"));
        }

        /// <summary>
        ///  Updates the player specific state. Gets called every update frame
        /// </summary>
        /// <param name="enable3d"> true if we are in 3d view</param>
        /// <param name="keyboard"> the keyboard object, for handling input</param>
        /// <param name="time">time elapsed since last update</param>
        internal void updateState(bool enable3d, KeyboardDevice keyboard, double time) {
			this.enable3d = enable3d;

			//Update timers
			if(stamina < maxStamina) {
				stamina = Math.Min(stamina + time, maxStamina);
			}

			//Update spinTimer even if not spinning, because it's also the timeout before you can spin again
			if(spinTimer > 0.0) {
				spinTimer -= time;
			}

			if(spinning) {
				stamina = Math.Max(stamina - 5.0 * time, 0.0);
				if(stamina <= 0.0 || spinTimer <= 0.0) {
					spinning = false;
					_speed = 75;
					if(velocity.X == 0) {
						_cycleNum = (int)(enable3d ? PlayerAnim.stand3d : PlayerAnim.stand2d);
					} else {
						_cycleNum = (int)(enable3d ? PlayerAnim.walk3d : PlayerAnim.walk2d);
					}
				}
			} else {
				_speed = runSpeed;
			}

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

			if(lookDownTimer >= 0.0) {
				lookDownTimer += time;
				if(lookDownTimer >= 0.75) {
					cam.moveToYPos(_location.Y - 50.0f);
					lookDownTimer = -2.0;
					isMobile = false;
				}
			}

			if(squishTimer >= 0.0) {
				squishTimer += time;
				float oldPboxY = _pbox.Y;
				if(squishTimer <= 0.125) {
					_scale.Y = (float)(25.0 * (0.125 - squishTimer));
					_pbox.Y = _scale.Y / 2.0f;
					_location.Y -= oldPboxY - _pbox.Y;
				} else if(squishTimer <= 3.125) {
					_scale.Y = _pbox.Y = 0.0f;
					_location.Y -= oldPboxY - _pbox.Y;
				} else if(squishTimer <= 3.4375) {
					_scale.Y = (float)(25.0 * (squishTimer - 3.125));
					_pbox.Y = _scale.Y / 2.0f;
					_location.Y += _pbox.Y - oldPboxY;
				} else {
					_scale.Y = 25.0f;
					_pbox.Y = 12.5f;
					_location.Y += _pbox.Y - oldPboxY;
					squishTimer = -1.0;
				}
			}

			//If the grenade is selected, draw parabola
			if(curProjectile.gravity) {
				addMarkers();
			}
			
            if (HasControl) {
				handleKeyboardInput(keyboard);

				//Keep this last
				handleMouseInput();
            }
        }

		#region leveldesigner
		/// <summary>
        ///  FOR LEVEL DESIGNER ONLY
        /// </summary>
        /// <param name="enable3d"> true if we are in 3d view</param>
        /// <param name="keyboard"> the keyboard object, for handling input</param>
        /// <param name="time">time elapsed since last update</param>
        internal void updateState(bool enable3d, KeyboardDevice keyboard, double time, bool leveldesign =true)
        {
            this.enable3d = enable3d;

            //Update timers
            if (stamina < maxStamina)
            {
                stamina = Math.Min(stamina + time, maxStamina);
            }

            //Update spinTimer even if not spinning, because it's also the timeout before you can spin again
            if (spinTimer > 0.0)
            {
                spinTimer -= time;
            }

			if(spinning) {
				stamina = Math.Max(stamina - 5.0 * time, 0.0);
				if(stamina <= 0.0 || spinTimer <= 0.0) {
					spinning = false;
					_speed = runSpeed;
					if(velocity.X == 0) {
						_cycleNum = (int)(enable3d ? PlayerAnim.stand3d : PlayerAnim.stand2d);
					} else {
						_cycleNum = (int)(enable3d ? PlayerAnim.walk3d : PlayerAnim.walk2d);
					}
				}
			} else {
				_speed = runSpeed;
			}

            if (projectileTimer > 0.0) {
                projectileTimer -= time;
            }

            if (Invincible)
                Invincibletimer = Invincibletimer - time;
            if (Invincibletimer <= 0) {
				// invincible is gone
                Invincibletimer = 0;
                Invincible = false;
            }

            if (!HasControl && NoControlTimer > 0.0)
                NoControlTimer = NoControlTimer - time;
            if (NoControlTimer <= 0.0) {
                NoControlTimer = 0.0;
                HasControl = true;
            }

            if (viewSwitchJumpTimer > 0.0) {
                viewSwitchJumpTimer -= time;
			}

            //If the grenade is selected, draw parabola
            if (curProjectile.gravity)
            {
                addMarkers();
            }

            if (HasControl)
            {
                handleKeyboardInput(keyboard);

                //Keep this last
                //handleMouseInput();
            }
        }
		#endregion

		/// <summary>
		/// Handles all keyboard input from the player
		/// </summary>
		/// <param name="keyboard">Contains the current keypresses</param>
		private void handleKeyboardInput(KeyboardDevice keyboard) {
			if(isMobile) {
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

					velocity.X = newVel.X * speed;
					velocity.Z = newVel.Y * speed;

					_animDirection = (velocity.X >= 0 ? 1 : -1);
					if(!spinning) {
						_cycleNum = (int)(velocity.X == 0 ? PlayerAnim.stand3d : PlayerAnim.walk3d);
					}
				} else {
					if(keyboard[Key.A] == keyboard[Key.D]) {
						velocity.X = 0f;
					} else if(keyboard[Key.D]) {
						velocity.X = _speed;
					} else { //a
						velocity.X = -_speed;
					}

					_animDirection = (velocity.X >= 0 ? 1 : -1);
					if(!spinning) {
						_cycleNum = (int)(velocity.X == 0 ? PlayerAnim.stand2d : PlayerAnim.walk2d);
					}
				}

#if DEBUG
				//Cloud
				//TODO: Implement animation etc, possibly change which key triggers this
				if(keyboard[Key.C]) {
					velocity.Y = _speed;
					fallTimer = 0.0;
				}
                //TMP PHYSICS TEST BUTTON suicide button
                if (keyboard[Key.X]) {
                    _health = 0;
                }
                if (keyboard[Key.P] && !pDown) {
                    pDown = true;
                    Console.WriteLine("Player position: (" + location.X + ", " + location.Y + ", " + location.Z + ")");
                }
                else if (!keyboard[Key.P])
                    pDown = false;

                if (keyboard[Key.BackSlash])
                    _location = new Vector3(3779, 138, 43);
#endif

				//Jump
				if(keyboard[Key.Space] && !spaceDown) {
					if(onGround) {
						accelerate(Vector3.UnitY * jumpspeed);
						onGround = false;
						viewSwitchJumpTimer = 0.0;
						//jumpSound.Play();
					}
					spaceDown = true;
				} else if(!keyboard[Key.Space]) {
					spaceDown = false;
				}
			} else { //not mobile - grenade selected
				velocity.X = 0.0f;
				velocity.Z = 0.0f;
			}

			//Look down
			if(!enable3d && keyboard[Key.S] && !keyboard[Key.A] && !keyboard[Key.D]) {
				if(lookDownTimer == -1.0) {
					lookDownTimer = 0.0;
				}
			} else if(lookDownTimer != -1.0) {
				cam.moveToYPos(_location.Y);
				lookDownTimer = -1.0;
				isMobile = true;
			}

			//Toggle Grenade
			if(keyboard[Key.E] && !eDown) {
				if(curProjectile.gravity) { //Turn grenades off
					curProjectile = projectiles[0];
					isMobile = true;
					markerList.Clear();
				} else { //Turn grenades on
					curProjectile = projectiles[1];
					velocity.X = 0.0f;
					velocity.Z = 0.0f;
					isMobile = false;
					_cycleNum = (int)(enable3d ? PlayerAnim.stand3d : PlayerAnim.stand2d);
				}
				eDown = true;
			} else if(!keyboard[Key.E]) {
				eDown = false;
			}
			
			//Secret skip to boss
			if(keyboard[Key.Period] && keyboard[Key.Comma]) {
				//floor at 125 + 12.5 player pbox
				_location = new Vector3(playstate.bossAreaCenter) + Vector3.UnitY * (_pbox.Y + 0.1f);

				//The following should maybe be moved to a function in Camera
				playstate.camera.eye.Y = _location.Y + (enable3d ? 24.0f : 31.25f);
				playstate.camera.lookat.Y = _location.Y + (enable3d ? 20.5f : 31.25f);
				cam.moveToYPos(_location.Y);

				playstate.enterBossMode();
			}

		}
		
        /// <summary>
        /// Provides all the mouse input for the player.  Will currently check for a left click and shoot a projectile in the direction
        /// </summary>
        /// <param name="playstate">a pointer to play state, for accessing the obj lists in playstate</param>
        private void handleMouseInput()
        {
            if (playstate.eng.ThisMouse.LeftPressed()) {

				if(stamina >= curProjectile.staminaCost && projectileTimer <= 0.0) {
					spawnProjectile(calcProjDir());

					stamina -= curProjectile.staminaCost;
					projectileTimer = 0.25;

					bananaSound.Play();
				}
				//TODO: If the player was out of stamina, flash the stamina bar to let them know what happened

				//Note: do this whether we actually launched a grenade or not, because if we didn't, it's
				//      probably because of stamina, and the player may not have time to switch back or to
				//      wait for stamina to refill.
				if(curProjectile.gravity) {
					//Automatically switch back to normal projectile
					curProjectile = projectiles[0];
					isMobile = true;
					markerList.Clear();

					projectileTimer = 0.25;
				}
            }

            if (isMobile && playstate.eng.ThisMouse.RightPressed() && spinTimer <= 0.0 && stamina > 0.0) {
                spinning = true;
				spinTimer = 0.5;
                _speed = spinSpeed;
				_cycleNum = (int)(enable3d ? PlayerAnim.spin3d : PlayerAnim.spin2d);
            }
        }

        /// <summary>
        /// Spawns the players projectile in the param direction
        /// </summary>
        /// <param name="playstate">a pointer to play state, for accessing the obj lists in playstate</param>
        /// <param name="direction">The direction to fire the projectile in</param>
        private void spawnProjectile(Vector3 direction) {
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
            playstate.collisionList.Add(shot);
            playstate.physList.Add(shot);
            playstate.combatList.Add(shot);
        }

		/// <summary>
		/// Adds indicators to show where the players projectile is going to go
		/// Used for gravity based projectiles (specifically the grenade)
		/// </summary>
		public void addMarkers() {
			if(curProjectile.gravity) {
				markerList.Clear();

				Vector3 vel = calcProjDir() * curProjectile.speed;
				//vel -= new Vector3(velocity.X, 0.0f, velocity.Z);
				Vector3 pos = new Vector3(_location);

// 				for(int i = 0; i < 40; i++) {
// 					vel.Y -= (float)gravity / 20.0f;
// 					//vel.Y -= 20.0f;
// 					pos += (vel / 20.0f);
// 					markerList.Add(new Decoration(pos, new Vector3(5, 5, 5), true, true, Billboarding.Yes, marker));
				// 				}

				if(enable3d) {
					for(int i = 0; i < 10; i++) {
						double time = 1 / 20.0;
						vel.Y -= (float)(gravity * time);

						//now check for collisions and move
						List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

						while(time > 0.0) {
							PhysicsObject collidingObj = null;
							float collidingT = 1.0f / 0.0f; //pos infinity
							int collidingAxis = -1;

							foreach(PhysicsObject obj in playstate.physList) {
								// don't do collision physics to things you already hit this frame
								if(obj.collidesIn3d && !alreadyCollidedList.Contains(obj)) {
									//find possible ranges of values for which a collision may have occurred
									Vector3 maxTvals = VectorUtil.div(obj.location + obj.pbox - pos + curProjectile.pbox, vel);
									Vector3 minTvals = VectorUtil.div(obj.location - obj.pbox - pos - curProjectile.pbox, vel);
									VectorUtil.sort(ref minTvals, ref maxTvals);

									float minT = VectorUtil.maxVal(minTvals);
									float maxT = VectorUtil.minVal(maxTvals);

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

							if(collidingAxis == -1) { //no collision
								pos += vel * (float)time;
								time = 0.0;
							} else {
								alreadyCollidedList.Add(collidingObj);
								Vector3 startLoc = new Vector3(pos);
								switch(collidingAxis) {
									case 0: //x
										if(pos.X < collidingObj.location.X) {
											pos.X = collidingObj.location.X - (curProjectile.pbox.X + collidingObj.pbox.X) - 0.0001f;
										} else {
											pos.X = collidingObj.location.X + curProjectile.pbox.X + collidingObj.pbox.X + 0.0001f;
										}
										if(vel.X != 0) { //should always be true, but just in case...
											double deltaTime = (pos.X - startLoc.X) / vel.X;
											pos.Y += (float)(vel.Y * deltaTime);
											pos.Z += (float)(vel.Z * deltaTime);
											time -= deltaTime;
										}
										vel.X *= -0.6f; //bounce
										break;
									case 1: //y
										if(pos.Y < collidingObj.location.Y) {
											pos.Y = collidingObj.location.Y - (curProjectile.pbox.Y + collidingObj.pbox.Y) - 0.0001f;
										} else {
											pos.Y = collidingObj.location.Y + curProjectile.pbox.Y + collidingObj.pbox.Y + 0.0001f;
										}
										if(vel.Y != 0) { //should always be true, but just in case...
											double deltaTime = (pos.Y - startLoc.Y) / vel.Y;
											pos.X += (float)(vel.X * deltaTime);
											pos.Z += (float)(vel.Z * deltaTime);
											time -= deltaTime;
										}

										vel.Y *= -0.6f; //bounce
										if(pos.Y > collidingObj.location.Y) {
											//Simulate friction for rolling
											if(Math.Abs(vel.X) >= 50.0f) {
												vel.X = (vel.X >= 0 ? vel.X - (float)(Projectile.friction * time) : vel.X + (float)(Projectile.friction * time));
											}
											if(vel.Y < 25.0f) {
												vel.Y = 0.0f; //stop bounces when they get too small
											}
										}
										break;
									case 2: //z
										if(pos.Z < collidingObj.location.Z) {
											pos.Z = collidingObj.location.Z - (curProjectile.pbox.Z + collidingObj.pbox.Z) - 0.0001f;
										} else {
											pos.Z = collidingObj.location.Z + curProjectile.pbox.Z + collidingObj.pbox.Z + 0.0001f;
										}
										if(vel.Z != 0) { //should always be true, but just in case...
											double deltaTime = (pos.Z - startLoc.Z) / vel.Z;
											pos.X += (float)(vel.X * deltaTime);
											pos.Y += (float)(vel.Y * deltaTime);
											time -= deltaTime;
										}
										vel.Z *= -0.6f; //bounce
										break;
								}
							}
						}
						markerList.Add(new Decoration(pos, new Vector3(2, 2, 2), true, true, Billboarding.Yes, marker));
					}
				} else { //2D
					for(int i = 0; i < 10; i++) {
						double time = 1 / 20.0;
						vel.Y -= (float)(gravity * time);

						//now check for collisions and move
						List<PhysicsObject> alreadyCollidedList = new List<PhysicsObject>();

						while(time > 0.0) {
							PhysicsObject collidingObj = null;
							float collidingT = 1.0f / 0.0f; //pos infinity
							int collidingAxis = -1;

							foreach(PhysicsObject obj in playstate.physList) {
								// don't do collision physics to things you already hit this frame
								if(obj.collidesIn2d && !alreadyCollidedList.Contains(obj)) {
									//find possible ranges of values for which a collision may have occurred
									Vector3 maxTvals = VectorUtil.div(obj.location + obj.pbox - pos + curProjectile.pbox, vel);
									Vector3 minTvals = VectorUtil.div(obj.location - obj.pbox - pos - curProjectile.pbox, vel);
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

							if(collidingAxis == -1) { //no collision
								pos += vel * (float)time;
								time = 0.0;
							} else {
								alreadyCollidedList.Add(collidingObj);
								Vector3 startLoc = new Vector3(pos);
								switch(collidingAxis) {
									case 0: //x
										if(pos.X < collidingObj.location.X) {
											pos.X = collidingObj.location.X - (curProjectile.pbox.X + collidingObj.pbox.X) - 0.0001f;
										} else {
											pos.X = collidingObj.location.X + curProjectile.pbox.X + collidingObj.pbox.X + 0.0001f;
										}
										if(vel.X != 0) { //should always be true, but just in case...
											double deltaTime = (pos.X - startLoc.X) / vel.X;
											pos.Y += (float)(vel.Y * deltaTime);
											pos.Z += (float)(vel.Z * deltaTime);
											time -= deltaTime;
										}
										vel.X *= -0.6f; //bounce
										break;
									case 1: //y
										if(pos.Y < collidingObj.location.Y) {
											pos.Y = collidingObj.location.Y - (curProjectile.pbox.Y + collidingObj.pbox.Y) - 0.0001f;
										} else {
											pos.Y = collidingObj.location.Y + curProjectile.pbox.Y + collidingObj.pbox.Y + 0.0001f;
										}
										if(vel.Y != 0) { //should always be true, but just in case...
											double deltaTime = (pos.Y - startLoc.Y) / vel.Y;
											pos.X += (float)(vel.X * deltaTime);
											pos.Z += (float)(vel.Z * deltaTime);
											time -= deltaTime;
										}

										vel.Y *= -0.6f; //bounce
										if(pos.Y > collidingObj.location.Y) {
											//Simulate friction for rolling
											if(Math.Abs(vel.X) >= 50.0f) {
												vel.X = (vel.X >= 0 ? vel.X - (float)(Projectile.friction * time) : vel.X + (float)(Projectile.friction * time));
											}
											if(vel.Y < 25.0f) {
												vel.Y = 0.0f; //stop bounces when they get too small
											}
										}
										break;
								}
							}
						}
						markerList.Add(new Decoration(pos, new Vector3(2, 2, 2), true, true, Billboarding.Yes, marker));
					}
				}
			}
		}

		private Vector3 calcProjDir() {
			// Grab Mouse coord.  Since Y goes down, just subtract from height to get correct orientation
			Vector3d mousecoord = new Vector3d((double)playstate.eng.Mouse.X, (double)(playstate.eng.Height - playstate.eng.Mouse.Y), 1.0);
			Vector3d mouseWorld;

			if(enable3d) {
				//mousecoord.X -= 400.0;

				//If 3d view, get z coordinate of mouse
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
        /// Starts the timer and animation to squish the player if he got squished in the zookeeper encounter
        /// </summary>
        public void squish() {
            //TODO: implement, + implement a timer so you cant get double squished in 2d
			if(squishTimer < 0.0) {
				health = health - 3;
				squishTimer = 0.0;
			}
        }

		/// <summary>
		/// Helper method for physUpdate2d and 3d, updates current scale when squishing
		/// </summary>
		/// <param name="time"></param>
		private void squishUpdate(double time) {
			squishTimer += time;
			if(squishTimer <= 0.125) {
				_scale.Y = (float)(25.0 * (0.125 - squishTimer));
			} else if(squishTimer <= 3.125) {
				_scale.Y = 0.0f;
			} else if(squishTimer <= 3.5) {
				_scale.Y = (float)(25.0 * (squishTimer - 3.125));
			} else {
				_scale.Y = 25.0f;
				squishTimer = -1.0;
			}
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
                bool endspin = false; // flag to end spin

                foreach (PhysicsObject obj in physList) {
                    //if we are spinning, do a simple non-physics collision test against combat objects
                    //If we hit a Combat obj, damage it, despwan it w/e, then flag the spin attack to end
                    if (spinning && obj.collidesIn3d && obj.hascbox && obj != this && !alreadyCollidedList.Contains(obj)) {
                        //note: we cant do collisions the 'good' way like below with a cbox or pbox
                        // because we may start spinning with an enemy already inside of our box. Thus we must just simply check whether an enemy
                        // is inside our 'zone'(location + spinSize in x and z)
                        //Note: if we impliment an enemy with a not square cbox this may look... odd. until then, doing zones is very fast/easy
                        double enemyZone = (((CombatObject)obj).cbox.X + ((CombatObject)obj).cbox.Z) / 2; // avg(in case z and x are not equal)
                        double xdist = Math.Abs(obj.location.X - location.X);
                        double zdist = Math.Abs(obj.location.Z - location.Z);
                        double ydist = Math.Abs(obj.location.Y - location.Y);
                        if ((xdist < (enemyZone + spinSize)) && (zdist < (enemyZone + spinSize)) && ydist < ((CombatObject)obj).cbox.Y + cbox.Y) {
							if(((CombatObject)obj).type == (int)CombatType.enemy) {// obj is an enemy
                                ((CombatObject)obj).health = ((CombatObject)obj).health - spinDamage;
                                HasControl = false;
                                NoControlTimer = 0.5;
                                knockback(true, obj);
								((Enemy)obj).frozen = true;
                            } else if (((CombatObject)obj).type == (int)CombatType.projectile) { // obj is a projectile, despawn projectile do damage
                                //if projectile was not spawned by the player, deal with it. Ignore all player spawned projectiles
                                if (!((Projectile)obj).playerspawned) {
                                    //despawn the projectile
                                    ((CombatObject)obj).health = 0;
                                }
                            }
                            else if (((CombatObject)obj).type ==3){ // obj is zookeeper
                                ((Boss)obj).dodamage(spinDamage);
                                HasControl = false;
                                NoControlTimer = 0.5;
                                knockback(true, obj);
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

				if(endspin) {
					spinning = false;
                    _speed = runSpeed;
					_cycleNum = (int)(velocity.X == 0 ? PlayerAnim.stand3d : PlayerAnim.walk3d);
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
									if(VectorUtil.overGround3dStrict(this, physList)) {
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
                        if (((CombatObject)collidingObj).type == 3) {// obj is the boss
                            time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
                            _health = _health - ((CombatObject)collidingObj).damage;
                            Invincible = true;
                            HasControl = false;
                            NoControlTimer = 0.5;
                            knockback(true, collidingObj);
                        }
                        if (((CombatObject)collidingObj).type == 4) {// obj is a special projectile that will squish the player(underside of a box)
                            time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
                            if (onGround)
                                squish();
                            else {
                                squish();
                                accelerate(((Projectile)collidingObj).velocity);
                            }
                            ((CombatObject)collidingObj).health = 0;
                        }
					}
				}
			}

            //Now that everything is done, move the camera if we're below the bottom of the screen
            if (fallTimer > 0.0 || !cam.playerIsAboveScreenBottom())
            {
                fallTimer += origTime;
				if(!inLevelDesignMode) {
					cam.trackPlayer();
				}
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
				bool endspin = false; // flag to end spin

				foreach(PhysicsObject obj in physList) {
					//if we are spinning, do a simple non-physics collision test against combat objects
					//If we hit a Combat obj, damage it, despwan it w/e, then flag the spin attack to end
					if(spinning && obj.collidesIn2d && obj.hascbox && obj != this && !alreadyCollidedList.Contains(obj)) {
						//note: we cant do collisions the 'good' way like below with a cbox or pbox
						// because we may start spinning with an enemy already inside of our box. Thus we must just simply check whether an enemy
						// is inside our 'zone'(location + spinSize in x and z)
						//Note: if we impliment an enemy with a not square cbox this may look... odd. until then, doing zones is very fast/easy
						double enemyZone = (((CombatObject)obj).cbox.X + ((CombatObject)obj).cbox.Z) / 2; // avg(in case z and x are not equal)
						double xdist = Math.Abs(obj.location.X - location.X);//only do x dist in 2d
						double ydist = Math.Abs(obj.location.Y - location.Y);
						if((xdist < (enemyZone + spinSize)) && ydist < ((CombatObject)obj).cbox.Y + cbox.Y) {

							if(((CombatObject)obj).type == 1) {// obj is an enemy or boss, hurt it
								((CombatObject)obj).health = ((CombatObject)obj).health - spinDamage;
								HasControl = false;
								NoControlTimer = 0.5;
								knockback(false, obj);
								if(((CombatObject)obj).type == 1)
									((Enemy)obj).frozen = true;
							} else if(((CombatObject)obj).type == 2) { // obj is a projectile, despawn projectile do damage
								//if projectile was not spawned by the player, deal with it. Ignore all player spawned projectiles
								if(!((Projectile)obj).playerspawned) {
									//despawn the projectile
									((CombatObject)obj).health = 0;
								}
							} else if(((CombatObject)obj).type == 3) { // obj is zookeeper
								((Boss)obj).dodamage(spinDamage);
								HasControl = false;
								NoControlTimer = 0.5;
								knockback(false, obj);
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

				if(endspin) {
					spinning = false;
					_speed = runSpeed;
					_cycleNum = (int)(velocity.X == 0 ? PlayerAnim.stand3d : PlayerAnim.walk3d);
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

									//Using 3D instead of 2D is intentional here - don't save position if switching views could make you fall
									if(VectorUtil.overGround3dStrict(this, physList)) {
										lastPosOnGround = new Vector3(_location);
									}
									if(lookDownTimer != -2.0) {
										cam.moveToYPos(_location.Y);
									}
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
						if(((CombatObject)collidingObj).type == 3) {// obj is the boss, treat it basicially like an enemy
							time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
							_health = _health - ((CombatObject)collidingObj).damage;
							Invincible = true;
							Invincibletimer = 0.5;
							HasControl = false;
							NoControlTimer = 0.5;
							knockback(false, collidingObj);
						}
						if(((CombatObject)collidingObj).type == 4) {// obj is a special projectile that will squish the player(underside of a box)
							time = 0.0; //WARNING: Ending early like this is a bit lazy, so if we have problems later, do like physics collisions instead
							//despawn the projectile
							if(onGround)
								squish();
							else {
								squish();
								accelerate(((Projectile)collidingObj).velocity);
							}
							((CombatObject)collidingObj).health = 0;
						}

					}
				}
			}

			//Now that everything is done, move the camera if we're below the bottom of the screen
			if(fallTimer > 0.0 || !cam.playerIsAboveScreenBottom()) {
				fallTimer += origTime;
				if(!inLevelDesignMode) {
					cam.trackPlayer();
				}
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

				_cycleNum = (int)PlayerAnim.stand3d; //TODO: Change to player damage animation
            } else {
                Vector3 direction = new Vector3(location.X - collidingObj.location.X, 0, 0);
                direction.Normalize();

                float origX = location.X;

                location = new Vector3(collidingObj.location.X + (collidingObj.pbox.X * direction.X), collidingObj.location.Y + collidingObj.pbox.Y, location.Z);
                deltax = location.X - origX;
                velocity = new Vector3(0, 0, 0);
                accel = new Vector3(kbspeed.X * direction.X, kbspeed.Y, 0);

				_cycleNum = (int)PlayerAnim.stand2d; //TODO: Change to player damage animation
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

        public bool canSquish {
            get { return false; }
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

		public bool hasTwoAnims {
			get { return true; }
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

		public Effect deathAnim {
			//Change if we give him a death animation later
			get { return null; }
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
