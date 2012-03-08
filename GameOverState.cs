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


namespace U5Designs {
    class GameOverState : GameState {

        internal GameEngine eng;
        
        protected Vector3 eye, lookat;
        Obstacle background;
        MainMenuState menu;
        float xf, yf;
        Texture go_texture, restart_btn, quit_btn;

        public GameOverState(MainMenuState prvstate, GameEngine engine)
        {
            eng = engine;
            menu = prvstate;

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);
            xf = 1.0f;
            yf = 1.0f;

            eng.StateTextureManager.RenderSetup();
            eng.StateTextureManager.LoadTexture("game_over", "../../Resources/Textures/game_over_text.png");
           
            go_texture = eng.StateTextureManager.GetTexture("game_over");
            
            //restart_btn = eng.StateTextureManager.GetTexture("restart");
            
            //quit_btn = eng.StateTextureManager.GetTexture("quit_button");
        }

		public override void MakeActive() {
			
		}

        public override void Update(FrameEventArgs e)
        {
            DealWithInput();
        }

        public override void Draw(FrameEventArgs e)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            if (xf >= 0.4)
            {
                xf -= (float)(e.Time);
                yf -= (float)(e.Time);
                go_texture.Draw2DTexture(0, 0, xf, yf);
            }
            else
            {                
                go_texture.Draw2DTexture(0, 0, xf, yf);
                //restart_btn.Draw2DTexture(0, -400);
                //quit_btn.Draw2DTexture(0, -400);
            }            
        }

        private void DealWithInput()
        {
            if (eng.Keyboard[Key.Enter]) {
                // Exit Paused Menu state and return to playing
                Console.WriteLine("Exiting Game overstate");
                eng.GameInProgress = false;
                menu.enterdown = true;
                eng.ChangeState(menu);
            }
        }


    }
}
