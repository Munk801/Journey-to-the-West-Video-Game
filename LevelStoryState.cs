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
    class LevelStoryState : GameState
    {
        internal GameEngine eng;

        protected Vector3 eye, lookat;
        Obstacle background;
        PlayState _ps;

        Texture _p1, _p2, _p3, _p4;
        double curFrame;
        bool escapedown;
        internal int cur_page;

        // Fonts
        internal QFont button, credit;

        
        double cnt;
        Texture bg;
        int _cur_butn = 0;
        private int numOfButtons = 2;

        int lvl;
        OpenTK.Input.KeyboardState _old_state;
        public bool enterdown;

        String level_1 = "With the defeat of the Zoo Keeper our hero finally escapes the Zoo.  He now finds himself in a mysterious Jungle.  With no going back he pushes onward into the unknown...";
		String credits = "Game Finished\nThanks For Playing\n\n\nDylan Riddle - Game Designer\nKendal Gifford - Game Designer\nSeth Walsh - Game Designer\nStephen Lu - Game Designer\nKacy Shearer - Artist\nRae Montilla - Artist\n\n(Press Enter to return to Main Menu)";
        // End Fonts

        public LevelStoryState(GameEngine engine, int lvl_being_loaded)
        {
            cur_page = 0;
            eng = engine;
           // _ps = ps;
            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

            lvl = lvl_being_loaded;

            // QFont
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //eng.StateTextureManager.LoadTexture("menu", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.menu.png"));
            //bg = eng.StateTextureManager.GetTexture("menu");

            credit = QFont.FromQFontFile("Fonts/myStoryWhite.qfont", new QFontLoaderConfiguration(true, false));
            credit.Options.DropShadowActive = false;

            button = QFont.FromQFontFile("Fonts/myStoryBright.qfont", new QFontLoaderConfiguration(true, false));
            button.Options.DropShadowActive = true;
            
            // Set the current image to be displayed at 0 which is the first in the sequence
            curFrame = 0.0;
        }

        public override void MakeActive()
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            // Fonts
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);
            // Fonts


            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d projection = Matrix4d.CreateOrthographic(1280, 720, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);
            if (eng.Keyboard[Key.Escape])
            {
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
            if (lvl == eng.max_level)
            {
                MainMenuState ms = new MainMenuState(eng);
                eng.ChangeState(ms);
            }
            else
            {
                // Go to next level
                MainMenuState ms = new MainMenuState(eng);
                PlayState ps = new PlayState(eng, ms, lvl);
                LoadScreenState ls = new LoadScreenState(eng, ps, lvl);
                eng.ChangeState(ls);
            }
        }

        // Draw stuff
        public void drawButtons()
        {
            GL.PushMatrix();
            GL.Translate(new Vector3(0, 350, 0));
            GL.Scale(1, -1, 1);
            float yOffset = 0;
            // Credits
            if (lvl == 2)
            {                
                PrintWithBounds(credit, credits, new RectangleF(-0, 10, 1280, 300), QFontAlignment.Centre, ref yOffset);
            }
            else
            {
                yOffset = 720 / (button.Measure("S").Height + 10);

                // Draw non highlighted string
                button.Options.DropShadowActive = true;
                PrintWithBounds(button, level_1, new RectangleF(-530, 10, 1060, 300), QFontAlignment.Left, ref yOffset);
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
            font.Print(s, maxWidth, alignment, new Vector2(-bounds.X - (font.Measure(s).Width + 10), bounds.Height + 2 * font.Measure(text).Height));
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
    }
}
