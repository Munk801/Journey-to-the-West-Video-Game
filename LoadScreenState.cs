﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Engine;

namespace U5Designs {
	class LoadScreenState : GameState {
        internal GameEngine eng;
        
        protected Vector3 eye, lookat;

		Texture[] screens;
		double curFrame;

		private int lvl;
		private PlayState playstate;

		private bool doneLoading;

        public LoadScreenState(GameEngine engine, PlayState playstate, int lvl)
        {
            eng = engine;
			this.playstate = playstate;
			this.lvl = lvl;

			doneLoading = false;

            lookat = new Vector3(0, 0, 2);
            eye = new Vector3(0, 0, 5);

			screens = new Texture[4];
			for(int i = 0; i < 4; i++) {
				screens[i] = eng.StateTextureManager.GetTexture("load" + (i + 1));
			}

			// Set the current image to be displayed at 0 which is the first in the sequence
			curFrame = 0.0;

			Thread loaderThread = new Thread(new ThreadStart(loadLevel));
			loaderThread.Start();
        }

		private void loadLevel() {
			LoadLevel.Load(lvl, playstate);
			doneLoading = true;
		}

		public override void MakeActive() {
			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

			Matrix4 modelview = Matrix4.LookAt(eye, lookat, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref modelview);

			GL.MatrixMode(MatrixMode.Projection);
			Matrix4d projection = Matrix4d.CreateOrthographic(192, 108, 1.0f, 6400.0f);
			GL.LoadMatrix(ref projection);
		}

		public override void Update(FrameEventArgs e) {
			//Minus - Toggle fullscreen
			if(eng.Keyboard[Key.Minus]) {
				eng.toggleFullScreen();
			}

			if(doneLoading) {
				foreach(RenderObject ro in playstate.renderList) {
					if(ro.is3dGeo) {
						ro.texture.init();
					}
				}

				//Have to do this here for now because it requires the GraphicsContext
				//HUD Health Bar
				Assembly assembly = Assembly.GetExecutingAssembly();
				playstate.eng.StateTextureManager.LoadTexture("Healthbar", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.healthbar_top.png"));
				playstate.Healthbar = playstate.eng.StateTextureManager.GetTexture("Healthbar");
				playstate.eng.StateTextureManager.LoadTexture("bHealth", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.healthbar_bottom.png"));
				playstate.bHealth = playstate.eng.StateTextureManager.GetTexture("bHealth");

				//initialize camera
				playstate.camera = new Camera(playstate.eng.ClientRectangle.Width, playstate.eng.ClientRectangle.Height, playstate.player, playstate);
				playstate.player.cam = playstate.camera;

				eng.GameInProgress = true;
				eng.ChangeState(playstate);
			}
        }

        public override void Draw(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			curFrame = (curFrame + e.Time * 2) % 4;
			screens[(int)curFrame].Draw2DTexture(0, 0, 0.2f, 0.2f);
        }
	}
}
