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
	public abstract class GameObject 
    {
		//Note: GameObject should only have fields that are applicable
		//      to ALL types of objects.  If it only applies to some
		//      objects, it should be in the appropriate subclass.
		private static int lastID = 0;

		public readonly int ID;
		
		public int getID() {
			return ID;
		}

		protected GameObject() {
			ID = ++lastID;
			screenRegion = ON_SCREEN; //safest assumption for now
		}

		protected GameObject(int id) {
			ID = id;
			screenRegion = ON_SCREEN; //safest assumption for now
		}

		//Does this object exist when viewed in 3d?
		protected bool _existsIn3d;
		public bool existsIn3d {
			get { return _existsIn3d; }
			protected set { _existsIn3d = value; }
		}

		//Does this object exist when viewed in 2d?
		protected bool _existsIn2d;
		public bool existsIn2d {
			get { return _existsIn2d; }
			protected set { _existsIn2d = value; }
		}

		protected Vector3 _location;
		public Vector3 location {
			get { return _location; }
			set { _location = value; }
		}

        protected bool _hascbox;
        public bool hascbox {
            get { return _hascbox; }
            protected set { _hascbox = value; }
        }

		public int screenRegion; //0, 1, or 2 - see constants below

		//CONSTANTS
		public const double gravity = 500.0;
		public const int ON_SCREEN = 1;
		public const int OFF_SCREEN = 0;
	}
}
