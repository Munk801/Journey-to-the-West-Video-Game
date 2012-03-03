using System;
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
		private bool switchedBillboards;
		private PlayState playstate;

		private Vector4 lightOffset;

		private float fov;

		private bool movingInY; //true when slowly shifting to player's y position (when player is on ground)
		private bool trackingPlayer; //true when tracking player's exact y position (when player is in freefall below screen)
		private float playerYPos; //the player's y position the last time they were not in midair

        public Camera(int width, int height, Player p, PlayState ps, int[] viewport)
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
			switchedBillboards = true;
			Set2DCamera();
			this.playstate = ps;
        }

		public void setViewport(int[] viewport) {
			this.viewport = viewport;
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
			End = new Vector3(37.5f, 31.25f, 0.0f);
			End.X += player.location.X;
			End.Y += playerYPos;
			End.Z = player.location.Z;
			Position = new Vector3(37.5f, 31.25f, 100f);
			Position.X += player.location.X;
			Position.Y += playerYPos;
			Position.Z += player.location.Z;
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
			// timer is 0 at full 2d view, 1 at full 3d view
			//theta is pi/2 at full 2d view, pi at full 3d view (corresponds with coordinates in second quadrant of x,z plane)
			double theta = timer * Math.PI / 2 + Math.PI / 2;

			End.X = (float)(player.location.X + 37.5 + 62.5 * Math.Cos(theta));
			End.Z = (float)(player.location.Z);
			End.Y = (float)(playerYPos + 31.25f - 10.75 * timer);

			if(timer <= 0.5) { // start spiral
				double r = 1.0 + 18.0 * Math.Pow(timer - 0.5, 2); //radius of spiral
				Position.X = (float)(player.location.X + 37.5 + 157.5 * r * Math.Cos(theta));
				Position.Z = (float)(player.location.Z + 200.0 * r * Math.Sin(theta));
			} else { // no spiral
				Position.X = (float)(player.location.X + 37.5 + 157.5 * Math.Cos(theta));
				Position.Z = (float)(player.location.Z + 200.0 * Math.Sin(theta));
			}
			Position.Y = (float)(playerYPos + 31.25f - 7.25 * timer);

			//Dolly zoom FTW
			fov = (float)Math.Min(2.0 * Math.Atan(170.0 * Math.Tan(Math.PI / 10) / (Position - End).LengthFast), Math.PI / 5);

			SetPerspective();
			GL.Disable(EnableCap.Fog);
		}

		public bool TransitionState(bool enable3D, double time)
        {
			time *= 1.5;
			if (enable3D) {
				timer += time;
				if(!switchedBillboards && timer >= 0.5) {
					switchedBillboards = true;
					playstate.doBillboards();
				}
				if(timer >= 1.0) {
					fov = (float)(Math.PI / 5);
					Set3DCamera();
					return false;
				}
			} else { // 2D 
				timer -= time;
				if(!switchedBillboards && timer <= 0.5) {
					switchedBillboards = true;
					playstate.doBillboards();
				}
				if(timer <= 0.0) {
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

		public void startTransition(bool to3d) {
			timer = (to3d ? 0.0 : 1.0);
			switchedBillboards = false;
		}
    }
}
