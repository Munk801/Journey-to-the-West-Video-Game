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
		private bool in3d;
        private Matrix4 projection;

		private Vector4 lightOffset;

		private float fov;

		private bool movingInY;
		private float playerYPos; //the player's y position the last time they were not in midair

        public Camera(Vector3 position, Vector3 end, int width, int height)
        {
            Position = position;
            End = end;
            Width = width;
            Height = height;
			fov = (float)(Math.PI / 6);
			playerYPos = 12.5f;
        }

        /// <summary>
        /// CreateLight: Creates a light slightly above and behind the camera
        /// </summary>
        /// <param name="cam"> Takes account the position of the camera being passed in</param>
        /// <returns></returns>
        public void UpdateLight()
        {
			//GL.Light(LightName.Light0, LightParameter.Position, new Vector4(lightOffset.X + Position.X, lightOffset.Y + Position.Y, lightOffset.Z + Position.Z, 1));
			GL.Light(LightName.Light0, LightParameter.Position, new Vector4(-50.0f, 50.0f, 50.0f, 0.0f));
        }

        public void Set2DCamera(Vector3 playerLoc)
        {
			in3d = false;
            SetOrthographic();
			End = new Vector3(75f, 62.5f, 50f);
			End.X += playerLoc.X;
			End.Y += playerYPos;
			Position = new Vector3(75f, 62.5f, 150f);
			Position.X += playerLoc.X;
			Position.Y += playerYPos;
			lightOffset = new Vector4(50, 50, 0, 1);
			UpdateLight();
			GL.Disable(EnableCap.Fog);
        }

        public void Set3DCamera(Vector3 playerLoc)
        {
			in3d = true;
            SetPerspective();
			End = new Vector3(-25f, 12.5f, 50f);
			End.X += playerLoc.X;
			End.Y += playerYPos;
			Position = new Vector3(-100f, 16f, 50f);
			Position.X += playerLoc.X;
			Position.Y += playerYPos;
			lightOffset = new Vector4(0, 50, 50, 1);
			UpdateLight();
			GL.Enable(EnableCap.Fog);
        }

        public Matrix4 GetOthoProjectionMatrix()
        {
            return this.projection;
        }

        private void SetOrthographic()
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(Width / 4, Height / 4, 1.0f, 6400.0f);
            this.projection = projection;
			GL.LoadMatrix(ref projection);
        }

        private void SetPerspective()
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(fov, Width / (float)Height, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);
        }

        public void SetModelView()
        {
            Matrix4 modelview = Matrix4.LookAt(Position, End, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
        }

		public void updateForTransition() {
			double theta = timer * timer * Math.PI / 2 + Math.PI;

			//End.X = (float)(200.0 * theta / Math.PI - 225.0);
			End.Y = (float)(100.0 * theta / Math.PI - 75.0);
			End.X = (float)(100.0 * timer - 25.0);
			//End.Y = (float)(50.0 * timer + 25.0);
			//End.Z is always 50.0f

			//Position.X = End.X + (float)(100.0 * Math.Cos(theta));
			Position.X = (float)(End.X + (1.0 - timer) * -75.0);
			Position.Z = End.Z + (float)(-3200.0 * Math.Sin(theta));
			Position.Y = (float)(46.5 * theta / Math.PI - 18.0);

			//fov = 2 * (float)Math.Atan(Math.Tan(Math.PI / 12) * 335 / ((End - Position).Length - 50));
			//fov = 2 * (float)Math.Atan(Math.Tan(Math.PI / 12) * 75 / (End - Position).Length);
			//fov = (float)((0.05765 - 0.39347) * theta + 0.39347);
			//fov = (float)(-0.33582 * timer * timer + 0.39347);
			//fov = (float)(0.33582 * timer * timer + 0.05765);
			fov = (float)(-0.710578*timer*timer*timer+1.63705*timer*timer-1.26229*timer+0.39347);
			//fov = (float)(1.06511-0.21379*theta);
			SetPerspective();
			GL.Disable(EnableCap.Fog);
		}

		public bool TransitionState(bool enable3D, double time, Vector3 playerLoc)
        {
			if (enable3D)
            {
				timer -= time * 1.5;
				if(timer <= 0) {
					fov = (float)(Math.PI / 6);
					Set3DCamera(playerLoc);
					return false;
				}
            }
            else // 2D
            {
				timer += time * 1.5;
				if(timer >= 1.0) {
					Set2DCamera(playerLoc);
					return false;
				}
			}
			updateForTransition();
			return true;
        }

		public void MoveToYPos(float y) {
			if(y != playerYPos) {
				movingInY = true;
				playerYPos = y;
			}
		}

        public void Update(float playerDeltaX, double time)
        {
            Position.X += playerDeltaX;
            End.X += playerDeltaX;
			UpdateLight();

			if(movingInY) {
				float desiredY = playerYPos + (in3d ? 16.0f : 62.5f);
				if(desiredY > Position.Y) {
					Position.Y += (float)(400.0 * time);
					End.Y += (float)(400.0 * time);
					if(desiredY <= Position.Y) {
						Position.Y = desiredY;
						End.Y = playerYPos + (in3d ? 12.5f : 62.5f);
						movingInY = false;
					}
				} else {
					Position.Y -= (float)(400.0 * time);
					End.Y -= (float)(400.0 * time);
					if(desiredY >= Position.Y) {
						Position.Y = desiredY;
						End.Y = playerYPos + (in3d ? 12.5f : 62.5f);
						movingInY = false;
					}
				}
			}
        }
    }
}
