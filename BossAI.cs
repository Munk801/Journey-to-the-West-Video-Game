using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace U5Designs {
    public interface BossAI {

        void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d);

        int gethealth();

        void killBoss(PlayState ps);
    }

}
