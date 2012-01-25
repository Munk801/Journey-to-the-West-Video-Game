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

            //EXAMPLE: just loaded a background object from xml ( this code would be a loop or something)
            {
                GameObject obj = new GameObject(1); // pass the GameObject the type of the thing we loaded
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
