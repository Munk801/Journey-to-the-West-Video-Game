using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using Engine;

// XML Parser
using System.Xml;
using System.Reflection;
using System.IO;

// String splitter
using System.Text.RegularExpressions;

using System.Collections.Generic;

namespace U5Designs
{
	class LoadLevel {
		public static void Load(int level_to_load, PlayState ps) {
			ps.objList = new List<GameObject>();
			ps.renderList = new List<RenderObject>();
			ps.aiList = new List<AIObject>();
			ps.combatList = new List<CombatObject>();
			ps.physList = new List<PhysicsObject>();
			ps.colisionList = new List<PhysicsObject>();
			ps.backgroundList = new List<Background>();
            
			/**
			 * This next section of code will read in a level file and create an array of Enemy files to be parsed.
			 * */
			Assembly assembly = Assembly.GetExecutingAssembly();
			string file = "U5Designs.Resources.Data.Levels.level_test.dat";
			Stream fstream = assembly.GetManifestResourceStream(file);
			XmlDocument doc = new XmlDocument();
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			XmlReader reader = XmlReader.Create(fstream, settings);
			doc.Load(reader);
			XmlNode bossRegion = doc.GetElementsByTagName("bossRegion")[0];
			XmlNode endRegion = doc.GetElementsByTagName("endRegion")[0];
			XmlNode bossAreaCenter = doc.GetElementsByTagName("bossAreaCenter")[0];
			XmlNode bossAreaBounds = doc.GetElementsByTagName("bossAreaBounds")[0];
			XmlNodeList _b_list = doc.GetElementsByTagName("background");
			XmlNodeList _e_list = doc.GetElementsByTagName("enemy");
			XmlNodeList _o_list = doc.GetElementsByTagName("obstacle");
			XmlNodeList _d_list = doc.GetElementsByTagName("decoration");
            XmlNodeList _a_list = doc.GetElementsByTagName("audiofile");
			fstream.Close();

			//Regions and Boss Area
			ps.bossRegion = parseRegion(bossRegion);
			ps.endRegion = parseRegion(endRegion);
			ps.bossAreaCenter = parseVector3(bossAreaCenter);
			ps.bossAreaBounds = parseVector3(bossAreaBounds);
			
       XmlNode aud = _a_list[0];
            ps.levelMusic = new AudioFile(assembly.GetManifestResourceStream("U5Designs.Resources.Music." + aud.InnerText));
			

			//Various GameObjects

			List<Enemy> _elist = parse_Enemy_Files(_e_list);
			foreach(Enemy e in _elist) {
				ps.objList.Add(e);
				ps.physList.Add(e);
				ps.colisionList.Add(e);
				ps.renderList.Add(e);
				ps.aiList.Add(e);
                ps.combatList.Add(e);
			}

			List<Obstacle> _olist = parse_Obstacle_Files(_o_list);
			foreach(Obstacle o in _olist) {
				ps.objList.Add(o);
				ps.physList.Add(o);
				ps.renderList.Add(o);
			}

			List<Background> _blist = parse_Background_File(_b_list);
			foreach(Background b in _blist) {
				ps.objList.Add(b);
				ps.renderList.Add(b);
				ps.backgroundList.Add(b);
			}

			List<Decoration> _dlist = parse_Decoration_File(_d_list);
			foreach(Decoration d in _dlist) {
				ps.objList.Add(d);
				ps.renderList.Add(d);
			}          

			SpriteSheet.quad = new ObjMesh(assembly.GetManifestResourceStream("U5Designs.Resources.Geometry.quad.obj"));

			List<ProjectileProperties> playerProjectiles = new List<ProjectileProperties>();
			playerProjectiles.Add(parseProjectileFile("banana_projectile.dat"));
			playerProjectiles.Add(parseProjectileFile("coconut_grenade_projectile.dat"));

			//Player
			ps.player = new Player(parse_Sprite_File("player_sprite.dat"), playerProjectiles, ps);
			ps.player.marker = parse_Sprite_File("marker_sprite.dat");
			ps.physList.Add(ps.player);
			ps.renderList.Add(ps.player);

			//HUD Stamina Bar
			ps.staminaBack = parse_Sprite_File("stamina_back.dat");
			ps.staminaBar = parse_Sprite_File("stamina_bar.dat");
			ps.staminaFrame = parse_Sprite_File("stamina_frame.dat");
		}

		//Takes an XmlNode with attributes x, y, and z and turns it into a Vector3
		public static Vector3 parseVector3(XmlNode n) {
			Vector3 v = new Vector3();
			foreach(XmlNode a in n.Attributes) {
				switch(a.Name) {
					case "x":
						v.X = Convert.ToSingle(a.InnerText);
						break;
					case "y":
						v.Y = Convert.ToSingle(a.InnerText);
						break;
					case "z":
						v.Z = Convert.ToSingle(a.InnerText);
						break;
				}
			}
			return v;
		}

		//Takes an XmlNode with attributes x, y, z, and r and turns it into a SphereRegion
		public static SphereRegion parseRegion(XmlNode n) {
			Vector3 v = new Vector3();
			float r = 0.0f;
			foreach(XmlNode a in n.Attributes) {
				switch(a.Name) {
					case "x":
						v.X = Convert.ToSingle(a.InnerText);
						break;
					case "y":
						v.Y = Convert.ToSingle(a.InnerText);
						break;
					case "z":
						v.Z = Convert.ToSingle(a.InnerText);
						break;
					case "r":
						r = Convert.ToSingle(a.InnerText);
						break;
				}
			}
			return new SphereRegion(v, r);
		}

		public static ProjectileProperties parseProjectileFile(String path) {
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream fstream = assembly.GetManifestResourceStream("U5Designs.Resources.Data.Projectiles." + path);
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			XmlReader reader = XmlReader.Create(fstream, settings);
			XmlDocument doc = new XmlDocument();
			doc.Load(reader);

			Vector3 scale = parseVector3(doc.GetElementsByTagName("scale")[0]);
			Vector3 pbox = parseVector3(doc.GetElementsByTagName("pbox")[0]);
			Vector3 cbox = parseVector3(doc.GetElementsByTagName("cbox")[0]);
			bool draw2 = Convert.ToBoolean(doc.GetElementsByTagName("draw2")[0].InnerText);
			bool draw3 = Convert.ToBoolean(doc.GetElementsByTagName("draw3")[0].InnerText);
			int damage = Convert.ToInt32(doc.GetElementsByTagName("damage")[0].InnerText);
			float speed = Convert.ToSingle(doc.GetElementsByTagName("speed")[0].InnerText);
			bool grav = Convert.ToBoolean(doc.GetElementsByTagName("gravity")[0].InnerText);
			SpriteSheet ss = parse_Sprite_File(doc.GetElementsByTagName("sprite")[0].InnerText);
			
			XmlNodeList staminaCost = doc.GetElementsByTagName("staminaCost");
			XmlNodeList duration = doc.GetElementsByTagName("duration");
			fstream.Close();

			bool hasStamina = staminaCost.Count != 0;
			bool hasDuration = duration.Count != 0;

			if(!hasStamina && !hasDuration) {
				return new ProjectileProperties(scale, pbox, cbox, draw2, draw3, damage, speed, grav, ss);
			} else if(!hasDuration) {
				return new ProjectileProperties(scale, pbox, cbox, draw2, draw3, damage, speed, grav, ss, Convert.ToDouble(staminaCost[0].InnerText));
			} else if(!hasStamina) {
				return new ProjectileProperties(scale, pbox, cbox, draw2, draw3, damage, speed, grav, ss, 0.0, Convert.ToDouble(duration[0].InnerText));
			} else { //has both
				return new ProjectileProperties(scale, pbox, cbox, draw2, draw3, damage, speed, grav, ss, Convert.ToDouble(staminaCost[0].InnerText), Convert.ToDouble(duration[0].InnerText));
			}
		}

		public static SpriteSheet parse_Sprite_File(string path) {
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream fstream = assembly.GetManifestResourceStream("U5Designs.Resources.Data.Sprites." + path);
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			XmlReader reader = XmlReader.Create(fstream, settings);
			XmlDocument doc = new XmlDocument();
			doc.Load(reader);

			XmlNodeList _c_start_list = doc.GetElementsByTagName("c_starts");
			int[] cycleStarts = new int[_c_start_list.Count];
			for(int i = 0; i < _c_start_list.Count; i++) {
				cycleStarts[i] = Convert.ToInt32(_c_start_list.Item(i).InnerText);
			}
			XmlNodeList _c_length_list = doc.GetElementsByTagName("c_lengths");
			int[] cycleLengths = new int[_c_length_list.Count];
			for(int i = 0; i < _c_length_list.Count; i++) {
				cycleLengths[i] = Convert.ToInt32(_c_length_list.Item(i).InnerText);
			}
			XmlNodeList _sw = doc.GetElementsByTagName("t_width");
			int _width = Convert.ToInt32(_sw.Item(0).InnerText);
			XmlNodeList _sh = doc.GetElementsByTagName("t_height");
			int _height = Convert.ToInt32(_sh.Item(0).InnerText);
			bool _hasAlpha = Convert.ToBoolean(doc.GetElementsByTagName("hasAlpha")[0].InnerText);
			XmlNodeList _f = doc.GetElementsByTagName("fps");
			float _fps = (float)Convert.ToDouble(_f.Item(0).InnerText);

			XmlNodeList _bp = doc.GetElementsByTagName("bmp");
			fstream.Close();

			if(_bp.Count == 1) {
				string _bmp_path = "U5Designs.Resources.Textures." + _bp[0].InnerText;
				return new SpriteSheet(new Bitmap(assembly.GetManifestResourceStream(_bmp_path)), cycleStarts, cycleLengths, _width, _height, _hasAlpha, _fps);
			} else {
				Bitmap[] bmps = new Bitmap[_bp.Count];
				for(int i = 0; i < _bp.Count; i++) {
					bmps[i] = new Bitmap(assembly.GetManifestResourceStream("U5Designs.Resources.Textures." + _bp[i].InnerText));
				}
				return new SpriteSheet(bmps, cycleStarts, cycleLengths, _width, _height, _hasAlpha, _fps);
			}
		}

        public static Obstacle parse_single_3d_obstacle(string path) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string _o_path = "U5Designs.Resources.Data.Obstacles." + path;
            Stream fstream = assembly.GetManifestResourceStream(_o_path);
            XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			XmlReader reader = XmlReader.Create(fstream, settings);
			XmlDocument doc = new XmlDocument();
			doc.Load(reader);

            Vector3 scale = parseVector3(doc.GetElementsByTagName("scale")[0]);
            Vector3 pbox = parseVector3(doc.GetElementsByTagName("pbox")[0]);

            bool _draw2 = Convert.ToBoolean(doc.GetElementsByTagName("draw2")[0].InnerText);
            bool _draw3 = Convert.ToBoolean(doc.GetElementsByTagName("draw3")[0].InnerText);

            bool _collides2d = Convert.ToBoolean(doc.GetElementsByTagName("collidesIn2d")[0].InnerText);
            bool _collides3d = Convert.ToBoolean(doc.GetElementsByTagName("collidesIn3d")[0].InnerText);

            // Check to see if the current Obstacle is 2D or 3D and handle accordingly
            XmlNodeList _type = doc.GetElementsByTagName("is2d");
            if (Convert.ToBoolean(_type.Item(0).InnerText)) {
				fstream.Close();
                Console.Out.WriteLine("ERROR: obstacle +" + _o_path + "is not a 3d object, it must be 3d");
                return null;
            }
            else {
                XmlNodeList _m = doc.GetElementsByTagName("mesh");
                ObjMesh _mesh = new ObjMesh(assembly.GetManifestResourceStream("U5Designs.Resources.Geometry." + _m.Item(0).InnerText));

                XmlNodeList _b = doc.GetElementsByTagName("bmp");
                MeshTexture _tex = new MeshTexture(new Bitmap(assembly.GetManifestResourceStream("U5Designs.Resources.Textures." + _b.Item(0).InnerText)));

                fstream.Close();
                Vector3 loc = new Vector3(0, 0, 0);
                    return new Obstacle(loc, scale, pbox, _draw2, _draw3, _collides2d, _collides3d, _mesh, _tex);  
            }
        }

		/**
		 * This method will take an XMLNodeList that contains all the Obstacle objects that need to be created
		 * for the current level being loaded.  It will parse the XML .dat files, create an Obstacle object, and add it
		 * to a List<> which will be returned. 
		 * */
		public static List<Obstacle> parse_Obstacle_Files(XmlNodeList OList) {
			List<Obstacle> _o = new List<Obstacle>();
			Assembly assembly = Assembly.GetExecutingAssembly();
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;

			for(int i = 0; i < OList.Count; i++) {
				XmlDocument doc = new XmlDocument();
				string _o_path = "U5Designs.Resources.Data.Obstacles." + OList[i].FirstChild.InnerText;
				Stream fstream = assembly.GetManifestResourceStream(_o_path);
				XmlReader reader = XmlReader.Create(fstream, settings);
				doc.Load(reader);

				Vector3 scale = parseVector3(doc.GetElementsByTagName("scale")[0]);
				Vector3 pbox = parseVector3(doc.GetElementsByTagName("pbox")[0]);

				bool _draw2 = Convert.ToBoolean(doc.GetElementsByTagName("draw2")[0].InnerText);
				bool _draw3 = Convert.ToBoolean(doc.GetElementsByTagName("draw3")[0].InnerText);

				bool _collides2d = Convert.ToBoolean(doc.GetElementsByTagName("collidesIn2d")[0].InnerText);
				bool _collides3d = Convert.ToBoolean(doc.GetElementsByTagName("collidesIn3d")[0].InnerText);

				// Check to see if the current Obstacle is 2D or 3D and handle accordingly
				XmlNodeList _type = doc.GetElementsByTagName("is2d");
				if(Convert.ToBoolean(_type.Item(0).InnerText)) {
					String ss_path = doc.GetElementsByTagName("sprite")[0].InnerText;
					fstream.Close();
					SpriteSheet ss = parse_Sprite_File(ss_path);

					Billboarding bb = Billboarding.Yes;  //Have to put something here for it to compile
					switch(doc.GetElementsByTagName("billboards")[0].InnerText) {
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
							Console.WriteLine("Bad obstacle file: " + OList[i].FirstChild.InnerText);
							Environment.Exit(1);
							break;
					}

					for(int j = 1; j < OList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(OList[i].ChildNodes[j]);
						_o.Add(new Obstacle(loc, scale, pbox, _draw2, _draw3, _collides2d, _collides3d, bb, ss));
					}
				} else {
					fstream.Close();
					XmlNodeList _m = doc.GetElementsByTagName("mesh");
					ObjMesh _mesh = new ObjMesh(assembly.GetManifestResourceStream("U5Designs.Resources.Geometry." + _m.Item(0).InnerText));

					XmlNodeList _b = doc.GetElementsByTagName("bmp");
					MeshTexture _tex = new MeshTexture(new Bitmap(assembly.GetManifestResourceStream("U5Designs.Resources.Textures." + _b.Item(0).InnerText)));

					for(int j = 1; j < OList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(OList[i].ChildNodes[j]);
						_o.Add(new Obstacle(loc, scale, pbox, _draw2, _draw3, _collides2d, _collides3d, _mesh, _tex));
					}
					fstream.Close();
				}
			}

			return _o;
		}

		/**
		 * This method will take an XMLNodeList that contains all the Enemy objects that need to be created
		 * for the current level being loaded.  It will parse the XML .dat files, create an Enemy object, and add it
		 * to a List<> which will be returned. 
		 * */
		public static List<Enemy> parse_Enemy_Files(XmlNodeList EList) {
			// Instantiate the list
			List<Enemy> _e = new List<Enemy>();
			Assembly assembly = Assembly.GetExecutingAssembly();
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;

			// Loop over the string list containing all the Enemy.dat files associated with the current Level being loaded.
			// For every Enemy.dat file parse the file, and create an Enemy, then add it to the List.  When you're done with 
			// the list of Enemies to parse, return the List<Enemy>
			for(int i = 0; i < EList.Count; i++) {
				string _e_path = "U5Designs.Resources.Data.Enemies." + EList[i].FirstChild.InnerText;
				Stream fstream = assembly.GetManifestResourceStream(_e_path);
				XmlDocument doc = new XmlDocument();
				XmlReader reader = XmlReader.Create(fstream, settings);
				doc.Load(reader);

				bool draw_2d, draw_3d;
				int _health, _damage, _AI;
				float _speed;

				Vector3 scale = parseVector3(doc.GetElementsByTagName("scale")[0]);
				Vector3 pbox = parseVector3(doc.GetElementsByTagName("pbox")[0]);
				Vector3 cbox = parseVector3(doc.GetElementsByTagName("cbox")[0]);

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
				SpriteSheet ss = parse_Sprite_File(_sprite_path);

				if(_p.Count == 0) {
					// Create the Enemies
					for(int j = 1; j < EList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(EList[i].ChildNodes[j]);
						_e.Add(new Enemy(loc, scale, pbox, cbox, draw_2d, draw_3d, _health, _damage, _speed, _AI, ss));
					}
				} else {
					ProjectileProperties p = parseProjectileFile(_p.Item(0).InnerText);

					// Create the Enemies
					for(int j = 1; j < EList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(EList[i].ChildNodes[j]);
						_e.Add(new Enemy(loc, scale, pbox, cbox, draw_2d, draw_3d, _health, _damage, _speed, _AI, ss, p));
					}
				}
			}

			// Return list of Enemies               
			return _e;
		}


		/**
		 * This method will take an XMLNodeList that contains all the Background objects that need to be created
		 * for the current level being loaded.  It will parse the XML .dat files, create a Background object, and add it
		 * to a List<> which will be returned. 
		 * */
		public static List<Background> parse_Background_File(XmlNodeList BList) {
			// Instantiate the list
			List<Background> _b = new List<Background>();

			for(int i = 0; i < BList.Count; i++) {
				List<Vector3> locs = new List<Vector3>();
				Vector3 scale = new Vector3();
				String spritePath = "";
				float speed = -1;

				foreach(XmlNode n in BList[i].ChildNodes) {
					switch(n.Name) {
						case "loc":
							locs.Add(parseVector3(n));
							break;
						case "scale":
							scale = parseVector3(n);
							break;
						case "sprite":
							spritePath = n.InnerText;
							break;
						case "speed":
							speed = Convert.ToSingle(n.InnerText);
							break;
					}
				}
				foreach(Vector3 loc in locs) {
					_b.Add(new Background(loc, scale, parse_Sprite_File(spritePath), speed));
				}
			}

			return _b;
		}


		/**
		 * This method will take an XMLNodeList that contains all the Decoration objects that need to be created
		 * for the current level being loaded.  It will parse the XML .dat files, create a Decoration object, and add it
		 * to a List<> which will be returned. 
		 * */
		public static List<Decoration> parse_Decoration_File(XmlNodeList DList) {
			// Instantiate the list
			List<Decoration> _d = new List<Decoration>();
			Assembly assembly = Assembly.GetExecutingAssembly();
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;

			for(int i = 0; i < DList.Count; i++) {
				string _d_path = "U5Designs.Resources.Data.Decorations." + DList[i].FirstChild.InnerText;
				Stream fstream = assembly.GetManifestResourceStream(_d_path);
				XmlDocument doc = new XmlDocument();
				XmlReader reader = XmlReader.Create(fstream, settings);
				doc.Load(reader);

				Vector3 scale = parseVector3(doc.GetElementsByTagName("scale")[0]);

				bool _draw2 = Convert.ToBoolean(doc.GetElementsByTagName("draw2")[0].InnerText);
				bool _draw3 = Convert.ToBoolean(doc.GetElementsByTagName("draw3")[0].InnerText);


				// Check to see if the current Obstacle is 2D or 3D and handle accordingly
				if(Convert.ToBoolean(doc.GetElementsByTagName("is2d")[0].InnerText)) {
					// Create the SpriteSheet 
					SpriteSheet ss = parse_Sprite_File(doc.GetElementsByTagName("sprite")[0].InnerText);

					Billboarding bb = Billboarding.Yes;  //Have to put something here for it to compile
					switch(doc.GetElementsByTagName("billboards")[0].InnerText) {
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
							Console.WriteLine("Bad obstacle file: " + DList[i].FirstChild.InnerText);
							Environment.Exit(1);
							break;
					}

					for(int j = 1; j < DList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(DList[i].ChildNodes[j]);
						_d.Add(new Decoration(loc, scale, _draw2, _draw3, bb, ss));
					}
				} else {
					XmlNodeList _m = doc.GetElementsByTagName("mesh");
					ObjMesh _mesh = new ObjMesh(assembly.GetManifestResourceStream("U5Designs.Resources.Geometry." + _m.Item(0).InnerText));

					XmlNodeList _b = doc.GetElementsByTagName("bmp");
					MeshTexture _bmp = new MeshTexture(new Bitmap(assembly.GetManifestResourceStream("U5Designs.Resources.Textures." + _b.Item(0).InnerText)));

					for(int j = 1; j < DList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(DList[i].ChildNodes[j]);
						_d.Add(new Decoration(loc, scale, _draw2, _draw3, _mesh, _bmp));
					}
				}
				fstream.Close();
			}
			return _d;
		}

		private static ZookeeperAI parseZookeeperBoss() {
			//TODO: Implement me if needed (or leave things in constructor, TBD)
			return null;
		}
	}
}
