using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
     //Note, this has to be in U5Designs namespace if its going to pull from GameState
    class State_SplashScreen //: GameState
    {
        public void Update()
        {
            System.Console.WriteLine("Updating Splash Screen");
        }

        public void Draw()
        {
            System.Console.WriteLine("Rendering Splash Screen");
        }
    }
}
