using System;
using System.Collections.Generic;

using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using System.Reflection;

namespace U5Designs
{
    public class GameEngine : GameWindow
    {
        /**
         * Only the current state can know when it’s time to change to the next state.
         * HandleEvents()
         * Update()
         * Draw()
         * will call the corresponding methods in the State class that is on the "stack" List<>
         * */

        Stack<GameState> states; // This is the "Stack" of states with an LIFO structure mimicking an actual memory Stack
        internal bool GameInProgress; // this bool tracks if a game is in progress, mostly for the menu state to know if its the first menu, or has been brought up ingame
        internal TextureManager StateTextureManager;
        internal GameMouse ThisMouse;
        internal AudioFile selectSound;
        internal Assembly assembly;
		internal int xOffset, yOffset; //Used for interpreting mouse coordinates when letterboxing

        /// <summary>Creates a 1280x720 window.</summary>
        public GameEngine() : base(1280, 720, GraphicsMode.Default, "Journey to the East") {
            VSync = VSyncMode.On;
			this.WindowState = WindowState.Fullscreen;
			this.WindowBorder = WindowBorder.Hidden;

			this.CursorVisible = false;

			xOffset = 0;
			yOffset = 0;
        }

        /// <summary>Load resources here. This gets called ONCE at the start of the entire process(not once a state)</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Normalize);
			GL.Enable(EnableCap.Texture2D);
            StateTextureManager = new TextureManager();

            // Load up the 4 images that will be displayed in sequence giving the illusion of animation
            StateTextureManager.RenderSetup();

            assembly = Assembly.GetExecutingAssembly();

            // Splash Screen
            StateTextureManager.LoadTexture("logo", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.u5_logo.jpg"));
            // Game over State
            StateTextureManager.LoadTexture("game_over", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.game_over_text.png"));
            //StateTextureManager.LoadTexture("restart", "../../Resources/Textures/restart_button.png");
            //StateTextureManager.LoadTexture("quit_button", "../../Resources/Textures/go_quit_button.png");

            // Pause State
            StateTextureManager.LoadTexture("p1", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.PauseTextures.p1.png"));
            StateTextureManager.LoadTexture("p2", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.PauseTextures.p2.png"));
            StateTextureManager.LoadTexture("p3", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.PauseTextures.p3.png"));
			StateTextureManager.LoadTexture("p4", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.PauseTextures.p4.png"));

			// Load State
			for(int i = 1; i <= 8; i++) {
				StateTextureManager.LoadTexture("load" + i, assembly.GetManifestResourceStream("U5Designs.Resources.Textures.LoadingScreenTextures.load" + i + ".png"));
			}
        
            ThisMouse = new GameMouse(this);
            states = new Stack<GameState>();

            //this.ChangeState(ms);

            //MainMenuState ms = new MainMenuState(this);
            //this.ChangeState(ms);
            SplashScreenState ss = new SplashScreenState(this);
            this.ChangeState(ss);

            selectSound = new AudioFile(assembly.GetManifestResourceStream("U5Designs.Resources.Sound.select.ogg"));

            // Set the title bar of the window etc


			//Initialize Shader
			//Thanks to OpenTK samples for part of this shader code
			int shaderProgram = GL.CreateProgram();
			int frag = GL.CreateShader(ShaderType.FragmentShader);

			// GLSL for fragment shader.
			String fragSource = @"
				uniform sampler2D tex;

				void main( void )
				{
					vec4 col = texture2D(tex,gl_TexCoord[0].st);
					if( col.a < 0.5) {
						discard;
					}
					gl_FragColor = col;
				}	
			";

			GL.ShaderSource(frag, fragSource);
			GL.CompileShader(frag);
			GL.AttachShader(shaderProgram, frag);
			GL.LinkProgram(shaderProgram);
			GL.UseProgram(shaderProgram);
        }

        /** These 3 methods are for State handling **/
        // Pushes a new state onto the stack, calls the states Init method, deletes old state(ie launch game, nuke menu)
        public void ChangeState(GameState state)
        {
            // Cleanup the current state
            if (states.Count != 0)
            {
                states.Pop();
            }
            // Store new state
            states.Push(state);
			state.MakeActive();
        }
        // same as changestate but doesnt delete old state(ie pause game, bringup menu)
        public void PushState(GameState state)
        {
            // store and INIT the new state
            states.Push(state);
			state.MakeActive();
        }
        // pops the current state off and lets the next state have control(menu nukes self, resumes game)
        public void PopState()
        {
            // cleanup the current state
            if (states.Count > 0)
            {
                //GameState st = states.Peek();
                states.Pop();
				states.Peek().MakeActive();
            }
        }

        /// <summary>
        /// Called once per frame.  Contains game logic, etc.
        /// If its not drawing, and it needs to happen in the game loop, call it from here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // let the state update the game
            states.Peek().Update(e);      
        }

        /// <summary>
        /// Renders the current frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            // let the state draw the screen
            states.Peek().Draw(e);
            SwapBuffers();
        }


        /// <summary>
        /// Called when the window is resized. Sets viewport and updates projection matrix.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
			base.OnResize(e);

			if(ClientRectangle.Width / (double)ClientRectangle.Height > 16.0 / 9.0) { //bars on sides
				int width = (int)(ClientRectangle.Height * 16.0 / 9.0);
				xOffset = (ClientRectangle.Width - width) / 2;
				yOffset = 0;
				GL.Viewport(ClientRectangle.X + xOffset, ClientRectangle.Y, width, ClientRectangle.Height);
			} else { //letterbox
				int height = (int)(ClientRectangle.Width * 9.0 / 16.0);
				yOffset = (ClientRectangle.Height - height) / 2;
				xOffset = 0;
				GL.Viewport(ClientRectangle.X, ClientRectangle.Y + yOffset, ClientRectangle.Width, height);
			}

			if(states.Peek() is PlayState) {
				((PlayState)states.Peek()).camera.setViewport(new int[] { ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height });
			}
        }

		public void toggleFullScreen() {
			if(this.WindowState == WindowState.Fullscreen) {
				this.WindowState = WindowState.Normal;
				this.WindowBorder = WindowBorder.Resizable;
				this.ClientSize = new System.Drawing.Size(1280, 720);
			} else {
				this.WindowState = WindowState.Fullscreen;
				this.WindowBorder = WindowBorder.Hidden;
			}
		}
    }
}
