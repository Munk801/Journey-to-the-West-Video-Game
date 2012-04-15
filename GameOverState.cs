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
using System.Reflection;


namespace U5Designs {
    class GameOverState : GameState {

        internal GameEngine eng;
        
        protected Vector3 eye, lookat;
        Obstacle background;
        MainMenuState menu;
        float xf, yf;
        float arX, b1Y, b2Y, b3Y, b4Y;
        Texture goBackground, arrow, mainmenu, menu_pressed, restart, restart_pressed, exit, exit_pressed;
        public bool enterdown, escapedown;
        double timer;
        double timeTillNextState;

        private int numOfButtons = 2;
        int _cur_butn = 0;
        OpenTK.Input.KeyboardState _old_state;

        public GameOverState(MainMenuState prvstate, GameEngine engine)
        {
            eng = engine;
            menu = prvstate;

            _old_state = OpenTK.Input.Keyboard.GetState();

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);
            xf = 1.0f;
            yf = 1.0f;

            // TO DO: CHANGE THE GAME OVER SCREEN
            eng.StateTextureManager.RenderSetup();
            Assembly audAssembly = Assembly.GetExecutingAssembly();
            eng.StateTextureManager.LoadTexture("go", audAssembly.GetManifestResourceStream("U5Designs.Resources.Textures.GameOverTextures.gameover.png"));
            goBackground = eng.StateTextureManager.GetTexture("go");
            eng.StateTextureManager.LoadTexture("arrow", audAssembly.GetManifestResourceStream("U5Designs.Resources.Textures.GameOverTextures.arrow.png"));
            arrow = eng.StateTextureManager.GetTexture("arrow");
            eng.StateTextureManager.LoadTexture("back2menu", audAssembly.GetManifestResourceStream("U5Designs.Resources.Textures.GameOverTextures.btn_backtomenu.png"));
            mainmenu = eng.StateTextureManager.GetTexture("back2menu");
            eng.StateTextureManager.LoadTexture("back2menupress", audAssembly.GetManifestResourceStream("U5Designs.Resources.Textures.GameOverTextures.btn_backtomenu_hover.png"));
            menu_pressed = eng.StateTextureManager.GetTexture("back2menupress");
            eng.StateTextureManager.LoadTexture("restartlevel", audAssembly.GetManifestResourceStream("U5Designs.Resources.Textures.GameOverTextures.btn_restart.png"));
            restart = eng.StateTextureManager.GetTexture("restartlevel");
            eng.StateTextureManager.LoadTexture("restartlevelpress", audAssembly.GetManifestResourceStream("U5Designs.Resources.Textures.GameOverTextures.btn_restart_hover.png"));
            restart_pressed = eng.StateTextureManager.GetTexture("restartlevelpress");
            eng.StateTextureManager.LoadTexture("exitgame", audAssembly.GetManifestResourceStream("U5Designs.Resources.Textures.GameOverTextures.btn_exit.png"));
            exit = eng.StateTextureManager.GetTexture("exitgame");
            eng.StateTextureManager.LoadTexture("exitpress", audAssembly.GetManifestResourceStream("U5Designs.Resources.Textures.GameOverTextures.btn_exit_hover.png"));
            exit_pressed = eng.StateTextureManager.GetTexture("exitpress");
            
            //restart_btn = eng.StateTextureManager.GetTexture("restart");
            
            //quit_btn = eng.StateTextureManager.GetTexture("quit_button");

            arX = -200.0f;
            b1Y = -50.0f;
            b2Y = -100.0f;
            b3Y = -150.0f;

            timeTillNextState = 4;
        }

		public override void MakeActive() {
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

			Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref modelview);

			GL.MatrixMode(MatrixMode.Projection);
            Matrix4d projection = Matrix4d.CreateOrthographic(1280, 720, 1.0f, 6400.0f);
			GL.LoadMatrix(ref projection);
		}

        public override void Update(FrameEventArgs e)
        {
            DealWithInput();
            //timer = timer + e.Time;
            //if (timer > timeTillNextState) {
            //    Console.WriteLine("Exiting Game overstate");
            //    eng.GameInProgress = false;
            //    menu.enterdown = true;
            //    eng.ChangeState(menu);
            //}
        }

        public override void Draw(FrameEventArgs e)
        {
			GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			//We shouldn't need to reset all this camera stuff here, but if we don't we're getting screwy bugs sometimes...
			Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref modelview);

			GL.MatrixMode(MatrixMode.Projection);
			Matrix4d projection = Matrix4d.CreateOrthographic(1280, 720, 1.0f, 6400.0f);
			GL.LoadMatrix(ref projection);

            goBackground.Draw2DTexture();

            switch (_cur_butn)
            {
                case 0:
                    arrow.Draw2DTexture(arX, b1Y, 1.0f, 1.0f);
                    restart_pressed.Draw2DTexture(0, b1Y, 1.0f, 1.0f);
                    mainmenu.Draw2DTexture(0, b2Y);
                    exit.Draw2DTexture(0, b3Y);
                    break;
                case 1:
                    arrow.Draw2DTexture(arX, b2Y);
                    restart.Draw2DTexture(0, b1Y, 1.0f, 1.0f);
                    menu_pressed.Draw2DTexture(0, b2Y);
                    exit.Draw2DTexture(0, b3Y);
                    break;
                case 2:
                    arrow.Draw2DTexture(arX, b3Y);
                    restart.Draw2DTexture(0, b1Y, 1.0f, 1.0f);
                    mainmenu.Draw2DTexture(0, b2Y);
                    exit_pressed.Draw2DTexture(0, b3Y);
                    break;
            }
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

        /// <summary>
		/// Called by mouse or keyboard handlers when the user picked a button (by clicking or hitting enter)
		/// </summary>
        internal void handleButtonPress()
        {
            switch (_cur_butn)
            {
                case 0: //new game
                    loadPlayState(0);
                    break;
                case 1: //load saved game
                    eng.GameInProgress = false;
                    menu.enterdown = true;
                    eng.ChangeState(menu);
                    break;
                case 2: //quit
                    eng.Exit();
                    break;
            }
        }
        internal void loadPlayState(int lvl)
        {
            PlayState ps = new PlayState(eng, menu);
            LoadScreenState ls = new LoadScreenState(eng, ps, lvl);
            eng.ChangeState(ls);
        }
    }
}
