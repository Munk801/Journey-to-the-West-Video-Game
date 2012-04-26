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

using QuickFont;

namespace U5Designs
{
    /** Main Menu State of the game that will be active while the Main Menu is up **/
    public class MainMenuState : GameState
    {
        internal GameEngine eng;

        // Fonts
        internal QFont title, button, buttonHighlight;
        List<String> _buttons;
        double cnt;
        // End Fonts

        
        // A container which will hold the list of available saved games
        Stack<XmlNodeList> savedGameStates;
        Stack<string> savedGameChoices;
        int saved_level_index = -1;
        protected Vector3 eye, lookat;
        Obstacle background;
        MouseDevice mouse;
        public AudioFile musicFile;

        // testing buttons
        //Obstacle play_button_npress, play_button_press, load_button_npress, load_button_press, quit_button_press, quit_button_npress;
        private int numOfButtons = 3;
        int _cur_butn = 0;
        OpenTK.Input.KeyboardState _old_state;

        // New Buttons
        Texture menu, arrow, play_press, play_nopress, load_nopress, load_press, quit_nopress, quit_press, ld_nopress, ld_press;
        float arX, b1Y, b2Y, b3Y, b4Y;
        public bool enterdown, escapedown;

        public bool clickdown = false;

        public MainMenuState(GameEngine engine)
        {
            eng = engine;
            savedGameStates = new Stack<XmlNodeList>();
            savedGameChoices = new Stack<string>();
            mouse = eng.Mouse;
            // Load all the textures
            eng.StateTextureManager.RenderSetup();
            Assembly assembly = Assembly.GetExecutingAssembly();

            
            eng.StateTextureManager.LoadTexture("menu", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.menu.png"));
            menu = eng.StateTextureManager.GetTexture("menu");
            
            // QFont
            _buttons = new List<String>();
            _buttons.Add("Play Game");
            _buttons.Add("Load Saved Game");
            _buttons.Add("Level Designer");
            _buttons.Add("Quit");
            button = QFont.FromQFontFile("../../Fonts/myHappySans.qfont", new QFontLoaderConfiguration(true));
            button.Options.DropShadowActive = true;
            //title = QFont.FromQFontFile("myHappySans.qfont", new QFontLoaderConfiguration(true));
            title = QFont.FromQFontFile("../../Fonts/myRock.qfont", new QFontLoaderConfiguration(true));
            title.Options.DropShadowActive = false;
            buttonHighlight = QFont.FromQFontFile("../../Fonts/myHappySans2.qfont", new QFontLoaderConfiguration(true));
            buttonHighlight.Options.DropShadowActive = true;
           // QFont.CreateTextureFontFiles("../../Fonts/HappySans.TTF", 32, "myStoryBright"); // Use this to create new Fonts that you will texture
           // QFont.CreateTextureFontFiles("../../Fonts/Comfortaa-Regular.ttf", 32, "myComfort"); // Use this to create new Fonts that you will texture
            
            // End QFonts

			musicFile = new AudioFile(assembly.GetManifestResourceStream("U5Designs.Resources.Music.Menu.ogg"));
			musicFile.Play();

            // Setup saved game data 
            SavedGameDataSetup();

            // Display available saved game states
            DisplayAvailableSaves();

            // Clear the color to work with the SplashScreen so it doesn't white out
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

            _old_state = OpenTK.Input.Keyboard.GetState(); // Get the current state of the keyboard           

            arX = -200.0f;
            b1Y = 0.0f;
            b2Y = -50.0f;
            b3Y = -100.0f;
            b4Y = -150.0f;
            enterdown = false;

            // TEST //
            //LoadSavedState(1);

        }

        public override void MakeActive()
        {

            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(1280, 720, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);
            if (eng.Keyboard[Key.Escape]) {
                escapedown = true;
            }
            if (eng.Keyboard[Key.Enter])
                enterdown = true;
        }

        public override void Update(FrameEventArgs e)
        {
            cnt += e.Time;
            DealWithInput();
            MouseInput();
        }

        public override void Draw(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

			GL.MatrixMode(MatrixMode.Projection);
			Matrix4 projection = Matrix4.CreateOrthographic(1280, 720, 1.0f, 6400.0f);
			GL.LoadMatrix(ref projection);

            Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            // Fonts
            GL.Disable(EnableCap.DepthTest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
            // End Fonts

            menu.Draw2DTexture();            
           
            drawButtons();
            
            GL.Enable(EnableCap.DepthTest);
        }

		internal void loadPlayState(int lvl) {
			musicFile.Stop();

			PlayState ps = new PlayState(eng, this, lvl);
			LoadScreenState ls = new LoadScreenState(eng, ps, lvl);
			eng.ChangeState(ls);
		}

        // Draw stuff
        public void drawButtons()
        {
            //QFont.Begin();
            //GL.Translate(eng.Width * 0.5f, eng.Height*0.25f, 0f); 
			GL.PushMatrix();
			GL.Scale(1, -1, 1);

            float startY = 0.0f;
            title.Print("Main Menu", new Vector2(-(title.Measure("Main Menu").Width / 2), startY - (title.Measure("A").Height + 10)));
            title.Options.DropShadowActive = true;
            //float yOffset = startY + title.Measure("Available Save Points").Height; 
            float yOffset = startY;// -title.Measure("A").Height;
            int count = 0;
            foreach (string s in _buttons)
            {
                if (count == _cur_butn)
                {
                    // Draw highlighted string                   

                    GL.PushMatrix();
                    buttonHighlight.Options.Colour = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
                    GL.Translate(-16 * (float)(Math.Sin(cnt * 4)), yOffset, 0f);
                    buttonHighlight.Print(s, QFontAlignment.Centre);
                    GL.PopMatrix();
                }
                else
                {
                    GL.PushMatrix();
                    // Draw non highlighted string
                    button.Options.DropShadowActive = false;
                    button.Print(s, new Vector2(-(button.Measure(s).Width / 2), yOffset));
                    GL.PopMatrix();
                }
                yOffset += button.Measure(s).Height + (10);
                count++;
            }
            //QFont.End();
			GL.PopMatrix();
        }

		/// <summary>
		/// Called by mouse or keyboard handlers when the user picked a button (by clicking or hitting enter)
		/// </summary>
		internal void handleButtonPress() {
			switch(_cur_butn) {
				case 0: //new game
					//loadPlayState(0);
                    musicFile.Stop();
                    PlayerNameState _PS = new PlayerNameState(eng, this);
                    eng.ChangeState(_PS);
					break;
				case 1: //load saved game
					//loadPlayState(saved_level_index);
                    musicFile.Stop();
                    LoadGameState _L = new LoadGameState(eng, this);
                    eng.ChangeState(_L);
					break;				
				case 2: //level designer
                    musicFile.Stop();
					LevelDesignerState ls = new LevelDesignerState(eng, this, 13731);
					eng.ChangeState(ls);
					eng.GameInProgress = true;
					break;
                case 3: //quit
                    musicFile.Stop();
                    eng.Exit();
                    break;
			}
		}

        /// <summary>
        /// For mouse input on the main menu, allow the user to click on the buttons to select items
        /// </summary>
        private void MouseInput()
        {
            // COMMENTING FOR NOW.  BUGGY.  MAY RETURN TO FIX IF NECESSARY
            //if (eng.ThisMouse.inButtonRegion(play_press)) {
            //    eng.selectSound.Play();
            //    _cur_butn = 0;
            //} else if (eng.ThisMouse.inButtonRegion(load_press)) {
            //    eng.selectSound.Play();
            //    _cur_butn = 1;
            //} else if (eng.ThisMouse.inButtonRegion(quit_press)) {
            //    eng.selectSound.Play();
            //    _cur_butn = 2;
            //} else if(eng.ThisMouse.inButtonRegion(ld_press)) {
            //    eng.selectSound.Play();
            //    _cur_butn = 3;
            //} else {
            //    return; //If they didn't actively click on something, don't use the selected button from the keyboard
            //}

            //if(eng.ThisMouse.LeftPressed() && !clickdown) {
            //    clickdown = true;
            //    handleButtonPress();
            //} else {
            //    clickdown = false;
            //}
        }

        private void DealWithInput()
        {
            // Testing buttons
            OpenTK.Input.KeyboardState _new_state = OpenTK.Input.Keyboard.GetState();
            if ((_new_state.IsKeyDown(Key.Down) && !_old_state.IsKeyDown(Key.Down)) ||
                (_new_state.IsKeyDown(Key.S) && !_old_state.IsKeyDown(Key.S)))
            {
                // Down key was just pressed
                if (_cur_butn < numOfButtons)
                {
                    // Increment the current button index so you draw the highlighted button of the next button 
                    _cur_butn += 1;
                    eng.selectSound.Play();
                }
                else if (_cur_butn >= numOfButtons)
                {
                    // Were on the last button in the list so reset to the top of the button list
                    _cur_butn = 0;
                    eng.selectSound.Play();
                }
            }
            if ((_new_state.IsKeyDown(Key.Up) && !_old_state.IsKeyDown(Key.Up)) ||
                (_new_state.IsKeyDown(Key.W) && !_old_state.IsKeyDown(Key.W)))
            {
                // Down key was just pressed
                if (_cur_butn > 0)
                {
                    // Increment the current button index so you draw the highlighted button of the next button 
                    _cur_butn -= 1;
                    eng.selectSound.Play();
                }
                else if (_cur_butn <= 0)
                {
                    // Were on the last button in the list so reset to the top of the button list
                    _cur_butn = numOfButtons;
                    eng.selectSound.Play();
                }
            }
            _old_state = _new_state;

            if (eng.Keyboard[Key.Escape] && !escapedown){
                eng.Exit();
            }
            else if (!eng.Keyboard[Key.Escape]) {
                escapedown = false;
            }

            //********************** enter
            if (eng.Keyboard[Key.Enter] && !enterdown) {
                enterdown = true;
				handleButtonPress();
            } else if(!eng.Keyboard[Key.Enter]) {
				enterdown = false;
			}

			//Minus - Toggle fullscreen
			if(eng.Keyboard[Key.Minus]) {
				eng.toggleFullScreen();
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

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream fstream = assembly.GetManifestResourceStream("U5Designs.Resources.test.sav");
            doc.Load(fstream);
            XmlNodeList games = doc.GetElementsByTagName("save");

            foreach (XmlNode n in games)
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
                {
                    ps.setZone(Convert.ToInt32(n.InnerText));
                    saved_level_index = Convert.ToInt32(n.InnerText);
                }
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
