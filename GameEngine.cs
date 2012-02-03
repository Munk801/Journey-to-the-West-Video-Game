﻿using System;
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
         * HandleEvents()
         * Update()
         * Draw()
         * will call the corresponding methods in the State class that is on the "stack" List<>
         * 
         * */

        Stack<GameState> states;// This is the "Stack" of states with an LIFO structure mimicing an actual memory Stack
        internal bool GameInProgress;// this bool tracks if a game is in progress, mostly for the menu state to know if its the first menu, or has been brought up ingame

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
			GL.Enable(EnableCap.Texture2D);


            states = new Stack<GameState>();
            MainMenuState ms = new MainMenuState(this);
            this.ChangeState(ms);

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
                GameState st = states.Pop();
            }
            // Store new state
            states.Push(state);
        }
        // same as changestate but doesnt delete old state(ie pause game, bringup menu)
        public void PushState(GameState state)
        {
            // store and INIT the new state
            states.Push(state);
        }
        // pops the current state off and lets the next state have control(menu nukes self, resumes game)
        public void PopState()
        {
            // cleanup the current state
            if (states.Count != 0)
            {
                GameState st = states.Peek();
                states.Pop();
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

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            states.Peek().updateView();
        }
    }
}
