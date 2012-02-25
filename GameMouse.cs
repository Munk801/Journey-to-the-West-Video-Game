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


        public Vector3d Get2DVectorFromPlayerToClick(Vector3 PlayerLocation, Vector3d unprojectedMouse)
        {
            Vector3d projectileVector = new Vector3d();
            Vector3d convPlayer;
            // Vector3D does not do an internal cast.  
            unprojectedMouse.Z = 50.0;
            convPlayer.X = PlayerLocation.X;
            convPlayer.Y = PlayerLocation.Y;
            convPlayer.Z = PlayerLocation.Z;

            projectileVector = convPlayer + unprojectedMouse;

            return projectileVector;
        }


        /// <summary>
        /// Converts mouse coords to screen coordinates
        /// </summary>
        /// <param name="mCoord"> Coordinates on the screen that you are looking to convert</param>
        /// <param name="model"> Model view matrix.</param>
        /// <param name="projection"> The projection matrix.  Orthographic for 2D and perspective for 3D</param>
        /// <param name="viewport"> Viewport Array. [0] - X [1] - Y [2] - Width [3] - Height</param>
        /// <returns></returns>
        public Vector3d UnProject(Vector3d mCoord, Matrix4d model, Matrix4d projection, int[] viewport)
        {
            Vector3d v = new Vector3d(0.0f, 0.0f, 0.0f);
            Matrix4d modelProj;
            Vector4d screen;

            // Concatenate the model and projection matrices
            modelProj = Matrix4d.Mult(model, projection);
            modelProj.Invert();

            // Initalize our screen vector
            screen.X = mCoord.X;
            screen.Y = mCoord.Y;
            screen.Z = mCoord.Z;
            screen.W = 1.0;

            // Map X and Y coordinates from the screen
            screen.X = (screen.X - viewport[0]) / viewport[2];
            screen.Y = (screen.Y - viewport[1]) / viewport[3];

            // Convert to canonical coord
            screen.X = screen.X * 2 - 1;
            screen.Y = screen.Y * 2 - 1;
            screen.Z = screen.Z * 2 - 1;
            
            // Transform the vector by the concatenated matrix
            Vector4d newCoord = Vector4d.Transform(screen, modelProj);

            // Perform Homogeneous Divide
            if (newCoord.W == 0.0) return v;
            v.X = newCoord.X / newCoord.W;
            v.Y = newCoord.Y / newCoord.W;
            v.Z = newCoord.Z / newCoord.W;
            
            // Return the vector 
            return v;
        }

    }
}
