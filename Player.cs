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
        public PlayerState p_state;
        public bool shifter;
        int texID;
		public Vector3 velocity;
		private Vector3 accel;
		private bool doesGravity; //true if gravity affects this object

		public float deltax; //used for updating position of camera, etc.


        // SOUND FILES
        static string jumpSoundFile = "../../Resources/Sound/jump_sound.ogg";
        AudioFile jumpSound = new AudioFile(jumpSoundFile);

        public Player(SpriteSheet sprite)
        {
            p_state = new PlayerState("TEST player");
            p_state.setSpeed(300);
            _location = new Vector3(25, 12.5f, 50);
            _scale = new Vector3(12.5f, 12.5f, 12.5f);
            _pbox = new Vector3(12.5f, 12.5f, 12.5f);
            _cbox = new Vector3(12.5f, 12.5f, 12.5f);
            //cubemesh = new ObjMesh("../../Geometry/box.obj");
            //_texture = new Bitmap("../../Textures/player.png");
            _damage = 0;
            texID = GL.GenTexture();
			velocity = new Vector3(0, 0, 0);
			accel = new Vector3(0, 0, 0);
			doesGravity = true;
			_cycleNum = 0;
			_frameNum = 0;
			_is3dGeo = false;
			_sprite = sprite;
        }

        /**
         * Sets the PlayerState elements to the current Player values.  Call this method every update or simply when the state changes.  This will be used to store
         * the Players State when saving the game.
		 * 
		 * Returns the movement of the player to be used in updating camera, etc.
         * */
        bool spaceDown;
        public void updateState(bool enable3d, bool a, bool s, bool d, bool w, bool c, bool space, FrameEventArgs e) {
            //TODO: add control for other buttons, jump, projectile etc
            if (enable3d)
            {
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
            else
            {
                if (d)
                    velocity.X = (float)p_state.getSpeed();
                if (a)
                    velocity.X = -(float)p_state.getSpeed();
                if ((d && a) || (!d && !a))
                    velocity.X = 0f;
            }

            //TMP PHYSICS TEST BUTTON
            if (c)
                velocity.Y = (float)p_state.getSpeed()/3;

            //********************** space
            if (space && !spaceDown)
            {
                if (velocity.Y < 0.000001f && velocity.Y > -0.0000001f)
                {
                    accelerate(Vector3.UnitY * 230);
                    //jumpSound.Play();
                }
                spaceDown = true; 
            }
            else if (!space)
            {
                spaceDown = false;
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
			_location += velocity*(float)e.Time;
			
			deltax = (velocity * (float)e.Time).X;

            foreach (PhysicsObject obj in objlist) {
                // don't do collision physics to yourself
				if(obj != this) {

                    if ((Math.Abs(((GameObject)obj).location.Y - _location.Y) < pbox.Y + obj.pbox.Y)
                    && (Math.Abs(((GameObject)obj).location.Z - _location.Z) < pbox.Z + obj.pbox.Z)
                    && (Math.Abs(((GameObject)obj).location.X - _location.X) < pbox.X + obj.pbox.X)) {
                        // at this point the player is colliding with this obj

						//figure out which direction the collision happened on by looking for point where
						//only one axis is not colliding
						Vector3 temploc = _location - velocity * (float)e.Time;
						float originalX = temploc.X; //save the original x position for calculating camera offset
						Vector3 step = velocity * (float)e.Time * 0.25f;
						bool x = Math.Abs(((GameObject)obj).location.X - temploc.X) <= pbox.X + obj.pbox.X;
						bool y = Math.Abs(((GameObject)obj).location.Y - temploc.Y) <= pbox.Y + obj.pbox.Y;
						bool z = Math.Abs(((GameObject)obj).location.Z - temploc.Z) <= pbox.Z + obj.pbox.Z;
						int axes = (x ? 1 : 0) + (y ? 1 : 0) + (z ? 1 : 0);
						bool lastStepWasForward = true;
						while(axes != 2 && step != new Vector3(0,0,0)) {
							if(axes < 2) { //not far enough, step forward
								if(!lastStepWasForward) {
									step *= 0.5f;
								}
								lastStepWasForward = true;
								temploc += step;
								x = Math.Abs(((GameObject)obj).location.X - temploc.X) <= pbox.X + obj.pbox.X;
								y = Math.Abs(((GameObject)obj).location.Y - temploc.Y) <= pbox.Y + obj.pbox.Y;
								z = Math.Abs(((GameObject)obj).location.Z - temploc.Z) <= pbox.Z + obj.pbox.Z;
								axes = (x ? 1 : 0) + (y ? 1 : 0) + (z ? 1 : 0);
							} else { //too far, step backwards
								if(lastStepWasForward) {
									step *= 0.5f;
								}
								lastStepWasForward = false;
								temploc -= step;
								x = Math.Abs(((GameObject)obj).location.X - temploc.X) <= pbox.X + obj.pbox.X;
								y = Math.Abs(((GameObject)obj).location.Y - temploc.Y) <= pbox.Y + obj.pbox.Y;
								z = Math.Abs(((GameObject)obj).location.Z - temploc.Z) <= pbox.Z + obj.pbox.Z;
								axes = (x ? 1 : 0) + (y ? 1 : 0) + (z ? 1 : 0);
							}
						}

						//We're now at a point that two axes intersect - the third is the one that collided
						if(!x) {
							velocity.X = 0;
							if(_location.X < ((GameObject)obj).location.X) {
								_location.X = ((GameObject)obj).location.X - (pbox.X + obj.pbox.X);
							} else {
								_location.X = ((GameObject)obj).location.X + pbox.X + obj.pbox.X;
							}
							deltax = _location.X - originalX; //update this value for camera offset
						} else if(!y) {
							velocity.Y = 0;
							if(_location.Y < ((GameObject)obj).location.Y) {
								_location.Y = ((GameObject)obj).location.Y - (pbox.Y + obj.pbox.Y);
							} else {
								_location.Y = ((GameObject)obj).location.Y + pbox.Y + obj.pbox.Y;
							}
						} else { //z
							velocity.Z = 0;
							if(_location.Z < ((GameObject)obj).location.Z) {
								_location.Z = ((GameObject)obj).location.Z - (pbox.Z + obj.pbox.Z);
							} else {
								_location.Z = ((GameObject)obj).location.Z + pbox.Z + obj.pbox.Z;
							}
						}
                    }
                }
            }
		}

		public void physUpdate2d(FrameEventArgs e, List<PhysicsObject> objlist) {
			if(doesGravity) {
				accel.Y -= (float)(400 * e.Time); //TODO: turn this into a constant somewhere
			}
			velocity.X += accel.X;
			velocity.Y += accel.Y;
			accel.X = 0;
			accel.Y = 0;
			accel.Z = 0;
			_location += velocity * (float)e.Time;

			deltax = (velocity * (float)e.Time).X;

			foreach(PhysicsObject obj in objlist) {
				// don't do collision physics to yourself
				if(obj != this) {

					if((Math.Abs(((GameObject)obj).location.Y - _location.Y) < pbox.Y + obj.pbox.Y)
					&& (Math.Abs(((GameObject)obj).location.X - _location.X) < pbox.X + obj.pbox.X)) {
                        // at this point the player is colliding with this obj

						//figure out which direction the collision happened on by looking for point where
						//only one axis is not colliding
						Vector3 temploc = _location - velocity * (float)e.Time;
						float originalX = temploc.X; //save the original x position for calculating camera offset
						Vector3 step = velocity * (float)e.Time * 0.25f;
						bool x = Math.Abs(((GameObject)obj).location.X - temploc.X) <= pbox.X + obj.pbox.X;
						bool y = Math.Abs(((GameObject)obj).location.Y - temploc.Y) <= pbox.Y + obj.pbox.Y;
						bool lastStepWasForward = true;
						while(x == y) {
							if(!x) { //both false - not far enough, step forward
								if(!lastStepWasForward) {
									step *= 0.5f;
								}
								lastStepWasForward = true;
								temploc += step;
								x = Math.Abs(((GameObject)obj).location.X - temploc.X) <= pbox.X + obj.pbox.X;
								y = Math.Abs(((GameObject)obj).location.Y - temploc.Y) <= pbox.Y + obj.pbox.Y;
							} else { //both true - too far, step backwards
								if(lastStepWasForward) {
									step *= 0.5f;
								}
								lastStepWasForward = false;
								temploc -= step;
								x = Math.Abs(((GameObject)obj).location.X - temploc.X) <= pbox.X + obj.pbox.X;
								y = Math.Abs(((GameObject)obj).location.Y - temploc.Y) <= pbox.Y + obj.pbox.Y;
							}
						}

						//We're now at a point that two axes intersect - the third is the one that collided
						if(!x) {
							velocity.X = 0;
							if(_location.X < ((GameObject)obj).location.X) {
								_location.X = ((GameObject)obj).location.X - (pbox.X + obj.pbox.X);
							} else {
								_location.X = ((GameObject)obj).location.X + pbox.X + obj.pbox.X;
							}
							deltax = _location.X - originalX; //update this value for camera offset
						} else { //y
							velocity.Y = 0;
							if(_location.Y < ((GameObject)obj).location.Y) {
								_location.Y = ((GameObject)obj).location.Y - (pbox.Y + obj.pbox.Y);
							} else {
								_location.Y = ((GameObject)obj).location.Y + pbox.Y + obj.pbox.Y;
							}
						}
					}
				}
			}
		}

		public void accelerate(Vector3 acceleration) {
			accel += acceleration;
		}

		private float _health;
		public float health {
			get { return _health; }
			set { _health = value; }
		}

		private float _damage;
		public float damage {
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
