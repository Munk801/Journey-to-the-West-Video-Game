using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

/*
 * This is the super-class of all in game objects.  It defines
 * things that are common for everything, like location
 *
 * The int type is a flag used by the rest of the program to know what kind of object this is
 * 
 * Types:
 * 1 = background obj
 * 2 = ...
 * 
 * 15 = snowman enemy .. etc
 * 
 * 
 * 
 * 
 */
namespace Engine 
{
	public class GameObject 
    {
        //IF you want to change it in LoadLevel it MUST BE PUBLIC!!!
        int type;
        public bool shown3d;
        public float posx;
        public float posy;
        public float posz;
        // ...etc all the things that a game object could have

        public GameObject(int type)
        {
            this.type = type;
            if (type == 1)
            {
                shown3d = true;
            }
        }


		Vector3 getLocation() {

			return new Vector3(0,0,0);
		}
	}
}
