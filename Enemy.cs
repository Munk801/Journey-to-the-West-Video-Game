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
    class Enemy : GameObject, AIObject, RenderObject, PhysicsObject, CombatObject
    {

        private int texID;
        private int AItype;
        internal Vector3 velocity;
        internal Vector3 accel;
        private bool doesGravity; //true if gravity affects this object

		public Enemy(Vector3 location, Vector3 scale, Vector3 pbox, Vector3 cbox, bool existsIn2d, bool existsIn3d, int health, int damage, float speed, int AItype, ObjMesh mesh, Bitmap texture) {
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

			_mesh = mesh;
			_texture = texture;
			_sprite = null;
			_cycleNum = 0;
			_frameNum = 0;
			_is3dGeo = true;
            texID = GL.GenTexture();

            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = true;
		}


        public Enemy(Vector3 location, Vector3 scale, Vector3 pbox, Vector3 cbox, bool existsIn2d, bool existsIn3d, int health, int damage, float speed, int AItype, SpriteSheet sprite)
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

			_mesh = null;
			_texture = null;
			_sprite = sprite;
			_frameNum = 0;
			_is3dGeo = false;
			texID = GL.GenTexture();

            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = true;
		}


        /** Like the Player Status update call this every time you need to update an Enemies State before saving **/
        public void updateState()
        {
            // Add in the other State elements that will need to be maintained here..
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

/*            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData bmp_data = _texture.LockBits(new Rectangle(0, 0, _texture.Width, _texture.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            _texture.UnlockBits(bmp_data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
*/
            GL.Translate(_location);
            GL.Scale(_scale);
		}

		public void physUpdate3d(FrameEventArgs e, List<PhysicsObject> objlist) {
			if (doesGravity)
            {
                accel.Y -= (float)(400 * e.Time); //TODO: turn this into a constant somewhere
            }
            velocity += accel;
            accel.X = 0;
            accel.Y = 0;
            accel.Z = 0;
            _location += velocity * (float)e.Time;


			foreach(PhysicsObject obj in objlist) {
				// don't do collision physics to yourself
				if(obj != this) {

					if((Math.Abs(((GameObject)obj).location.Y - _location.Y) < pbox.Y + obj.pbox.Y)
					&& (Math.Abs(((GameObject)obj).location.Z - _location.Z) < pbox.Z + obj.pbox.Z)
					&& (Math.Abs(((GameObject)obj).location.X - _location.X) < pbox.X + obj.pbox.X)) {
						// at this point this enemy is colliding with this obj

						//figure out which direction the collision happened on by looking for point where
						//only one axis is not colliding
						Vector3 temploc = _location - velocity * (float)e.Time;
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

			foreach(PhysicsObject obj in objlist) {
				// don't do collision physics to yourself
				if(obj != this) {

					if((Math.Abs(((GameObject)obj).location.Y - _location.Y) < pbox.Y + obj.pbox.Y)
					&& (Math.Abs(((GameObject)obj).location.X - _location.X) < pbox.X + obj.pbox.X)) {
						// at this point obj is in collision with this enemy

						//figure out which direction the collision happened on by looking for point where
						//only one axis is not colliding
						Vector3 temploc = _location - velocity * (float)e.Time;
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

		public void reset() {
			throw new NotImplementedException();
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


		public void aiUpdate(FrameEventArgs e, Vector3 playerposn, bool enable3d) {
            if (AItype == 1) {
                //TODO change 2d view to only deal with 2d position vectors, so z movement doesnt happen in 2d
                if (dist(playerposn, _location) > 30) {
                    Vector3 dir = getdir(playerposn, _location);
                    velocity.X = dir.X * _speed;
                    if (enable3d)
                        velocity.Z = dir.Z * _speed;
                }
                else {
                    velocity.X = 0;
                    velocity.Z = 0;
                }



            }
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
