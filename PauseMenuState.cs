using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
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
        MainMenuState menu;

        Texture _p1, _p2, _p3, _p4;
		double curFrame;
        bool escapedown;

        public PauseMenuState(GameEngine engine, MainMenuState menustate)
        {
            eng = engine;
            menu = menustate;
            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

            _p1 = eng.StateTextureManager.GetTexture("p1");
            _p2 = eng.StateTextureManager.GetTexture("p2");
            _p3 = eng.StateTextureManager.GetTexture("p3");
            _p4 = eng.StateTextureManager.GetTexture("p4");

			// Set the current image to be displayed at 0 which is the first in the sequence
			curFrame = 0.0;


        }

		public override void MakeActive() {
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            
			Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref modelview);

			GL.MatrixMode(MatrixMode.Projection);
			Matrix4d projection = Matrix4d.CreateOrthographic(192, 108, 1.0f, 6400.0f);
			GL.LoadMatrix(ref projection);
            if (eng.Keyboard[Key.Escape]) {
                escapedown = true;
            }
		}

        public override void Update(FrameEventArgs e)
        {
            // Deal with user input from either the keyboard or the mouse
            DealWithInput();
        }

        public override void Draw(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			curFrame = (curFrame + e.Time * 2) % 4;

			switch((int)curFrame) {
				case 0:
					_p1.Draw2DTexture(0, 0, 0.2f, 0.2f);
					break;
				case 1:
					_p2.Draw2DTexture(0, 0, 0.2f, 0.2f);
					break;
				case 2:
					_p3.Draw2DTexture(0, 0, 0.2f, 0.2f);
					break;
				case 3:
					_p4.Draw2DTexture(0, 0, 0.2f, 0.2f);
					break;
			}             
        }

        private void DealWithInput()
        {
            if (eng.Keyboard[Key.Enter])
            {
                // Exit Paused Menu state and return to playing				
				eng.PopState();
			} 
            if(eng.Keyboard[Key.Q]) {
				eng.Exit();
			}

            if (eng.Keyboard[Key.Escape] && !escapedown) {
                eng.GameInProgress = false;
                eng.PopState(); //nuke playstate
                eng.ChangeState(menu);
            }
            else if (!eng.Keyboard[Key.Escape]) {
                escapedown = false;
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
