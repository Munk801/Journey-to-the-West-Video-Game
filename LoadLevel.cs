using System;
using System.Collections;
using System.Linq;
using System.Text;
using Engine;

namespace U5Designs
{
    class LoadLevel
    {
        public static ArrayList Load()
        {
            ArrayList objects = new ArrayList();
            //TODO write xml parser to populate this array list with all the objects in a level
            //TODO wire up subclasses of GameObject to inheriate from it, extend it where needed

            // Read in the current_level data file

            // Start reading

            // Load relevant data elements such as Text level name, level index, etc

            // Get the <object> type

            // Read the <loc> for the current <object> type

            // Split the <loc> at space and '\n' which should be 
            // x y z positions

            // Load up the particular <object> data file and get the attributes

            // Create a GameObject for every <loc> location found and store it on the list

            // Move to next <object> in file and repeat


            //EXAMPLE: just loaded a background object from xml ( this code would be a loop or something)
            {
                GameObject obj = new GameObject(); // make new object ( not an actual GameObject like here, but a Enemy, or Box or whatever)
                // initilize what needs to be initilized based on what the xml says
                obj.posx = 23f;
                obj.posy = 233f;
                obj.posz = 96f;
                // put the obj in the list, repeat till everything loaded
                objects.Add(obj);
            }

            return objects;
        }
        
    }
}
