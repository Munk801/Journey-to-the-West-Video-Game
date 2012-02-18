using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using Engine.Input;
using OpenTK.Input;

// XML parser
using System.Xml;
using System.Collections;
using System.Collections.Generic;


namespace U5Designs
{
    /** Main State of the game that will be active while the player is Playing **/
    class PlayState : GameState
    {
        
        internal GameEngine eng;
        internal Player player;
        internal bool enable3d; //true when being viewed in 3d
		internal bool isInTransition; //true when perspective is in process of switching

		//everything is in objList, and then also pointed to from the appropriate interface lists
        internal List<GameObject> objList;
		internal List<RenderObject> renderList;
        internal List<PhysicsObject> colisionList; // colision list = only things that are moving that need to detect colisions
		internal List<PhysicsObject> physList; // physList is a list of everything that has a bounding box
		internal List<AIObject> aiList;// aka list of enemies
		internal List<CombatObject> combatList; // list of stuff that effects the player in combat, projectiles, enemies
		internal List<Background> backgroundList;
        //TODO: projectile list

        int current_level = -1;// Member variable that will keep track of the current level being played.  This be used to load the correct data from the backends.

		bool tabDown;

        Camera camera;

        Background background;
        Vector3 location = new Vector3(0.0f, 0.0f, -10.0f);
        MainMenuState menustate;
        PauseMenuState pms;


        // Initialize graphics, etc here
        public PlayState(MainMenuState prvstate, GameEngine engine, int lvl) {

			//TODO: pass this the right file to load from
			// undo this when done testing ObjList = LoadLevel.Load(current_level);
			LoadLevel.Load(0, this);

            menustate = prvstate;
            eng = engine;

            pms = new PauseMenuState(engine);
            enable3d = false;
			tabDown = false;
            //test.Play();

			Vector3 lookat = new Vector3(100, 75, 50);
			Vector3 eye = lookat + new Vector3(0, 0, 100);
            camera = new Camera(eye, lookat, eng.ClientRectangle.Width, eng.ClientRectangle.Height);
            background = new Background(location, "../../resources/2DBackground1.jpg", -0.1f, enable3d);
        }

		public override void MakeActive() {
			GL.Enable(EnableCap.Lighting);
			GL.Enable(EnableCap.Light0);
			GL.Light(LightName.Light0, LightParameter.Diffuse, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			GL.Light(LightName.Light0, LightParameter.Specular, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.Fog(FogParameter.FogDensity, 0.0005f);

			GL.ShadeModel(ShadingModel.Smooth);

			if(enable3d) {
				camera.Set3DCamera(player.location);
			} else {
				camera.Set2DCamera(player.location);
			}
		}

        public override void Update(FrameEventArgs e)
        {
            //Console.WriteLine(e.Time); // WORST. BUG. EVER.
            background.UpdatePosition(player.deltax);

            //First deal with hardware input
            DealWithInput();

            //Next check if the player is dead. If he is, game over man
            if (player.health <= 0) {
                GameOverState GGbro = new GameOverState(menustate, eng);
                eng.ChangeState(GGbro);
            }


            //Deal with everyone's acceleration
            if (isInTransition)
            {
				isInTransition = camera.TransitionState(enable3d, e.Time, player.location);

				if(enable3d) {
					player.cycleNumber = 1;  //TODO: This is a hack!
					renderList[0].cycleNumber = 1;  //TODO: This is a hack!
				} else { //2d
					player.cycleNumber = 0;  //TODO: This is a hack!
					renderList[0].cycleNumber = 0;  //TODO: This is a hack!
				}
            } else {
				player.updateState(enable3d, eng.Keyboard[Key.A], eng.Keyboard[Key.S], eng.Keyboard[Key.D], eng.Keyboard[Key.W], eng.Keyboard[Key.C], eng.Keyboard[Key.X], eng.Keyboard[Key.Space], e);

				foreach(AIObject aio in aiList) {
					aio.aiUpdate(e, player.location, enable3d);
				}

				//Now that all everyone's had a chance to accelerate, actually
				//translate that into velocity and position
				if(enable3d) {
					player.physUpdate3d(e, physList); //TODO: Should player be first or last?
					foreach(PhysicsObject po in colisionList) {
						po.physUpdate3d(e, physList);
					}
				} else {
					player.physUpdate2d(e, physList); //TODO: Should player be first or last?
					foreach(PhysicsObject po in colisionList) {
						po.physUpdate2d(e, physList);
					}
				}

				//Stuff that uses deltax must be last
				camera.Update(player.deltax);
				foreach(Background b in backgroundList) {
					b.UpdatePosition(player.deltax);
				}
            }
        }

        public override void Draw(FrameEventArgs e)
        {
            //Origin is the left edge of the level, at the ground and the back wall
            //This means that all valid game coordinates will be positive
            //Ground is from 0 to 100 along the z-axis

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            // UNCOMMENT THIS AND LINE AFTER DRAW TO ADD MOTION BLUR
            //if (isInTransition)
            //{
            //GL.Accum(AccumOp.Return, 0.95f);
            //GL.Clear(ClearBufferMask.AccumBufferBit);
            //}

            camera.SetModelView();

			//Set up textures
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

            background.Draw(enable3d);
			foreach(RenderObject obj in renderList) {
				if(obj.is3dGeo) {
 					obj.doScaleTranslateAndTexture();
 					obj.mesh.Render();
				} else {
					obj.doScaleTranslateAndTexture();
					obj.frameNumber = obj.sprite.draw(enable3d ^ isInTransition, obj.cycleNumber, obj.frameNumber + e.Time);
				}
			}

			player.draw(enable3d ^ isInTransition, e.Time);


            // UNCOMMENT TO ADD MOTION BLUR
            //if (isInTransition)
            //{
            //GL.Accum(AccumOp.Accum, 0.9f);
            //}
        }

        private void DealWithInput()
        {
            //TODO: Change these keys to their final mappings when determined
            if (eng.Keyboard[Key.Escape])
            {
                //eng.PushState(menustate);
                eng.PushState(pms);
            }

			//********************** tab
			if(!isInTransition) {
				if(eng.Keyboard[Key.Tab] && !tabDown) {
					enable3d = !enable3d;
					tabDown = true;
					player.velocity.Z = 0;
					isInTransition = true;
					camera.timer = (enable3d ? 1 : 0);
				} else if(!eng.Keyboard[Key.Tab]) {
					tabDown = false;
				}
			}
        }

        /**
         * Change the current level being played to the parameter
         * */
        public void changeCurrentLevel(int l)
        {
            current_level = l;
        }      
    }
}
