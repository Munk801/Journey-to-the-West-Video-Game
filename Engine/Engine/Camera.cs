using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 End;
        public int Width;
        public int Height;
        public bool isInTransition;
        public int timer;


        private int visX = 120;
        private int visY = 30;
        private int visZ = 100;

        private float transZ = 10;
        private float transY = 3;
        private float transX = 12;

        // TO DO: ADD CODE TO TRANSITION TO AND FROM 2D AND 3D

        public Camera(Vector3 position, Vector3 end, int width, int height)
        {
            Position = position;
            End = end;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// CreateLight: Creates a light slightly above and behind the camera
        /// </summary>
        /// <param name="cam"> Takes account the position of the camera being passed in</param>
        /// <returns></returns>
        public void CreateLight(Vector4 offset)
        {

            offset.X += Position.X;
            offset.Y += Position.Y;
            offset.Z += Position.Z;

            GL.Light(LightName.Light0, LightParameter.Position, offset);
        }

        public void Set2DCamera()
        {
            //timer = 10;
            SetOrthographic(Width, Height);
            End.X += 100;
            End.Y += 50;
            Position.X = End.X;
            Position.Y = End.Y;
            Position.Z = End.Z + visZ;
            CreateLight(new Vector4(50, 50, 0, 1));
        }

        public void Set3DCamera()
        {
            //timer = 10;
            SetPerspective(Width, Height);
            End.X -= 100;
            End.Y -= 50;
            Position.X = End.X - visX;
            Position.Y = End.Y + visY;
            Position.Z = End.Z;
            CreateLight(new Vector4(0, 50, 50, 1));

        }

        public void SetOrthographic(int width, int height)
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(width / 4, height / 4, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);

        }

        public void SetPerspective(int width, int height)
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 6, width / (float)height, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);
            GL.Enable(EnableCap.Fog);
        }

        public void SetModelView()
        {
            Matrix4 modelview = Matrix4.LookAt(Position, End, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            GL.Disable(EnableCap.Fog);
        }

        public int TransitionState(bool enable3D, int cTimer)
        {
            if (enable3D)
            {
                SetPerspective(Width, Height);
                //End.X -= 100;
                //End.Y -= 50;
                Position.X -= transX;
                Position.Y += transY;
                //Position.Y = End.Y + visY;
                Position.Z -= transZ; 
            }
            else
            {
                SetOrthographic(Width, Height);
                //End.X += 100;
                //End.Y += 50;
                Position.X += transX;
                Position.Y -= transY;
                //Position.Y = End.Y;
                Position.Z += visZ;
            }
            cTimer = cTimer - 1;
            return cTimer;
        }

        public void Update(float playerDeltaX)
        {
            Position.X += playerDeltaX;
            End.X += playerDeltaX;
        }


    }
}
