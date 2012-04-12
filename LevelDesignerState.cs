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
using LevelDesignerTool;
// XML parser
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.Threading;
using System.Windows.Markup;
using System.IO;


namespace U5Designs
{
    /** Main State of the game that will be active while the player is Playing **/
    public class LevelDesignerState : PlayState
    {

        // Our current selected object for movement
        internal GameObject SelectedObject = null;
        
        System.Windows.Application app;
        MainWindow window;
        
        bool windowInit;
        // Changes ortho projection
        double orthoWidth = 192;
        double orthoHeight = 108;
        List<string> ObstaclesList;
        List<string> EnemiesList;
        // Number of items and current item in the Obstacles List
        int objectIter;
        // Number of items and current item in the Enemies List


        Dictionary<int, List<string>> ObjectList = new Dictionary<int,List<string>>();
        int listKey = 0;

        // Switches to control keyboard presses
        bool leftBDown = false;
        bool rightBDown = false;
        bool AllowSnapping = false;
        bool MoveKeyDown = false;

        float UnitsToMove = 1.0f;
        /** These Dictionarys will map Vector3's to different game objects that will then be written out to a level file.  When the user adds
         * a given object type into the level editor that object will be added to the appropriate Dictionary.  When the user deletes an object
         * it will be removed from the appropriate list.**/
        public struct BackGroundData
        {
            public Vector3 _scale;
            public String _sprite_file;
            public float _speed;
            public Vector3 _location;
        };
        Dictionary<int, List<Vector3>> _xml_obstacle_list;
        Dictionary<int, List<Vector3>> _xml_enemy_list;

        Dictionary<Vector3, int> _xml_dec_list;
        Dictionary<String, int> _xml_sound_list;
        Dictionary<BackGroundData, int> _xml_bg_list;
        Dictionary<Vector3, int> _xml_boss_area_list;
        Dictionary<Vector3, int> _xml_boss_bounds_list;
        Dictionary<Vector3, int> _xml_boss_center_list;
        List<Vector3> _xml_end_region;

        MyTextWriter textWriter;

        #region Constructor
        /// <summary>
        /// PlayState is the state in which the game is actually playing, this should only be called once when a new game is made.
        /// </summary>
        /// <param name="engine">Pointer to the game engine</param>
        /// <param name="lvl">the level ID</param>
		public LevelDesignerState(GameEngine engine, MainMenuState menustate, int lvl)
            : base(engine, menustate)
        {
			current_level = lvl;
			LoadLevel.Load(lvl, this);

			//Have to load the next few things here for now because they require the GraphicsContext
			foreach(RenderObject ro in renderList) {
				if(ro.is3dGeo) {
					ro.texture.init();
				}
			}

			//HUD Health Bar
			Assembly assembly = Assembly.GetExecutingAssembly();
			eng.StateTextureManager.LoadTexture("Healthbar", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.healthbar_top.png"));
			Healthbar = eng.StateTextureManager.GetTexture("Healthbar");
			eng.StateTextureManager.LoadTexture("bHealth", assembly.GetManifestResourceStream("U5Designs.Resources.Textures.healthbar_bottom.png"));
			bHealth = eng.StateTextureManager.GetTexture("bHealth");

			//initialize camera
			camera = new Camera(eng.ClientRectangle.Width, eng.ClientRectangle.Height, player, this);
			player.cam = camera;



            // undo this when done testing ObjList = LoadLevel.Load(current_level);
            player.inLevelDesignMode = true;
            levelMusic.Stop();

            //Thread t = new Thread(new ThreadStart(CreateLevelDesignForm));
            //t.ApartmentState = ApartmentState.STA;
            //t.Start();
            ObstaclesList = GetItemsFromDir("Obstacles.obstacles.txt");
            EnemiesList = GetItemsFromDir("Enemies.enemies.txt");
            ObjectList.Add(0, ObstaclesList);
            ObjectList.Add(1, EnemiesList);

            _xml_obstacle_list = new Dictionary<int, List<Vector3>>();
            // Add everything from previous list into this list.  Problem is we have no idea what the object is.
            //foreach (GameObject obj in objList)
            //{
            //    if (obj is Obstacle)
            //    {
            //    }
            //}

            _xml_dec_list = new Dictionary<Vector3,int>();
            _xml_enemy_list = new Dictionary<int, List<Vector3>>();
            _xml_sound_list = new Dictionary<string,int>();
            _xml_bg_list = new Dictionary<BackGroundData,int>();
            _xml_boss_area_list = new Dictionary<Vector3,int>();
            _xml_boss_bounds_list = new Dictionary<Vector3, int>();
            _xml_boss_center_list = new Dictionary<Vector3, int>();
            _xml_end_region = new List<Vector3>();

            StartDesignerThread();
            textWriter = new MyTextWriter(eng.ClientSize, new Size(200, 200));
        }
        #endregion

        #region WPF Stuff
        /// <summary>
        /// Opens the secondary tool menu which will be useful for user to choose items
        /// </summary>
        private void OpenWindow()
        {
            if (System.Windows.Application.Current == null)
            {
                app = new System.Windows.Application();
                window = new LevelDesignerTool.MainWindow();
                app.Run(window);
            }
        }

        /// <summary>
        /// Thread for secondary window
        /// </summary>
        private void StartDesignerThread()
        {
            var thread = new Thread(() =>
                {
                    OpenWindow();
                });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = false;
            thread.Start();
        }

        // IF WE DECIDE TO GO THE WINDOWS FORM ROUTE
        //[STAThread]
        //private void CreateLevelDesignForm()
        //{
        //        Application.EnableVisualStyles();
        //        Application.SetCompatibleTextRenderingDefault(false);
        //        LevelDesignerForm LDForm = new LevelDesignerForm();

        //        Application.Run(LDForm);
        //}
        #endregion

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
            textWriter.AddLine("TEST", new PointF(30, 30), new SolidBrush(Color.Blue));
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
            //textWriter.UpdateText();
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
            //textWriter.Draw();
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
            // SAVE THE CURRENT LEVEL TO A NEW LEVEL
            if (eng.Keyboard[Key.Period])
            {
                _write_level_file("TestLevelWrite", 99);
            }

            // Allow snapping for movement
            if (eng.Keyboard[Key.BackSlash])
            {
                AllowSnapping = true;
                UnitsToMove = 10.0f;
                Console.WriteLine("Snapping is: {0}", AllowSnapping.ToString());
            }

            if (eng.Keyboard[Key.Semicolon])
            {
                int totalKeys = ObjectList.Keys.Count;
                listKey += 1;
                objectIter = 0;
                if (listKey > totalKeys - 1)
                {
                    listKey = 0;
                }
                Console.WriteLine(" You are in {0}", listKey);
            }

            // If we are in 2D View, Allow user to zoom out and in with W and S
            if (!enable3d)
            {

                if (eng.Keyboard[Key.W])
                {
                    orthoWidth = Math.Max(192, orthoWidth - orthoWidth * 0.01);
                    orthoHeight = Math.Max(108, orthoHeight - orthoHeight * 0.01);
                    camera.Set2DCamera(orthoWidth, orthoHeight);
                }
                if (eng.Keyboard[Key.S])
                {
                    orthoWidth = Math.Min(1920, orthoWidth + orthoWidth * 0.01);
                    orthoHeight = Math.Min(1080, orthoHeight + orthoHeight * 0.01);
                    camera.Set2DCamera(orthoWidth, orthoHeight);
                }
            }
            // Testing the Level Design feature of re-loading LoadLevel after changing coords for a given game object
            if (eng.Keyboard[Key.F5])
            {
                LevelDesignerState lds = new LevelDesignerState(eng, menustate, current_level);
                if (!window.IsActive)
                {
                    window.Show();
                }
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
            // Allow movement of the selected objects
            if (!enable3d)
            {
                if (eng.Keyboard[Key.Up] && !MoveKeyDown)
                {
                    MoveKeyDown = true;
                    MoveObjects(SelectedObject, new Vector3(0.0f, UnitsToMove, 0.0f));
                }
                if (eng.Keyboard[Key.Down] && !MoveKeyDown)
                {
                    MoveKeyDown = true;
                    MoveObjects(SelectedObject, new Vector3(0.0f, -UnitsToMove, 0.0f));
                }
                if (eng.Keyboard[Key.Left] && !MoveKeyDown)
                {
                    MoveKeyDown = true;
                    MoveObjects(SelectedObject, new Vector3(-UnitsToMove, 0.0f, 0.0f));
                }
                if (eng.Keyboard[Key.Right] && !MoveKeyDown)
                {
                    MoveKeyDown = true;
                    MoveObjects(SelectedObject, new Vector3(UnitsToMove, 0.0f, 0.0f));
                }

                if (!eng.Keyboard[Key.Up] && !eng.Keyboard[Key.Down] && !eng.Keyboard[Key.Left] && !eng.Keyboard[Key.Right])
                {
                    MoveKeyDown = false;
                }
                if (!eng.Keyboard[Key.BracketLeft])
                {
                    leftBDown = false;
                }
                if (!eng.Keyboard[Key.BracketRight])
                {
                    rightBDown = false;
                }
                // Moving between obstacles
                if (eng.Keyboard[Key.BracketLeft] && !leftBDown)
                {
                    leftBDown = true;
                    if (objectIter - 1 < 0)
                        objectIter = ObjectList[listKey].Count - 1;
                    else objectIter--;
                    
                    Console.WriteLine("Using {0}", ObjectList[listKey][objectIter]);
                }
                if (eng.Keyboard[Key.BracketRight] && !rightBDown)
                {
                    rightBDown = true;
                    if (objectIter + 1 > ObjectList[listKey].Count -1)
                        objectIter = 0;
                    else objectIter++;
                    

                    Console.WriteLine("Using {0}", ObjectList[listKey][objectIter]);
                }
            }
            #region 3D Move Controls
            else
            {
                if (eng.Keyboard[Key.Up] && !eng.Keyboard[Key.ControlLeft])
                {
                    MoveObjects(SelectedObject, new Vector3(1.0f, 0.0f, 0.0f));
                }
                if (eng.Keyboard[Key.Down] && !eng.Keyboard[Key.ControlLeft])
                {
                    MoveObjects(SelectedObject, new Vector3(-1.0f, 0.0f, 0.0f));
                }
                if (eng.Keyboard[Key.Left])
                {
                    MoveObjects(SelectedObject, new Vector3(0.0f, 0.0f, -1.0f));
                }
                if (eng.Keyboard[Key.Right])
                {
                    MoveObjects(SelectedObject, new Vector3(0.0f, 0.0f, 1.0f));
                }
                if (eng.Keyboard[Key.Up] && eng.Keyboard[Key.ControlLeft])
                {
                    MoveObjects(SelectedObject, new Vector3(0.0f, 1.0f, 0.0f));
                }
                if (eng.Keyboard[Key.Down] && eng.Keyboard[Key.ControlLeft])
                {
                    MoveObjects(SelectedObject, new Vector3(0.0f, -1.0f, 0.0f));
                }
            }
            #endregion
        }

        private void MoveObjects( GameObject gameObj, Vector3 newLoc)
        {
            if (gameObj == null) return;

            if (gameObj is Obstacle)
            {
                foreach (var key in _xml_obstacle_list.Keys)
                {
                    int xmlIndex;
                    if ((xmlIndex = _xml_obstacle_list[key].FindIndex(q => q == gameObj.location)) != -1)
                    {

                        _xml_obstacle_list[key][xmlIndex] += newLoc;
                    }
                }
                int index = objList.FindIndex(p => p.location == gameObj.location);
                if (index == -1) return;
                objList[index].location += newLoc;
                // Alter the dictionary location when we move objects
            }
            if (gameObj is Enemy)
            {
                foreach (var key in _xml_enemy_list.Keys)
                {
                    int xmlIndex;
                    if ((xmlIndex = _xml_enemy_list[key].FindIndex(q => q == gameObj.location)) != -1)
                    {

                        _xml_enemy_list[key][xmlIndex] += newLoc;
                    }
                }
                int index = objList.FindIndex(p => p.location == gameObj.location);
                if (index == -1) return;
                objList[index].location += newLoc;
                // Alter the dictionary location when we move objects
            }
            
        }
        private void HandleMouseInput()
        {
            if (!eng.ThisMouse.LeftPressed())
            {
                clickdown = false;
            }
            // ADDING OBJECTS TO THE LIST
            if (eng.ThisMouse.LeftPressed() && !clickdown && !enable3d)
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
                    if (obj is Obstacle || obj is Enemy)
                    {
                        if (eng.ThisMouse.inObjectRegion(obj, mouseWorld, enable3d))
                        {
                            SelectedObject = obj;
                            Console.WriteLine("Selected Object");
                            return;
                        }
                    }
                }

                Vector3 loc = new Vector3((float)mouseWorld.X, (float)mouseWorld.Y, 50.0f);
                
                // Handle adding enemies
                if (listKey == 1)
                {
                    Enemy e = parseEnemy(ObjectList[listKey][objectIter], loc, player);
                    objList.Add(e);
                    physList.Add(e);
                    renderList.Add(e);
                    clickdown = false;
                    // Add to obstacle list
                    if (_xml_enemy_list.ContainsKey(objectIter))
                    {
                        _xml_enemy_list[objectIter].Add(e.location);
                    }
                    else
                    {
                        List<Vector3> locList = new List<Vector3>();
                        locList.Add(e.location);
                        _xml_enemy_list.Add(objectIter, locList);
                    }
                }


                else
                {
                    Obstacle o = ParseObstacle(ObjectList[listKey][objectIter], loc);
                    if (o == null)
                    {
                        Decoration dec = ParseDecoration(ObjectList[listKey][objectIter], loc);
                        objList.Add(dec);
                        renderList.Add(dec);

                        // Add decoration to decoration list
                        _xml_dec_list.Add(dec.location, objectIter);

                        /** TODO !!!! ***/
                        // Need to parse Enemies, Sound files, boss regions, backgrounds

                        /// PSEUDO-CODE
                        // if ( boss region )
                        //      _xml_boss_region.Add(Vector3, index)

                        // if ( boss bounds )
                        //      _xml_boss_bounds.Add(Vector3, index)

                        // if ( boss center )
                        //      _xml_boss_center.Add(Vector3, index)

                        // if ( sound file )
                        //      _xml_sound.Add(object, index)

                        // if ( end region )
                        //      _xml_end_region.Add(Vector3)

                        // if ( enemy )
                        //      _xml_enemy.Add(object, index)

                        // if ( background )
                        //      _xml_bg.Add(BackGroundData, index)  /// BackGroundData is a struct to hold the multiple data items needed to create a BG
                    }
                    else
                    {
                        objList.Add(o);
                        physList.Add(o);
                        renderList.Add(o);
                        clickdown = false;

                        // Add to obstacle list
                        if (_xml_obstacle_list.ContainsKey(objectIter))
                        {
                            _xml_obstacle_list[objectIter].Add(o.location);
                        }
                        else
                        {
                            List<Vector3> locList = new List<Vector3>();
                            locList.Add(o.location);
                            _xml_obstacle_list.Add(objectIter, locList);
                        }

                    }
                }
            }
            else if (eng.ThisMouse.LeftPressed() && !clickdown && enable3d)
            {
                // TO DO: ALLOW SELECTION AND OF OBJECTS IN 3D
                clickdown = true;
                // Pull the projection and model view matrix from the camera.   
                Matrix4d project = this.camera.GetProjectionMatrix();
                Matrix4d model = this.camera.GetModelViewMatrix();
                float[] t = new float[1];
                GL.ReadPixels(this.eng.Mouse.X, this.eng.Height - this.eng.Mouse.Y, 1, 1, PixelFormat.DepthComponent, PixelType.Float, t);
                Vector3d mousecoord = new Vector3d((double)this.eng.Mouse.X, (double)(this.eng.Height - this.eng.Mouse.Y), 1.0);
                // Unproject the coordinates to convert from mouse to world coordinates
                mousecoord.Z = 0.0;
                Vector3d near = GameMouse.UnProject(mousecoord, model, project, this.camera.getViewport());
                mousecoord.Z = 1.0;
                Vector3d far = GameMouse.UnProject(mousecoord, model, project, this.camera.getViewport());

                //Interpolate to find coordinates just in front of player
                double tA = (player.location.X + (100.0f * t[0]) - near.X) / (far.X - near.X);
                Vector3d mouseWorld = new Vector3d(player.location.X + (100.0 * (1 - t[0])), near.Y + (far.Y - near.Y) * tA, near.Z + (far.Z - near.Z) * tA);
                Console.WriteLine("{0} \t {1} \t {2}", mouseWorld.X, mouseWorld.Y, mouseWorld.Z);
                // Console.WriteLine(t[0]);
                clickdown = false;
            }
            else if (eng.ThisMouse.RightPressed())
            {
                // Pull the projection and model view matrix from the camera.   
                Matrix4d project = this.camera.GetProjectionMatrix();
                Matrix4d model = this.camera.GetModelViewMatrix();
                Vector3d mousecoord = new Vector3d((double)this.eng.Mouse.X, (double)(this.eng.Height - this.eng.Mouse.Y), 1.0);
                // Unproject the coordinates to convert from mouse to world coordinates
                Vector3d mouseWorld = GameMouse.UnProject(mousecoord, model, project, this.camera.getViewport());
                GameObject objToRemove = null;
                foreach (GameObject obj in objList)
                {
                    if (obj is Obstacle)
                    {
                        if (eng.ThisMouse.inObjectRegion(obj, mouseWorld, enable3d))
                        {
                            objToRemove = (Obstacle)obj;
                            foreach (int key in _xml_obstacle_list.Keys)
                            {
                                if(_xml_obstacle_list[key].Contains(objToRemove.location))
                                {
                                    _xml_obstacle_list[key].Remove(objToRemove.location);
                                }
                            }
                        }
                    }

                    // Remove object if it is an enemy
                    if (obj is Enemy)
                    {
                        if (eng.ThisMouse.inObjectRegion(obj, mouseWorld, enable3d))
                        {
                            objToRemove = obj;
                            foreach (int key in _xml_enemy_list.Keys)
                            {
                                if (_xml_enemy_list[key].Contains(objToRemove.location))
                                {
                                    _xml_enemy_list[key].Remove(objToRemove.location);
                                }
                            }
                        }
                    }
                    //else if (obj is Enemy)
                    //{
                    //    _xml_enemy_list.Remove(obj.location);
                    //    break;
                    //}
                    //else if (obj is Background)
                    //{
                    //    // Set temp BGD properties to the currently selected Background object and then search for a match in the Dictionary
                    //    Background b = (Background)obj;
                    //    BackGroundData bgd;
                    //    bgd._location = obj.location;
                    //    bgd._scale = b.scale;
                    //    bgd._speed = b.Speed;
                    //    bgd._sprite_file = b.Path;

                    //    // ?? Not sure this works, hard to test at the moment so be aware this may be a broken hack...sorry coding blind on this one ...
                    //    foreach (KeyValuePair<BackGroundData, int> pair in _xml_bg_list)
                    //    {
                    //        BackGroundData _b = pair.Key;
                    //        if (_b._location == bgd._location && _b._scale == bgd._scale && _b._speed == bgd._speed && _b._sprite_file.CompareTo(bgd._sprite_file) == 0)
                    //        {
                    //            _xml_bg_list.Remove(_b);
                    //            break;
                    //        }
                    //    }
                    //    break;
                    //}

                }
                if (objToRemove != null)
                {
                    objList.Remove(objToRemove);
                    physList.Remove((PhysicsObject)objToRemove);
                    renderList.Remove((RenderObject)objToRemove);
                }
                
            }

        }

        #region Remove/Add Items to List
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
        #endregion

        /// <summary>
        /// Change the current level being played to the parameter
        /// </summary>
        /// <param name="l">Level to be changed to</param>
        new public void changeCurrentLevel(int l)
        {
            current_level = l;
        }

        /*--------------------------------------------------------------------------------------------------------------
         * 
         *                                              LEVEL DESIGNER FILE METHODS
         * 
         * -------------------------------------------------------------------------------------------------------------
         * */
        #region Parse Level Files
        public List<string> GetItemsFromDir(string filename)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string _o_path ="U5Designs.Resources.Data." + filename;
            Stream fstream = assembly.GetManifestResourceStream(_o_path);
            StreamReader r = new StreamReader(fstream);
            List<string> obstacleList = new List<string>();
            string obs = r.ReadLine();
            while( obs != null)
            {
                obstacleList.Add(obs);
                obs = r.ReadLine();
            }
            return obstacleList;
        }


        private Decoration ParseDecoration(string path, Vector3 loc)
        {
            Decoration dec = null;
            // Instantiate the list
            Assembly assembly = Assembly.GetExecutingAssembly();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;

            string _d_path = "U5Designs.Resources.Data.Decorations." + path;
            Stream fstream = assembly.GetManifestResourceStream(_d_path);
            XmlDocument doc = new XmlDocument();
            XmlReader reader = XmlReader.Create(fstream, settings);
            doc.Load(reader);

            Vector3 scale = LoadLevel.parseVector3(doc.GetElementsByTagName("scale")[0]);

            bool _draw2 = Convert.ToBoolean(doc.GetElementsByTagName("draw2")[0].InnerText);
            bool _draw3 = Convert.ToBoolean(doc.GetElementsByTagName("draw3")[0].InnerText);


            // Check to see if the current Obstacle is 2D or 3D and handle accordingly
            if (Convert.ToBoolean(doc.GetElementsByTagName("is2d")[0].InnerText))
            {
                // Create the SpriteSheet 
                SpriteSheet ss = LoadLevel.parseSpriteFile(doc.GetElementsByTagName("sprite")[0].InnerText);

                Billboarding bb = Billboarding.Yes;  //Have to put something here for it to compile
                switch (doc.GetElementsByTagName("billboards")[0].InnerText)
                {
                    case "yes":
                    case "Yes":
                        bb = Billboarding.Yes;
                        break;
                    case "lock2d":
                    case "Lock2d":
                        bb = Billboarding.Lock2d;
                        break;
                    case "lock3d":
                    case "Lock3d":
                        bb = Billboarding.Lock3d;
                        break;
                    default:
                        Console.WriteLine("Bad obstacle file: " + path);
                        Environment.Exit(1);
                        break;
                }
                dec = new Decoration(loc, scale, _draw2, _draw3, bb, ss);
            }
            else
            {
                XmlNodeList _m = doc.GetElementsByTagName("mesh");
                ObjMesh _mesh = new ObjMesh(assembly.GetManifestResourceStream("U5Designs.Resources.Geometry." + _m.Item(0).InnerText));

                XmlNodeList _b = doc.GetElementsByTagName("bmp");
				List<Bitmap> texFrames = new List<Bitmap>();
				foreach(XmlNode n in _b) {
					texFrames.Add(new Bitmap(assembly.GetManifestResourceStream("U5Designs.Resources.Textures." + n.InnerText)));
				}
                MeshTexture _bmp = new MeshTexture(texFrames);
				_bmp.init();

                dec = new Decoration(loc, scale, _draw2, _draw3, _mesh, _bmp);
            }
            fstream.Close();
            return dec;
        }


        private Obstacle ParseObstacle(string path, Vector3 loc)
        {
            Obstacle o = null;
            Assembly assembly = Assembly.GetExecutingAssembly();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlDocument doc = new XmlDocument();
            string _o_path = "U5Designs.Resources.Data.Obstacles." + path;
            Stream fstream = assembly.GetManifestResourceStream(_o_path);
            XmlReader reader = XmlReader.Create(fstream, settings);
            doc.Load(reader);

            Vector3 scale = LoadLevel.parseVector3(doc.GetElementsByTagName("scale")[0]);
            Vector3 pbox = LoadLevel.parseVector3(doc.GetElementsByTagName("pbox")[0]);

            bool _draw2 = Convert.ToBoolean(doc.GetElementsByTagName("draw2")[0].InnerText);
            bool _draw3 = Convert.ToBoolean(doc.GetElementsByTagName("draw3")[0].InnerText);

            bool _collides2d = Convert.ToBoolean(doc.GetElementsByTagName("collidesIn2d")[0].InnerText);
            bool _collides3d = Convert.ToBoolean(doc.GetElementsByTagName("collidesIn3d")[0].InnerText);

            // Check to see if the current Obstacle is 2D or 3D and handle accordingly
            XmlNodeList _type = doc.GetElementsByTagName("is2d");
            if (Convert.ToBoolean(_type.Item(0).InnerText))
            {
                String ss_path = doc.GetElementsByTagName("sprite")[0].InnerText;
                fstream.Close();
                SpriteSheet ss = LoadLevel.parseSpriteFile(ss_path);

                Billboarding bb = Billboarding.Yes;  //Have to put something here for it to compile
                switch (doc.GetElementsByTagName("billboards")[0].InnerText)
                {
                    case "yes":
                    case "Yes":
                        bb = Billboarding.Yes;
                        break;
                    case "lock2d":
                    case "Lock2d":
                        bb = Billboarding.Lock2d;
                        break;
                    case "lock3d":
                    case "Lock3d":
                        bb = Billboarding.Lock3d;
                        break;
                    default:
                        Console.WriteLine("Bad obstacle file: " + path);
                        Environment.Exit(1);
                        break;
                }
                o = new Obstacle(loc, scale, pbox, _draw2, _draw3, _collides2d, _collides3d, bb, ss);
            }
            else
            {
                fstream.Close();
                XmlNodeList _m = doc.GetElementsByTagName("mesh");
                ObjMesh _mesh = new ObjMesh(assembly.GetManifestResourceStream("U5Designs.Resources.Geometry." + _m.Item(0).InnerText));

                XmlNodeList _b = doc.GetElementsByTagName("bmp");
				List<Bitmap> texFrames = new List<Bitmap>();
				foreach(XmlNode n in _b) {
					texFrames.Add(new Bitmap(assembly.GetManifestResourceStream("U5Designs.Resources.Textures." + n.InnerText)));
				}
                MeshTexture _tex = new MeshTexture(texFrames);
				_tex.init();

                o = new Obstacle(loc, scale, pbox, _draw2, _draw3, _collides2d, _collides3d, _mesh, _tex);
                fstream.Close();
            }

            return o;
        }
        #endregion

        #region Enemy Parsing
        private Enemy parseEnemy(string path, Vector3 location, Player player)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;

                string _e_path = "U5Designs.Resources.Data.Enemies." + path;
                Stream fstream = assembly.GetManifestResourceStream(_e_path);
                XmlDocument doc = new XmlDocument();
                XmlReader reader = XmlReader.Create(fstream, settings);
                doc.Load(reader);

                bool draw_2d, draw_3d;
                int _health, _damage, _AI;
                float _speed;

                Vector3 scale = LoadLevel.parseVector3(doc.GetElementsByTagName("scale")[0]);
                Vector3 pbox = LoadLevel.parseVector3(doc.GetElementsByTagName("pbox")[0]);
                Vector3 cbox = LoadLevel.parseVector3(doc.GetElementsByTagName("cbox")[0]);

                draw_2d = Convert.ToBoolean(doc.GetElementsByTagName("draw2")[0].InnerText);
                draw_3d = Convert.ToBoolean(doc.GetElementsByTagName("draw3")[0].InnerText);

                XmlNodeList _h = doc.GetElementsByTagName("health");
                _health = Convert.ToInt32(_h.Item(0).InnerText);
                XmlNodeList _d = doc.GetElementsByTagName("damage");
                _damage = Convert.ToInt32(_d.Item(0).InnerText);
                XmlNodeList _s = doc.GetElementsByTagName("speed");
                _speed = Convert.ToSingle(_s.Item(0).InnerText);
                XmlNodeList _ai = doc.GetElementsByTagName("AI");
                _AI = Convert.ToInt32(_ai.Item(0).InnerText);
                XmlNodeList _sp = doc.GetElementsByTagName("sprite");
                string _sprite_path = _sp.Item(0).InnerText;

                // Projectile stuff
                XmlNodeList _p = doc.GetElementsByTagName("proj");


                // Pause now and parse the Sprite.dat to create the necessary Sprite that is associated with the current Enemy object
                fstream.Close();

                // Create the SpriteSheet              
                SpriteSheet ss = LoadLevel.parseSpriteFile(_sprite_path);

                if (_p.Count == 0)
                {
                    return new Enemy(player, location, scale, pbox, cbox, draw_2d, draw_3d, _health, _damage, _speed, _AI, ss);
                }
                else
                {
                    ProjectileProperties p = LoadLevel.parseProjectileFile(_p.Item(0).InnerText);

                    return new Enemy(player, location, scale, pbox, cbox, draw_2d, draw_3d, _health, _damage, _speed, _AI, ss, p);
                }
        }
        #endregion

        /**
         * This method will take the various lists containing all the game elements that the user added and write them out in the correct format
         * to create a valid level.dat file.  It requires a level Name and Index value.  Right now the level Name is not being used but in the future it may be
         * 
         * */
        public void _write_level_file(String lName, int lIndex)
        {
            // File structure
            /*
             * <level>
             *  <bossregion>
             *  <endregion>
             *  <audiofile>
             *  <bossAreaCenter>
             *  <bossAreaBounds>
             *  <backgroundlist>
             *      <background>
             *  <enemylist>
             *      <enemy>
             *  <obstaclelist>
             *      <obstacle>
             *  <decorationlist>
             *      <decoration>
             * </level>
             * 
             * */
            string file = "../../Resources/Data/Levels/level_" + lIndex.ToString() + ".dat";

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            //settings.NewLineOnAttributes = true;
            using (XmlWriter writer = XmlWriter.Create(file, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("level");

                if (_xml_boss_area_list.Count > 0)
                {
                    writer.WriteStartElement("bossRegion");
                    foreach (KeyValuePair<Vector3, int> p1 in _xml_boss_area_list)
                    {
                        writer.WriteAttributeString("x", ((int)(p1.Key.X)).ToString());
                        writer.WriteAttributeString("y", ((int)(p1.Key.Y)).ToString());
                        writer.WriteAttributeString("z", ((int)(p1.Key.Z)).ToString());
                    }
                    writer.WriteEndElement();
                }

                if (_xml_end_region.Count > 0)
                {                    
                    foreach (Vector3 v in _xml_end_region)
                    {
                        writer.WriteStartElement("endRegion");
                        writer.WriteAttributeString("x", ((int)(v.X)).ToString());
                        writer.WriteAttributeString("y", ((int)(v.Y)).ToString());
                        writer.WriteAttributeString("z", ((int)(v.Z)).ToString());
                        writer.WriteEndElement();
                    }                    
                }

                if (_xml_sound_list.Count > 0)
                {
                    writer.WriteStartElement("audiofile");
                    foreach (KeyValuePair<String, int> p2 in _xml_sound_list)
                    {
                        writer.WriteStartElement("file");
                        writer.WriteString(p2.Key);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (_xml_boss_area_list.Count > 0)
                {
                    foreach (KeyValuePair<Vector3, int> p3 in _xml_boss_area_list)
                    {
                        writer.WriteStartElement("bossAreaCenter");
                        writer.WriteAttributeString("x", ((int)(p3.Key.X)).ToString());
                        writer.WriteAttributeString("y", ((int)(p3.Key.Y)).ToString());
                        writer.WriteAttributeString("z", ((int)(p3.Key.Z)).ToString());
                        writer.WriteEndElement();
                    }
                }

                if (_xml_boss_bounds_list.Count > 0)
                {
                    foreach (KeyValuePair<Vector3, int> p4 in _xml_boss_bounds_list)
                    {
                        writer.WriteStartElement("bossAreaBounds");
                        writer.WriteAttributeString("x", ((int)(p4.Key.X)).ToString());
                        writer.WriteAttributeString("y", ((int)(p4.Key.Y)).ToString());
                        writer.WriteAttributeString("z", ((int)(p4.Key.Z)).ToString());
                        writer.WriteEndElement();
                    }
                }

                if (_xml_bg_list.Count > 0)
                {
                    writer.WriteStartElement("backgroundlist");
                    foreach (KeyValuePair<BackGroundData, int> p5 in _xml_bg_list)
                    {
                        BackGroundData _tmp = p5.Key;
                        writer.WriteStartElement("background");

                        writer.WriteStartElement("scale");
                        writer.WriteAttributeString("x", ((int)(_tmp._scale.X)).ToString());
                        writer.WriteAttributeString("y", ((int)(_tmp._scale.Y)).ToString());
                        writer.WriteAttributeString("z", ((int)(_tmp._scale.Z)).ToString());
                        writer.WriteEndElement();

                        writer.WriteStartElement("sprite");
                        writer.WriteString(_tmp._sprite_file);
                        writer.WriteEndElement();

                        writer.WriteStartElement("speed");
                        writer.WriteString(_tmp._speed.ToString());
                        writer.WriteEndElement();

                        // Keeping it simple I chose to not maintain a list of <loc> attributes in BackGroundData so the file can have multiple
                        // <background> tags
                        writer.WriteStartElement("loc");
                        writer.WriteAttributeString("x", ((int)(_tmp._location.X)).ToString());
                        writer.WriteAttributeString("y", ((int)(_tmp._location.Y)).ToString());
                        writer.WriteAttributeString("z", ((int)(_tmp._location.Z)).ToString());
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (_xml_enemy_list.Count > 0)
                {
                    writer.WriteStartElement("enemylist");
                    foreach (int key in _xml_enemy_list.Keys)
                    {
                        writer.WriteStartElement("enemy");

                        writer.WriteStartElement("path");
                        writer.WriteString(EnemiesList[key]); // need to use the index to look up which obstacle .dat file to use
                        writer.WriteEndElement();
                        foreach (Vector3 loc in _xml_enemy_list[key])
                        {
                            writer.WriteStartElement("loc");
                            writer.WriteAttributeString("x", ((int)(loc.X)).ToString());
                            writer.WriteAttributeString("y", ((int)(loc.Y)).ToString());
                            writer.WriteAttributeString("z", ((int)(loc.Z)).ToString());
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (_xml_obstacle_list.Count > 0)
                {
                    writer.WriteStartElement("obstaclelist");
                    foreach (int key in _xml_obstacle_list.Keys)
                    {
                        writer.WriteStartElement("obstacle");

                        writer.WriteStartElement("path");
                        writer.WriteString(ObstaclesList[key]); // need to use the index to look up which obstacle .dat file to use
                        writer.WriteEndElement();
                        foreach (Vector3 loc in _xml_obstacle_list[key])
                        {
                            writer.WriteStartElement("loc");
                            writer.WriteAttributeString("x", ((int)(loc.X)).ToString());
                            writer.WriteAttributeString("y", ((int)(loc.Y)).ToString());
                            writer.WriteAttributeString("z", ((int)(loc.Z)).ToString());
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (_xml_dec_list.Count > 0)
                {
                    writer.WriteStartElement("decorationlist");
                    foreach (KeyValuePair<Vector3, int> p8 in _xml_dec_list)
                    {
                        writer.WriteStartElement("decoration");

                        writer.WriteStartElement("path");
                        // TO DO !!!!!!!! writer.WriteString(); // need to use the index to look up which decoration .dat file to use
                        writer.WriteEndElement();

                        writer.WriteStartElement("loc");
                        writer.WriteAttributeString("x", ((int)(p8.Key.X)).ToString());
                        writer.WriteAttributeString("y", ((int)(p8.Key.Y)).ToString());
                        writer.WriteAttributeString("z", ((int)(p8.Key.Z)).ToString());
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
