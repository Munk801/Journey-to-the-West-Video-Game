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
        internal bool enable3d;

		//everything is in objList, and then also pointed to from the appropriate interface lists
        //internal List<GameObject> objList;
		internal List<RenderObject> renderList;
		internal List<PhysicsObject> physList;
		internal List<AIObject> aiList;// aka list of enemies
		internal List<CombatObject> combatList; // list of stuff that effects the player in combat, projectiles, enemies
        //TODO: projectile list

        int current_level = -1;// Member variable that will keep track of the current level being played.  This be used to load the correct data from the backends.

        //static string testFile = "Retribution.ogg";
        //AudioFile test = new AudioFile(testFile);

        // Initialize graphics, etc here
        public PlayState(GameEngine engine, int lvl)
        {

            //AudioManager.Manager.StartAudioServices();
            eng = engine;
            player = new Player();

            enable3d = false;

            //eye = new Vector3(0, 50, 250);
            //lookat = new Vector3(0, 50, 200);
            eye = new Vector3(0, 0, 0); //gets initialized in updateView
            lookat = new Vector3(0, 50, 200);


            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Normalize);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            //test.Play();
            updateView();

            //AudioManager.Manager.StartAudioServices();

            // Testing...remove when done //

            //TODO: pass this the right file to load from
            // undo this when done testing ObjList = LoadLevel.Load(current_level);
            LoadLevel.Load(0, this);
        }

        public override void Update(FrameEventArgs e)
        {
            DealWithInput();
            player.updateState(enable3d, a, s, d, w);
            //TODO: deal with physics list

        }

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
            GL.Light(LightName.Light0, LightParameter.Diffuse, whitelight);
            GL.Light(LightName.Light0, LightParameter.Specular, whitelight);
            GL.Light(LightName.Light0, LightParameter.Ambient, whitelight);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, noglow);

            //Fog
            GL.Fog(FogParameter.FogDensity, 0.0005f);

            //Set up for rendering using arrays
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

			foreach(RenderObject obj in renderList) {

				// 				GL.Material(MaterialFace.Front, MaterialParameter.Specular, groundSpecular);
				// 				GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, groundDiffuse);
				// 				GL.Material(MaterialFace.Front, MaterialParameter.Ambient, groundAmbient);
				// 				GL.Material(MaterialFace.Front, MaterialParameter.Shininess, groundShininess);
                obj.doScaleTranslateAndTexture(); //TODO FIX. BROKE.
				if(obj.is3dGeo) {
					obj.mesh.Render();
				} else {
					obj.sprite.draw(0, 0); //change later. must call pop
				}
			}

            //Ground
//             GL.VertexPointer(3, VertexPointerType.Float, 0, cubeVertices);
//             GL.NormalPointer(NormalPointerType.Byte, 0, cubeNormals);
//             GL.PushMatrix();
//             GL.Scale(5000.0f, 500.0f, 200.0f);
//             GL.Translate(0.0f, -1.0f, 1.0f);
//             GL.Material(MaterialFace.Front, MaterialParameter.Specular, groundSpecular);
//             GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, groundDiffuse);
//             GL.Material(MaterialFace.Front, MaterialParameter.Ambient, groundAmbient);
//             GL.Material(MaterialFace.Front, MaterialParameter.Shininess, groundShininess);
//             GL.DrawElements(BeginMode.Quads, 24, DrawElementsType.UnsignedByte, cubeIndices);
// 			GL.PopMatrix();
			player.draw();
        }

        bool spaceDown, a, s, d, w;
        //if its inconvenent to have key detection outside of the update method, move it back in
        private void DealWithInput()
        {
            //TODO: Change these keys to their final mappings when determined

            if (eng.Keyboard[Key.Escape])
            {
                MainMenuState ms = new MainMenuState(eng);
                eng.PushState(ms);
            }
            //********************** space
            if (eng.Keyboard[Key.Space] && !spaceDown)
            {
                enable3d = !enable3d;
                updateView();
                spaceDown = true;
            }
            else if (!eng.Keyboard[Key.Space])
            {
                spaceDown = false;
            }

            //********************** a
            if (eng.Keyboard[Key.A] && !a)
            {
                a = true;
            }
            else if (!eng.Keyboard[Key.A])
            {
                a = false;
            }

            //*********************** s
            if (eng.Keyboard[Key.S] && !s)
            {
                s = true;
            }
            else if (!eng.Keyboard[Key.S])
            {
                s = false;
            }
            //********************** d
            if (eng.Keyboard[Key.D] && !d)
            {
                d = true;
            }
            else if (!eng.Keyboard[Key.D])
            {
                d = false;
            }
            //********************** w
            if (eng.Keyboard[Key.W] && !w)
            {
                w = true;
            }
            else if (!eng.Keyboard[Key.W])
            {
                w = false;
            }
        }


        protected Vector3 eye, lookat, forward, right;

        protected float[,] cubeVertices = new[,] {{1.0f, -1.0f, 1.0f},   {1.0f, -1.0f, -1.0f},  {-1.0f, -1.0f, -1.0f}, {-1.0f, -1.0f, 1.0f},
							                    {1.0f, 1.0f, -1.0f},   {1.0f, -1.0f, -1.0f},  {1.0f, -1.0f, 1.0f},   {1.0f, 1.0f, 1.0f},
							                    {1.0f, -1.0f, -1.0f},  {1.0f, 1.0f, -1.0f},   {-1.0f, 1.0f, -1.0f},  {-1.0f, -1.0f, -1.0f},
							                    {-1.0f, -1.0f, -1.0f}, {-1.0f, 1.0f, -1.0f},  {-1.0f, 1.0f, 1.0f},   {-1.0f, -1.0f, 1.0f},
							                    {-1.0f, 1.0f, 1.0f},   {-1.0f, 1.0f, -1.0f},  {1.0f, 1.0f, -1.0f},   {1.0f, 1.0f, 1.0f},
							                    {1.0f, -1.0f, 1.0f},   {-1.0f, -1.0f, 1.0f},  {-1.0f, 1.0f, 1.0f},   {1.0f, 1.0f, 1.0f}};

        protected float[,] cubeNormals = new[,] {{0.0f, -1.0f, 0.0f}, {0.0f, -1.0f, 0.0f}, {0.0f, -1.0f, 0.0f}, {0.0f, -1.0f, 0.0f},
							                   {1.0f, 0.0f, 0.0f},  {1.0f, 0.0f, 0.0f},  {1.0f, 0.0f, 0.0f},  {1.0f, 0.0f, 0.0f},
							                   {0.0f, 0.0f, -1.0f}, {0.0f, 0.0f, -1.0f}, {0.0f, 0.0f, -1.0f}, {0.0f, 0.0f, -1.0f},
							                   {-1.0f, 0.0f, 0.0f}, {-1.0f, 0.0f, 0.0f}, {-1.0f, 0.0f, 0.0f}, {-1.0f, 0.0f, 0.0f},
							                   {0.0f, 1.0f, 0.0f},  {0.0f, 1.0f, 0.0f},  {0.0f, 1.0f, 0.0f},  {0.0f, 1.0f, 0.0f},
							                   {0.0f, 0.0f, 1.0f},  {0.0f, 0.0f, 1.0f},  {0.0f, 0.0f, 1.0f},  {0.0f, 0.0f, 1.0f}};

        protected byte[] cubeIndices = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };

        protected float[] groundAmbient = { 0.0215f, 0.1745f, 0.0215f }; //{0.40f, 0.53f, 0.13f, 1.0f};
        protected float[] groundDiffuse = { 0.07568f, 0.61424f, 0.07568f }; //{0.5f, 0.5f, 0.5f, 1.0f};
        protected float[] groundSpecular = { 0.633f, 0.727811f, 0.633f }; //{0.0f, 0.0f, 0.0f, 1.0f};
        protected float[] groundShininess = { 76.8f }; //{0.0f};


        protected float[] noglow = { 0.0f, 0.0f, 0.0f, 1.0f };
        protected float[] lightPos = { 25.0f, 50.0f, 250.0f };
        protected float[] whitelight = { 0.5f, 0.5f, 0.5f, 0.5f };


        /// <summary>Updates the projection matrix for the current view (2D/3D)</summary>
        public override void updateView()
        {
            //TODO: Animate view transition
            GL.MatrixMode(MatrixMode.Projection);
            if (enable3d)
            {
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 6, eng.ClientRectangle.Width / (float)eng.ClientRectangle.Height, 1.0f, 6400.0f);
                GL.LoadMatrix(ref projection);
                //eye.X = lookat.X - 500;
                //eye.Y = lookat.Y + 75;
                //eye.Z = lookat.Z;
                //TODO: Make these constants into #defines
                //TODO: Make these constants resolution-independent
                lookat.X -= 400;
                lookat.Y -= 200;
                eye.X = lookat.X - 500;
                eye.Y = lookat.Y + 75;
                eye.Z = lookat.Z;
                GL.Enable(EnableCap.Fog);
            }
            else
            { //2d
                Matrix4 projection = Matrix4.CreateOrthographic(eng.ClientRectangle.Width, eng.ClientRectangle.Height, 1.0f, 6400.0f);
                GL.LoadMatrix(ref projection);
                //TODO: Make these constants into #defines
                //TODO: Make these constants resolution-independent
                lookat.X += 400;
                lookat.Y += 200;
                eye.X = lookat.X;
                eye.Y = lookat.Y;
                eye.Z = lookat.Z + 200;
                GL.Disable(EnableCap.Fog);
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
