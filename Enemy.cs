using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs
{
    class Enemy
    {
        /** This struct will contain the Players Status **/
        struct EnemyState
        {
            public int e_health;
            public int e_damage;
            public double e_speed;
            public string e_equip_ability;
            public int e_current_zone;

            // texture...

        };

        int health, damage;
        double speed;
        // texture = .... ;

        EnemyState e_state;

        public Enemy(int hp, int dam, double spd)
        {
            health = hp;
            damage = dam;
            speed = spd;
            // type = ... ;
        }


        /** Like the Player Status update call this every time you need to update an Enemies State before saving **/
        public void updateState()
        {
            e_state.e_health = health;
            e_state.e_damage = damage;
            e_state.e_speed = speed;

            // Add in the other State elements that will need to be maintained here..
        }
    }
}
