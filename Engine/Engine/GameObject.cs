using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

/*
 * This is the super-class of all in game objects.  It defines
 * things that are common for everything, like location
 *
 * 
 */
namespace Engine 
{
	public class GameObject 
    {
        //IF you want to change it in LoadLevel it MUST BE PUBLIC!!!
        public bool shown3d;
        public float posx;
        public float posy;
        public float posz;
        // ...etc all the things that all game objects will have

        public GameObject()
        {

        }


		Vector3 getLocation() {

			return new Vector3(0,0,0);
		}
	}
}
