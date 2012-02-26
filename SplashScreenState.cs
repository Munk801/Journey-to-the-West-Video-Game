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

using Tao.DevIl;

// P/Invoke
using System.Runtime.InteropServices;
 
namespace U5Designs
{
    /** Main Menu State of the game that will be active while the Main Menu is up **/
    class SplashScreenState : GameState
    {
        internal GameEngine eng;
        double timeTilMain = 0.0f;
        Texture logo;
        
//        // WILL NEED TO BE MOVED SOMEWHERE ELSE

        public SplashScreenState(GameEngine engine)
        {
            eng = engine;

            eng.StateTextureManager.LoadTexture("logo", "../../Resources/Textures/u5_logo.jpg");
            logo = eng.StateTextureManager.GetTexture("logo");

            // Graphics
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            GL.Enable(EnableCap.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, logo.Id);

            //GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(1280, 720, 0.0f, 1.0f);
            GL.LoadMatrix(ref projection);

        }

        public override void Update(FrameEventArgs e)
        {
            timeTilMain += e.Time;
            
            //transition into PlayState
            //if (eng.GameInProgress)
            //{
            //    eng.PopState();


            //}
#if DEBUG
			if(eng.Keyboard[Key.Enter]) {
				eng.ChangeState(new PlayState(new MainMenuState(eng), eng, 0));
				eng.GameInProgress = true;
			}
#endif

            if (timeTilMain > 2)
            {
                MainMenuState ms = new MainMenuState(eng);
                //eng.StateTextureManager.Dispose();
                eng.ChangeState(ms);
                //eng.GameInProgress = true;
            }
        }

        public override void Draw(FrameEventArgs e)
        {
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            logo.Draw2DTexture(0, 0, 0.5f, 0.5f);   
        }
    }
}
