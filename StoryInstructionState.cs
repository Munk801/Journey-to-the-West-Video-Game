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

using QuickFont;

namespace U5Designs
{
    class StoryInstructionState : GameState
    {
        internal GameEngine eng;
        
        protected Vector3 eye, lookat;
        Obstacle background;
        MainMenuState menu;

        Texture _p1, _p2, _p3, _p4;
		double curFrame;
        bool escapedown;
        internal int cur_page;

        // Fonts
        internal QFont title, button, buttonHighlight;

        List<String> _buttons;
        double cnt;
        Texture bg;
        int _cur_butn = 0;
        private int numOfButtons = 2;        
        OpenTK.Input.KeyboardState _old_state;
        public bool enterdown;

        String page1 = @"Once upon a time lived a young monkey in a dreary metropolitan zoo.  He spent his life trapped within the cold iron bars of his cage watching the world pass him by.  But there was more to this monkey than the strangers moving past him could realize..." + Environment.NewLine;
               
               
        String page2 = "For within this monkey lived a spirit that no cage could contain, though he had yet to fully realize this.  All he knew was that there had to be more to life than this.  That somewhere, out there in the strange and wonderful world, was his Destiny." + Environment.NewLine;
        String page3 = "And so it was one fateful night, when the mean Zoo Keeper was not watching, the monkey snatched the key to his cage off his belt.  And when the Zoo Keeper turned his back the monkey quietly unlocked his cage door and took the first steps on a path that would change his life forever...";
        String instructions1, instructions2, instructions3;
        // End Fonts

        public StoryInstructionState(GameEngine engine, MainMenuState menustate)
        {
            cur_page = 0;
            eng = engine;
            menu = menustate;
            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

          
            // QFont
           // Assembly assembly = Assembly.GetExecutingAssembly();
            //eng.StateTextureManager.LoadTexture("menu", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.menu_plain.png"));
            //bg = eng.StateTextureManager.GetTexture("menu");

            _buttons = new List<String>();
			instructions1 = "D - Moves Forward" + Environment.NewLine + "A - Moves Back" + Environment.NewLine + "S - Looks Down";
			instructions2 = "W - Moves Forward" + Environment.NewLine + "A - Moves Left" + Environment.NewLine + "D - Moves Right" + Environment.NewLine +
				"S - Moves Back";
			instructions3 = "Tab - Switch Views" + Environment.NewLine + "Space - Jump" + Environment.NewLine + "Left Mouse - Fire Projectile" + Environment.NewLine +
				"Right Mouse - Spin Attack" + Environment.NewLine + "E - Coconut Grenade" + Environment.NewLine + "ESC - Pause";

//             instructions = "W - Moves Forward in 3D" + Environment.NewLine + "A - Moves Left in 3D" + Environment.NewLine + "D - Moves Right in 3D" + Environment.NewLine +
//                 "    Forward in 2D" + Environment.NewLine + "S - Moves Back in 3D" + Environment.NewLine + "Space - Jump" + Environment.NewLine +
//                 "Left Mouse - Fire Projectile" + Environment.NewLine + "Right Mouse - Spin Attack" + Environment.NewLine + "E - Coconut Grenade" + Environment.NewLine +
//                 "ESC - Pause";
            
            
            //button = QFont.FromQFontFile("../../Fonts/myStoryWhite.qfont", new QFontLoaderConfiguration(true, false));
            button = QFont.FromQFontFile("../../Fonts/myStoryBright.qfont", new QFontLoaderConfiguration(true, false));
            
            button.Options.DropShadowActive = false;
            //title = QFont.FromQFontFile("myHappySans.qfont", new QFontLoaderConfiguration(true));
            title = QFont.FromQFontFile("../../Fonts/myRock.qfont", new QFontLoaderConfiguration(true, false));
            title.Options.DropShadowActive = false;
            //buttonHighlight = QFont.FromQFontFile("../../Fonts/myHappySans2.qfont", new QFontLoaderConfiguration(true));
            //buttonHighlight.Options.DropShadowActive = true;
            //QFont.CreateTextureFontFiles("Fonts/Rock.TTF", 48, "myRock"); // Use this to create new Fonts that you will texture
            // End QFonts

			// Set the current image to be displayed at 0 which is the first in the sequence
			curFrame = 0.0;
        }

		public override void MakeActive() {
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            // Fonts
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);
            // Fonts


			GL.MatrixMode(MatrixMode.Projection);
			Matrix4d projection = Matrix4d.CreateOrthographic(1280, 720, 1.0f, 6400.0f);
			GL.LoadMatrix(ref projection);
            if (eng.Keyboard[Key.Escape]) {
                escapedown = true;
            }
		}

        public override void Update(FrameEventArgs e)
        {
            // Deal with user input from either the keyboard or the mouse
            cnt += e.Time; // Fonts
            DealWithInput();
        }

        public override void Draw(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 0.1f);

            Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
			curFrame = (curFrame + e.Time * 2) % 4;

            // Fonts
            GL.Disable(EnableCap.DepthTest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
            
            //bg.Draw2DTexture();
            
            drawButtons();            
            
            // Fonts
            GL.Enable(EnableCap.DepthTest);
        }

        /// <summary>
        /// Called by mouse or keyboard handlers when the user picked a button (by clicking or hitting enter)
        /// </summary>
        internal void handleButtonPress()
        {            
            // Go to next page
            switch (cur_page)
            { 
                
                case 3:
                    PlayState ps = new PlayState(eng, menu, 0);
                    LoadScreenState ls = new LoadScreenState(eng, ps, 0);
                    eng.ChangeState(ls);
                    break;
            }
        }

        // Draw stuff
        public void drawButtons()
        {
            GL.PushMatrix();
            GL.Translate(new Vector3(0, 350, 0));
			GL.Scale(1, -1, 1);            
            float yOffset = 720 / (button.Measure("S").Height + 10);
            
            // Draw non highlighted string
            button.Options.DropShadowActive = true;
            //button.Print(s, new Vector2(eng.Width * 0.5f - (button.Measure(s).Width / 2), yOffset));

            if (cur_page == 0)
            {
				button.Print("2D", new Vector2(-320 - button.Measure("2D").Width / 2, yOffset));
				button.Print(instructions1, new Vector2(-320 - button.Measure(instructions1).Width / 2, yOffset + button.Measure("2D").Height));
				button.Print("3D", new Vector2(320 - button.Measure("3D").Width / 2, yOffset));
				button.Print(instructions2, new Vector2(320 - button.Measure(instructions2).Width / 2, yOffset + button.Measure("3D").Height));
				button.Print(instructions3, new Vector2(-button.Measure(instructions3).Width / 2, yOffset + 360));
                //PrintWithBounds(button, instructions, new RectangleF(-530, 10, 1280, 300), QFontAlignment.Left, ref yOffset);
                // Test
                //button.Print("[ Press Enter ] ", new Vector2(-button.Measure(instructions).Width - button.Measure("[ Press Enter ]").Width, button.Measure(instructions).Height + 10));
            }
            else if(cur_page == 1)
            {
                //button.Print(page1, new Vector2(-button.Measure(page1).Width / 2, yOffset));
                PrintWithBounds(button, page1, new RectangleF(-530, 10, 1060, 300), QFontAlignment.Left, ref yOffset);
            }
            else if (cur_page == 2)
            {
                //button.Print(page1, new Vector2(-button.Measure(page1).Width / 2, yOffset));
                PrintWithBounds(button, page2, new RectangleF(-530, 10, 1060, 300), QFontAlignment.Left, ref yOffset);
            }
            else if (cur_page == 3)
            {
                //button.Print(page1, new Vector2(-button.Measure(page1).Width / 2, yOffset));
                PrintWithBounds(button, page3, new RectangleF(-530, 10, 1060, 300), QFontAlignment.Left, ref yOffset);
            }
            yOffset += yOffset;              
            GL.PopMatrix();            
        }

        private void PrintWithBounds(QFont font, string text, RectangleF bounds, QFontAlignment alignment, ref float yOffset)
        {

            GL.Disable(EnableCap.Texture2D);
            GL.Color4(1.0f, 0f, 0f, 1.0f);


            float maxWidth = bounds.Width;

            float height = font.Measure(text, maxWidth, alignment).Height;

            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(bounds.X, bounds.Y, 0f);
            GL.Vertex3(bounds.X + bounds.Width, bounds.Y, 0f);
            GL.Vertex3(bounds.X + bounds.Width, bounds.Y + height, 0f);
            GL.Vertex3(bounds.X, bounds.Y + height, 0f);
            GL.End();

            font.Print(text, maxWidth, alignment, new Vector2(bounds.X, bounds.Y));

            // Print ENTER
            string s = "[ Press Enter ]";
            font.Print(s, maxWidth, alignment, new Vector2(-bounds.X - (font.Measure(s).Width+10), bounds.Height + 2*font.Measure(text).Height));
            //font.Print(s, maxWidth, QFontAlignment.Right, new Vector2(bounds.X + bounds.Width - font.Measure(s).Height, bounds.Y + bounds.Height - font.Measure(s).Height));

            yOffset += height;

        }
        private void DealWithInput()
        {            
            // Fonts
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
                                
                handleButtonPress();

                // Move to next page
                if (cur_page < 3)
                    cur_page++;
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
