using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Engine;
namespace U5Designs {
    class GorillaAI : BossAI {
        bool active;

        public GorillaAI(Player player, PlayState ps) {
        }

        public void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d) {
            if (active) {
                //do Ai code
            }
        }

        public int gethealth() {
            return 1;
        }

        public void killBoss(PlayState ps) {
            //TODO: whatever happens when the boss dies
      /*      ps.objList.Remove(bossobject);
            ps.physList.Remove(bossobject);
            ps.collisionList.Remove(bossobject);
            ps.renderList.Remove(bossobject);
            ps.combatList.Remove(bossobject); */
            active = false;
        }

        public void dodamage(int hit) {
            
        }

        public void spawnProjetile(Vector3 point) {


        }
    }
}
