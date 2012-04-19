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

        // Fonts
        internal QFont title, button, buttonHighlight;
        List<String> _buttons;
        double cnt;
        Texture bg;
        int _cur_butn = 0;
        private int numOfButtons = 2;        
        OpenTK.Input.KeyboardState _old_state;
        public bool enterdown;

        String page1 = @"Once upon a time lived a young monkey in a dreary metropolitan Zoo.  He spent his life trapped within the cold iron bars of his cage watching as the world seemed to pass him by.  But there was more to this monkey than the strangers moving past him could realize..." + Environment.NewLine + Environment.NewLine +
               "For within this monkey lived a spirit that no cage could contain, though he had yet to fully realize this.  All he knew was that there was more to his life than this.  That somewhere, out there in the strange and wonderful world, was his Destiny." + Environment.NewLine + Environment.NewLine +
               "And so it was that one fateful night when the mean Zoo Keeper was not watching the monkey snatched the key to his cage off his belt.  And when the Zoo Keeper turned his back the monkey quitely unlocked his cage door and took the first steps on a path that would change his life forever...";
        String page2;
        String page3;
        // End Fonts

        public StoryInstructionState(GameEngine engine, MainMenuState menustate)
        {
            eng = engine;
            menu = menustate;
            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);
//             _p1 = eng.StateTextureManager.GetTexture("p1");
//             _p2 = eng.StateTextureManager.GetTexture("p2");
//             _p3 = eng.StateTextureManager.GetTexture("p3");
//             _p4 = eng.StateTextureManager.GetTexture("p4");

            // QFont
            Assembly assembly = Assembly.GetExecutingAssembly();
            eng.StateTextureManager.LoadTexture("menu", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.menu.png"));
            bg = eng.StateTextureManager.GetTexture("menu");

            _buttons = new List<String>();
            _buttons.Add("W - Moves Forward in 3D");
            _buttons.Add("A - Moves Left in 3D");
            _buttons.Add("D - Moves Right in 3D, Forward in 2D");
            _buttons.Add("S - Moves Back in 3D");
            _buttons.Add("Space - Jump");
            _buttons.Add("C - Fly if you have the Nimbus Cloud");
            _buttons.Add("Left Mouse - Fire Projectile");
            _buttons.Add("Right Mouse - Spin Attack");
            _buttons.Add("E - Coconut Grenade");
            _buttons.Add("ESC - Pause");
            
            button = QFont.FromQFontFile("../../Fonts/myStory.qfont", new QFontLoaderConfiguration(true, false));
            button.Options.DropShadowActive = true;
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
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

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
            
            bg.Draw2DTexture();
            
            drawButtons();            
            
            // Fonts
            GL.Enable(EnableCap.DepthTest);
        }

        /// <summary>
        /// Called by mouse or keyboard handlers when the user picked a button (by clicking or hitting enter)
        /// </summary>
        internal void handleButtonPress()
        {
            switch (_cur_butn)
            {
                case 0: //Continue
                    // Exit Paused Menu state and return to playing				
                    eng.PopState();
                    break;
				case 1: //Main menu
					eng.GameInProgress = false;
					menu.enterdown = true;
					eng.ChangeState(menu);
					break;
                case 2: //Quit
                    eng.Exit();
                    break;
                
            }
        }

        // Draw stuff
        public void drawButtons()
        {            
			GL.PushMatrix();
			GL.Scale(1, -1, 1);            
            float startY = 0.0f;
            title.Print("CONTROLS", new Vector2(-(title.Measure("PAUSED").Width / 2), startY - (title.Measure("A").Height + 10)));
            float yOffset = eng.Height / (title.Measure("S").Height + 10);
            foreach (string s in _buttons)
            {
                GL.PushMatrix();
                // Draw non highlighted string
                button.Options.DropShadowActive = true;
                //button.Print(s, new Vector2(eng.Width * 0.5f - (button.Measure(s).Width / 2), yOffset));
                button.Print(s, new Vector2(0, yOffset));                
                yOffset += yOffset;  
                GL.PopMatrix();             
            }
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

            yOffset += height;

        }
        private void DealWithInput()
        {
            /*
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
             * */

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
