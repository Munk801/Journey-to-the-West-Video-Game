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
    internal interface Airoutine {
        void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d);
    }

    internal class Kidmoveto : Airoutine{

        public void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d) {
            me.attackspeed = 1; //hack initilization
			if(VectorUtil.dist(playerposn, me.location) > 90) {
				Vector3 dir = VectorUtil.getdir(playerposn, me.location);
                me.velocity.X = dir.X * me.speed;
                if (enable3d)
                    me.velocity.Z = dir.Z * me.speed;
            }
            else {
                me.velocity.X = 0;
                me.velocity.Z = 0;
                me.ChangeState(new KidThrowTime());
            }
        }
    }

    internal class KidThrowTime : Airoutine {
        public void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d) {
			if(VectorUtil.dist(playerposn, me.location) <= 90) {
                if (me.attackdelayed)
                    me.attacktimer = me.attacktimer + time;
                if (me.attacktimer > me.attackspeed) {
                    me.attackdelayed = false;
                    me.attacktimer = 0;
                }
                if (!me.attackdelayed) {
					Vector3 dir = VectorUtil.getdir(playerposn, me.location);
                    //throw icecream
					Vector3 projlocation = me.location;
					if(!enable3d) {
						projlocation.Z += 0.001f; //break the rendering tie between enemy and projectile, or else they flicker
					}
                    Vector3 direction = VectorUtil.getdir(playerposn, me.location);
					Projectile shot = new Projectile(projlocation, direction, false, me.projectile);
                    //Projectile shot = new Projectile(projlocation, direction, new Vector3(12.5f, 12.5f, 12.5f), new Vector3(6.25f, 6.25f, 6.25f), new Vector3(6.25f, 6.25f, 6.25f), true, true, playstate.enable3d, me.damage, 150, false, false, me.projectileSprite);
                    playstate.objList.Add(shot);
                    playstate.renderList.Add(shot);
                    playstate.colisionList.Add(shot);
                    playstate.physList.Add(shot);
                    playstate.combatList.Add(shot);
                    me.attackdelayed = true;
                }
            }
            else
                me.ChangeState(new Kidmoveto());
        }
    }


    internal class Birdmoveto : Airoutine {
        public void update(double time, PlayState playstate, Vector3 playerposn, Enemy me, bool enable3d) {
        }
    }


}
