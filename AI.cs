using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs {
    public interface Airoutine {
        void update();
    }

    public class RunTo : Airoutine{

        public void update() {
        }
    }


    public class StandStill : Airoutine {
        public void update() {
        }
    }


}
