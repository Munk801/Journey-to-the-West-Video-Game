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
			Assembly assembly_new = Assembly.GetExecutingAssembly();
			string file_new = "U5Designs.Resources.Data.Levels.level_test.dat";
			Stream fstream_new = assembly_new.GetManifestResourceStream(file_new);
			XmlDocument doc_new = new XmlDocument();
			doc_new.Load(fstream_new);
			XmlNodeList _b_list = doc_new.GetElementsByTagName("background");
			XmlNodeList _e_list = doc_new.GetElementsByTagName("enemy");
			XmlNodeList _o_list = doc_new.GetElementsByTagName("obstacle");
			XmlNodeList _d_list = doc_new.GetElementsByTagName("decoration");
			fstream_new.Close();

			List<Enemy> _elist = parse_Enemy_File(_e_list);
			foreach(Enemy e in _elist) {
				ps.objList.Add(e);
				ps.physList.Add(e);
				ps.colisionList.Add(e);
				ps.renderList.Add(e);
				ps.aiList.Add(e);
			}

			List<Obstacle> _olist = parse_Obstacle_File(_o_list);
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

			//temporary hard-coded background
// 			int[] cycleStarts = { 0 };
// 			int[] cycleLengths = { 1 };
// 			Background bg = new Background(new Vector3(926.0f, 75.0f, -200.0f), new Vector3(3704.0f, 200.0f, 100.0f),
// 											new SpriteSheet(new Bitmap("../../Resources/test_background.png"),
// 											cycleStarts, cycleLengths, 10000, 1080), 0.75f);
// 			ps.backgroundList.Add(bg);
// 			ps.renderList.Add(bg);



			SpriteSheet.quad = new ObjMesh(assembly_new.GetManifestResourceStream("U5Designs.Resources.Geometry.quad.obj"));

			//Load Player
			//int[] cycleStarts = new int[] { 0, 4 };
			//int[] cycleLengths = new int[] { 4, 4 };
			//SpriteSheet ss = new SpriteSheet(new Bitmap(assembly_new.GetManifestResourceStream("U5Designs.Resources.Textures.test_sprite.png")), cycleStarts, cycleLengths, 128, 128, 4.0);
			ps.player = new Player(parse_Sprite_File("test_player_sprite.dat"));
			ps.physList.Add(ps.player);
			ps.renderList.Add(ps.player);
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

		public static SpriteSheet parse_Sprite_File(string path) {
			Assembly assembly_new = Assembly.GetExecutingAssembly();
			Stream fstream_new;
			XmlDocument doc_new = new XmlDocument();

			fstream_new = assembly_new.GetManifestResourceStream("U5Designs.Resources.Data.Sprites." + path);
			doc_new.Load(fstream_new);
			XmlNodeList _bp = doc_new.GetElementsByTagName("bmp");
			string _bmp_path = "U5Designs.Resources.Textures." + _bp.Item(0).InnerText;
			XmlNodeList _c_start_list = doc_new.GetElementsByTagName("c_starts");
			int[] cycleStarts = new int[_c_start_list.Count];
			for(int i = 0; i < _c_start_list.Count; i++) {
				cycleStarts[i] = Convert.ToInt32(_c_start_list.Item(i).InnerText);
			}
			XmlNodeList _c_length_list = doc_new.GetElementsByTagName("c_lengths");
			int[] cycleLengths = new int[_c_length_list.Count];
			for(int i = 0; i < _c_length_list.Count; i++) {
				cycleLengths[i] = Convert.ToInt32(_c_length_list.Item(i).InnerText);
			}
			XmlNodeList _sw = doc_new.GetElementsByTagName("t_width");
			int _width = Convert.ToInt32(_sw.Item(0).InnerText);
			XmlNodeList _sh = doc_new.GetElementsByTagName("t_height");
			int _height = Convert.ToInt32(_sh.Item(0).InnerText);
			bool _hasAlpha = Convert.ToBoolean(doc_new.GetElementsByTagName("hasAlpha")[0].InnerText);
			XmlNodeList _f = doc_new.GetElementsByTagName("fps");
			float _fps = (float)Convert.ToDouble(_f.Item(0).InnerText);
			fstream_new.Close();

			// Create the SpriteSheet
			return new SpriteSheet(new Bitmap(assembly_new.GetManifestResourceStream(_bmp_path)), cycleStarts, cycleLengths, _width, _height, _hasAlpha, _fps);
		}

		/**
		 * This method will take an XMLNodeList that contains all the Obstacle objects that need to be created
		 * for the current level being loaded.  It will parse the XML .dat files, create an Obstacle object, and add it
		 * to a List<> which will be returned. 
		 * */
		public static List<Obstacle> parse_Obstacle_File(XmlNodeList OList) {
			List<Obstacle> _o = new List<Obstacle>();
			Assembly assembly_new = Assembly.GetExecutingAssembly();
			Stream fstream_new;
			XmlDocument doc_new;

			for(int i = 0; i < OList.Count; i++) {
				doc_new = new XmlDocument();
				string _o_path = "U5Designs.Resources.Data.Obstacles." + OList[i].FirstChild.InnerText;
				fstream_new = assembly_new.GetManifestResourceStream(_o_path);
				doc_new.Load(fstream_new);


				Vector3 scale = parseVector3(doc_new.GetElementsByTagName("scale")[0]);
				Vector3 pbox = parseVector3(doc_new.GetElementsByTagName("pbox")[0]);

				bool _draw2 = Convert.ToBoolean(doc_new.GetElementsByTagName("draw2")[0].InnerText);
				bool _draw3 = Convert.ToBoolean(doc_new.GetElementsByTagName("draw3")[0].InnerText);

				// Check to see if the current Obstacle is 2D or 3D and handle accordingly
				XmlNodeList _type = doc_new.GetElementsByTagName("is2d");
				if(Convert.ToBoolean(_type.Item(0).InnerText)) {
					String ss_path = doc_new.GetElementsByTagName("sprite")[0].InnerText;
					fstream_new.Close();
					SpriteSheet ss = parse_Sprite_File(ss_path);

					for(int j = 1; j < OList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(OList[i].ChildNodes[j]);
						_o.Add(new Obstacle(loc, scale, pbox, _draw2, _draw3, ss));
					}
				} else {
					XmlNodeList _m = doc_new.GetElementsByTagName("mesh");
					ObjMesh _mesh = new ObjMesh(assembly_new.GetManifestResourceStream("U5Designs.Resources.Geometry." + _m.Item(0).InnerText));

					XmlNodeList _b = doc_new.GetElementsByTagName("bmp");
					MeshTexture _tex = new MeshTexture(new Bitmap(assembly_new.GetManifestResourceStream("U5Designs.Resources.Textures." + _b.Item(0).InnerText)));

					for(int j = 1; j < OList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(OList[i].ChildNodes[j]);
						_o.Add(new Obstacle(loc, scale, pbox, _draw2, _draw3, _mesh, _tex));
					}
					fstream_new.Close();
				}
			}

			return _o;
		}

		/**
		 * This method will take an XMLNodeList that contains all the Enemy objects that need to be created
		 * for the current level being loaded.  It will parse the XML .dat files, create an Enemy object, and add it
		 * to a List<> which will be returned. 
		 * */
		public static List<Enemy> parse_Enemy_File(XmlNodeList EList) {
			// Instantiate the list
			List<Enemy> _e = new List<Enemy>();
			Assembly assembly_new = Assembly.GetExecutingAssembly();
			Stream fstream_new;
			XmlDocument doc_new;

			// Loop over the string list containing all the Enemy.dat files associated with the current Level being loaded.
			// For every Enemy.dat file parse the file, and create an Enemy, then add it to the List.  When you're done with 
			// the list of Enemies to parse, return the List<Enemy>
			for(int i = 0; i < EList.Count; i++) {
				doc_new = new XmlDocument();
				bool draw_2d, draw_3d;
				int _health, _damage, _AI;
				float _speed;

				string _e_path = "U5Designs.Resources.Data.Enemies." + EList[i].FirstChild.InnerText;
				fstream_new = assembly_new.GetManifestResourceStream(_e_path);
				doc_new.Load(fstream_new);

				Vector3 scale = parseVector3(doc_new.GetElementsByTagName("scale")[0]);
				Vector3 pbox = parseVector3(doc_new.GetElementsByTagName("pbox")[0]);
				Vector3 cbox = parseVector3(doc_new.GetElementsByTagName("cbox")[0]);

				draw_2d = Convert.ToBoolean(doc_new.GetElementsByTagName("draw2")[0].InnerText);
				draw_3d = Convert.ToBoolean(doc_new.GetElementsByTagName("draw3")[0].InnerText);

				XmlNodeList _h = doc_new.GetElementsByTagName("health");
				_health = Convert.ToInt32(_h.Item(0).InnerText);
				XmlNodeList _d = doc_new.GetElementsByTagName("damage");
				_damage = Convert.ToInt32(_d.Item(0).InnerText);
				XmlNodeList _s = doc_new.GetElementsByTagName("speed");
				_speed = Convert.ToSingle(_s.Item(0).InnerText);
				XmlNodeList _ai = doc_new.GetElementsByTagName("AI");
				_AI = Convert.ToInt32(_ai.Item(0).InnerText);
				XmlNodeList _sp = doc_new.GetElementsByTagName("sprite");
				string _sprite_path = _sp.Item(0).InnerText;

				// Pause now and parse the Sprite.dat to create the necessary Sprite that is associated with the current Enemy object
				fstream_new.Close();

				// Create the SpriteSheet              
				SpriteSheet ss = parse_Sprite_File(_sprite_path);

				// Create the Enemies
				for(int j = 1; j < EList[i].ChildNodes.Count; j++) {
					Vector3 loc = parseVector3(EList[i].ChildNodes[j]);
                    //TODO: SETH: change the last 'ss' in this enemy declaration to be the projectile sprite!!!!!!!
					_e.Add(new Enemy(loc, scale, pbox, cbox, draw_2d, draw_3d, _health, _damage, _speed, _AI, ss, ss));
				}
				fstream_new.Close();
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
				Vector3 loc = new Vector3();
				Vector3 scale = new Vector3();
				String spritePath = "";
				float speed = -1;

				foreach(XmlNode n in BList[i].ChildNodes) {
					switch(n.Name) {
						case "loc":
							loc = parseVector3(n);
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

				_b.Add(new Background(loc, scale, parse_Sprite_File(spritePath), speed));
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
			Assembly assembly_new = Assembly.GetExecutingAssembly();
			Stream fstream_new;
			XmlDocument doc_new;

			for(int i = 0; i < DList.Count; i++) {
				doc_new = new XmlDocument();
				string _d_path = "U5Designs.Resources.Data.Decorations." + DList[i].FirstChild.InnerText;
				fstream_new = assembly_new.GetManifestResourceStream(_d_path);
				doc_new.Load(fstream_new);

				Vector3 scale = parseVector3(doc_new.GetElementsByTagName("scale")[0]);

				bool _draw2 = Convert.ToBoolean(doc_new.GetElementsByTagName("draw2")[0].InnerText);
				bool _draw3 = Convert.ToBoolean(doc_new.GetElementsByTagName("draw3")[0].InnerText);


				// Check to see if the current Obstacle is 2D or 3D and handle accordingly
				if(Convert.ToBoolean(doc_new.GetElementsByTagName("is2d")[0].InnerText)) {
					// Create the SpriteSheet 
					SpriteSheet ss = parse_Sprite_File(doc_new.GetElementsByTagName("sprite")[0].InnerText);

					for(int j = 1; j < DList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(DList[i].ChildNodes[j]);
						_d.Add(new Decoration(loc, scale, _draw2, _draw3, ss));
					}
				} else {
					XmlNodeList _m = doc_new.GetElementsByTagName("mesh");
					ObjMesh _mesh = new ObjMesh(assembly_new.GetManifestResourceStream("U5Designs.Resources.Geometry." + _m.Item(0).InnerText));

					XmlNodeList _b = doc_new.GetElementsByTagName("bmp");
					MeshTexture _bmp = new MeshTexture(new Bitmap(assembly_new.GetManifestResourceStream("U5Designs.Resources.Textures." + _b.Item(0).InnerText)));

					for(int j = 1; j < DList[i].ChildNodes.Count; j++) {
						Vector3 loc = parseVector3(DList[i].ChildNodes[j]);
						_d.Add(new Decoration(loc, scale, _draw2, _draw3, _mesh, _bmp));
					}
				}
				fstream_new.Close();
			}
			return _d;
		}
	}
}
