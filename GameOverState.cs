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
        Texture goBackground, arrow, mainmenu, menu_pressed, restart, restart_pressed, exit, exit_pressed;

        double timer;
        double timeTillNextState;

        public GameOverState(MainMenuState prvstate, GameEngine engine)
        {
            eng = engine;
            menu = prvstate;

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
            timer = timer + e.Time;
            if (timer > timeTillNextState) {
                Console.WriteLine("Exiting Game overstate");
                eng.GameInProgress = false;
                menu.enterdown = true;
                eng.ChangeState(menu);
            }
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
        }

        private void DealWithInput()
        {
            if (eng.Keyboard[Key.Enter]) {
                // Exit game over and return to menu
                Console.WriteLine("Exiting Game overstate");
                eng.GameInProgress = false;
                menu.enterdown = true;
                eng.ChangeState(menu);
            }
        }


    }
}
