using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    class State_SplashScreen : GameState
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
