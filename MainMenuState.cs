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
    /** Main Menu State of the game that will be active while the Main Menu is up **/
    public class MainMenuState : GameState
    {
        internal GameEngine eng;

        // A container which will hold the list of available saved games
        Stack<XmlNodeList> savedGameStates;
        Stack<string> savedGameChoices;
        int saved_level_index = -1;
        protected Vector3 eye, lookat;
        Obstacle background;
        MouseDevice mouse;

        static string test = "../../Resources/Sound/Retribution.ogg";
        public AudioFile testFile = new AudioFile(test);

        // testing buttons
        //Obstacle play_button_npress, play_button_press, load_button_npress, load_button_press, quit_button_press, quit_button_npress;
        int _cur_butn = 0;
        OpenTK.Input.KeyboardState _old_state;

        // New Buttons
        //TextureManager StateTextureManager;
        Texture play_press, play_nopress, load_nopress, load_press, quit_nopress, quit_press;
        public bool enterdown;

        public bool clickdown = false;

        public MainMenuState(GameEngine engine)
        {
            eng = engine;
            savedGameStates = new Stack<XmlNodeList>();
            savedGameChoices = new Stack<string>();
            mouse = eng.Mouse;
            // Load all the textures
            eng.StateTextureManager.RenderSetup();
            eng.StateTextureManager.LoadTexture("load", "../../Resources/Textures/load_button_no_press.png");
            load_nopress = eng.StateTextureManager.GetTexture("load");
            eng.StateTextureManager.LoadTexture("loadpress", "../../Resources/Textures/load_button_press.png");
            load_press = eng.StateTextureManager.GetTexture("loadpress");
            eng.StateTextureManager.LoadTexture("quit", "../../Resources/Textures/quit_button_no_press.png");
            quit_nopress = eng.StateTextureManager.GetTexture("quit");
            eng.StateTextureManager.LoadTexture("quitpress", "../../Resources/Textures/quit_button_press.png");
            quit_press = eng.StateTextureManager.GetTexture("quitpress");
            eng.StateTextureManager.LoadTexture("play", "../../Resources/Textures/play_button_no_press.png");
            play_nopress = eng.StateTextureManager.GetTexture("play");
            eng.StateTextureManager.LoadTexture("playpress", "../../Resources/Textures/play_button_press.png");
            play_press = eng.StateTextureManager.GetTexture("playpress");



            // Setup saved game data 
            SavedGameDataSetup();

            // Display available saved game states
            DisplayAvailableSaves();

            // Plays the audio file.  Should be in a data file later
            testFile.Play();

            // Clear the color to work with the SplashScreen so it doesn't white out
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

            //SpriteSheet.quad = new ObjMesh("../../Geometry/quad.obj");
            //int[] cycleStarts = { 0 };
            //int[] cycleLengths = { 1 };
            //SpriteSheet ss = new SpriteSheet(new Bitmap("../../Geometry/testbg.png"), cycleStarts, cycleLengths, 1280, 720);
            //background = new Obstacle(new Vector3(0, 0, 2), new Vector3(1280, 720, 1), new Vector3(0,0,0), true, true, ss);



            // testing buttons
            _old_state = OpenTK.Input.Keyboard.GetState(); // Get the current state of the keyboard           
            //SpriteSheet pb_np_ss = new SpriteSheet(new Bitmap("../../Geometry/play_button_no_press.png"), cycleStarts, cycleLengths, 320, 100);
            //play_button_npress = new Obstacle(new Vector3(0, 100, 2), new Vector3(320, 100, 1), new Vector3(0, 0, 0), true, true, pb_np_ss);
            //SpriteSheet pb_p_ss = new SpriteSheet(new Bitmap("../../Geometry/play_button_press.png"), cycleStarts, cycleLengths, 320, 100);
            //play_button_press = new Obstacle(new Vector3(0, 100, 2), new Vector3(320, 100, 1), new Vector3(0, 0, 0), true, true, pb_p_ss);

            //SpriteSheet lb_np_ss = new SpriteSheet(new Bitmap("../../Geometry/load_button_no_press.png"), cycleStarts, cycleLengths, 320, 100);
            //load_button_npress = new Obstacle(new Vector3(0, 0, 2), new Vector3(320, 100, 1), new Vector3(0, 0, 0), true, true, lb_np_ss);
            //SpriteSheet lb_p_ss = new SpriteSheet(new Bitmap("../../Geometry/load_button_press.png"), cycleStarts, cycleLengths, 320, 100);
            //load_button_press = new Obstacle(new Vector3(0, 0, 2), new Vector3(320, 100, 1), new Vector3(0, 0, 0), true, true, lb_p_ss);

            //SpriteSheet qb_np_ss = new SpriteSheet(new Bitmap("../../Geometry/quit_button_no_press.png"), cycleStarts, cycleLengths, 320, 100);
            //quit_button_npress = new Obstacle(new Vector3(0, -100, 2), new Vector3(320, 100, 1), new Vector3(0, 0, 0), true, true, qb_np_ss);
            //SpriteSheet qb_p_ss = new SpriteSheet(new Bitmap("../../Geometry/quit_button_press.png"), cycleStarts, cycleLengths, 320, 100);
            //quit_button_press = new Obstacle(new Vector3(0, -100, 2), new Vector3(320, 100, 1), new Vector3(0, 0, 0), true, true, qb_p_ss);

            // TEST //
            enterdown = false;
            LoadSavedState(1);

        }

        public override void MakeActive()
        {

            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(1280, 720, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);

            //Set up texture properties for sprites
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new OpenTK.Graphics.Color4(1, 1, 1, 0)); //transparent
        }

        public override void Update(FrameEventArgs e)
        {
            DealWithInput();
            MouseInput();
        }

        public override void Draw(FrameEventArgs e)
        {
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            //((RenderObject)background).doScaleTranslateAndTexture();

            // testing buttons
            switch (_cur_butn)
            {
                case 0:
                    play_press.Draw2DTexture(0, 100, 1.0f, 1.0f);
                    load_nopress.Draw2DTexture(0, 0);
                    quit_nopress.Draw2DTexture(0, -100);
                    //((RenderObject)play_button_press).doScaleTranslateAndTexture();
                    //((RenderObject)play_button_press).sprite.draw(false);
                    //((RenderObject)load_button_npress).doScaleTranslateAndTexture();
                    //((RenderObject)load_button_npress).sprite.draw(false);
                    //((RenderObject)quit_button_npress).doScaleTranslateAndTexture();
                    //((RenderObject)quit_button_npress).sprite.draw(false);
                    break;
                case 1:
                    play_nopress.Draw2DTexture(0, 100, 1.0f, 1.0f);
                    load_press.Draw2DTexture(0, 0);
                    quit_nopress.Draw2DTexture(0, -100);
                    //((RenderObject)play_button_npress).doScaleTranslateAndTexture();
                    //((RenderObject)play_button_npress).sprite.draw(false);
                    //((RenderObject)load_button_press).doScaleTranslateAndTexture();
                    //((RenderObject)load_button_press).sprite.draw(false);
                    //((RenderObject)quit_button_npress).doScaleTranslateAndTexture();
                    //((RenderObject)quit_button_npress).sprite.draw(false);
                    break;
                case 2:
                    play_nopress.Draw2DTexture(0, 100, 1.0f, 1.0f);
                    load_nopress.Draw2DTexture(0, 0);
                    quit_press.Draw2DTexture(0, -100);
                    //((RenderObject)play_button_npress).doScaleTranslateAndTexture();
                    //((RenderObject)play_button_npress).sprite.draw(false);
                    //((RenderObject)load_button_npress).doScaleTranslateAndTexture();
                    //((RenderObject)load_button_npress).sprite.draw(false);
                    //((RenderObject)quit_button_press).doScaleTranslateAndTexture();
                    //((RenderObject)quit_button_press).sprite.draw(false);
                    break;
            }

            //GL.PushMatrix();
            //GL.Translate(-640, -360, -10);
            //GL.Scale(426.5f, 240, 1);
            //GL.Scale(1280, 720, 100);    // Weird coincidence that this is really close to the images dimensions???   
            // Actually, not a coincidence at all...
            //((RenderObject)background).sprite.draw(false);                      
        }

        private void MouseInput()
        {
            //if (mouse.X - eng.Width/2 > play_press.XLoc && mouse.X - eng.Width/2 < play_press.XLoc + play_press.Width
            //    && -(mouse.Y - eng.Height / 2) < play_press.YLoc && -(mouse.Y - eng.Height / 2) > play_press.YLoc - play_press.Height)
            //{
            //    _cur_butn = 0;
            //}

            //if (mouse.X - eng.Width / 2 > load_press.XLoc && mouse.X - eng.Width / 2 < load_press.XLoc + load_press.Width
            //    && -(mouse.Y - eng.Height / 2) < load_press.YLoc && -(mouse.Y - eng.Height / 2) > load_press.YLoc - load_press.Height)
            //{
            //    _cur_butn = 1;
            //}

            //if (mouse.X - eng.Width / 2 > quit_press.XLoc && mouse.X - eng.Width / 2 < quit_press.XLoc + quit_press.Width
            //    && -(mouse.Y - eng.Height / 2) < quit_press.YLoc && -(mouse.Y - eng.Height / 2) > quit_press.YLoc - quit_press.Height)
            //{
            //    _cur_butn = 2;
            //}

            if (eng.ThisMouse.inButtonRegion(play_press))
            {
                _cur_butn = 0;
            }

            if (eng.ThisMouse.inButtonRegion(load_press))
            {
                _cur_butn = 1;
            }
            if (eng.ThisMouse.inButtonRegion(quit_press))
            {
                _cur_butn = 2;
            }

            if (eng.ThisMouse.LeftPressed() && !clickdown)
            {
                clickdown = true;
                if (_cur_butn == 0)
                {
                    //transition into PlayState
                    if (eng.GameInProgress)
                    {
                        eng.PopState();
                    }
                    else
                    {
                        // If you're NOT loading a saved game then pass 0 as the argument (default starter level index)
                        PlayState ps = new PlayState(this, eng, 0);

                        testFile.Stop();

                        // Otherwise pass the level index from the saved game
                        //PlayState ps = new PlayState(saved_level_index);
                        eng.ChangeState(ps);
                        eng.GameInProgress = true;
                    }
                }
                if (_cur_butn == 1)
                {
                    // load saved game pressed
                    //transition into PlayState
                    if (eng.GameInProgress)
                    {
                        eng.PopState();
                    }
                    else
                    {
                        // If you're NOT loading a saved game then pass 0 as the argument (default starter level index)
                        testFile.Stop();

                        // Otherwise pass the level index from the saved game
                        PlayState ps = new PlayState(this, eng, saved_level_index);
                        eng.ChangeState(ps);
                        eng.GameInProgress = true;
                    }
                }
                if (_cur_butn == 2)
                {
                    eng.Exit();
                }
            }
            else clickdown = false;
        }

        private void DealWithInput()
        {
            // Testing buttons
            //if (eng.Keyboard[Key.Down])
            OpenTK.Input.KeyboardState _new_state = OpenTK.Input.Keyboard.GetState();
            if ((_new_state.IsKeyDown(Key.Down) && !_old_state.IsKeyDown(Key.Down)) ||
                (_new_state.IsKeyDown(Key.S) && !_old_state.IsKeyDown(Key.S)))
            {
                // Down key was just pressed
                if (_cur_butn < 2)
                {
                    // Increment the current button index so you draw the highlighted button of the next button 
                    _cur_butn += 1;
                    eng.selectSound.Play();
                }
                else if (_cur_butn >= 2)
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
                    _cur_butn = 2;
                    eng.selectSound.Play();
                }
            }
            _old_state = _new_state;

            //TODO: Change these keys to their final mappings when determined

            if (eng.Keyboard[Key.Q])
            {
                eng.Exit();
            }

            //********************** enter
            if (eng.Keyboard[Key.Enter] && !enterdown)
            {
                enterdown = true;
                if (_cur_butn == 0)
                {
                    //transition into PlayState
                    if (eng.GameInProgress)
                    {
                        eng.PopState();
                    }
                    else
                    {
                        // If you're NOT loading a saved game then pass 0 as the argument (default starter level index)
                        PlayState ps = new PlayState(this, eng, 0);

                        testFile.Stop();

                        // Otherwise pass the level index from the saved game
                        //PlayState ps = new PlayState(saved_level_index);
                        eng.ChangeState(ps);
                        eng.GameInProgress = true;
                    }
                }
                if (_cur_butn == 1)
                {
                    // load saved game pressed
                    //transition into PlayState
                    if (eng.GameInProgress)
                    {
                        eng.PopState();
                    }
                    else
                    {
                        // If you're NOT loading a saved game then pass 0 as the argument (default starter level index)
                        testFile.Stop();

                        // Otherwise pass the level index from the saved game
                        PlayState ps = new PlayState(this, eng, saved_level_index);
                        eng.ChangeState(ps);
                        eng.GameInProgress = true;
                    }
                }
                if (_cur_butn == 2)
                {
                    eng.Exit();
                }
            }
            else if (!eng.Keyboard[Key.Enter])
                enterdown = false;
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
