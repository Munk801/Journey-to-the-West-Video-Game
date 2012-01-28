using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		bool alive {
			get;
			set;
		}

		void reset(); //resets to alive and full health; used for resetting level
	}
}
