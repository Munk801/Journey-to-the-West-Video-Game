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
using System.Collections.Generic;

namespace U5Designs
{
    /** Main Menu State of the game that will be active while the Main Menu is up **/
    class MainMenuState : GameState
    {

        // A container which will hold the list of available saved games
        Stack<PlayerState> savedGames;

        // Initialize graphics here for the game playing
        public override void Init(GameEngine eng)
        {
            // Initialize the saved games stack
            savedGames = new Stack<PlayerState>();

            // Load saved game data
            LoadSavedGame();

        }

        // Cleanup any resources you created here
        public override void Cleanup()
        {
        }

        public override void Pause()
        {
        }

        public override void Resume()
        {
        }

        public override void HandleEvents(GameEngine eng)
        {
            DealWithKeys(eng);

        }

        public override void Update(GameEngine eng, FrameEventArgs e)
        {
        }

        public override void Draw(GameEngine eng, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
        }

        private void DealWithKeys(GameEngine eng)
        {
            //TODO: Change these keys to their final mappings when determined

            if (eng.Keyboard[Key.Q])
            {
                eng.Exit();
            }

            //********************** enter
            if (eng.Keyboard[Key.Enter])
            {
                //transition into PlayState
                if (eng.GameInProgress)
                {
                    eng.PopState();
                }
                else
                {
                    PlayState ps = new PlayState();
                    eng.ChangeState(ps);
                    eng.GameInProgress = true;
                }
            }
        }

        /**
        * This will load the saved game data into memory.  The player can then choose from a list of available saved games if they choose during the menu state
        * */
        public void LoadSavedGame()
        {

            // Parse XML saved game data file and store the information

            using (XmlReader reader = XmlReader.Create("save.xml"))
            {
                string str;
                int val;
                double dval;

                reader.Read();

                // Read to start <element> that you are looking to parse                
                reader.ReadToFollowing("pname");

                // Debug
                Console.WriteLine(reader.ReadString());
                str = reader.ReadString();

                if (!String.IsNullOrEmpty(str))
                {
                    PlayerState ps = new PlayerState(reader.ReadString());


                    // finish with the current <element> and move to the next
                    reader.ReadEndElement();

                    reader.ReadToFollowing("level");
                    Console.WriteLine("Level: " + reader.ReadString());
                }
            }

        }
    }
}
