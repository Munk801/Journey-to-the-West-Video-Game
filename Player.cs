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

namespace U5Designs
{
    class Player
    {
        public PlayerState p_state;
        public bool shifter; 

        //position
        float x = 0;
        float y = 0;
        float z = 0;

        public Player()
        {
            p_state = new PlayerState("TEST player");
            p_state.setSpeed(3);
            x = 50f;
            y = 12.5f;
            z = 50f;
        }

        /**
         * Sets the PlayerState elements to the current Player values.  Call this method every update or simply when the state changes.  This will be used to store
         * the Players State when saving the game.
         * */
        public void updateState(bool enable3d, bool a, bool s, bool d, bool w)
        {
            //TODO: add control for other buttons, jump, projectile etc
            if (enable3d)
            {
                if (w)
                    x = x + (1f *(float)p_state.getSpeed());
                if (s)
                    x = x - (1f * (float)p_state.getSpeed());
                if (d)
                    z = z + (1f * (float)p_state.getSpeed());
                if (a)
                    z = z - (1f * (float)p_state.getSpeed());
            }
            else
            {
                if (d)
                    x = x + (1f * (float)p_state.getSpeed());
                if (a)
                    x = x - (1f * (float)p_state.getSpeed());
            }


        }


        protected float[] redAmbient = { 0.4f, 0.0f, 0.0f, 1.0f };
        protected float[] redDiffuse = { 0.4f, 0.0f, 0.0f, 1.0f };
        protected float[] redSpecular = { 1.0f, 1.0f, 1.0f, 1.0f };
        protected float[] redShininess = { 0.5f };
        protected byte[] cubeIndices = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 };

        public void draw()
        {
            //Test cube
            GL.PushMatrix();
            GL.Translate(x, y, z);
            GL.Scale(12.5f, 12.5f, 12.5f);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, redSpecular);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, redDiffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, redAmbient);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, redShininess);
            GL.DrawElements(BeginMode.Quads, 24, DrawElementsType.UnsignedByte, cubeIndices);
            GL.PopMatrix();
        }
    }
}
