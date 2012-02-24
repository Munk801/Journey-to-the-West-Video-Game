using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using OpenTK;
using OpenTK.Input;

namespace U5Designs
{
    public class GameMouse
    {
        public MouseDevice Mouse;
        GameEngine Window;
        private bool PreviousLCState;
        private bool CurrentLCState;

        public GameMouse(GameEngine eng)
        {
            this.Mouse = eng.Mouse;
            Window = eng;
        }

        public bool inButtonRegion(Texture texture)
        {
            if (Mouse.X - Window.Width / 2 > texture.XLoc && Mouse.X - Window.Width / 2 < texture.XLoc + texture.Width
                && -(Mouse.Y - Window.Height / 2) < texture.YLoc && -(Mouse.Y - Window.Height / 2) > texture.YLoc - texture.Height)
            {
                return true;
            }
            else return false;
        }

        public bool LeftPressed()
        {
            return (Mouse[MouseButton.Left]);
        }


        public Vector3 GetVectorFromPlayerToClick(Vector3 PlayerLocation)
        {
            Vector3 projectileVector = new Vector3();


            return projectileVector;
        }

        public Vector4 UnProject(ref Matrix4 projection, Matrix4 view, int viewportWidth, int viewportHeight, Vector2 Mouse)
        {
            Vector4 v;

            v.X = 2.0f * Mouse.X / (float)viewportWidth - 1;
            v.Y = -(2.0f * Mouse.Y/ (float)viewportHeight -1);
            v.Z = 0;
            v.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref v, ref projInv, out v);
            Vector4.Transform(ref v, ref viewInv, out v);

            if (v.W > float.Epsilon || v.W < float.Epsilon)
            {
                v.X /= v.W;
                v.Y /= v.W;
                v.Z /= v.W;
            }

            return v;
        }

    }
}
