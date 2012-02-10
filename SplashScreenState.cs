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
    class SplashScreenState : GameState
    {
        internal GameEngine eng;

        // A container which will hold the list of available saved games
        Stack<XmlNodeList> savedGameStates;
        Stack<string> savedGameChoices;
        int saved_level_index = -1;

        protected Vector3 eye, lookat;
        Obstacle background;


        public SplashScreenState(GameEngine engine)
        {
            eng = engine;

            //AudioManager.Manager.StartAudioServices();


            savedGameStates = new Stack<XmlNodeList>();
            savedGameChoices = new Stack<string>();

            // Graphics
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Normalize);

            //test.Play();

            //AudioManager.Manager.StartAudioServices();

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(853, 480, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);


            // TO CHANGE, PUT THE U5 LOGO
            SpriteSheet.quad = new ObjMesh("../../Geometry/quad.obj");
            int[] cycleStarts = { 0 };
            int[] cycleLengths = { 1 };
            SpriteSheet ss = new SpriteSheet(new Bitmap("../../Geometry/testbg.png"), cycleStarts, cycleLengths, 853, 480);
            Bitmap test = new Bitmap("../../Geometry/testbg.png");
            background = new Obstacle(new Vector3(0, 0, 2), new Vector3(426.5f, 240, 1), new Vector3(0,0,0), true, true, ss);


        }

        public override void Update(FrameEventArgs e)
        {
            DealWithInput();

            // UPDATE POSSIBLE FADE IN FADE OUT FOR SCREEN STATE

            // AFTER A CERTAIN TIME, POP THIS STATE

        }

        public override void Draw(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            GL.PushMatrix();
            GL.Translate(0, 0, 2);
            GL.Scale(426.5f, 240, 1);
            ((RenderObject)background).sprite.draw(false);
        }

        private void DealWithInput()
        {
            //TODO: Change these keys to their final mappings when determined

            if (eng.Keyboard[Key.Q])
            {
                eng.Exit();
            }

            //********************** enter
            if (eng.Keyboard[Key.Enter])
            {
                //transition into PlayState
                if (eng.GameInProgress)
                {
                    eng.PopState();
                }
                else
                {
                    //TODO update this to whatever this state does when it ends.
                    // the only thing that should ever start a new play state is MainMenuState(as thats the state that will be loading saved games)
                  /*  PlayState ps = new PlayState(this, eng, 0);

                    // Otherwise pass the level index from the saved game
                    //PlayState ps = new PlayState(saved_level_index);
                    eng.ChangeState(ps);
                    eng.GameInProgress = true;
                   */
                }
            }
        }

    }
}
