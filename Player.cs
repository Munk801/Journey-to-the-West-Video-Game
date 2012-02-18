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
    class Player : GameObject, RenderObject, PhysicsObject, CombatObject
    {
        //Knockback physics constants:
        private Vector3 kbspeed = new Vector3(70, 100, 70);

        public PlayerState p_state;
        int texID;
		public Vector3 velocity;
		private Vector3 accel;
		private bool doesGravity; //true if gravity affects this object
        private bool Invincible, HasControl;
        private double Invincibletimer, NoControlTimer;

		public float deltax; //used for updating position of camera, etc.


        // SOUND FILES
        static string jumpSoundFile = "../../Resources/Sound/jump_sound.ogg";
        AudioFile jumpSound = new AudioFile(jumpSoundFile);

        public Player(SpriteSheet sprite)
        {
            p_state = new PlayerState("TEST player");
            p_state.setSpeed(130);
            _location = new Vector3(25, 12.5f, 50);
            _scale = new Vector3(12.5f, 12.5f, 12.5f);
            _pbox = new Vector3(6.25f, 6.25f, 6.25f);
            _cbox = new Vector3(6.25f, 6.25f, 6.25f);
            //cubemesh = new ObjMesh("../../Geometry/box.obj");
            //_texture = new Bitmap("../../Textures/player.png");
            texID = GL.GenTexture();
			velocity = new Vector3(0, 0, 0);
			accel = new Vector3(0, 0, 0);
			doesGravity = true;
			_cycleNum = 0;
			_frameNum = 0;
			_is3dGeo = false;
			_sprite = sprite;
            _hascbox = true;
            _type = 0; // hack, this means nothing, will never matter as player never collides with himself

            _damage = 0;
            _health = 5;
            Invincible = false;
            HasControl = false;
            Invincibletimer = 0;
            NoControlTimer = 0;
        }

        /**
         * Sets the PlayerState elements to the current Player values.  Call this method every update or simply when the state changes.  This will be used to store
         * the Players State when saving the game.
		 * 
		 * Returns the movement of the player to be used in updating camera, etc.
         * */
        bool spaceDown;
        public void updateState(bool enable3d, bool a, bool s, bool d, bool w, bool c, bool x, bool space, FrameEventArgs e) {

            if (Invincible)
                Invincibletimer = Invincibletimer + e.Time;
            if (Invincibletimer >= 0.5) { // invincible for 2 seconds
                Invincibletimer = 0;
                Invincible = false;
            }

            if (!HasControl)
                NoControlTimer = NoControlTimer + e.Time;
            if (NoControlTimer >= 0.5) {
                NoControlTimer = 0;
                HasControl = true;
            }

            if (HasControl) {
                if (enable3d) {
                    if (w)
                        velocity.X = (float)p_state.getSpeed();
                    if (s)
                        velocity.X = -(float)p_state.getSpeed();
                    if ((s && w) || (!s && !w))
                        velocity.X = 0f;

                    if (d)
                        velocity.Z = (float)p_state.getSpeed();
                    if (a)
                        velocity.Z = -(float)p_state.getSpeed();
                    if ((d && a) || (!d && !a))
                        velocity.Z = 0f;

                    if (w && d) {
                        velocity.X = (float)p_state.getSpeed() * 0.707f;
                        velocity.Z = (float)p_state.getSpeed() * 0.707f;
                    }
                    if (w && a) {
                        velocity.X = (float)p_state.getSpeed() * 0.707f;
                        velocity.Z = -((float)p_state.getSpeed() * 0.707f);
                    }
                    if (a && s) {
                        velocity.X = -((float)p_state.getSpeed() * 0.707f);
                        velocity.Z = -((float)p_state.getSpeed() * 0.707f);
                    }
                    if (d && s) {
                        velocity.X = -((float)p_state.getSpeed() * 0.707f);
                        velocity.Z = ((float)p_state.getSpeed() * 0.707f);
                    }
                    if (a && d)
                        velocity.Z = 0;
                    if (w && s)
                        velocity.X = 0;
                }
                else {
                    if (d)
                        velocity.X = (float)p_state.getSpeed();
                    if (a)
                        velocity.X = -(float)p_state.getSpeed();
                    if ((d && a) || (!d && !a))
                        velocity.X = 0f;
                }

                //TMP PHYSICS TEST BUTTON and suicide button
                if (c)
                    velocity.Y = (float)p_state.getSpeed() / 3;
                if (x)
                    _health = 0;


                //********************** space
                if (space && !spaceDown) {
                    if (velocity.Y < 0.000001f && velocity.Y > -0.0000001f) {
                        accelerate(Vector3.UnitY * 230);
                        //jumpSound.Play();
                    }
                    spaceDown = true;
                }
                else if (!space) {
                    spaceDown = false;
                }
            }
        }

        public void draw(bool viewIs3d, double time)
        {
			doScaleTranslateAndTexture();
			frameNumber = sprite.draw(viewIs3d, cycleNumber, frameNumber + time);
		}

		private bool _is3dGeo;
		public bool is3dGeo {
			get { return _is3dGeo; }
		}

		private ObjMesh _mesh; //null for sprites
		public ObjMesh mesh {
			get { return _mesh; }
		}

		private Bitmap _texture; //null for sprites
		public Bitmap texture {
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

		public bool isAnimated() {
			throw new Exception("The method or operation is not implemented.");
		}

        public void doScaleTranslateAndTexture() {
			GL.PushMatrix();

// 			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
// 			GL.BindTexture(TextureTarget.Texture2D, texID);
// 			BitmapData bmp_data = _texture.LockBits(new Rectangle(0, 0, _texture.Width, _texture.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
// 			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
// 				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
// 			_texture.UnlockBits(bmp_data);
// 			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
// 			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			GL.Translate(_location);
			GL.Scale(_scale);
        }

		public void physUpdate3d(FrameEventArgs e, List<PhysicsObject> objlist) {
			if(doesGravity) {
				accel.Y -= (float)(400*e.Time); //TODO: turn this into a constant somewhere
			}
			velocity += accel;
			accel.X = 0;
			accel.Y = 0;
			accel.Z = 0;

			PhysicsObject collidingObj = null;
			float collidingT = 1.0f/0.0f; //pos infinity
			int collidingAxis = -1;

            foreach (PhysicsObject obj in objlist) {
				// don't do collision physics to yourself
				if(obj != this) {
					Vector3 mybox, objbox;
					if(!Invincible && ((GameObject)obj).hascbox) {
						mybox = _cbox;
						objbox = ((CombatObject)obj).cbox;
					} else {
						mybox = _pbox;
						objbox = obj.pbox;
					}
					//find possible ranges of values for which a collision may have occured
					Vector3 maxTvals = VectorUtil.div(((GameObject)obj).location + objbox - _location + mybox, velocity);
					Vector3 minTvals = VectorUtil.div(((GameObject)obj).location - objbox - _location - mybox, velocity);
					VectorUtil.sort(ref minTvals, ref maxTvals);

					float minT = VectorUtil.maxVal(minTvals);
					float maxT = VectorUtil.minVal(maxTvals);

					// all three axes? || forward? || within range?
					if(minT > maxT || minT < 0.0f || minT > e.Time) {
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

			//Assume no collision happened; if it did, adjust below
			_location += velocity * (float)e.Time;
			deltax = (velocity * (float)e.Time).X;
			if(collidingAxis != -1) {
				if(Invincible || !((GameObject)collidingObj).hascbox) { //if this is a normal physics collision
					switch(collidingAxis) {
						case 0: //x
							velocity.X = 0;
							float endX = _location.X;
							if(_location.X < ((GameObject)collidingObj).location.X) {
								_location.X = ((GameObject)collidingObj).location.X - (pbox.X + collidingObj.pbox.X);
							} else {
								_location.X = ((GameObject)collidingObj).location.X + pbox.X + collidingObj.pbox.X;
							}
							deltax -= endX - _location.X; //update this value for camera offset
							break;
						case 1: //y
							velocity.Y = 0;
							if(_location.Y < ((GameObject)collidingObj).location.Y) {
								_location.Y = ((GameObject)collidingObj).location.Y - (pbox.Y + collidingObj.pbox.Y);
							} else {
								_location.Y = ((GameObject)collidingObj).location.Y + pbox.Y + collidingObj.pbox.Y;
							}
							break;
						case 2: //z
							velocity.Z = 0;
							if(_location.Z < ((GameObject)collidingObj).location.Z) {
								_location.Z = ((GameObject)collidingObj).location.Z - (pbox.Z + collidingObj.pbox.Z);
							} else {
								_location.Z = ((GameObject)collidingObj).location.Z + pbox.Z + collidingObj.pbox.Z;
							}
							break;
					}
				} else { //this is a combat collision
					if(((CombatObject)collidingObj).type == 1) {// obj is an enemy, do damage, knock player back
						_health = _health - ((CombatObject)collidingObj).damage;
						Invincible = true;
						HasControl = false;
						((Enemy)collidingObj).frozen = true;

						// direction we need to be knocked back in.
						Vector3 direction = new Vector3(location.X - ((GameObject)collidingObj).location.X, 0, location.Z - ((GameObject)collidingObj).location.Z);
						direction.Normalize();

						location = new Vector3(((GameObject)collidingObj).location.X + (collidingObj.pbox.X * direction.X), ((GameObject)collidingObj).location.Y + collidingObj.pbox.Y, ((GameObject)collidingObj).location.Z + (collidingObj.pbox.Z * direction.Z));
                        velocity = new Vector3(0, 0, 0);
						accel = new Vector3(kbspeed.X * direction.X, kbspeed.Y, kbspeed.Z * direction.Z);


					}
					if(((CombatObject)collidingObj).type == 2) { // obj is a projectile, despawn projectile do damage
						//TODO: projectile implementation


					}
				}
			}
		}

		public void physUpdate2d(FrameEventArgs e, List<PhysicsObject> objlist) {
			if(doesGravity) {
				accel.Y -= (float)(400 * e.Time); //TODO: turn this into a constant somewhere
			}
			velocity += accel;
			accel.X = 0;
			accel.Y = 0;
			accel.Z = 0;

			velocity.Z = 0; //special case for 2d

			PhysicsObject collidingObj = null;
			float collidingT = 1.0f / 0.0f; //pos infinity
			int collidingAxis = -1;

			foreach(PhysicsObject obj in objlist) {
				// don't do collision physics to yourself
				if(obj != this) {
					Vector3 mybox, objbox;
					if(!Invincible && ((GameObject)obj).hascbox) {
						mybox = _cbox;
						objbox = ((CombatObject)obj).cbox;
					} else {
						mybox = _pbox;
						objbox = obj.pbox;
					}
					//find possible ranges of values for which a collision may have occured
					Vector3 maxTvals = VectorUtil.div(((GameObject)obj).location + objbox - _location + mybox, velocity);
					Vector3 minTvals = VectorUtil.div(((GameObject)obj).location - objbox - _location - mybox, velocity);
					VectorUtil.sort(ref minTvals, ref maxTvals);

					float minT = VectorUtil.maxVal(minTvals.Xy);
					float maxT = VectorUtil.minVal(maxTvals.Xy);

					// both axes? || forward? || within range?
					if(minT > maxT || minT < 0.0f || minT > e.Time) {
// 						if(((GameObject)obj).hascbox && velocity.X != 0 && velocity.Y != 0 && minT >= 0) {
// 							Console.WriteLine(minT + ", " + maxT);
// 						}
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

			if(collidingAxis != -1) {
				if(Invincible || !((GameObject)collidingObj).hascbox) { //if this is a normal physics collision
					//Assume no collision happened; if it did, adjust below
					_location += velocity * (float)e.Time;
					deltax = (velocity * (float)e.Time).X;
					switch(collidingAxis) {
						case 0: //x
							velocity.X = 0;
							float endX = _location.X;
							if(_location.X < ((GameObject)collidingObj).location.X) {
								_location.X = ((GameObject)collidingObj).location.X - (pbox.X + collidingObj.pbox.X);
							} else {
								_location.X = ((GameObject)collidingObj).location.X + pbox.X + collidingObj.pbox.X;
							}
							deltax -= endX - _location.X; //update this value for camera offset
							break;
						case 1: //y
							velocity.Y = 0;
							if(_location.Y < ((GameObject)collidingObj).location.Y) {
								_location.Y = ((GameObject)collidingObj).location.Y - (pbox.Y + collidingObj.pbox.Y);
							} else {
								_location.Y = ((GameObject)collidingObj).location.Y + pbox.Y + collidingObj.pbox.Y;
							}
							break;
					}
				} else { //this is a combat collision
					if(((CombatObject)collidingObj).type == 1) {// obj is an enemy, do damage, knock player back
						_health = _health - ((CombatObject)collidingObj).damage;
						Invincible = true;
						HasControl = false;
						((Enemy)collidingObj).frozen = true;

						// direction we need to be knocked back in.
						Vector3 direction = new Vector3(location.X - ((GameObject)collidingObj).location.X, 0, 0);
						direction.Normalize();

						float origX = location.X;

						location = new Vector3(((GameObject)collidingObj).location.X + (collidingObj.pbox.X * direction.X), ((GameObject)collidingObj).location.Y + collidingObj.pbox.Y, location.Z);
						deltax = location.X - origX;
                        velocity = new Vector3(0, 0, 0);
						accel = new Vector3(kbspeed.X * direction.X, kbspeed.Y, 0);


					}
					if(((CombatObject)collidingObj).type == 2) { // obj is a projectile, despawn projectile do damage
						//TODO: projectile implementation


					}
				}
			} else { //no collision, behave normally
				_location += velocity * (float)e.Time;
				deltax = (velocity * (float)e.Time).X;
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
