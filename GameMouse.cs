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

    }
}
