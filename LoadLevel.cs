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
    class LoadLevel
    {
        public static void Load(int level_to_load, PlayState ps)
        {
            ps.objList = new List<GameObject>();
            ps.renderList = new List<RenderObject>();
            ps.aiList = new List<AIObject>();
            ps.combatList = new List<CombatObject>();
            ps.physList = new List<PhysicsObject>();
            ps.colisionList = new List<PhysicsObject>();
			ps.backgroundList = new List<Background>();


            //First we need to pull the location of the meshes.(or keep this hardcoded one?..)
            ObjMesh cubemesh = new ObjMesh("../../Geometry/box.obj");


            //example obstacle load
            //use ps to access object lists!!
            Vector3 testloc = new Vector3(50, -50, 50); // location is a point 
			Vector3 testscale = new Vector3(50, 50, 50); // scale is a multiplyer
            Bitmap testmap = new Bitmap("../../Textures/test.png"); // sprite
            Vector3 testpbox = new Vector3(50, 50, 50);// the size of the physicsbox extending from the center in x,y,z
			//Bitmap testmap = new Bitmap("../../Geometry/test_sprite.png");
            Obstacle testfloor = new Obstacle(testloc, testscale, testpbox, true, true, cubemesh, testmap);

            ps.objList.Add(testfloor);
            ps.physList.Add(testfloor);
            ps.renderList.Add(testfloor);
            //xml file needs to contain for 3d object:
            // vector3 location (( x, y, z) cords)
            // vector3 scale
            // bool existsin2d?
            // bool exsistsin3d?
			// obj file
            // bitmap file
            // vector3 physics box size

            /***
             * New XML enemy testing please don't move.  This code will eventually go in a function which will return a List<Enemy>()
             * */
            Assembly assembly_new = Assembly.GetExecutingAssembly();
            string file_new = "U5Designs.Resources.level_test.dat";
            Stream fstream_new = assembly_new.GetManifestResourceStream(file_new);

            // Start reading
            XmlDocument doc_new = new XmlDocument();
            doc_new.Load(fstream_new);

            // Create a list of enemy.dat files to load
            XmlNodeList _e_list = doc_new.GetElementsByTagName("enemy");
            int _e_list_size = _e_list.Count;
            string[] _e_files = new string[_e_list_size];
            for (int i = 0; i < _e_list_size; i++)
            {
                XmlNode n = _e_list.Item(i);
                _e_files[i] = n.InnerText;
            }
            fstream_new.Close();
            // Load enemy.dat files (for now just loading the first one to test)
            List<Enemy> _elist = parse_Enemy_File(_e_files);

            //tmp add 5 more floor boxes
            for (int i = 0; i < 20; i++)
            {  
                testfloor = new Obstacle(testloc+(new Vector3(i*100, 0,0)), testscale, testpbox, true, true, cubemesh, testmap);
                ps.objList.Add(testfloor);
                ps.physList.Add(testfloor);
                ps.renderList.Add(testfloor);
            }
            // test for physics
            testscale = new Vector3(12.5f, 12.5f, 12.5f);
            testpbox = new Vector3(12.5f, 12.5f, 12.5f);
            testfloor = new Obstacle(testloc + (new Vector3(50, 70, 20)), testscale, testpbox, true, true, cubemesh, testmap);
            ps.objList.Add(testfloor);
            ps.physList.Add(testfloor);
            ps.renderList.Add(testfloor);

            testfloor = new Obstacle(testloc + (new Vector3(100, 150, 10)), testscale, testpbox, true, true, cubemesh, testmap);
            ps.objList.Add(testfloor);
            ps.physList.Add(testfloor);
            ps.renderList.Add(testfloor);


            //example enemy load
            //Vector3 enloc = new Vector3(250, 50, 50);
            //Vector3 enscale = new Vector3(12.5f, 12.5f, 12.5f);
            //Vector3 enpbox = new Vector3(6.25f, 6.25f, 6.25f);
            //Vector3 encbox = new Vector3(6.25f, 6.25f, 6.25f);
            //Bitmap enmap = new Bitmap("../../Textures/enemy.png");
            SpriteSheet.quad = new ObjMesh("../../Geometry/quad.obj");
            int[] cycleStarts = { 0, 4 };
            int[] cycleLengths = { 4, 4 };
            SpriteSheet ss = new SpriteSheet(new Bitmap("../../Geometry/test_sprite.png"), cycleStarts, cycleLengths, 128, 128, 4.0);


            //Enemy testenemy = new Enemy(enloc, enscale, enpbox, encbox, true, true, 10, 10, 80f, 1, ss);
            foreach (Enemy e in _elist)
            {
                ps.objList.Add(e);
                ps.physList.Add(e);
                ps.colisionList.Add(e);
                ps.renderList.Add(e);
                ps.aiList.Add(e);
            }

            //XML for enemy needs to contain:
            // vector3 location (( x, y, z) cords)
            // vector3 scale
            // bool existsin2d?
            // bool exsistsin3d?
            //int health
            // int damage
            // float speed
            // int AItype
            // obj file
            // bitmap file
            // vector3 physics box size
            // vector3 combat box size



			//testing sprite sheet....
            // moved 4 lines up in enemy.
//          Obstacle testSprite = new Obstacle(new Vector3(0, 50, 0), new Vector3(25, 25, 25), testpbox, true, true, ss);
//			ps.objList.Add(testSprite);
//			ps.physList.Add(testSprite);
//			ps.renderList.Add(testSprite);


			//Load Player
			ps.player = new Player(ss);


            //TODO write xml parser to populate this array list with all the objects in a level
            //TODO wire up subclasses of GameObject to inheriate from it, extend it where needed

            // Read in the current_level data file            
            Assembly assembly = Assembly.GetExecutingAssembly();
            string file = "U5Designs.Resources.level_" + level_to_load.ToString() + ".dat";
            Stream fstream = assembly.GetManifestResourceStream(file);
            
            // Start reading
            XmlDocument doc = new XmlDocument();
            doc.Load(fstream);
            
            // Load relevant data elements such as Text level name, level index, etc
            XmlNodeList level_text_name = doc.GetElementsByTagName("l_name");
            Console.WriteLine("Textname: " + level_text_name[0].InnerText);

            XmlNodeList level_index = doc.GetElementsByTagName("l_index");
            int l_index = Convert.ToInt32(level_index[0].InnerText);

            XmlNodeList has_boss = doc.GetElementsByTagName("l_has_boss");
            int boss_flag = Convert.ToInt32(has_boss[0].InnerText);

            // Get the <object> type
            XmlNodeList object_list = doc.GetElementsByTagName("object");
            string[] locations = null;
            for (int i = 0; i < object_list.Count; i++)
            {
                // For each object type found in this level we create a list of children.  Things like object-type, interactable, locations
                // then parse those children
                XmlNode node = object_list[i];
                XmlNodeList child_nodes = node.ChildNodes;

                // Split the locations into seperate strings
                locations = split_locations(child_nodes[2].InnerText);

                // Load up the required entity .dat file
                string obj_file = "U5Designs.Resources." + child_nodes[0].InnerText.ToString() + ".dat";
                Stream obj_stream = assembly.GetManifestResourceStream(obj_file);
                XmlDocument obj_doc = new XmlDocument();
                obj_doc.Load(obj_stream);

                // For each location found for this particular GameObject type create a new GameObject and add it to the master list
                for (int x = 0; x < (locations.Length/3); x++)
                {
                    XmlNodeList sfx_0 = obj_doc.GetElementsByTagName("e_sfx_0");

                    XmlNodeList texture = obj_doc.GetElementsByTagName("e_texture");

                    XmlNodeList health = obj_doc.GetElementsByTagName("e_health");                    

                    XmlNodeList abil_0 = obj_doc.GetElementsByTagName("e_ability_0");
                }
            } 
        }

        public static string[] split_locations(string xml_loc_str)
        {  
            // Strip out tabs, returns, and spaces and replace with commas
            string n_str = strip_chars(xml_loc_str);

            // Split the comma deliminated string into an array of strings
            return n_str.Split(',');
        }

        public static string strip_chars(string s)
        {
            return Regex.Replace(s, "[\t\r\n ]+", ",", RegexOptions.Compiled);
        }

        public static List<Enemy> parse_Enemy_File(string[] EList)
        {
            // Instantiate the list
            List<Enemy> _e = new List<Enemy>();
            Assembly assembly_new = Assembly.GetExecutingAssembly();            
            Stream fstream_new;          
            XmlDocument doc_new;            

            // Loop over the string list containing all the Enemy.dat files associated with the current Level being loaded.
            // For every Enemy.dat file parse the file, and create an Enemy, then add it to the List.  When you're done with 
            // the list of Enemies to parse, return the List<Enemy>
            for (int i = 0; i < EList.Length; i++)
            {
                doc_new = new XmlDocument();
                int xpos, ypos, zpos, _is_2d, _is_3d, _health, _damage, _AI;
                float s1, s2, s3, p1, p2, p3, c1, c2, c3, _speed;

                // Temp fix
                XmlElement n;

                string _e_path = "U5Designs.Resources." + EList[0];
                fstream_new = assembly_new.GetManifestResourceStream(_e_path);
                doc_new.Load(fstream_new);
                XmlNodeList _loc_list = doc_new.GetElementsByTagName("loc");
                
                xpos = Convert.ToInt32(_loc_list.Item(0).InnerText);
                ypos = Convert.ToInt32(_loc_list.Item(1).InnerText);
                zpos = Convert.ToInt32(_loc_list.Item(2).InnerText);
                
                XmlNodeList _scale_list = doc_new.GetElementsByTagName("scale");
                s1 = (float)Convert.ToDouble(_scale_list.Item(0).InnerText);
                s2 = (float)Convert.ToDouble(_scale_list.Item(1).InnerText);
                s3 = (float)Convert.ToDouble(_scale_list.Item(2).InnerText);
                XmlNodeList _pbox_list = doc_new.GetElementsByTagName("pbox");
                p1 = (float)Convert.ToDouble(_pbox_list.Item(0).InnerText);
                p2 = (float)Convert.ToDouble(_pbox_list.Item(1).InnerText);
                p3 = (float)Convert.ToDouble(_pbox_list.Item(2).InnerText);
                XmlNodeList _cbox_list = doc_new.GetElementsByTagName("cbox");
                c1 = (float)Convert.ToDouble(_cbox_list.Item(0).InnerText);
                c2 = (float)Convert.ToDouble(_cbox_list.Item(1).InnerText);
                c3 = (float)Convert.ToDouble(_cbox_list.Item(2).InnerText);

                // Convert ints to bools that need to be such as is_2D, is_3D
                /** !!!!! 0 = FALSE everything_else = TRUE !!!!!!!**/
                XmlNodeList _2d = doc_new.GetElementsByTagName("draw2");
                _is_2d = Convert.ToInt32(_2d.Item(0).InnerText);
                bool draw_2d = Convert.ToBoolean(_is_2d);
                XmlNodeList _3d = doc_new.GetElementsByTagName("draw3");
                _is_3d = Convert.ToInt32(_3d.Item(0).InnerText);
                bool draw_3d = Convert.ToBoolean(_is_3d);
                XmlNodeList _h = doc_new.GetElementsByTagName("health");
                _health = Convert.ToInt32(_h.Item(0).InnerText);
                XmlNodeList _d = doc_new.GetElementsByTagName("damage");
                _damage = Convert.ToInt32(_d.Item(0).InnerText);
                XmlNodeList _s = doc_new.GetElementsByTagName("speed");
                _speed = (float)Convert.ToDouble(_s.Item(0).InnerText);
                XmlNodeList _ai = doc_new.GetElementsByTagName("AI");
                _AI = Convert.ToInt32(_ai.Item(0).InnerText);
                XmlNodeList _sp = doc_new.GetElementsByTagName("sprite");
                string _sprite_path = "U5Designs.Resources." + _sp.Item(0).InnerText;

                // Pause now and parse the Sprite.dat to create the necessary Sprite that is associated with the current Enemy object
                fstream_new.Close();
                fstream_new = assembly_new.GetManifestResourceStream(_sprite_path);
                doc_new.Load(fstream_new);
                XmlNodeList _bp = doc_new.GetElementsByTagName("bmp");
                string _bmp_path = "../../Geometry/" + _bp.Item(0).InnerText;
                XmlNodeList _c_start_list = doc_new.GetElementsByTagName("c_starts");
                int _c_start1 = Convert.ToInt32(_c_start_list.Item(0).InnerText);
                int _c_start2 = Convert.ToInt32(_c_start_list.Item(1).InnerText);
                XmlNodeList _c_length_list = doc_new.GetElementsByTagName("c_lengths");
                int _c_len1 = Convert.ToInt32(_c_length_list.Item(0).InnerText);
                int _c_len2 = Convert.ToInt32(_c_length_list.Item(1).InnerText);
                XmlNodeList _sw = doc_new.GetElementsByTagName("t_width");
                int _width = Convert.ToInt32(_sw.Item(0).InnerText);
                XmlNodeList _sh = doc_new.GetElementsByTagName("t_height");
                int _height = Convert.ToInt32(_sh.Item(0).InnerText);
                XmlNodeList _f = doc_new.GetElementsByTagName("fps");
                float _fps = (float)Convert.ToDouble(_f.Item(0).InnerText);
                fstream_new.Close();                

                // Create the SpriteSheet
                int[] cycleStarts = { _c_start1, _c_start2 };
                int[] cycleLengths = { _c_len1, _c_len2 };
                SpriteSheet.quad = new ObjMesh("../../Geometry/quad.obj");                
                SpriteSheet ss = new SpriteSheet(new Bitmap(_bmp_path), cycleStarts, cycleLengths, _width, _height, _fps);

                // Create the Enemy
                Vector3 enloc = new Vector3(xpos, ypos, zpos);
                Vector3 enscale = new Vector3(s1,s2,s3);
                Vector3 enpbox = new Vector3(p1,p2,p3);
                Vector3 encbox = new Vector3(c1,c2,c3);
                //Enemy testenemy = new Enemy(enloc, enscale, enpbox, encbox, draw_2d, draw_3d, _health, _damage, _speed, _AI, ss);
                Enemy testenemy = new Enemy(enloc, enscale, enpbox, encbox, true, true, _health, _damage, _speed, _AI, ss);

                // Add the Enemy to the list
                _e.Add(testenemy);
            }

            // Return list of Enemies
            return _e;
        }
    }    
}
