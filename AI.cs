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
    public interface Airoutine {
        void update(FrameEventArgs e, Vector3 playerposn, Enemy me, bool enable3d);
    }

    public class Kidmoveto : Airoutine{

        public void update(FrameEventArgs e, Vector3 playerposn, Enemy me, bool enable3d) {
            if (dist(playerposn, me.location) > 90) {
                Vector3 dir = getdir(playerposn, me.location);
                me.velocity.X = dir.X * me.speed;
                if (enable3d)
                    me.velocity.Z = dir.Z * me.speed;
            }
            else {
                me.velocity.X = 0;
                me.velocity.Z = 0;
            }
        }
        double dist(Vector3 v1, Vector3 v2) {
            Vector3 tmp = new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
            return Math.Sqrt((tmp.X * tmp.X) + (tmp.Y * tmp.Y) + (tmp.Z * tmp.Z));
        }
        Vector3 getdir(Vector3 player, Vector3 enemy) {
            Vector3 tmp = new Vector3(player.X - enemy.X, player.Y - enemy.Y, player.Z - enemy.Z);
            tmp.Normalize();
            return tmp;
        }
    }


    public class Birdmoveto : Airoutine {
        public void update(FrameEventArgs e, Vector3 playerposn, Enemy me, bool enable3d) {
        }
    }


}
