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
        private Vector3 velocity;
        private Vector3 accel;
        private bool doesGravity; //true if gravity affects this object

		public Enemy(Vector3 location, Vector3 scale, bool existsIn2d, bool existsIn3d, int health, int damage, float speed, int AItype, ObjMesh mesh, Bitmap texture) {
			_location = location;
            _scale = scale;
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
			_frameNum = 0;
			_is3dGeo = true;
            texID = GL.GenTexture();

            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = true;
		}


        public Enemy(Vector3 location, Vector3 scale, bool existsIn2d, bool existsIn3d, int health, int damage, float speed, int AItype, SpriteSheet sprite)
        {
			_location = location;
			_scale = scale;
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
		bool RenderObject.is3dGeo {
			get { return _is3dGeo; }
		}

		private ObjMesh _mesh; //null for sprites
		ObjMesh RenderObject.mesh {
			get { return _mesh; }
		}

		private Bitmap _texture; //null for sprites
		Bitmap RenderObject.texture {
			get { return _texture; }
		}

		private SpriteSheet _sprite; //null for 3d objects
		SpriteSheet RenderObject.sprite {
			get { return _sprite; }
		}

        private Vector3 _scale;
        Vector3 RenderObject.scale
        {
            get { return _scale; }
        }

		private int _frameNum; //index of the current animation frame
		int RenderObject.frameNumber {
			get { return _frameNum; }
			set { _frameNum = value; }
		}

		bool RenderObject.isAnimated() {
			throw new Exception("The method or operation is not implemented.");
		}

		void RenderObject.doScaleTranslateAndTexture() {
            GL.PushMatrix();

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

		void PhysicsObject.physUpdate(FrameEventArgs e, List<PhysicsObject> objlist) {
			
            //TODO: impliment gravity, colisions etc...



		}

		void AIObject.aiUpdate(FrameEventArgs e, Vector3 playerposn) {
            if (AItype == 1) {





            }

		}

		void CombatObject.reset() {
			throw new NotImplementedException();
		}

        public void accelerate(Vector3 acceleration) {
            accel += acceleration;
        }

        private float _health;
        float CombatObject.health {
            get { return _health; }
            set { _health = value; }
        }

        private float _damage;
        float CombatObject.damage {
            get { return _damage; }
        }

        private float _speed;
        float CombatObject.speed {
            get { return _speed; }
            set { _speed = value; }
        }

        private bool _alive;
        bool CombatObject.alive {
            get { return _alive; }
            set { _alive = value; }
        }
	}
}
