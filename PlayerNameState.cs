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
    class PlayerNameState : GameState
    {
        internal GameEngine eng;
        Stack<XmlNodeList> savedGameStates;
        Stack<string> savedGameChoices;
        MainMenuState _ms;
        QFont saveFont, title, saveFontHighlighted;


        /** Naming Stuff **/
        String name;
        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();        
        int _max_name_length = 20; // Set the max length for the character name here
        float start_x;

        // A container which will hold the list of available saved games        
        protected Vector3 eye, lookat;
        MouseDevice mouse;

        // testing buttons
        //Obstacle play_button_npress, play_button_press, load_button_npress, load_button_press, quit_button_press, quit_button_npress;
        private int numOfButtons;
        int _cur_butn = 0;
        OpenTK.Input.KeyboardState _old_state;

        // New Buttons
        Texture menu;
        public bool enterdown, escapedown;
        public bool clickdown = false;
        double cnt;

        public PlayerNameState(GameEngine engine, MainMenuState ms)
        {
            eng = engine;
            mouse = eng.Mouse;
            savedGameStates = new Stack<XmlNodeList>();
            savedGameChoices = new Stack<string>();
            _ms = ms;

            Assembly assembly = Assembly.GetExecutingAssembly();
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

            _old_state = OpenTK.Input.Keyboard.GetState(); // Get the current state of the keyboard

            eng.StateTextureManager.LoadTexture("menu", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.menu.png"));
            menu = eng.StateTextureManager.GetTexture("menu");

            saveFont = QFont.FromQFontFile("Fonts/myHappySans.qfont", new QFontLoaderConfiguration(true));
            saveFont.Options.DropShadowActive = true;

            start_x = saveFont.Measure("Name: ").Width;

            //title = QFont.FromQFontFile("myHappySans.qfont", new QFontLoaderConfiguration(true));
            title = QFont.FromQFontFile("Fonts/myRock.qfont", new QFontLoaderConfiguration(true));
            title.Options.DropShadowActive = true;

            name = "";          

            numOfButtons = savedGameChoices.Count - 1;
        }

        public override void MakeActive()
        {
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
            cnt += e.Time;
        }

        public override void Draw(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ClearColor(0.15f, 0.15f, 0.15f, 1.0f);
            Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);  
            GL.Disable(EnableCap.DepthTest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

            // Draw the background
            menu.Draw2DTexture();

            drawSaves();
            GL.Enable(EnableCap.DepthTest);
        }

        // Draw stuff
        public void drawSaves()
        {
			GL.PushMatrix();
			GL.Scale(1, -1, 1);

            float startY = -(title.Measure("A").Height * 2.0f);
            title.Print("Character Name", new Vector2(-(title.Measure("Character Name").Width / 2), startY));
            title.Options.DropShadowActive = false;

			// Draw non highlighted string
            float yOffset = startY + title.Measure("C").Height + 10;
            saveFont.Options.DropShadowActive = false;
            saveFont.Print("Name: " + name, new Vector2(-(start_x), yOffset));
            GL.PopMatrix();
        }

        private void DealWithInput()
        {
            OpenTK.Input.KeyboardState _new_state = OpenTK.Input.Keyboard.GetState();
//             if ((_new_state.IsKeyDown(Key.Down) && !_old_state.IsKeyDown(Key.Down)) ||
//                 (_new_state.IsKeyDown(Key.S) && !_old_state.IsKeyDown(Key.S)))
//             {
//                 // Down key was just pressed                
//                 if (_cur_butn < numOfButtons)
//                 {
//                     // Increment the current button index so you draw the highlighted button of the next button 
//                     _cur_butn += 1;
//                     eng.selectSound.Play();
//                 }
//                 else if (_cur_butn >= numOfButtons)
//                 {
//                     // Were on the last button in the list so reset to the top of the button list
//                     _cur_butn = 0;
//                     eng.selectSound.Play();
//                 }
//             }
//             if ((_new_state.IsKeyDown(Key.Up) && !_old_state.IsKeyDown(Key.Up)) ||
//                 (_new_state.IsKeyDown(Key.W) && !_old_state.IsKeyDown(Key.W)))
//             {
//                 // Down key was just pressed
//                 if (_cur_butn > 0)
//                 {
//                     // Increment the current button index so you draw the highlighted button of the next button 
//                     _cur_butn -= 1;
//                     eng.selectSound.Play();
//                 }
//                 else if (_cur_butn <= 0)
//                 {
//                     // Were on the last button in the list so reset to the top of the button list
//                     _cur_butn = numOfButtons;
//                     eng.selectSound.Play();
//                 }
//             }

            // Loop over all available keys and type out to the screen
            for (int i = 0; i < (int)Key.LastKey; i++)
            {
                 Key k = (Key)i; // 49 = enter 
                 bool current = _new_state.IsKeyDown(k);
                 bool previous = _old_state.IsKeyDown(k);
                 if (current && !previous)
                 {
                     // new key pressed
                     //NewKeyDownDetected(k);
                     for (int i2 = 0; i2 < alpha.Length; i2++)
                     {
                         if (k.ToString().CompareTo(alpha[i2].ToString()) == 0)
                         {
                             name += k.ToString();
                         }
                     }
                     if (i == 53 && name.Length > 0)
                     {
                         name = name.Remove(name.Length - 1);
                     }
                 }
                 else if (!current && previous)
                 {
                     //NewKeyUpDetected(k);                     
                 }
                 else
                 {
                     // Constant keypress
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
				loadPlayState(0);  // Load game
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

        /** This loads the chosen game **/
        internal void loadPlayState(int lvl)
        {
            eng._player_name = name;
            //StoryInstructionState sis = new StoryInstructionState(eng, _ms);
            //eng.ChangeState(sis);
            PlayState ps = new PlayState(eng, _ms, lvl);
            LoadScreenState ls = new LoadScreenState(eng, ps, lvl);
            eng.ChangeState(ls);
        }
    }
}
