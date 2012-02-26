﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace U5Designs
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 End;
		private int[] viewport;
        public int Width;
        public int Height;
        public bool isInTransition;
        public double timer;
		private bool in3d;
        private Matrix4d projection;
        private Matrix4d model;
		private Player player;

		private Vector4 lightOffset;

		private float fov;

		private bool movingInY; //true when slowly shifting to player's y position (when player is on ground)
		private bool trackingPlayer; //true when tracking player's exact y position (when player is in freefall below screen)
		private float playerYPos; //the player's y position the last time they were not in midair

        public Camera(int width, int height, Player p, int[] viewport)
        {
            Position = new Vector3();
            End = new Vector3();
            Width = width;
            Height = height;
			this.viewport = viewport;
			fov = (float)(Math.PI / 5);
			player = p;
			playerYPos = p.location.Y;
			movingInY = false;
			trackingPlayer = false;
			Set2DCamera();
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

        public void Set2DCamera()
        {
			in3d = false;
            SetOrthographic();
			End = new Vector3(37.5f, 31.25f, 50f);
			End.X += player.location.X;
			End.Y += playerYPos;
			Position = new Vector3(37.5f, 31.25f, 150f);
			Position.X += player.location.X;
			Position.Y += playerYPos;
			lightOffset = new Vector4(50, 50, 0, 1);
			SetModelView();
			UpdateLight();
			GL.Disable(EnableCap.Fog);
        }

        public void Set3DCamera()
        {
			in3d = true;
            SetPerspective();
			End = new Vector3(-25f, 20.5f, 0f);
			End.X += player.location.X;
			End.Y += playerYPos;
			End.Z += player.location.Z;
			Position = new Vector3(-125f, 24f, 0f);
			Position.X += player.location.X;
			Position.Y += playerYPos;
			Position.Z += player.location.Z;
			lightOffset = new Vector4(0, 50, 50, 1);
			SetModelView();
			UpdateLight();
			//GL.Enable(EnableCap.Fog);
        }

        public Matrix4d GetProjectionMatrix()
        {
            return this.projection;
        }

        public Matrix4d GetModelViewMatrix()
        {
            return this.model;
        }

		public int[] getViewport() {
			return this.viewport;
		}

        private void SetOrthographic()
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d Projection = Matrix4d.CreateOrthographic(192, 108, 1.0f, 6400.0f);
            this.projection = Projection;
			GL.LoadMatrix(ref projection);
        }

        private void SetPerspective()
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d projection = Matrix4d.CreatePerspectiveFieldOfView(fov, Width / (float)Height, 1.0f, 6400.0f);
            this.projection = projection;
            GL.LoadMatrix(ref projection);
        }

        public void SetModelView()
        {
            Matrix4d modelview = Matrix4d.LookAt((Vector3d)Position, (Vector3d)End, Vector3d.UnitY);
            this.model = modelview;
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

		public bool TransitionState(bool enable3D, double time)
        {
			if (enable3D)
            {
				timer -= time * 1.5;
				if(timer <= 0) {
					fov = (float)(Math.PI / 5);
					Set3DCamera();
					return false;
				}
            }
            else // 2D
            {
				timer += time * 1.5;
				if(timer >= 1.0) {
					Set2DCamera();
					return false;
				}
			}
			updateForTransition();
			return true;
        }

		public void moveToYPos(float y) {
			trackingPlayer = false;
			if(y != playerYPos) {
				movingInY = true;
				playerYPos = y;
			}
		}

		public void trackPlayer() {
			trackingPlayer = true;
		}

        public void Update(float playerDeltaX, double time)
        {
            Position.X += playerDeltaX;
            End.X += playerDeltaX;
			if(in3d) {
				Position.Z = player.location.Z;
				End.Z = player.location.Z;
			}
			UpdateLight();

			if(trackingPlayer) {
				if(in3d) {
					playerYPos = player.location.Y + 8.0f;
					Set3DCamera();
				} else {
					playerYPos = player.location.Y + 7.0f;
					Set2DCamera();
				}
			} else if(movingInY) {
				float desiredY = playerYPos + (in3d ? 24.0f : 31.25f);
				if(desiredY > Position.Y) {
					Position.Y += (float)(400.0 * time);
					End.Y += (float)(400.0 * time);
					if(desiredY <= Position.Y) {
						Position.Y = desiredY;
						End.Y = playerYPos + (in3d ? 20.5f : 31.25f);
						movingInY = false;
					}
				} else {
					Position.Y -= (float)(400.0 * time);
					End.Y -= (float)(400.0 * time);
					if(desiredY >= Position.Y) {
						Position.Y = desiredY;
						End.Y = playerYPos + (in3d ? 20.5f : 31.25f);
						movingInY = false;
					}
				}
			}
        }

		//Used in determining if camera needs to move down to follow player
		public bool playerIsAboveScreenBottom() {
			if(in3d) {
			    return player.location.Y - player.pbox.Y - 3.0 >= End.Y - 45;
			} else {
				return player.location.Y - player.pbox.Y - 3.0 >= End.Y - 54;
			}
		}
    }
}
