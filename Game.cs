// Copyright 2012 U5 Designs

using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

namespace U5Designs
{
    class Game : GameWindow
    {
		internal bool enable3d;
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

		protected float[] groundAmbient = {0.0215f, 0.1745f, 0.0215f}; //{0.40f, 0.53f, 0.13f, 1.0f};
		protected float[] groundDiffuse = {0.07568f, 0.61424f, 0.07568f}; //{0.5f, 0.5f, 0.5f, 1.0f};
        protected float[] groundSpecular = {0.633f, 0.727811f, 0.633f}; //{0.0f, 0.0f, 0.0f, 1.0f};
        protected float[] groundShininess = {76.8f}; //{0.0f};

		protected float[] redAmbient = {0.4f, 0.0f, 0.0f, 1.0f};
		protected float[] redDiffuse = {0.4f, 0.0f, 0.0f, 1.0f};
		protected float[] redSpecular = {1.0f, 1.0f, 1.0f, 1.0f};
		protected float[] redShininess = {0.5f};

        protected float[] noglow = {0.0f, 0.0f, 0.0f, 1.0f};
        protected float[] lightPos = {25.0f, 50.0f, 250.0f};
        protected float[] whitelight = {0.5f, 0.5f, 0.5f, 0.5f};

        //game logic class
        internal Logic logic;

        /// <summary>Creates a 1280x720 window.</summary>
        //TODO: Change this to a dynamic screen resolution
        public Game()
            : base(1280, 720, GraphicsMode.Default, "U5 Designs Untitled Project") {
            VSync = VSyncMode.On;
            
        }

        /// <summary>Load resources here. This gets called ONCE</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            logic = new Logic(this);
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
        }

        /// <summary>Updates the projection matrix for the current view (2D/3D)</summary>
        internal void updateView() {
			//TODO: Animate view transition
            GL.MatrixMode(MatrixMode.Projection);
            if (enable3d) {
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 6, ClientRectangle.Width / (float)ClientRectangle.Height, 1.0f, 6400.0f);
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
            } else { //2d
                Matrix4 projection = Matrix4.CreateOrthographic(ClientRectangle.Width, ClientRectangle.Height, 1.0f, 6400.0f);
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

        /// <summary>
        /// Called once per frame.  Contains game logic, etc.
        /// Unless something specifically needs to be here, it belongs in the Logic class.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);
            logic.update(e);
        }

        /// <summary>
        /// Renders the current frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

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

            //Ground
			GL.VertexPointer(3, VertexPointerType.Float, 0, cubeVertices);
			GL.NormalPointer(NormalPointerType.Byte, 0, cubeNormals);
            GL.PushMatrix();
            GL.Scale(5000.0f, 500.0f, 200.0f);
            GL.Translate(0.0f, -1.0f, 1.0f);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, groundSpecular);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, groundDiffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, groundAmbient);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, groundShininess);
            GL.DrawElements(BeginMode.Quads, 24, DrawElementsType.UnsignedByte, cubeIndices);
            GL.PopMatrix();

			//Test cube
			GL.PushMatrix();
			GL.Translate(50.0f, 12.5f, 50.0f);
			GL.Scale(12.5f, 12.5f, 12.5f);
			GL.Material(MaterialFace.Front, MaterialParameter.Specular, redSpecular);
			GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, redDiffuse);
			GL.Material(MaterialFace.Front, MaterialParameter.Ambient, redAmbient);
			GL.Material(MaterialFace.Front, MaterialParameter.Shininess, redShininess);
			GL.DrawElements(BeginMode.Quads, 24, DrawElementsType.UnsignedByte, cubeIndices);
			GL.PopMatrix();

            SwapBuffers();
        }


        /// <summary>
        /// Called when the window is resized. Sets viewport and updates projection matrix.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            updateView();
        }

    }
}