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
using System.Drawing;

// XML parser
using System.Xml;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Timers;

namespace U5Designs
{
    class PauseMenuState : GameState
    {
        internal GameEngine eng;
        
        protected Vector3 eye, lookat;
        Obstacle background;

        static string test = "../../Resources/Sound/Retribution.ogg";        
        AudioFile testFile = new AudioFile(test);

        Texture _p1, _p2, _p3, _p4;
        int _current_img;
        System.Timers.Timer _timer;

        public PauseMenuState(GameEngine engine)
        {
            eng = engine;

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

            _p1 = eng.StateTextureManager.GetTexture("p1");
            _p2 = eng.StateTextureManager.GetTexture("p2");
            _p3 = eng.StateTextureManager.GetTexture("p3");
            _p4 = eng.StateTextureManager.GetTexture("p4");  

            // Create the timer used to switch the current image
            _timer = new System.Timers.Timer();
            
            // Set the timer interval to 1 second
            _timer.Interval = 500;

            // elapse a timer tick
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            // Enable the timer
            _timer.Enabled = true;

            // Set the current image to be displayed at 0 which is the first in the sequence
            _current_img = 0;


        }

        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {            
            if (_current_img < 3)
                _current_img += 1;
            else
                _current_img = 0;            
        }
		public override void MakeActive() {
           
		}

        public override void Update(FrameEventArgs e)
        {
            // Deal with user input from either the keyboard or the mouse
            DealWithInput();
        }

        public override void Draw(FrameEventArgs e)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);            

            if (_current_img == 0)
            {
                _p1.Draw2DTexture(0, 0, 0.2f, 0.2f);                
            }
            if (_current_img == 1)
            {
                _p2.Draw2DTexture(0, 0, 0.2f, 0.2f);               
            }
            if (_current_img == 2)
            {
                _p3.Draw2DTexture(0, 0, 0.2f, 0.2f);                
            }
            if (_current_img == 3)
            {
                _p4.Draw2DTexture(0, 0, 0.2f, 0.2f);               
            }                
        }

        private void DealWithInput()
        {
            if (eng.Keyboard[Key.Enter])
            {
                // Exit Paused Menu state and return to playing				
				eng.PopState();
			} else if(eng.Keyboard[Key.Q]) {
				eng.Exit();
			}
        }

        /**
         * This method will handle setting up the needed structures for Saved Game states.  It will populate the choices data structure, and the state data 
         * structure.
        * */
        public void SavedGameDataSetup()
        {
                        
        }

        /**
         * This method will display the available saved games to the user and allow for them to choose.  The choice will be used to load the correct
         * saved game state into play
         * */
        public void DisplayAvailableSaves()
        {
            
        }

        /**
         * This method will take the players choice chosen during the Main Menu Load Game and load the appropriate PlayerState
         * */
        public PlayerState LoadSavedState(int choice)
        {
            PlayerState ps = new PlayerState();
            return ps;
        }
    }
}
