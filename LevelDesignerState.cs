using System;
using System.Windows.Forms;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Engine;
using OpenTK.Input;

// XML parser
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace U5Designs
{
    /// <summary>
    /// This state will act as the designer state for which levels can be created.
    /// The state will act just like the play state with some exceptions.
    /// - There will be no player
    /// - Everything will be assumed in a pause state
    /// - User is given master rights *can move anywhere in the level -- even out of bounds*
    /// 
    /// There are three main components
    /// - Movement - User needs the ability to move around
    /// - Adding/Removing - User needs the ability to add or remove elements from the level
    /// - Saving/Loading - User needs the ability to load or save the level that was designed
    /// 
    /// Movement
    /// WASD - Allows the user to move left and right, and zoom in and out in both 2d and 3d
    /// 
    /// Adding/Removing
    /// User will be able to select between either obstacles or enemies
    /// obstacles - ground / other objects in game
    /// enemies - placements of enemies
    /// Left Click - Place selected object in place
    /// Left Hold - if mouse is in the spot of object, move that object
    /// Right Click - Remove object in that spot
    /// 
    /// Saving/Loading
    /// User will be able to save the current state of the level
    /// As the level is being built, this will be placed in memory
    /// Once, the user saves the level, it can then be placed in file for loading later
    /// </summary>
    /** Main State of the game that will be active while the player is Playing **/
    public class LevelDesignerState : GameState
    {
        //debug
        bool aienabled = true;

		internal GameEngine eng;
		MainMenuState menustate;
		PauseMenuState pms;
		internal Player player;
		internal Camera camera;

        //These are the lists of all objects in the game
        List<GameObject> objList = new List<GameObject>(); //everything is in objList, and then also pointed to from the appropriate interface lists
		List<RenderObject> renderList = new List<RenderObject>();
        List<PhysicsObject> colisionList = new List<PhysicsObject>(); // colision list = only things that are moving that need to detect colisions
		List<PhysicsObject> physList = new List<PhysicsObject>(); // physList is a list of everything that has a bounding box
		List<AIObject> aiList = new List<AIObject>();// aka list of enemies
		List<CombatObject> combatList = new List<CombatObject>(); // list of stuff that effects the player in combat, projectiles, enemies
		List<Background> backgroundList = new List<Background>();

		public SphereRegion bossRegion;
		public SphereRegion endRegion;

		internal bool enable3d; //true when being viewed in 3d
		internal bool isInTransition; //true when perspective is in process of switching
        int current_level = -1;// Member variable that will keep track of the current level being played.  This be used to load the correct data from the backends.
		private bool nowBillboarding; //true when billboarding objects should rotate into 3d view
        Texture Healthbar, bHealth;
		int MaxHealth;
		public SpriteSheet staminaBar, staminaBack, staminaFrame;

		bool tabDown;
		public bool clickdown = false;


        /// <summary>
        /// PlayState is the state in which the game is actually playing, this should only be called once when a new game is made.
        /// </summary>
        /// <param name="prvstate">The previous state(the menu that spawned this playstate)</param>
        /// <param name="engine">Pointer to the game engine</param>
        /// <param name="lvl">the level ID</param>
        public LevelDesignerState(MainMenuState prvstate, GameEngine engine, int lvl) {

            //Every AI object needs a pointer to the player, initlize this here
            foreach (AIObject aio in aiList) {
                ((Enemy)aio).player = player;
            }

            //deal with states
            menustate = prvstate;
            eng = engine;
            pms = new PauseMenuState(engine);
            enable3d = false;
			tabDown = false;
            //test.Play();
            //initlize camera
            camera = new Camera(eng.ClientRectangle.Width, eng.ClientRectangle.Height, 25, 12.5f, 50, this, 
									new int[] { eng.ClientRectangle.X, eng.ClientRectangle.Y, eng.ClientRectangle.Width, eng.ClientRectangle.Height });
			player.cam = camera;
			nowBillboarding = false;

            // Add healthbar texture to texture manager
            Assembly audAssembly = Assembly.GetExecutingAssembly();

			//Thanks to OpenTK samples for part of this shader code
			//Initialize Shader
            int shaderProgram = GL.CreateProgram();
            int frag = GL.CreateShader(ShaderType.FragmentShader);

            // GLSL for fragment shader.
            String fragSource = @"
				uniform sampler2D tex;

				void main( void )
				{
					vec4 col = texture2D(tex,gl_TexCoord[0].st);
					if( col.a < 0.5) {
						discard;
					}
					gl_FragColor = col;
				}	
			";

            GL.ShaderSource(frag, fragSource);
            GL.CompileShader(frag);
            GL.AttachShader(shaderProgram, frag);
            GL.LinkProgram(shaderProgram);
            GL.UseProgram(shaderProgram);
        }

        /// <summary>
        /// Refreshes graphics when this state becomes active again after being frozen.
        /// </summary>
		public override void MakeActive() {
			GL.Enable(EnableCap.Lighting);
			GL.Enable(EnableCap.Light0);
			GL.Light(LightName.Light0, LightParameter.Diffuse, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			GL.Light(LightName.Light0, LightParameter.Specular, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.ShadeModel(ShadingModel.Smooth);
			GL.ClearColor(0.26667f, 0.86667f, 1.0f, 1.0f);

			if(enable3d) {
				camera.Set3DCamera();
			} else {
				camera.Set2DCamera();
			}

			if(player.curProjectile.gravity) {
				eng.CursorVisible = false;
			} else {
				eng.CursorVisible = true; //TODO: When we have a crosshair, we'll change this
			}
		}

        /// <summary>
        /// Update, this gets called once every update frame
        /// </summary>
        /// <param name="e">FrameEventArgs from OpenTK's update</param>
        public override void Update(FrameEventArgs e) {
            //First deal with hardware input
            DealWithInput();

            //Next check if the player is dead. If he is, game over man
            if (player.health <= 0) {
                GameOverState GGbro = new GameOverState(menustate, eng);
                eng.ChangeState(GGbro);
            }

			//See if we need to trigger an event (like the end of the level or a boss)
			if(endRegion.contains(player.location)) {
				//Finished level
				eng.ChangeState(new MainMenuState(eng)); //Later we should go to the next level when applicable
			}
			//Uncomment this when we have an actual boss
			//if(bossRegion.contains(player.location) {
			//    //Entered boss area - make changes to camera, etc
			//}

			//Determine which screen region everything is in
			foreach(GameObject go in objList) {
				float dist = VectorUtil.dist(go.location, player.location);
				if(dist < 400.0) {
					go.screenRegion = GameObject.ON_SCREEN;
				} else if(dist > 500.0) {
					go.screenRegion = GameObject.OFF_SCREEN;
				}
				//If between the two distances, then leave as is
			}
            
            //handle death and despawning for everything else
			for(int i=combatList.Count-1; i>=0; i--) {
				CombatObject co = combatList[i];
                if (co.type == 1) { //enemy
					if(co.health <= 0) {
						objList.Remove((GameObject)co);
						physList.Remove((PhysicsObject)co);
						colisionList.Remove((PhysicsObject)co);
						renderList.Remove((RenderObject)co);
						aiList.Remove((AIObject)co);
						combatList.Remove(co);
                    }
                }
                if (co.type == 2) {//projectile
					if(co.health <= 0 || co.ScreenRegion == GameObject.OFF_SCREEN) {
						objList.Remove((GameObject)co);
						physList.Remove((PhysicsObject)co);
						colisionList.Remove((PhysicsObject)co);
						renderList.Remove((RenderObject)co);
						combatList.Remove(co);
                    }
                }
            }

			//If the camera is transitioning, everything else is paused
            if (isInTransition) {
				isInTransition = camera.TransitionState(enable3d, e.Time);
			} else {
				//Deal with everyone's acceleration


				//Now that everyone's had a chance to accelerate, actually
				//translate that into velocity and position
				if(enable3d) {
                    player.physUpdate3d(e.Time, physList); //TODO: Should player be first or last?
					foreach(PhysicsObject po in colisionList) {
                        po.physUpdate3d(e.Time, physList);
					}
				} else {
                    player.physUpdate2d(e.Time, physList); //TODO: Should player be first or last?
					foreach(PhysicsObject po in colisionList) {
                        po.physUpdate2d(e.Time, physList);
					}
				}

				//Stuff that uses deltax must be last
				camera.Update(player.deltax, e.Time);
				foreach(Background b in backgroundList) {
					b.UpdatePositionX(player.deltax);
				}
            }
		}

		//Called by camera for parallaxing
		public void updateBackgroundsYPos(float deltay) {
			foreach(Background b in backgroundList) {
				b.UpdatePositionY(deltay);
			}
		}

		////sorts RenderObjects by ascending Z coordinate
		//private static int compare2dView(RenderObject lhs, RenderObject rhs) {
		//    if(lhs.location.Z < rhs.location.Z) {
		//        return 1;
		//    } else if(lhs.location.Z > rhs.location.Z) {
		//        return -1;
		//    } else {
		//        return lhs.getID() - rhs.getID(); //consistent tiebreaker to avoid flicker
		//    }
		//}

		////sorts RenderObjects by descending X coordinate
		//private static int compare3dView(RenderObject lhs, RenderObject rhs) {
		//    if(lhs.location.X > rhs.location.X) {
		//        return 1;
		//    } else if(lhs.location.X < rhs.location.X) {
		//        return -1;
		//    } else {
		//        return lhs.getID() - rhs.getID(); //consistent tiebreaker to avoid flicker
		//    }
		//}

        /// <summary>
        /// The Draw update, happens every frame
        /// </summary>
        /// <param name="e">FrameEventArgs from OpenTK's update</param>
        public override void Draw(FrameEventArgs e) {
            //e = new FrameEventArgs(e.Time * 0.1);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            camera.SetModelView();

			//First render all opaque sprites, and all 3d geometry (since they are assumed to be opaque)
			foreach(RenderObject obj in renderList) {
				if((enable3d && obj.existsIn3d) || (!enable3d && obj.existsIn2d) || isInTransition) {
					if(obj.is3dGeo) {
						obj.doScaleTranslateAndTexture();
						obj.mesh.Render();
					} else {
						//if(!obj.sprite.hasAlpha) {
							obj.doScaleTranslateAndTexture();
							if(isInTransition) { //Pause all animations in transition
								obj.sprite.draw(nowBillboarding, obj.billboards, obj.cycleNumber, obj.frameNumber + obj.animDirection * e.Time);
							} else {
								obj.frameNumber = obj.sprite.draw(nowBillboarding, obj.billboards, obj.cycleNumber, obj.frameNumber + obj.animDirection * e.Time);
							}
						//}
					}
				}
			}

			foreach(Decoration m in player.markerList) {
				m.doScaleTranslateAndTexture();
				m.frameNumber = m.sprite.draw(nowBillboarding, m.billboards, m.cycleNumber, m.frameNumber + m.animDirection * e.Time);
			}

			//Now render transparent sprites in sorted order
			//foreach(RenderObject obj in renderList) {
			//    if((enable3d && obj.existsIn3d) || (!enable3d && obj.existsIn2d) || isInTransition) {
			//        if((!obj.is3dGeo) && obj.sprite.hasAlpha) {
			//            obj.doScaleTranslateAndTexture();
			//            obj.frameNumber = obj.sprite.draw(nowBillboarding, obj.billboards, obj.cycleNumber, obj.frameNumber + obj.animDirection * e.Time);
			//        }
			//    }
			//}
        }

        /// <summary>
        /// Switches the sprites and bounding boxes of anything that billboards
        /// Called by camera at appropriate time
        /// </summary>
		public void doBillboards() {
			if(enable3d) {
				nowBillboarding = true;
			} else {
				nowBillboarding = false;
			}

			//Temporary hack until we get a setting added to everything that does this
			player.cycleNumber += (enable3d ? +1 : -1);

			//Switch pboxes and cboxes for things that billboard
			foreach(PhysicsObject p in physList) {
				if(p.billboards == Billboarding.Yes) {
					p.swapPBox();
				}
			}
			foreach(CombatObject c in combatList) {
				if(c.billboards == Billboarding.Yes) {
					c.swapCBox();
				}
			}

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
		}

        /// <summary>
        /// Deals with Hardware input relivant to the playstate
        /// </summary>
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

            if (eng.Keyboard[Key.Escape] || eng.Keyboard[Key.Tilde])
            {
                //eng.PushState(menustate);
                eng.PushState(pms);
            }

			//********************** tab
			if(!isInTransition && player.onGround) {
				if(eng.Keyboard[Key.Tab] && !tabDown) {
					enable3d = !enable3d;
					tabDown = true;
					player.velocity.Z = 0;
					isInTransition = true;
					camera.startTransition(enable3d);

					//figure out if the player gets a grace jump
					if(enable3d && player.onGround) {
						Vector3 tmpLoc = new Vector3(player.location);
						Vector3 tmpVel = new Vector3(player.velocity);
						int tmpHealth = player.health;
						//Try letting gravity drop us, if we fall then we get a jump.  This is a bit hackish, because
						//if we hit an enemy bad things will happen, but since we were on the ground it should be fairly safe.... hopefully.
						player.velocity = new Vector3(0, 0, 0);
						player.physUpdate3d(0.01, physList);
						Debug.Assert(tmpHealth == player.health, "ERROR: Player touched an enemy while checking for grace jump");
						if(!player.onGround) { //we're now in midair, so we get a jump
							player.viewSwitchJumpTimer = 1.0;
							player.setLocation = tmpLoc;
							player.onGround = true;
						}
						player.velocity = tmpVel;
					}
				} else if(!eng.Keyboard[Key.Tab]) {
					tabDown = false;
				}
			}
        }


        /// <summary>
        /// Change the current level being played to the parameter
        /// </summary>
        /// <param name="l">Level to be changed to</param>
        public void changeCurrentLevel(int l)
        {
            current_level = l;
        }      
    }
}
