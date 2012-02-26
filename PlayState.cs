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

        internal Camera camera;

        MainMenuState menustate;
		PauseMenuState pms;

        public bool clickdown = false;
        // Initialize graphics, etc here
        public PlayState(MainMenuState prvstate, GameEngine engine, int lvl) {

			//TODO: pass this the right file to load from
			// undo this when done testing ObjList = LoadLevel.Load(current_level);
			LoadLevel.Load(0, this);
            foreach (AIObject aio in aiList) {
                ((Enemy)aio).player = player;
            }


            menustate = prvstate;
            eng = engine;
            pms = new PauseMenuState(engine);
            enable3d = false;
			tabDown = false;
            //test.Play();
			camera = new Camera(eng.ClientRectangle.Width, eng.ClientRectangle.Height, player,
									new int[] { eng.ClientRectangle.X, eng.ClientRectangle.Y, eng.ClientRectangle.Width, eng.ClientRectangle.Height });
			player.cam = camera;
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
				camera.Set3DCamera();
			} else {
				camera.Set2DCamera();
			}
		}

        List<CombatObject> EnemyKillList = new List<CombatObject>();
        List<CombatObject> ProjectileKillList = new List<CombatObject>(); 

        public override void Update(FrameEventArgs e)
        {
            //Console.WriteLine(e.Time); // WORST. BUG. EVER.

            //First deal with hardware input
            DealWithInput();
            //MouseInput();

            //Next check if the player is dead. If he is, game over man
            if (player.health <= 0) {
                GameOverState GGbro = new GameOverState(menustate, eng);
                eng.ChangeState(GGbro);
            }
            
            //handle death and despawning for everything else
            EnemyKillList.Clear();
            ProjectileKillList.Clear();
            foreach (CombatObject CO in combatList) {
                if (CO.type == 1) { //enemy
                    if (CO.health <= 0) {
                        EnemyKillList.Add(CO);
                    }
                }
                if (CO.type == 2) {//projectile
                    if (CO.health <= 0) {
                        ProjectileKillList.Add(CO);
                    }
                }
            }
            foreach (CombatObject CO in EnemyKillList) {
                objList.Remove((GameObject)CO);
                physList.Remove((PhysicsObject)CO);
                colisionList.Remove((PhysicsObject)CO);
                renderList.Remove((RenderObject)CO);
                aiList.Remove((AIObject)CO);
                combatList.Remove(CO);
            }
            foreach (CombatObject CO in ProjectileKillList) {
                objList.Remove((GameObject)CO);
                physList.Remove((PhysicsObject)CO);
                colisionList.Remove((PhysicsObject)CO);
                renderList.Remove((RenderObject)CO);
                combatList.Remove(CO);
            }

            //Deal with everyone's acceleration
            if (isInTransition)
            {
				isInTransition = camera.TransitionState(enable3d, e.Time);

				//TODO: This if/else block is a hack!
				if(enable3d) {
					//player.cycleNumber = 1;
					foreach(AIObject o in aiList) {
						((RenderObject)o).cycleNumber = 1;
					}
				} else { //2d
					//player.cycleNumber = 0;
					foreach(AIObject o in aiList) {
						((RenderObject)o).cycleNumber = 0;
					}
				}
            } else {
				player.updateState(enable3d, eng.Keyboard[Key.A], eng.Keyboard[Key.S], eng.Keyboard[Key.D], eng.Keyboard[Key.W], eng.Keyboard[Key.C], eng.Keyboard[Key.X], eng.Keyboard[Key.Space], eng.Keyboard[Key.E], e, this);

				foreach(AIObject aio in aiList) {
					((Enemy)aio).aiUpdate(e, this, player.location, enable3d);
				}

				//Now that everyone's had a chance to accelerate, actually
				//translate that into velocity and position
				if(enable3d) {
                    player.physUpdate3d(e.Time, objList, renderList, colisionList, physList, combatList); //TODO: Should player be first or last?
					foreach(PhysicsObject po in colisionList) {
                        po.physUpdate3d(e.Time, objList, renderList, colisionList, physList, combatList);
					}
				} else {
                    player.physUpdate2d(e.Time, objList, renderList, colisionList, physList, combatList); //TODO: Should player be first or last?
					foreach(PhysicsObject po in colisionList) {
                        po.physUpdate2d(e.Time, objList, renderList, colisionList, physList, combatList);
					}
				}

				//Stuff that uses deltax must be last
				camera.Update(player.deltax, e.Time);
				foreach(Background b in backgroundList) {
					b.UpdatePosition(player.deltax);
				}
            }
		}

		//sorts RenderObjects by ascending Z coordinate
		private static int compare2dView(RenderObject lhs, RenderObject rhs) {
			if(lhs.location.Z < rhs.location.Z) {
				return -1;
			} else if(lhs.location.Z > rhs.location.Z) {
				return 1;
			} else {
				return 0;
			}
		}

		//sorts RenderObjects by descending X coordinate
		private static int compare3dView(RenderObject lhs, RenderObject rhs) {
			if(lhs.location.X > rhs.location.X) {
				return -1;
			} else if(lhs.location.X < rhs.location.X) {
				return 1;
			} else {
				return 0;
			}
		}

        public override void Draw(FrameEventArgs e)
        {
            //Origin is the left edge of the level, at the ground and the back wall
            //This means that all valid game coordinates will be positive
            //Ground is from 0 to 100 along the z-axis

			GL.ClearColor(Color4.Black);
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
 			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

			//Sort objects by depth for proper alpha rendering
			if(enable3d) {
				renderList.Sort(compare3dView);
			} else {
				renderList.Sort(compare2dView);
			}

			//First render all opaque sprites, and all 3d geometry (since they are assumed to be opaque)
			foreach(RenderObject obj in renderList) {
				if((enable3d && obj.existsIn3d) || (!enable3d && obj.existsIn2d) || isInTransition) {
					if(obj.is3dGeo) {
						obj.doScaleTranslateAndTexture();
						obj.mesh.Render();
					} else {
						if(!obj.sprite.hasAlpha) {
							obj.doScaleTranslateAndTexture();
							obj.frameNumber = obj.sprite.draw(enable3d && !isInTransition, obj.cycleNumber, obj.frameNumber + e.Time);
						}
					}
				}
			}

			//Now render transparent sprites in sorted order
			foreach(RenderObject obj in renderList) {
				if((enable3d && obj.existsIn3d) || (!enable3d && obj.existsIn2d) || isInTransition) {
					if((!obj.is3dGeo) && obj.sprite.hasAlpha) {
						obj.doScaleTranslateAndTexture();
						obj.frameNumber = obj.sprite.draw(enable3d && !isInTransition, obj.cycleNumber, obj.frameNumber + e.Time);
					}
				}
			}
            //player.he
			//Finally, draw the player
			//player.draw(enable3d ^ isInTransition, e.Time);


            // UNCOMMENT TO ADD MOTION BLUR
            //if (isInTransition)
            //{
            //GL.Accum(AccumOp.Accum, 0.9f);
            //}
        }

        //private void MouseInput()
        //{
        //    if (eng.ThisMouse.LeftPressed() && !clickdown)
        //    {
                
        //        Vector3d mousecoord = new Vector3d((double)eng.Mouse.X, (double)(eng.Height - eng.Mouse.Y), 0);
        //        Matrix4d ortho = camera.GetOthoProjectionMatrix();
        //        Matrix4d model = camera.GetModelViewMatrix();
        //        //Glu.UnProject(mousecoord, model, ortho, Viewport, ref mouseWorld);
        //        mouseWorld = eng.ThisMouse.UnProject(mousecoord, model, ortho, Viewport);
        //        Vector3d proj = eng.ThisMouse.Get2DVectorFromPlayerToClick(player.location, mouseWorld);
        //        //Console.WriteLine(mouseWorld.ToString());
        //        Console.WriteLine(proj.ToString());
        //        //Console.WriteLine("X Coord: " + eng.ThisMouse.Mouse.X.ToString()+  " Y Coord: " + eng.ThisMouse.Mouse.Y.ToString());
        //        clickdown = true;
        //    }
        //    else if (eng.ThisMouse.LeftPressed() == false)
        //    {
        //        clickdown = false;
        //    }
        //}

        private void DealWithInput()
        {
            // Testing the Level Design feature of re-loading LoadLevel after changing coords for a given game object
            if (eng.Keyboard[Key.F5])
            {
                PlayState pst = new PlayState(this.menustate, eng, 0);
                
                //eng.PushState(pst);
                // test
                eng.ChangeState(pst);
                
                //LoadLevel.Load(0, pst);
            }

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
