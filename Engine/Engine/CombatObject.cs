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
		float health {
			get;
			set;
		}

		float damage {
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

        // cbox is the size of the combat physics box
        Vector3 cbox {
            get;
        }

		void reset(); //resets to alive and full health; used for resetting level
	}
}
