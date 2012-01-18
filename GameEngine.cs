using System;
using System.Collections.Generic;

using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using Engine.Input;

namespace U5Designs
{
    public class GameEngine : GameWindow
    {
        /**
         * Only the current state can know when it’s time to change to the next state.
         * 
         * 
         * HandleEvents()
         * Update()
         * Draw()
         * will call the corresponding methods in the State class that is on the "stack" List<>
         * 
         * */

        /// <summary>Creates a 1280x720 window.</summary>
        //TODO: Change this to a dynamic screen resolution
        public GameEngine() : base(1280, 720, GraphicsMode.Default, "U5 Designs Untitled Project") {
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
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            
            states = new Stack<GameState>();
            MainMenuState ms = new MainMenuState();
            this.ChangeState(ms);

            // Set the screen resolution (Fullscreen / windowed)

            // Set the title bar of the window etc

        }

        /** Init and Cleanup are to be used to initialize the Game and Cleanup the game when done **
         * 
         * Init as far as being in the Engine, is literally OnLoad, and Cleanup for the game engine is 
         * irrelivant since were in c# not c++, managed memory ftw!
        */

        /*
        public void Init()
        {
            // Set the running flag
            m_running = true;
        }
        
        public void Cleanup()
        {
            while (states.Count > 0)
            {
                // Pop off the top of the Stack and clean it up
                GameState st = states.Pop();
                st.Cleanup();
            }
        }
        */

        /** These 3 methods are for State handling **/
        public void ChangeState(GameState state)
        {
            // Cleanup the current state
            if (states.Count != 0)
            {
                GameState st = states.Pop();
                st.Cleanup();
            }

            // Store and INIT the new state
            states.Push(state);
            state.Init();
        }

        public void PushState(GameState state)
        {
            // pause the current state
            if (states.Count != 0)
            {
                GameState st = states.Peek();
                st.Pause();
            }

            // store and INIT the new state
            states.Push(state);
            state.Init();
        }

        public void PopState()
        {
            // cleanup the current state
            if (states.Count != 0)
            {
                GameState st = states.Peek();
                st.Cleanup();
                states.Pop();
            }

            // resume the previous state
            if (states.Count != 0)
            {
                GameState st = states.Peek();
                st.Resume();
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

            // let the state handle events
            states.Peek().HandleEvents(this);

            // let the state update the game
            states.Peek().Update(this, e);
        }

        /// <summary>
        /// Renders the current frame.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e) {
            base.OnRenderFrame(e);

            // let the state draw the screen
            states.Peek().Draw(this, e);



            SwapBuffers();
        }

        /** These are the methods to process game stuff most likely where the game logic or calls will go**/

        // Handle Events and Update both happen in OnUpdateFrame, where Draw is literally OnRenderFrame
        /*
        public void HandleEvents()
        {
            // let the state handle events
        }

        public void Update(GameEngine gameEng, FrameEventArgs e)
        {
            // let the state update the game
        }

        public void Draw(GameEngine gameEng, FrameEventArgs e)
        {
            // let the state draw the screen
        }
        */

        /** Use these 2 methods in the Main() loop in EntryPoint to check if the game is still running and to Quit**/
        public bool Running()
        {
            return m_running;
        }

        public void Quit()
        {
            m_running = false;
        }
             
        // This is the "Stack" of states with an LIFO structure mimicing an actual memory Stack
        Stack<GameState> states;

        // FLAG that tells if the game is running or not
        bool m_running;

        /// <summary>
        /// Called when the window is resized. Sets viewport and updates projection matrix.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            states.Peek().updateView(this);
        }
    }
}
