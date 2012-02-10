using System;
using System.Collections;
using System.Linq;
using System.Text;
using Engine;
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


            //tmp add 5 more floor boxes
            for (int i = 0; i < 5; i++)
            {  
                testfloor = new Obstacle(testloc+(new Vector3(i*100, 0,0)), testscale, testpbox, true, true, cubemesh, testmap);
                ps.objList.Add(testfloor);
                ps.physList.Add(testfloor);
                ps.renderList.Add(testfloor);
            }


            //example enemy load
            Vector3 enloc = new Vector3(250, 5, 50);
            Vector3 enscale = new Vector3(5, 5, 5);
            Vector3 enpbox = new Vector3(5, 5, 5);
            Vector3 encbox = new Vector3(5, 5, 5);
            Bitmap enmap = new Bitmap("../../Textures/enemy.png");

            Enemy testenemy = new Enemy(enloc, enscale, enpbox, encbox, true, true, 10, 10, 80f, 1, cubemesh, enmap);
            ps.objList.Add(testenemy);
            ps.physList.Add(testenemy);
            ps.colisionList.Add(testenemy);
            ps.renderList.Add(testenemy);
            ps.aiList.Add(testenemy);

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
			SpriteSheet.quad = new ObjMesh("../../Geometry/quad.obj");
			int[] cycleStarts = {0, 4};
			int[] cycleLengths = {4, 4};
			SpriteSheet ss = new SpriteSheet(new Bitmap("../../Geometry/test_sprite.png"), cycleStarts, cycleLengths, 512, 512, false);
			Obstacle testSprite = new Obstacle(new Vector3(50, 50, 50), testscale,testpbox, true, true, ss);
			ps.objList.Add(testSprite);
			ps.physList.Add(testSprite);
			ps.renderList.Add(testSprite);




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
    }    
}
