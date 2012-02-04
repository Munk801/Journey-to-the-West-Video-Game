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
        int id;

        public Player()
        {
            p_state = new PlayerState("TEST player");
            p_state.setSpeed(3);
            _location = new Vector3(50, 50f, 20f);
            _scale = new Vector3(5, 5, 5);
            cubemesh = new ObjMesh("../../Geometry/box.obj");
            _texture = new Bitmap("test.png");
            id = GL.GenTexture();
        }

        /**
         * Sets the PlayerState elements to the current Player values.  Call this method every update or simply when the state changes.  This will be used to store
         * the Players State when saving the game.
         * */
        public void updateState(bool enable3d, bool a, bool s, bool d, bool w)
        {
            //TODO: add control for other buttons, jump, projectile etc
            if (enable3d)
            {
                if (w)
                    _location += (Vector3.UnitX *(float)p_state.getSpeed());
                if (s)
                    _location -= (Vector3.UnitX * (float)p_state.getSpeed());
                if (d)
                    _location += (Vector3.UnitZ * (float)p_state.getSpeed());
                if (a)
                    _location -= (Vector3.UnitZ * (float)p_state.getSpeed());
            }
            else
            {
                if (d)
                    _location += (Vector3.UnitX * (float)p_state.getSpeed());
                if (a)
                    _location -= (Vector3.UnitX * (float)p_state.getSpeed());
            }


        }


        protected float[] redAmbient = { 0.4f, 0.0f, 0.0f, 1.0f };
        protected float[] redDiffuse = { 0.4f, 0.0f, 0.0f, 1.0f };
        protected float[] redSpecular = { 1.0f, 1.0f, 1.0f, 1.0f };
        protected float[] redShininess = { 0.5f };
        protected byte[] cubeIndices = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };

        public void draw()
        {
            //Test cube
 /*            GL.PushMatrix();
             GL.Translate(x, y, z);
             GL.Scale(12.5f, 12.5f, 12.5f);
             GL.Material(MaterialFace.Front, MaterialParameter.Specular, redSpecular);
             GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, redDiffuse);
             GL.Material(MaterialFace.Front, MaterialParameter.Ambient, redAmbient);
             GL.Material(MaterialFace.Front, MaterialParameter.Shininess, redShininess);
             GL.DrawElements(BeginMode.Quads, 24, DrawElementsType.UnsignedByte, cubeIndices);
             GL.PopMatrix();
            */
            GL.PushMatrix();
            GL.BindTexture(TextureTarget.Texture2D, id);

            BitmapData bmp_data = _texture.LockBits(new Rectangle(0, 0, _texture.Width, _texture.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            _texture.UnlockBits(bmp_data);

            // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.Translate(_location);
            GL.Scale(_scale);

            cubemesh.Render();



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

        void RenderObject.doScaleTranslateAndTexture()
        {

        }

		void PhysicsObject.physUpdate(FrameEventArgs e, List<PhysicsObject> objlist) {
			throw new Exception("The method or operation is not implemented.");
		}

		void PhysicsObject.accelerate(double acceleration) {
			throw new Exception("The method or operation is not implemented.");
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

		private bool _alive;
		bool CombatObject.alive {
			get { return _alive; }
			set { _alive = value; }
		}

		//TODO: Don't know if reset really applies to player or not...
		void CombatObject.reset() {
			throw new NotImplementedException();
		}
	}
}
