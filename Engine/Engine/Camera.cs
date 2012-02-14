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
        public double timer;

		private Vector4 lightOffset;


        private int visX = 120;
        private int visY = 30;
        private int visZ = 100;

        private float transZ = 100;
        private float transY = 30;
        private float transX = 120;

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
        public void UpdateLight()
        {
			GL.Light(LightName.Light0, LightParameter.Position, new Vector4(lightOffset.X + Position.X, lightOffset.Y + Position.Y, lightOffset.Z + Position.Z, 1));
        }

        public void Set2DCamera(Vector3 playerLoc)
        {
            SetOrthographic(Width, Height);
			End = new Vector3(75f, 75f, 50f);
			End.X += playerLoc.X;
			Position = new Vector3(75f, 75f, 150f);
			Position.X += playerLoc.X;
			lightOffset = new Vector4(50, 50, 0, 1);
			UpdateLight();
        }

        public void Set3DCamera(Vector3 playerLoc)
        {
            SetPerspective(Width, Height);
			End = new Vector3(-25f, 25f, 50f);
			End.X += playerLoc.X;
			Position = new Vector3(-100f, 28.5f, 50f);
			Position.X += playerLoc.X;
			lightOffset = new Vector4(0, 50, 50, 1);
			UpdateLight();
        }

        private void SetOrthographic(int width, int height)
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(width / 4, height / 4, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);
        }

        private void SetPerspective(int width, int height)
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

		public bool TransitionState(bool enable3D, double time, Vector3 playerLoc)
        {
			timer += time;
            if (enable3D)
            {
                SetPerspective(Width, Height);
                //End.X -= 100;
                //End.Y -= 50;
                Position.X -= (float)(transX*time);
                Position.Y += (float)(transY*time);
                //Position.Y = End.Y + visY;
				Position.Z -= (float)(transZ*time);
				if(timer >= 1.0) {
					Set3DCamera(playerLoc);
					return false;
				}
            }
            else
            {
                SetOrthographic(Width, Height);
                //End.X += 100;
                //End.Y += 50;
                Position.X += (float)(transX*time);
                Position.Y -= (float)(transY*time);
                //Position.Y = End.Y;
				Position.Z += (float)(visZ * time);
				if(timer >= 1.0) {
					Set2DCamera(playerLoc);
					return false;
				}
            }
			return true;
        }

        public void Update(float playerDeltaX)
        {
            Position.X += playerDeltaX;
            End.X += playerDeltaX;
			UpdateLight();
        }


    }
}
