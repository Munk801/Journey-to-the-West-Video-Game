using System;
using System.Collections;
using System.Linq;
using System.Text;
using Engine;

// XML Parser
using System.Xml;
using System.Reflection;
using System.IO;

// String splitter
using System.Text.RegularExpressions;

namespace U5Designs
{
    class LoadLevel
    {
        public static ArrayList Load(int level_to_load)
        {
            ArrayList objects = new ArrayList();
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

                    GameObject go = new GameObject();
                    go.posx = Convert.ToInt32(locations[x]);
                    go.posy = Convert.ToInt32(locations[x + 1]);
                    go.posz = Convert.ToInt32(locations[x + 2]);

                    go.health = Convert.ToInt32(health[0].InnerText);
                    go.sfx_path = sfx_0[0].InnerText;
                    go.texture_path = texture[0].InnerText;
                    go.ability = abil_0[0].InnerText;

                    // Add it to the master list
                    objects.Add(go);
                }
            } 

            //EXAMPLE: just loaded a background object from xml ( this code would be a loop or something)
            /*{
                GameObject obj = new GameObject(); // make new object ( not an actual GameObject like here, but a Enemy, or Box or whatever)
                // initilize what needs to be initilized based on what the xml says
                obj.posx = 23f;
                obj.posy = 233f;
                obj.posz = 96f;
                // put the obj in the list, repeat till everything loaded
                objects.Add(obj);
            }
            */
            return objects;
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
