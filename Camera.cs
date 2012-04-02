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
        public Vector3 eye;
        public Vector3 lookat;
		private int[] viewport;
        public int width;
        public int height;
        private Matrix4d projection;
        private Matrix4d model;
		private Player player;
		private PlayState playstate;
        //private LevelDesignerState LevelDesignState;
		private Vector3 bossAreaCenter, bossAreaBounds;

		public double timer;
		private float fov;

		private bool in3d;
		public bool isInTransition;
		private bool switchedBillboards; //tracks if we have triggered the billboarding switch yet
		private bool movingInY; //true when slowly shifting to player's y position (when player is on ground)
		public bool trackingPlayer; //true when tracking player's exact y position (when player is in freefall below screen)
		private float playerYPos; //the player's y position the last time they were not in midair
		private bool bossMode;

		public Camera(int width, int height, Player p, PlayState ps, int[] viewport) {
			this.width = width;
			this.height = height;
			player = p;
			playstate = ps;
			this.viewport = viewport;

            eye = new Vector3();
            lookat = new Vector3();
			fov = (float)(Math.PI / 5);
			playerYPos = p.location.Y;

			movingInY = false;
			trackingPlayer = false;
			switchedBillboards = true;
			bossMode = false;
			isInTransition = false;

			Set2DCamera();
        }

		public void setViewport(int[] viewport) {
			this.viewport = viewport;
		}

        public void Set2DCamera(double width=192, double height=108) {
			in3d = false;
            SetOrthographic(width, height);
			lookat = new Vector3(37.5f, 31.25f, 0.0f);
			lookat.X += player.location.X;
			lookat.Y += playerYPos;
			lookat.Z = player.location.Z;
			eye = new Vector3(37.5f, 31.25f, 500.0f);
			eye.X += player.location.X;
			eye.Y += playerYPos;
			eye.Z += player.location.Z;
			SetModelView();
        }

        public void Set3DCamera() {
			in3d = true;
            SetPerspective();
			lookat = new Vector3(-25f, 20.5f, 0f);
			lookat.X += player.location.X;
			lookat.Y += playerYPos;
			lookat.Z += player.location.Z;
			eye = new Vector3(-125f, 24f, 0f);
			eye.X += player.location.X;
			eye.Y += playerYPos;
			eye.Z += player.location.Z;
			SetModelView();
        }

        public Matrix4d GetProjectionMatrix() {
            return this.projection;
        }

        public Matrix4d GetModelViewMatrix() {
            return this.model;
        }

		public int[] getViewport() {
			return this.viewport;
		}

        private void SetOrthographic(double width = 192, double height = 108) {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d Projection = Matrix4d.CreateOrthographic(width, height, 1.0f, 6400.0f);
            this.projection = Projection;
			GL.LoadMatrix(ref projection);
        }

        private void SetPerspective() {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d projection = Matrix4d.CreatePerspectiveFieldOfView(fov, width / (float)height, 1.0f, 6400.0f);
            this.projection = projection;
            GL.LoadMatrix(ref projection);
        }

        public void SetModelView() {
            Matrix4d modelview = Matrix4d.LookAt((Vector3d)eye, (Vector3d)lookat, Vector3d.UnitY);
            this.model = modelview;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
        }

		/// <summary>
		/// Called when camera should start panning to player's new y position
		/// </summary>
		/// <param name="y">Player's y coordinate</param>
		public void moveToYPos(float y) {
			trackingPlayer = false;
			if(y != playerYPos) {
				movingInY = true;
				playerYPos = y;
			}
		}

		/// <summary>
		/// Called when player falls below screen to start closely following player's y
		/// </summary>
		public void trackPlayer() {
			trackingPlayer = true;
		}

		/// <summary>
		/// Called to put the camera in boss mode.
		/// </summary>
		public void enterBossMode(Vector3 bossAreaCenter, Vector3 bossAreaBounds) {
			bossMode = true;
			this.bossAreaCenter = bossAreaCenter;
			this.bossAreaBounds = bossAreaBounds;
		}

		/// <summary>
		/// Called to start the camera transitioning to the other view
		/// </summary>
		/// <param name="to3d">True if moving to 3D, false for 2D</param>
		public void startTransition(bool to3d) {
			timer = (to3d ? 0.0 : 1.0);
			switchedBillboards = false;
			in3d = to3d;
			isInTransition = true;
		}

		/// <summary>
		/// Updates the current position of the camera
		/// </summary>
		/// <param name="time">Number of seconds since the last update</param>
		public void Update(double time) {
			//Check if we need to an alternate update
			if(isInTransition) {
				TransitionUpdate(time);
				return;
			}
			if(bossMode) {
				bossUpdate();
				return;
			}

			//Update camera x and z position
			if(in3d) {
				eye.X = player.location.X - 125.0f;
				lookat.X = player.location.X - 25.0f;

				eye.Z = player.location.Z;
				lookat.Z = player.location.Z;
			} else {
				eye.X = lookat.X = player.location.X + 37.5f;
			}

			//If necessary, update camera y position
			if(trackingPlayer) { //True when following player's y exactly (like when falling)
				float tmpY = eye.Y;
				if(in3d) {
					playerYPos = player.location.Y + 8.0f;
					Set3DCamera();
				} else {
					playerYPos = player.location.Y + 7.0f;
					Set2DCamera();
				}
				playstate.updateBackgroundsYPos(eye.Y - tmpY);
			} else if(movingInY) { //True when following panning slowly to player's y
				float desiredY = playerYPos + (in3d ? 24.0f : 31.25f);
				float tmpY = eye.Y;
				if(desiredY > eye.Y) {
					eye.Y += (float)(400.0 * time);
					lookat.Y += (float)(400.0 * time);
					if(desiredY <= eye.Y) {
						eye.Y = desiredY;
						lookat.Y = playerYPos + (in3d ? 20.5f : 31.25f);
						movingInY = false;
					}
				} else {
					eye.Y -= (float)(400.0 * time);
					lookat.Y -= (float)(400.0 * time);
					if(desiredY >= eye.Y) {
						eye.Y = desiredY;
						lookat.Y = playerYPos + (in3d ? 20.5f : 31.25f);
						movingInY = false;
					}
				}
				playstate.updateBackgroundsYPos(eye.Y - tmpY);
			}
        }

		/// <summary>
		/// Alternate update to be used when in boss mode - automatically called by normal update
		/// </summary>
		private void bossUpdate() {
			if(in3d) {
				fov = (float)(Math.PI / 8);

				eye.X = bossAreaCenter.X - 250.0f;
				lookat.X = bossAreaCenter.X - 150.0f;

				eye.Z = lookat.Z = player.location.Z;

				//Clamp Z to boss area
				eye.Z = lookat.Z = Math.Min(eye.Z, bossAreaCenter.Z + bossAreaBounds.Z - 96); //96 = half of viewport width
				eye.Z = lookat.Z = Math.Max(eye.Z, bossAreaCenter.Z - bossAreaBounds.Z + 96);

			} else { //2D
				eye.X = lookat.X = player.location.X;

				//Clamp X to boss area
				eye.X = lookat.X = Math.Min(eye.X, bossAreaCenter.X + bossAreaBounds.X - 96); //96 = half of viewport width
				eye.X = lookat.X = Math.Max(eye.X, bossAreaCenter.X - bossAreaBounds.X + 96);
			}
		}

		/// <summary>
		/// Alternate update for when the camera is switching between 2D and 3D views
		/// </summary>
		/// <param name="time">Number of seconds since last update</param>
		private void TransitionUpdate(double time) {
			time *= 1.5;
			timer = (in3d ? timer + time : timer - time);
			if(!switchedBillboards && (in3d ? timer >= 0.5 : timer <= 0.5)) {
				switchedBillboards = true;
				playstate.doBillboards();
			}
			if(in3d ? timer >= 1.0 : timer <= 0.0) {
				isInTransition = false;
				if(in3d) {
					fov = (float)(Math.PI / 5);
					Set3DCamera();
				} else {
					Set2DCamera();
				}
				return;
			}

			//If the transition isn't over, update the camera properties

			// timer is 0 at full 2d view, 1 at full 3d view
			//theta is pi/2 at full 2d view, pi at full 3d view (corresponds with coordinates in second quadrant of x,z plane)
			double theta = timer * Math.PI / 2 + Math.PI / 2;

			lookat.X = (float)(player.location.X + 37.5 + 62.5 * Math.Cos(theta));
			lookat.Z = (float)(player.location.Z);
			lookat.Y = (float)(playerYPos + 31.25f - 10.75 * timer);

			if(timer <= 0.5) { // start spiral
				double r = 1.0 + 18.0 * Math.Pow(timer - 0.5, 2); //radius of spiral
				eye.X = (float)(player.location.X + 37.5 + 157.5 * r * Math.Cos(theta));
				eye.Z = (float)(player.location.Z + 200.0 * r * Math.Sin(theta));
			} else { // no spiral
				eye.X = (float)(player.location.X + 37.5 + 157.5 * Math.Cos(theta));
				eye.Z = (float)(player.location.Z + 200.0 * Math.Sin(theta));
			}
			eye.Y = (float)(playerYPos + 31.25f - 7.25 * timer);

			//Dolly zoom for the win
			fov = (float)Math.Min(2.0 * Math.Atan(170.0 * Math.Tan(Math.PI / 10) / (eye - lookat).LengthFast), Math.PI / 5);

			SetPerspective();
		}

		//Used in determining if camera needs to move down to follow player
		public bool playerIsAboveScreenBottom() {
			if(in3d) {
			    return player.location.Y - player.pbox.Y - 3.0 >= lookat.Y - 45;
			} else {
				return player.location.Y - player.pbox.Y - 3.0 >= lookat.Y - 54;
			}
		}
    }
}
