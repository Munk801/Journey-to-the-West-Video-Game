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

            /**
             * This next section of code will read in a level file and create an array of Enemy files to be parsed.
             * */
            Assembly assembly_new = Assembly.GetExecutingAssembly();
            string file_new = "U5Designs.Resources.level_test.dat";
            Stream fstream_new = assembly_new.GetManifestResourceStream(file_new);
            XmlDocument doc_new = new XmlDocument();
            doc_new.Load(fstream_new);
            XmlNodeList _e_list = doc_new.GetElementsByTagName("enemy");
            XmlNodeList _o_list = doc_new.GetElementsByTagName("obstacle");
            int _e_list_size = _e_list.Count;
            string[] _e_files = new string[_e_list_size];
            string[] _o_files = new string[_o_list.Count];
            for (int i = 0; i < _e_list_size; i++)
            {
                XmlNode n = _e_list.Item(i);
                _e_files[i] = n.InnerText;
            }
            for (int i = 0; i < _o_files.Length; i++)
            {
                XmlNode n = _o_list.Item(i);
                _o_files[i] = n.InnerText;
            }
            fstream_new.Close();

            // Load enemy.dat files (for now just loading the first one to test)
            List<Enemy> _elist = parse_Enemy_File(_e_files);

            // Load obstacle.dat files
            List<Obstacle> _olist = parse_Obstacle_File(_o_files);
            
            //for (int i = 1; i < 5; i++)
            //{
                //testfloor = new Obstacle(testloc + (new Vector3(i * 100, 0, 0)), testscale, testpbox, true, true, cubemesh, testmap);
                //Obstacle _to = _olist.ElementAt(0);
                //_to._move_Location(new Vector3(i * 100, 0, 0));
                //ps.objList.Add(_to);
                //ps.physList.Add(_to);
                //ps.renderList.Add(_to);
            //}            
            
            //SpriteSheet.quad = new ObjMesh("../../Geometry/quad.obj");
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
            foreach (Obstacle o in _olist)
            {
                ps.objList.Add(o);
                ps.physList.Add(o);
                ps.renderList.Add(o);
            }
                        
            //Load Player
            ps.player = new Player(ss);
        }

        public static SpriteSheet parse_Sprite_File(string[] SList)
        {
            SpriteSheet s;
            Assembly assembly_new = Assembly.GetExecutingAssembly();
            Stream fstream_new;
            XmlDocument doc_new = new XmlDocument();
            string _sprite_path = SList[0];

            fstream_new = assembly_new.GetManifestResourceStream(_sprite_path);
            doc_new.Load(fstream_new);
            XmlNodeList _bp = doc_new.GetElementsByTagName("bmp");
            string _bmp_path = "../../Textures/" + _bp.Item(0).InnerText;
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
            s = new SpriteSheet(new Bitmap(_bmp_path), cycleStarts, cycleLengths, _width, _height, _fps);

            return s;
        }

        public static List<Obstacle> parse_Obstacle_File(string[] OList)
        {
            List<Obstacle> _o = new List<Obstacle>();
            Assembly assembly_new = Assembly.GetExecutingAssembly();
            Stream fstream_new;
            XmlDocument doc_new;

            for (int i = 0; i < OList.Length; i++)
            {
                doc_new = new XmlDocument();
                string _o_path = "U5Designs.Resources." + OList[i];
                fstream_new = assembly_new.GetManifestResourceStream(_o_path);
                doc_new.Load(fstream_new);

                // Check to see if the current Obstacle is 2D or 3D and handle accordingly
                XmlNodeList _type = doc_new.GetElementsByTagName("is2d");                
                if (Convert.ToBoolean(Convert.ToInt32(_type.Item(0).InnerText)))
                {
                    /** Type is 2D **/
                    XmlNodeList _locs = doc_new.GetElementsByTagName("loc");
                    int xpos = Convert.ToInt32(_locs.Item(0).InnerText);
                    int ypos = Convert.ToInt32(_locs.Item(1).InnerText);
                    int zpos = Convert.ToInt32(_locs.Item(2).InnerText);

                    XmlNodeList _scale = doc_new.GetElementsByTagName("scale");
                    int sx = Convert.ToInt32(_scale.Item(0).InnerText);
                    int sy = Convert.ToInt32(_scale.Item(1).InnerText);
                    int sz = Convert.ToInt32(_scale.Item(2).InnerText);

                    XmlNodeList _pbox = doc_new.GetElementsByTagName("pbox");
                    int px = Convert.ToInt32(_pbox.Item(0).InnerText);
                    int py = Convert.ToInt32(_pbox.Item(1).InnerText);
                    int pz = Convert.ToInt32(_pbox.Item(2).InnerText);

                    XmlNodeList _d2 = doc_new.GetElementsByTagName("draw2");
                    bool _draw2 = Convert.ToBoolean(Convert.ToInt32(_d2.Item(0).InnerText));
                    XmlNodeList _d3 = doc_new.GetElementsByTagName("draw3");
                    bool _draw3 = Convert.ToBoolean(Convert.ToInt32(_d3.Item(0).InnerText));

                    XmlNodeList _m = doc_new.GetElementsByTagName("mesh");
                    ObjMesh _mesh = new ObjMesh("../../Geometry/" + _m.Item(0).InnerText);
                    
                    XmlNodeList _b = doc_new.GetElementsByTagName("bmp");
                    Bitmap _bmp = new Bitmap("../../Textures/" + _b.Item(0).InnerText);

                    Obstacle _obs = new Obstacle(new Vector3(xpos, ypos, zpos), new Vector3(sx, sy, sz), new Vector3(px, py, pz), _draw2, _draw3, _mesh, _bmp);

                    _o.Add(_obs);
                }
                else
                {
                    /** Type is 3D **/
                    // obstacle2.dat
                    XmlNodeList _locs = doc_new.GetElementsByTagName("loc");
                    int xpos = Convert.ToInt32(_locs.Item(0).InnerText);
                    int ypos = Convert.ToInt32(_locs.Item(1).InnerText);
                    int zpos = Convert.ToInt32(_locs.Item(2).InnerText);

                    XmlNodeList _scale = doc_new.GetElementsByTagName("scale");
                    int sx = Convert.ToInt32(_scale.Item(0).InnerText);
                    int sy = Convert.ToInt32(_scale.Item(1).InnerText);
                    int sz = Convert.ToInt32(_scale.Item(2).InnerText);

                    XmlNodeList _pbox = doc_new.GetElementsByTagName("pbox");
                    int px = Convert.ToInt32(_pbox.Item(0).InnerText);
                    int py = Convert.ToInt32(_pbox.Item(1).InnerText);
                    int pz = Convert.ToInt32(_pbox.Item(2).InnerText);

                    XmlNodeList _d2 = doc_new.GetElementsByTagName("draw2");
                    bool _draw2 = Convert.ToBoolean(Convert.ToInt32(_d2.Item(0).InnerText));
                    XmlNodeList _d3 = doc_new.GetElementsByTagName("draw3");
                    bool _draw3 = Convert.ToBoolean(Convert.ToInt32(_d3.Item(0).InnerText));

                    XmlNodeList _spr = doc_new.GetElementsByTagName("sprite");
                    string _sprite_path = "U5Designs.Resources." + _spr.Item(0).InnerText;                    

                    // Create the SpriteSheet 
                    string[] _slist = { _sprite_path };
                    SpriteSheet ss = parse_Sprite_File(_slist);

                    Obstacle _obs = new Obstacle(new Vector3(xpos, ypos, zpos), new Vector3(sx, sy, sz), new Vector3(px, py, pz), _draw2, _draw3, ss);

                    _o.Add(_obs);
                }
                fstream_new.Close();
            }

            return _o;
        }

        /**
         * This method will take an array of strings that contains all the Enemy objects that need to be created
         * for the current level being loaded.  It will parse the XML .dat files, create an Enemy object, and add it
         * to a List<> which will be returned. 
         * */
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

                // Create the SpriteSheet 
                string[] _slist = { _sprite_path };              
                SpriteSheet ss = parse_Sprite_File(_slist);

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
