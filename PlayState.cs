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
    /** Main State of the game that will be active while the player is Playing **/
    public class PlayState : GameState
    {
        //debug
		internal bool aienabled;
		internal bool musicenabled;

		internal GameEngine eng;
		internal MainMenuState menustate;
		internal PauseMenuState pms;
		internal Player player;
        internal ZookeeperAI bossAI;
        public bool bossMode;
		internal Camera camera;

        //These are the lists of all objects in the game
        internal List<GameObject> objList; //everything is in objList, and then also pointed to from the appropriate interface lists
		internal List<RenderObject> renderList;
        internal List<PhysicsObject> colisionList; // colision list = only things that are moving that need to detect colisions
		internal List<PhysicsObject> physList; // physList is a list of everything that has a bounding box
		internal List<AIObject> aiList;// aka list of enemies
		internal List<CombatObject> combatList; // list of stuff that effects the player in combat, projectiles, enemies
		internal List<Background> backgroundList;
        internal AudioFile levelMusic;

		public SphereRegion bossRegion, endRegion;
		public Vector3 bossAreaCenter, bossAreaBounds;

		internal bool enable3d; //true when being viewed in 3d
		internal int current_level = -1;// Member variable that will keep track of the current level being played.  This be used to load the correct data from the backends.
		internal bool nowBillboarding; //true when billboarding objects should rotate into 3d view
        public Texture Healthbar, bHealth;
		public SpriteSheet staminaBar, staminaBack, staminaFrame;

		internal bool tabDown;
		public bool clickdown;

		/// <summary>
		/// PlayState is the state in which the game is actually playing, this should only be called once when a new game is made.
		/// </summary>
		/// <param name="engine">Pointer to the game engine</param>
		public PlayState(GameEngine engine, MainMenuState menustate) {
			eng = engine;
			this.menustate = menustate;

			bossMode = false;
			enable3d = false;
			tabDown = false;
			clickdown = false;
			nowBillboarding = false;
			aienabled = true;
			musicenabled = false;

			pms = new PauseMenuState(eng);
		}

        /// <summary>
        /// Refreshes graphics when this state becomes active again after being frozen.
        /// </summary>
		public override void MakeActive() {
            if (musicenabled)
                levelMusic.ReplayFile();

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			GL.ShadeModel(ShadingModel.Smooth);
			GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

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

            if (musicenabled) {
                // Loop music when necessary
                if (levelMusic.CurrentSource.FileHasEnded) {
                    levelMusic.CurrentSource.FileHasEnded = false;
                    levelMusic.ReplayFile();
                }
            }
            //Next check if the player is dead. If he is, game over man
            if (player.health <= 0) {
                GameOverState GGbro = new GameOverState(menustate, eng);
                levelMusic.Stop();
                eng.ChangeState(GGbro);
            }

			//See if we need to trigger an event (like the end of the level or a boss)
			if(!bossMode && bossRegion.contains(player.location)) {
			    //Entered boss area - make changes to camera, etc
                enterBossMode();
			}
			if(endRegion.contains(player.location)) {
				//Finished level
				eng.ChangeState(new MainMenuState(eng)); //Later we should go to the next level when applicable
			}

            //do boss dies stuff, transition to next level?
            if (bossMode && (bossAI.gethealth() <= 0)) {
                bossAI.killBoss(this);
				bossMode = false;
                //transition to next level? or state? or w/e
            }

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
                if (co.type == (int)CombatType.enemy) {
					if(co.health <= 0) {
						objList.Remove((GameObject)co);
						physList.Remove((PhysicsObject)co);
						colisionList.Remove((PhysicsObject)co);
						renderList.Remove((RenderObject)co);
						aiList.Remove((AIObject)co);
						combatList.Remove(co);
                    }
				} else if(co.type == (int)CombatType.projectile ||
						  co.type == (int)CombatType.squish ||
						  co.type == (int)CombatType.grenade) {
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
            if (!camera.isInTransition) {
				//Deal with everyone's acceleration, run AI on enemies
				player.updateState(enable3d, eng.Keyboard, e.Time);
                if (!bossMode) {
                    foreach (AIObject aio in aiList) {
                        if (aio.ScreenRegion == GameObject.ON_SCREEN) {
                            if (aienabled) {
                                aio.aiUpdate(e.Time, this, player.location, enable3d, physList);
                            }
                        }
                    }
                }
                else { //if we are in boss mode then route control to the bosses code instead of the enemy updates
                    //update the boss
                    bossAI.update(e.Time, this, player.location, enable3d);
                }

				//Now that everyone's had a chance to accelerate, actually
				//translate that into velocity and position
				if(enable3d) {
                    player.physUpdate3d(e.Time, physList);
					foreach(PhysicsObject po in colisionList) {
                        po.physUpdate3d(e.Time, physList);
					}
				} else {
                    player.physUpdate2d(e.Time, physList);
					foreach(PhysicsObject po in colisionList) {
                        po.physUpdate2d(e.Time, physList);
					}
				}

				//These updates must be last
				foreach(Background b in backgroundList) {
					b.UpdatePositionX(player.deltax);
				}
			}
			//Update the camera whether we're transitioning or not
			camera.Update(e.Time);
		}

		//Called by camera for parallaxing
		public void updateBackgroundsYPos(float deltay) {
			foreach(Background b in backgroundList) {
				b.UpdatePositionY(deltay);
			}
		}

		public void enterBossMode() {
			bossMode = true;
			camera.enterBossMode(bossAreaCenter, bossAreaBounds);
		}

        /// <summary>
        /// The Draw update, happens every frame
        /// </summary>
        /// <param name="e">FrameEventArgs from OpenTK's update</param>
        public override void Draw(FrameEventArgs e) {
            //e = new FrameEventArgs(e.Time * 0.1);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // UNCOMMENT THIS AND LINE AFTER DRAW TO ADD MOTION BLUR
            //if (isInTransition)
            //{
            //    GL.Accum(AccumOp.Return, 0.95f);
            //    GL.Clear(ClearBufferMask.AccumBufferBit);
            //}

            camera.SetModelView();

			foreach(RenderObject obj in renderList) {
				if((camera.isInTransition && obj.existsIn2d && obj.existsIn3d) ||
				   (!camera.isInTransition && ((enable3d && obj.existsIn3d) || (!enable3d && obj.existsIn2d)))) {
					if(obj.is3dGeo) {
						obj.doScaleTranslateAndTexture();
						obj.mesh.Render();
					} else {
						obj.doScaleTranslateAndTexture();
						if(camera.isInTransition) { //Pause all animations in transition
							obj.sprite.draw(nowBillboarding, obj.billboards, obj.cycleNumber, obj.frameNumber + obj.animDirection * e.Time);
						} else {
							obj.frameNumber = obj.sprite.draw(nowBillboarding, obj.billboards, obj.cycleNumber, obj.frameNumber + obj.animDirection * e.Time);
						}
					}
				}
			}

			//Draw the parabola if grenade is active
			foreach(Decoration m in player.markerList) {
				m.doScaleTranslateAndTexture();
				m.frameNumber = m.sprite.draw(nowBillboarding, m.billboards, m.cycleNumber, m.frameNumber + m.animDirection * e.Time);
			}

			//Draw HUD
			float dec = (float)player.health / player.maxHealth;
            bHealth.DrawHUDElement(bHealth.Width, bHealth.Height, 300, 670, scaleX: 0.5f, scaleY: 0.5f);
			Healthbar.DrawHUDElement(Healthbar.Width, Healthbar.Height, 300, 670, scaleX: 0.5f, scaleY: 0.5f, decrementX: dec);

			drawStaminaBar();

            // UNCOMMENT TO ADD MOTION BLUR
            //if (isInTransition)
            //{
            //    GL.Accum(AccumOp.Accum, 0.9f);
            //}
        }

		public void drawStaminaBar() {
			GL.Disable(EnableCap.DepthTest);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Replace);

			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0, 1280, 0, 720, 0, 1);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();
			GL.LoadIdentity();

			GL.PushMatrix();
			GL.Translate(1000, 675, 0);
			GL.Scale(512, 25, 0);
			staminaBack.draw(false, Billboarding.Lock2d);

			GL.PushMatrix();
			double scale = player.stamina / player.maxStamina;
			GL.Translate(744.0 + 256.0 * scale, 675, 0);
			GL.Scale(512.0 * scale, 25, 0);

			staminaBar.draw(false, Billboarding.Lock2d);

			GL.PushMatrix();
			GL.Translate(1000, 675, 0);
			GL.Scale(512, 25, 0);
			staminaFrame.draw(false, Billboarding.Lock2d);

			GL.PopMatrix();
			GL.Enable(EnableCap.DepthTest);
			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
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

			//Switch animation cycle for anything with separate 2D/3D animations
			foreach(RenderObject o in renderList) {
				if(o.hasTwoAnims) {
					o.cycleNumber += (enable3d ? +1 : -1);
				}
			}

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
		}

        /// <summary>
        /// Deals with Hardware input relevant to the playstate
        /// </summary>
        private void DealWithInput() {

            if (eng.Keyboard[Key.Escape] || eng.Keyboard[Key.Tilde]) {
                levelMusic.Stop();
                eng.PushState(pms);
            }

			//********************** tab
			if(!camera.isInTransition && player.onGround && player.lookDownTimer != -2.0) {
				if(eng.Keyboard[Key.Tab] && !tabDown) {
					enable3d = !enable3d;
					tabDown = true;
					player.velocity.Z = 0;
					camera.startTransition(enable3d);

					//figure out if the player gets a grace jump
					if(enable3d && player.onGround && !VectorUtil.overGround3dLoose(player, physList)) {
						player.viewSwitchJumpTimer = 1.0;
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
