using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using OpenTK.Input;
using System.Drawing;
using System.Reflection;

using QuickFont;

// XML parser
using System.Xml;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace U5Designs
{
    public class LoadGameState : GameState
    {
        internal GameEngine eng;
        Stack<XmlNodeList> savedGameStates;
        Stack<string> savedGameChoices;
        MainMenuState _ms;
        QFont saveFont;

        // A container which will hold the list of available saved games        
        protected Vector3 eye, lookat;        
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

        public LoadGameState(GameEngine engine, MainMenuState ms)
        {
            eng = engine;
            mouse = eng.Mouse;
            savedGameStates = new Stack<XmlNodeList>();
            savedGameChoices = new Stack<string>();
            _ms = ms;

            Assembly assembly = Assembly.GetExecutingAssembly();
            musicFile = new AudioFile(assembly.GetManifestResourceStream("U5Designs.Resources.Music.Menu.ogg"));
            musicFile.Play();

            // Clear the color to work with the SplashScreen so it doesn't white out
            //GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

            _old_state = OpenTK.Input.Keyboard.GetState(); // Get the current state of the keyboard   

            eng.StateTextureManager.LoadTexture("menu", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.menu.png"));
            menu = eng.StateTextureManager.GetTexture("menu");

            // Load available saved games
            // Setup saved game data 
            SavedGameDataSetup();

            // Display available saved game states
            DisplayAvailableSaves();
        }

        public override void MakeActive()
        {
            saveFont = new QFont("Fonts/Rock.TTF", 32, new QFontBuilderConfiguration(true));
            saveFont.Options.Colour = new Color4(1.0f, 0.2f, 0.2f, 1.0f);

            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(1280, 720, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);
            if (eng.Keyboard[Key.Escape])
            {
                escapedown = true;
            }
            if (eng.Keyboard[Key.Enter])
                enterdown = true;
        }

        public override void Update(FrameEventArgs e)
        {
            DealWithInput();
            //MouseInput();
        }

        public override void Draw(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ClearColor(1.0f, 1.0f, 1.0f, 0.0f);
            //Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadMatrix(ref modelview);  

            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            // Draw the background
            menu.Draw2DTexture();            
            drawSaves();
        }

        // Draw stuff
        public void drawSaves()
        {
            QFont.Begin(); 
            GL.PushMatrix();
            //GL.Translate(eng.Width * 0.5f, eng.Height*0.25f, 0f);
            float yOffset = 0;
            foreach (string s in savedGameChoices)
            {
                //saveFont.Print(s, QFontAlignment.Centre);
                saveFont.Print(s, new Vector2(eng.Width * 0.5f, yOffset));
                yOffset += saveFont.Measure(s).Height + (0.5f * saveFont.Measure(s).Height);
            }
            GL.PopMatrix();
            QFont.End(); 
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

            if (eng.Keyboard[Key.Escape] && !escapedown)
            {
                eng.Exit();
            }
            else if (!eng.Keyboard[Key.Escape])
            {
                escapedown = false;
            }

            //********************** enter
            if (eng.Keyboard[Key.Enter] && !enterdown)
            {
                enterdown = true;
                //handleButtonPress();
            }
            else if (!eng.Keyboard[Key.Enter])
            {
                enterdown = false;
            }

            //Minus - Toggle fullscreen
            if (eng.Keyboard[Key.Minus])
            {
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

        /** This loads the chosen game **/
        internal void loadPlayState(int lvl)
        {
            musicFile.Stop();

            PlayState ps = new PlayState(eng, _ms);
            LoadScreenState ls = new LoadScreenState(eng, ps, lvl);
            eng.ChangeState(ls);
        }
    }
}
