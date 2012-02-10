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

namespace U5Designs
{
    class PauseMenuState : GameState
    {
        internal GameEngine eng;
        
        protected Vector3 eye, lookat;
        Obstacle background;

        static string test = "../../Resources/Sound/Retribution.ogg";        
        AudioFile testFile = new AudioFile(test);


        public PauseMenuState(GameEngine engine)
        {
            eng = engine;
        }

		public override void MakeActive() {
			
		}

        public override void Update(FrameEventArgs e)
        {
            DealWithInput();
        }

        public override void Draw(FrameEventArgs e)
        {
            
        }

        private void DealWithInput()
        {
            if (eng.Keyboard[Key.Enter])
            {
                // Exit Paused Menu state and return to playing
				Console.WriteLine("Exiting paused menu state");
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
