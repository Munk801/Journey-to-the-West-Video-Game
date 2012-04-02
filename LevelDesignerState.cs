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
using System.Drawing;

namespace U5Designs
{
    /** Main State of the game that will be active while the player is Playing **/
    public class LevelDesignerState : PlayState
    {
        //debug
        bool aienabled = true;

        internal GameEngine eng;
        MainMenuState menustate;
        PauseMenuState pms;
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
        int current_level = -1;// Member variable that will keep track of the current level being played.  This be used to load the correct data from the backends.
        private bool nowBillboarding; //true when billboarding objects should rotate into 3d view

        bool tabDown;
        new public bool clickdown = false;

        // Our current selected object for movement
        internal Obstacle SelectedObject = null;

        // Changes ortho projection
        double orthoWidth = 192;
        double orthoHeight = 108;

        /// <summary>
        /// PlayState is the state in which the game is actually playing, this should only be called once when a new game is made.
        /// </summary>
        /// <param name="prvstate">The previous state(the menu that spawned this playstate)</param>
        /// <param name="engine">Pointer to the game engine</param>
        /// <param name="lvl">the level ID</param>
        public LevelDesignerState(MainMenuState prvstate, GameEngine engine, int lvl)
            : base(prvstate, engine, lvl)
        {
            //TODO: pass this the right file to load from

            // undo this when done testing ObjList = LoadLevel.Load(current_level);
            //LoadLevel.Load(0, this);
            player = base.player;
            player.inLevelDesignMode = true;
            objList = base.objList;
            renderList = base.renderList;
            colisionList = base.colisionList;
            physList = base.physList;
            aiList = base.aiList;
            combatList = base.combatList;
            backgroundList = base.backgroundList;
            base.levelMusic.Stop();
            //Every AI object needs a pointer to the player, initlize this here
            foreach (AIObject aio in aiList)
            {
                ((Enemy)aio).player = player;
            }

            //deal with states
            menustate = prvstate;
            eng = engine;
            pms = new PauseMenuState(engine);
            enable3d = false;
            tabDown = false;
            //initlize camera
            camera = new Camera(eng.ClientRectangle.Width, eng.ClientRectangle.Height, player, this,
                                    new int[] { eng.ClientRectangle.X, eng.ClientRectangle.Y, eng.ClientRectangle.Width, eng.ClientRectangle.Height });
            player.cam = camera;
            nowBillboarding = false;

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
            //levelMusic.Stop();
        }

        /// <summary>
        /// Refreshes graphics when this state becomes active again after being frozen.
        /// </summary>
        public override void MakeActive()
        {
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(0.26667f, 0.86667f, 1.0f, 1.0f);

            // TO DO: MAKE ORTHOGRAPHIC VIEW FURTHER BACK
            if (enable3d)
            {
                camera.Set3DCamera();
            }
            else
            {
                camera.Set2DCamera();
            }

            if (player.curProjectile.gravity)
            {
                eng.CursorVisible = false;
            }
            else
            {
                eng.CursorVisible = true; //TODO: When we have a crosshair, we'll change this
            }
        }

        /// <summary>
        /// Update, this gets called once every update frame
        /// </summary>
        /// <param name="e">FrameEventArgs from OpenTK's update</param>
        public override void Update(FrameEventArgs e)
        {
            player.cam.trackingPlayer = false;
            //First deal with hardware input
            DealWithInput();
            HandleMouseInput();

            //If the camera is transitioning, everything else is paused
            if (!camera.isInTransition)
            {
                //Deal with everyone's acceleration, run AI on enemies
                player.updateState(enable3d, eng.Keyboard, e.Time, true);

                //Now that everyone's had a chance to accelerate, actually
                //translate that into velocity and position
                if (enable3d)
                {
                    player.physUpdate3d(e.Time, physList);
                    foreach (PhysicsObject po in colisionList)
                    {
                        po.physUpdate3d(e.Time, physList);
                    }
                }
                else
                {
                    player.physUpdate2d(e.Time, physList);
                    foreach (PhysicsObject po in colisionList)
                    {
                        po.physUpdate2d(e.Time, physList);
                    }
                }

                //These updates must be last
                foreach (Background b in backgroundList)
                {
                    b.UpdatePositionX(player.deltax);
                }
            }
            //Update the camera whether we're transitioning or not
            camera.Update(e.Time);
        }

        //Called by camera for parallaxing
        public void updateBackgroundsYPos(float deltay)
        {
            foreach (Background b in backgroundList)
            {
                b.UpdatePositionY(deltay);
            }
        }

        public void enterBossMode()
        {
            bossMode = true;
            camera.enterBossMode(bossAreaCenter, bossAreaBounds);
        }

        /// <summary>
        /// The Draw update, happens every frame
        /// </summary>
        /// <param name="e">FrameEventArgs from OpenTK's update</param>
        public override void Draw(FrameEventArgs e)
        {
            //e = new FrameEventArgs(e.Time * 0.1);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            camera.SetModelView();

            foreach (RenderObject obj in renderList)
            {
                if ((enable3d && obj.existsIn3d) || (!enable3d && obj.existsIn2d) || camera.isInTransition)
                {
                    if (obj.is3dGeo)
                    {
                        obj.doScaleTranslateAndTexture();
                        obj.mesh.Render();
                    }
                    else
                    {
                        obj.doScaleTranslateAndTexture();
                        if (camera.isInTransition)
                        { //Pause all animations in transition
                            obj.sprite.draw(nowBillboarding, obj.billboards, obj.cycleNumber, obj.frameNumber + obj.animDirection * e.Time);
                        }
                        else
                        {
                            obj.frameNumber = obj.sprite.draw(nowBillboarding, obj.billboards, obj.cycleNumber, obj.frameNumber + obj.animDirection * e.Time);
                        }
                    }
                }
            }

            //Draw the parabola if grenade is active
            foreach (Decoration m in player.markerList)
            {
                m.doScaleTranslateAndTexture();
                m.frameNumber = m.sprite.draw(nowBillboarding, m.billboards, m.cycleNumber, m.frameNumber + m.animDirection * e.Time);
            }

        }


        /// <summary>
        /// Switches the sprites and bounding boxes of anything that billboards
        /// Called by camera at appropriate time
        /// </summary>
        new public void doBillboards()
        {
            if (enable3d)
            {
                nowBillboarding = true;
            }
            else
            {
                nowBillboarding = false;
            }

            //Temporary hack until we get a setting added to everything that does this
            player.cycleNumber += (enable3d ? +1 : -1);

            //Switch pboxes and cboxes for things that billboard
            foreach (PhysicsObject p in physList)
            {
                if (p.billboards == Billboarding.Yes)
                {
                    p.swapPBox();
                }
            }
            foreach (CombatObject c in combatList)
            {
                if (c.billboards == Billboarding.Yes)
                {
                    c.swapCBox();
                }
            }

            //TODO: This if/else block is a hack!
            //      Implement a flag that controls whether to switch these or not
            if (enable3d)
            {
                foreach (AIObject o in aiList)
                {
                    ((RenderObject)o).cycleNumber = 1;
                }
            }
            else
            { //2d
                foreach (AIObject o in aiList)
                {
                    ((RenderObject)o).cycleNumber = 0;
                }
            }
        }

        /// <summary>
        /// Deals with Hardware input relivant to the playstate
        /// </summary>
        private void DealWithInput()
        {
            if (eng.Keyboard[Key.W])
            {
                orthoWidth = Math.Max(192, orthoWidth - orthoWidth* 0.01);
                orthoHeight = Math.Max(108, orthoHeight - orthoHeight * 0.01);
                camera.Set2DCamera(orthoWidth, orthoHeight);
            }
            if (eng.Keyboard[Key.S])
            {
                orthoWidth = Math.Min(1920, orthoWidth + orthoWidth * 0.01);
                orthoHeight = Math.Min(1080, orthoHeight + orthoHeight * 0.01);
                camera.Set2DCamera(orthoWidth, orthoHeight);
            }

            // Testing the Level Design feature of re-loading LoadLevel after changing coords for a given game object
            if (eng.Keyboard[Key.F5])
            {
                LevelDesignerState lds = new LevelDesignerState(this.menustate, eng, 0);
                
                eng.ChangeState(lds);

                //LoadLevel.Load(0, pst);
            }

            if (eng.Keyboard[Key.Escape] || eng.Keyboard[Key.Tilde])
            {
                //eng.PushState(menustate);
                eng.PushState(pms);
            }

            //********************** tab
            if (!camera.isInTransition && player.onGround)
            {
                if (eng.Keyboard[Key.Tab] && !tabDown)
                {
                    enable3d = !enable3d;
                    tabDown = true;
                    player.velocity.Z = 0;
                    camera.startTransition(enable3d);

                    //figure out if the player gets a grace jump
                    if (enable3d && player.onGround && !VectorUtil.overGround3dLoose(player, physList))
                    {
                        player.viewSwitchJumpTimer = 1.0;
                    }
                }
                else if (!eng.Keyboard[Key.Tab])
                {
                    tabDown = false;
                }
            }

            if (eng.Keyboard[Key.Up])
            {
                MoveObjects(SelectedObject, new Vector3(0.0f, 1.0f, 0.0f));
            }
            if (eng.Keyboard[Key.Down])
            {
                MoveObjects(SelectedObject, new Vector3(0.0f, -1.0f, 0.0f));
            }
            if (eng.Keyboard[Key.Left])
            {
                MoveObjects(SelectedObject, new Vector3(-1.0f, 0.0f, 0.0f));
            }
            if (eng.Keyboard[Key.Right])
            {
                MoveObjects(SelectedObject, new Vector3(1.0f, 0.0f, 0.0f));
            }
        }

        private void MoveObjects(Obstacle o, Vector3 newLoc)
        {
            if (o == null) return;
            int index = objList.FindIndex(p => p.location == o.location);
            if (index == -1) return;
            objList[index].location += newLoc;
            
        }

        private void RemoveItemFromAllLists(Obstacle objToRemove)
        {
            if (objToRemove != null)
            {
                objList.Remove(objToRemove);
                physList.Remove(objToRemove);
                renderList.Remove(objToRemove);
            }
        }

        private void AddItemsToAllLists(Obstacle objToAdd)
        {
            if (objToAdd != null)
            {
                objList.Add(objToAdd);
                physList.Add(objToAdd);
                renderList.Add(objToAdd);
            }
        }
        private void HandleMouseInput()
        {
            // ADDING OBJECTS TO THE LIST
            if (eng.ThisMouse.LeftPressed() && !clickdown)
            {
                clickdown = true;
                // Pull the projection and model view matrix from the camera.   
                Matrix4d project = this.camera.GetProjectionMatrix();
                Matrix4d model = this.camera.GetModelViewMatrix();
                Vector3d mousecoord = new Vector3d((double)this.eng.Mouse.X, (double)(this.eng.Height - this.eng.Mouse.Y), 1.0);
                // Unproject the coordinates to convert from mouse to world coordinates
                Vector3d mouseWorld = GameMouse.UnProject(mousecoord, model, project, this.camera.getViewport());

                foreach (GameObject obj in objList)
                {
                    if (obj is Obstacle)
                    {
                        if (eng.ThisMouse.inObjectRegion((Obstacle)obj, mouseWorld))
                        {
                            SelectedObject = (Obstacle)obj;
                            Console.WriteLine("Selected Object");
                            return;
                        }
                    }
                }
                Console.WriteLine("X: " + eng.ThisMouse.Mouse.X);
                Console.WriteLine("Y: " + eng.ThisMouse.Mouse.Y);
                string path = "test_obstacle.dat";
                Assembly assembly = Assembly.GetExecutingAssembly();
                string _o_path = "U5Designs.Resources.Data.Obstacles." + path;

                // if 2D = false there is a mesh...otherwise there isn't
                ObjMesh _mesh = new ObjMesh(assembly.GetManifestResourceStream("U5Designs.Resources.Geometry." + "box.obj"));

                //XmlNodeList _b = doc.GetElementsByTagName("bmp");
                MeshTexture _tex = new MeshTexture(new Bitmap(assembly.GetManifestResourceStream("U5Designs.Resources.Textures." + "city_ground.png")));


                Vector3 loc = new Vector3((float)mouseWorld.X, (float)mouseWorld.Y, 50.0f);

                //Vector3 loc = new Vector3(eng.ThisMouse.Mouse.X, eng.ThisMouse.Mouse.Y, 0);

                bool _draw2 = true;
                bool _draw3 = true;
                Vector3 scale = new Vector3(50, 50, 50);
                Vector3 pbox = new Vector3(50, 50, 50);
                bool _collides2d = true;
                bool _collides3d = true;
                Obstacle o = new Obstacle(loc, scale, pbox, _draw2, _draw3, _collides2d, _collides3d, _mesh, _tex);
                objList.Add(o);
                physList.Add(o);
                renderList.Add(o);
            }
            else if (eng.ThisMouse.RightPressed())
            {
                // Pull the projection and model view matrix from the camera.   
                Matrix4d project = this.camera.GetProjectionMatrix();
                Matrix4d model = this.camera.GetModelViewMatrix();
                Vector3d mousecoord = new Vector3d((double)this.eng.Mouse.X, (double)(this.eng.Height - this.eng.Mouse.Y), 1.0);
                // Unproject the coordinates to convert from mouse to world coordinates
                Vector3d mouseWorld = GameMouse.UnProject(mousecoord, model, project, this.camera.getViewport());
                Obstacle objToRemove = null;
                foreach (GameObject obj in objList)
                {
                    if (obj is Obstacle)
                    {
                        if (eng.ThisMouse.inObjectRegion((Obstacle)obj, mouseWorld))
                        {
                            objToRemove = (Obstacle)obj;
                            break;
                        }
                    }
                }
                if (objToRemove != null)
                {
                    objList.Remove(objToRemove);
                    physList.Remove(objToRemove);
                    renderList.Remove(objToRemove);
                }
            }
            else clickdown = false;

        }

        /// <summary>
        /// Change the current level being played to the parameter
        /// </summary>
        /// <param name="l">Level to be changed to</param>
        new public void changeCurrentLevel(int l)
        {
            current_level = l;
        }
    }
}
