using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

/*
 * Any object that has health or the ability to do damage.
 */
namespace Engine {
	public interface CombatObject {
		int health {
			get;
			set;
		}

		int damage {
			get;
		}

        float speed {
            get;
            set;
        }

		bool alive {
			get;
			set;
		}

        /*
         * IMPORTANT!!! this variable determins the type of combat object for the sake of handling collision with the player
         * 1 = enemy
         * 2 = projectile
         */
        int type {
            get;
        }
        // cbox is the size of the combat physics box
        Vector3 cbox {
            get;
        }

		void reset(); //resets to alive and full health; used for resetting level
	}
}
