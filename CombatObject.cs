using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Engine;

/*
 * Any object that has health or the ability to do damage.
 */
namespace U5Designs {
	public enum CombatType { player=0, enemy=1, projectile=2, boss=3, squish=4, grenade=5 };

	public interface CombatObject {
		//duplicated from GameObject
		Vector3 location { get; }
		bool hascbox { get; }
		bool existsIn3d { get; }
		bool existsIn2d { get; }
		int health { get; set; }
		int damage { get; }
        float speed { get; set; }
		bool alive { get; set; }
		int ScreenRegion { get; }
		Effect deathAnim { get; }

        /*
         * IMPORTANT!!! this variable determines the type of combat object for the sake of handling collision between objects
         * 0 = player
         * 1 = enemy
         * 2 = projectile
         * 3 = zookeeper
         * 4 = projectile that causes squish player to happen(underside of a box)
         */
        int type { get; }

        // cbox is the size of the combat physics box
        Vector3 cbox { get; }

		Billboarding billboards { get; }

		//swaps combat box x and z coordinates (used for sprites that billboard)
		void swapCBox();

		void reset(); //resets to alive and full health; used for resetting level
	}
}
