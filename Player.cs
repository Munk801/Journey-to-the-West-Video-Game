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
        ObjMesh cubemesh;
        int texID;
		public Vector3 velocity;
		private Vector3 accel;
		private bool doesGravity; //true if gravity affects this object

		public float deltax; //used for updating position of camera, etc.

        //tmp
      //  System.Media.SoundPlayer k = new System.Media.SoundPlayer("../../Resources/Sound/jump_sound.wav");


        // SOUND FILES
        static string jumpSoundFile = "../../Resources/Sound/jump_sound.ogg";
        AudioFile jumpSound = new AudioFile(jumpSoundFile);

        public Player()
        {
            p_state = new PlayerState("TEST player");
            p_state.setSpeed(300);
            _location = new Vector3(50, 5f, 50f);
            _scale = new Vector3(5, 5, 5);
            cubemesh = new ObjMesh("../../Geometry/box.obj");
            _texture = new Bitmap("../../Textures/player.png");
            _damage = 0;
            texID = GL.GenTexture();
			velocity = new Vector3(0, 0, 0);
			accel = new Vector3(0, 0, 0);
			doesGravity = true;
        }

        /**
         * Sets the PlayerState elements to the current Player values.  Call this method every update or simply when the state changes.  This will be used to store
         * the Players State when saving the game.
		 * 
		 * Returns the movement of the player to be used in updating camera, etc.
         * */
        bool spaceDown;
        public void updateState(bool enable3d, bool a, bool s, bool d, bool w, bool space, FrameEventArgs e) {
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
                    velocity.X = (float)p_state.getSpeed()/2;
                    velocity.Z = (float)p_state.getSpeed()/2;
                }
                if (w && a) {
                    velocity.X = (float)p_state.getSpeed() / 2;
                    velocity.Z = -((float)p_state.getSpeed() / 2);
                }
                if (a && s) {
                    velocity.X = -((float)p_state.getSpeed() / 2);
                    velocity.Z = -((float)p_state.getSpeed() / 2);
                }
                if (d && s) {
                    velocity.X =-((float)p_state.getSpeed() / 2);
                    velocity.Z = ((float)p_state.getSpeed() / 2);
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

            //********************** space
            if (space && !spaceDown)
            {
                if (velocity.Y < 0.000001f && velocity.Y > -0.0000001f)
                {
                    accelerate(Vector3.UnitY * 230);
                   // k.Play();
                }
                spaceDown = true;
                jumpSound.Play();
                
            }
            else if (!space)
            {
                spaceDown = false;
            }
        }

        public void draw()
        {
			doScaleTranslateAndTexture();
            cubemesh.Render();
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

		private int _frameNum; //index of the current animation frame
		public int frameNumber {
			get { return _frameNum; }
			set { _frameNum = value; }
		}

		public bool isAnimated() {
			throw new Exception("The method or operation is not implemented.");
		}

        public void doScaleTranslateAndTexture() {
			GL.PushMatrix();

			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
			GL.BindTexture(TextureTarget.Texture2D, texID);
			BitmapData bmp_data = _texture.LockBits(new Rectangle(0, 0, _texture.Width, _texture.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
			_texture.UnlockBits(bmp_data);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			GL.Translate(_location);
			GL.Scale(_scale);
        }

		public void physUpdate(FrameEventArgs e, List<PhysicsObject> objlist) {
			if(doesGravity && _location.Y != 0) {
				accel.Y -= (float)(400*e.Time); //TODO: turn this into a constant somewhere
			}
			velocity += accel;
			accel.X = 0;
			accel.Y = 0;
			accel.Z = 0;
			_location += velocity*(float)e.Time;
			if(_location.Y-5 <= 0) {
				_location.Y = 5;
				velocity.Y = 0;
				accel.Y = 0;
			}
			deltax = (velocity * (float)e.Time).X;
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
        float CombatObject.speed {
            get { return _speed; }
            set { _speed = value; }
        }

		private bool _alive;
		public bool alive {
			get { return _alive; }
			set { _alive = value; }
		}

        public Vector3 location {
            get { return _location; }
        }

		//TODO: Don't know if reset really applies to player or not...
		public void reset() {
			throw new NotImplementedException();
		}
	}
}
