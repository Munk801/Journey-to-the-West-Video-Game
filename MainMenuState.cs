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
        Stack<XmlNodeList> savedGameStates;
        Stack<string> savedGameChoices;
        AudioSource test;

        // Initialize graphics here for the game playing
        public override void Init(GameEngine eng)
        {
            // Initialize the saved games stack
            savedGameStates = new Stack<XmlNodeList>();
            savedGameChoices = new Stack<string>();

            // Setup saved game data 
            SavedGameDataSetup();

            // Display available saved game states
            DisplayAvailableSaves(); 

            // TEST //
            LoadSavedState(1);

            //AudioManager.CreateAudioDevice();
            //test = new AudioSource();
            //test.LoadSource("sound/Hydrate-Kenny_Beltrey.ogg");
            //test.PlaySource();
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
            //AudioSource.Update();
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
         * This method will handle setting up the needed structures for Saved Game states.  It will populate the choices data structure, and the state data 
         * structure.
        * */
        public void SavedGameDataSetup()
        {
            // Parse XML saved game data file and store the information             
            XmlDocument doc = new XmlDocument();
            doc.Load("test.sav");
            XmlNodeList games = doc.GetElementsByTagName("save");            
            foreach(XmlNode n in games)
            {
                // Create a new list of nodes that will contain each data element from a particular saved game state
                XmlNodeList nd = n.ChildNodes;

                // Push each "saved" game state onto the stack
                savedGameStates.Push(nd);

                string str = "";
                foreach (XmlNode n2 in nd)
                {
                    if (n2.Name.CompareTo("p_name") == 0)
                    {
                        str += "Name: " + n2.InnerText;
                    }
                    if (n2.Name.CompareTo("p_current_zone") == 0)
                    {
                        str += " Zone: " + n2.InnerText;
                    }                    
                }
                //Console.WriteLine(str);
                savedGameChoices.Push(str);
            }                     
        }

        /**
         * This method will display the available saved games to the user and allow for them to choose.  The choice will be used to load the correct
         * saved game state into play
         * */
        public void DisplayAvailableSaves()
        {
            Console.WriteLine("Available Saved Games:");
            foreach (string s in savedGameChoices)
            {
                // Need to use this data to create button graphics during the Load Game stage                
                Console.WriteLine(s);
            }
        }

        /**
         * This method will take the players choice chosen during the Main Menu Load Game and load the appropriate PlayerState
         * */
        public PlayerState LoadSavedState(int choice)
        {
            PlayerState ps = new PlayerState();
            XmlNodeList[] n_list = savedGameStates.ToArray();
            XmlNodeList chosen_state = n_list[choice];
            List<int> p_abilities = new List<int>();

            // Loop over each element in the XML saved state and start populating the PlayerState with it
            foreach (XmlNode n in chosen_state)
            {
                /** Every item that is added to the PlayerState.cs file NEEDS to be accounted for here **/
                if (n.Name.CompareTo("p_name") == 0)
                    ps.setName(n.InnerText);
                if (n.Name.CompareTo("p_current_zone") == 0)
                    ps.setZone(Convert.ToInt32(n.InnerText));
                if (n.Name.CompareTo("p_health") == 0)
                    ps.setHealth(Convert.ToInt32(n.InnerText));
                if (n.Name.CompareTo("p_speed") == 0)
                    ps.setSpeed(Convert.ToDouble(n.InnerText));
                if (n.Name.CompareTo("p_damage") == 0)
                    ps.setDamage(Convert.ToInt32(n.InnerText));

                if (n.Name.CompareTo("abilities") == 0)
                {
                    string[] a_str = n.InnerText.Split(' ');
                    foreach (string s in a_str)
                    {
                        // Convert each ability to an integer and add it to the list
                        p_abilities.Add(Convert.ToInt32(s));
                    }

                    // Set the players list of available abilities to the saved abilities
                    ps.setAbilities(p_abilities);
                }
            }

            // Return the fully loaded PlayerState which can be used to fill out the rest of the GameState
            return ps;
        }
    }
}
