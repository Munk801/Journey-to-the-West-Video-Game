using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using Engine.Input;
using OpenTK.Input;

// XML parser
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace U5Designs
{
    /** Main State of the game that will be active while the player is Playing **/
    class PlayState : GameState
    {
        internal GameEngine eng;
        internal Player player;
        internal bool enable3d; //true when being viewed in 3d
		internal bool switchingPerspective; //true when perspective is in process of switching

		//everything is in objList, and then also pointed to from the appropriate interface lists
        internal List<GameObject> objList;
		internal List<RenderObject> renderList;
        internal List<PhysicsObject> colisionList; // colision list = only things that are moving that need to detect colisions
		internal List<PhysicsObject> physList; // physList is a list of everything that has a bounding box
		internal List<AIObject> aiList;// aka list of enemies
		internal List<CombatObject> combatList; // list of stuff that effects the player in combat, projectiles, enemies
        //TODO: projectile list

        int current_level = -1;// Member variable that will keep track of the current level being played.  This be used to load the correct data from the backends.

		bool tabDown;

		protected Vector3 eye, lookat;
		protected Vector4 lightPos;

        MainMenuState menustate;

       

        // Initialize graphics, etc here
        public PlayState(MainMenuState prvstate, GameEngine engine, int lvl) {

			//TODO: pass this the right file to load from
			// undo this when done testing ObjList = LoadLevel.Load(current_level);
			LoadLevel.Load(0, this);

            //AudioContext ac = new AudioContext();
            //XRamExtension xram = new XRamExtension();

            menustate = prvstate;
            eng = engine;
            player = new Player();

            enable3d = false;
			tabDown = false;
            //test.Play();

            //AudioManager.Manager.StartAudioServices();

			lookat = new Vector3(100, 75, 50);
			eye = lookat + new Vector3(0, 0, 100);
			lightPos = new Vector4(50, 50, 0, 1);
			lightPos.X += eye.X;
			lightPos.Y += eye.Y;
			lightPos.Z += eye.Z;

			GL.MatrixMode(MatrixMode.Projection);
			Matrix4 projection = Matrix4.CreateOrthographic(eng.ClientRectangle.Width/4, eng.ClientRectangle.Height/4, 1.0f, 6400.0f);
			GL.LoadMatrix(ref projection);
        }

		public override void MakeActive() {
			GL.Enable(EnableCap.Lighting);
			GL.Enable(EnableCap.Light0);

			GL.MatrixMode(MatrixMode.Projection);
			if(enable3d) {
				Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 6, eng.ClientRectangle.Width / (float)eng.ClientRectangle.Height, 1.0f, 6400.0f);
				GL.LoadMatrix(ref projection);
				GL.Enable(EnableCap.Fog);
			} else { //2d
				Matrix4 projection = Matrix4.CreateOrthographic(eng.ClientRectangle.Width / 4, eng.ClientRectangle.Height / 4, 1.0f, 6400.0f);
				GL.LoadMatrix(ref projection);
				GL.Disable(EnableCap.Fog);
			}
		}

        public override void Update(FrameEventArgs e)
        {
            DealWithInput();
			player.updateState(enable3d, eng.Keyboard[Key.A], eng.Keyboard[Key.S], eng.Keyboard[Key.D], eng.Keyboard[Key.W], eng.Keyboard[Key.Space], e);
			
			updateView(); 

			//TODO: parallax background based on player movement

			player.physUpdate(e, physList); //TODO: Should player be first or last?
             foreach (PhysicsObject po in colisionList) {
 				po.physUpdate(e, physList);
             }

             foreach (AIObject aio in aiList) {
                 aio.aiUpdate(e, player.location, enable3d);
             }

        }

        double i = 0;
        public override void Draw(FrameEventArgs e)
        {
            //Origin is the left edge of the level, at the ground and the back wall
            //This means that all valid game coordinates will be positive
            //Ground is from 0 to 100 along the z-axis

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            //TODO: Do lights and fog need to happen every frame?
            //Light
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Light(LightName.Light0, LightParameter.Position, lightPos);
            GL.Light(LightName.Light0, LightParameter.Diffuse, new Vector4(1.0f, 1.0f, 1.0f, 1.0f) );
			GL.Light(LightName.Light0, LightParameter.Specular, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, noglow);

            //Fog
			if(enable3d) {
				GL.Fog(FogParameter.FogDensity, 0.0005f);
			}

            //Set up for rendering using arrays
            GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.NormalArray);
			GL.EnableClientState(ArrayCap.TextureCoordArray);

			//Set up textures
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

            
			foreach(RenderObject obj in renderList) {
				if(obj.is3dGeo) {
 					obj.doScaleTranslateAndTexture();
 					obj.mesh.Render();
				} else {
					GL.PushMatrix();
					GL.Translate(0, 50, 0);
					GL.Scale(50, 50, 1);
					i += e.Time*4;
					if(obj.sprite.draw(0, (int)i) == 0 && i > 1f) {
						i = 0;
					}
				}
			}
			//Console.WriteLine(i);

			player.draw();
        }

        private void DealWithInput()
        {
            //TODO: Change these keys to their final mappings when determined
            if (eng.Keyboard[Key.Escape])
            {
                eng.PushState(menustate);
            }

			//********************** tab
            if (eng.Keyboard[Key.Tab] && !tabDown)
            {
                enable3d = !enable3d;
				switchingPerspective = true;
                tabDown = true;
                player.velocity.Z = 0;
            }
            else if (!eng.Keyboard[Key.Tab])
            {
                tabDown = false;
            }
        }

        protected float[] noglow = { 0.0f, 0.0f, 0.0f, 1.0f };
/*        protected float[] lightPos = { 25.0f, 50.0f, 250.0f };*/
/*	      protected float[] whitelight = { 0.5f, 0.5f, 0.5f, 0.5f };*/


        /// <summary>Updates the projection matrix for the current view (2D/3D)</summary>
		public void updateView() {
			eye.X += player.deltax;
			lookat.X += player.deltax;
			lightPos.X += player.deltax;

            //TODO: Animate view transition
			if(switchingPerspective) {
				GL.MatrixMode(MatrixMode.Projection);
				if(enable3d) {
					Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 6, eng.ClientRectangle.Width / (float)eng.ClientRectangle.Height, 1.0f, 6400.0f);
					GL.LoadMatrix(ref projection);
					//eye.X = lookat.X - 500;
					//eye.Y = lookat.Y + 75;
					//eye.Z = lookat.Z;
					//TODO: Make these constants into #defines
					//TODO: Make these constants resolution-independent
					lookat.X -= 100;
					lookat.Y -= 50;
					eye.X = lookat.X - 120;
					eye.Y = lookat.Y + 25;
					eye.Z = lookat.Z;
					lightPos = new Vector4(0, 50, 50, 1);
					lightPos.X += eye.X;
					lightPos.Y += eye.Y;
					lightPos.Z += eye.Z;
					GL.Enable(EnableCap.Fog);
				} else { //2d
					Matrix4 projection = Matrix4.CreateOrthographic(eng.ClientRectangle.Width/4, eng.ClientRectangle.Height/4, 1.0f, 6400.0f);
					GL.LoadMatrix(ref projection);
					//TODO: Make these constants into #defines
					//TODO: Make these constants resolution-independent
					lookat.X += 100;
					lookat.Y += 50;
					eye.X = lookat.X;
					eye.Y = lookat.Y;
					eye.Z = lookat.Z + 100;
					lightPos = new Vector4(50, 50, 0, 1);
					lightPos.X += eye.X;
					lightPos.Y += eye.Y;
					lightPos.Z += eye.Z;
					GL.Disable(EnableCap.Fog);
				}
				switchingPerspective = false;
			}
        }

        /**
         * Change the current level being played to the parameter
         * */
        public void changeCurrentLevel(int l)
        {
            current_level = l;
        }      
    }
}
