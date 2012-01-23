using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs
{
    /**
     * A data structure that will contain the State for the Player.  Every value must be initialized within the State or the structure 
     * cannot be used.  A default constructor will initialize the values to the default.  There will be getters and setters to provide for
     * updating the State of the player, loading saved game data into the struct, etc.
     * 
     * */

    public struct PlayerState
    {
        public int p_health;
        public int p_damage;
        public double p_speed;
        public string p_equip_ability;
        public string p_name;
        public int p_current_zone;
        public List<int> p_abilities;

        // The default struct constructor which will take the players name and initialize all other values to
        // their default settings.  Use this when creating the initial player state
        public PlayerState(string playername)
        {
            p_health = 10;
            p_damage = 1;
            p_speed = 1;
            p_equip_ability = "none";
            p_name = playername;
            p_current_zone = 0;
            p_abilities = new List<int>();
        }

        /** Every Player State element will need a Getter here **/
        public string getName() { return p_name; }
        public int getHealth() { return p_health; }
        public int getDamage() { return p_damage; }
        public double getSpeed() { return p_speed; }
        public string getAbility() { return p_equip_ability; }
        public int getCurrentZone() { return p_current_zone; }
        public List<int> getAbilities() { return p_abilities; }

        /** Every Player State element will need a Setter here **/
        public void setName(string n) { p_name = n; }
        public void setHealth(int h) { p_health = h; }
        public void setDamage(int d) { p_damage = d; }
        public void setSpeed(double s) { p_speed = s; }
        public void setAbility(string a) { p_equip_ability = a; }
        public void setZone(int z) { p_current_zone = z; }
        public void setAbilities(List<int> alist) { p_abilities = alist; }

        /** Add a single ability to the player state **/
        public void addAbility(int a) { p_abilities.Add(a); }
    }
}
