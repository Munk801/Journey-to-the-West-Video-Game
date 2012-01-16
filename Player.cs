using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs
{
    class Player
    {
        /** This struct will contain the Players State **/
        struct PlayerState
        {
            public int p_health;
            public int p_damage;
            public double p_speed;
            public string p_equip_ability;
            public int p_current_zone;

            // texture...

        };

        PlayerState p_state;

        //player globals here
        int health, damage;
        double speed;
        //texture .... ;               

        public Player()
        {
            health = 10;
            damage = 1;
            speed = 2.0;
        }

        /**
         * Sets the PlayerState elements to the current Player values.  Call this method every update or simply when the state changes.  This will be used to store
         * the Players State when saving the game.
         * */
        public void updateState()
        {
            p_state.p_health = health;
            p_state.p_damage = damage;
            p_state.p_speed = speed;

            // Put the rest in here like texture, current zone, etc

        }
    }
}
