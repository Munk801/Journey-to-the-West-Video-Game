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

        Stack<GameState> states;// This is the "Stack" of states with an LIFO structure mimicing an actual memory Stack
        internal bool GameInProgress;// this bool tracks if a game is in progress, mostly for the menu state to know if its the first menu, or has been brought up ingame
        internal TextureManager StateTextureManager;
        internal GameMouse ThisMouse;
        internal AudioFile selectSound;
        internal Assembly assembly;

        /// <summary>Creates a 1280x720 window.</summary>
        public GameEngine() : base(1280, 720, GraphicsMode.Default, "Journey to the East") {
            VSync = VSyncMode.On;
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
        
            ThisMouse = new GameMouse(this);
            states = new Stack<GameState>();

            //this.ChangeState(ms);

            //MainMenuState ms = new MainMenuState(this);
            //this.ChangeState(ms);
            SplashScreenState ss = new SplashScreenState(this);
            this.ChangeState(ss);

            selectSound = new AudioFile(assembly.GetManifestResourceStream("U5Designs.Resources.Sound.select.ogg"));

            // Set the screen resolution (Fullscreen / windowed)

            // Set the title bar of the window etc

        }

        /** These 3 methods are for State handling **/
        // Pushes a new state onto the stack, calls the states Init method, deletes old state(ie launch game, nuke menu)
        public void ChangeState(GameState state)
        {
            // Cleanup the current state
            if (states.Count != 0)
            {
                /*GameState st =*/ states.Pop();
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
			if(ClientRectangle.Width > 1280 || ClientRectangle.Height > 720) {
				int xExtra = (ClientRectangle.Width - 1280) / 2;
				int yExtra = (ClientRectangle.Height - 720) / 2;
				GL.Viewport(ClientRectangle.X + xExtra, ClientRectangle.Y + yExtra, ClientRectangle.Width - (xExtra * 2), ClientRectangle.Height - (yExtra * 2));
			} else {
				GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
			}
            //TODO: Something should happen here to handle this gracefully
			if(states.Peek() is PlayState) {
				((PlayState)states.Peek()).camera.setViewport(new int[] { ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height });
			}

			//states.Peek().updateView();
        }
    }
}
