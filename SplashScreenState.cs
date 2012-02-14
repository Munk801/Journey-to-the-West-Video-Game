﻿using System;
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

namespace U5Designs
{
    /** Main Menu State of the game that will be active while the Main Menu is up **/
    class SplashScreenState : GameState
    {
        internal GameEngine eng;

        protected Vector3 eye, lookat;
        Obstacle background;

        double timeTilMain = 0.0f;
        
//        // WILL NEED TO BE MOVED SOMEWHERE ELSE
        TextureManager StateTextureManager;
        Texture logo;

        public SplashScreenState(GameEngine engine)
        {
            eng = engine;

            // WILL NEED TO BE PLACED SOMEWHERE ELSE LATER
            TextureManager texturemanager = new TextureManager();
            StateTextureManager = texturemanager;
            Il.ilInit();
            Ilu.iluInit();
            Ilut.ilutInit();

            Ilut.ilutRenderer(Ilut.ILUT_OPENGL);

            texturemanager.LoadTexture("logo", "../../Resources/test.tga");

            Texture texture = StateTextureManager.GetTexture("logo");
            logo = texture;
            // Graphics
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            GL.Enable(EnableCap.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, texture.Id);

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
             if (timeTilMain > 2)
            {
                MainMenuState ms = new MainMenuState(eng);

                eng.ChangeState(ms);
                //eng.GameInProgress = true;
            }
        }

        public override void Draw(FrameEventArgs e)
        {
            logo.Draw2DTexture(logo.Width, logo.Height);   
        }
    }
}