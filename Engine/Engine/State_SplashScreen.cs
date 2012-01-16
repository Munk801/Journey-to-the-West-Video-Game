using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    class State_SplashScreen : GameObject
    {
        public void Update(double elapsedTime)
        {
            System.Console.WriteLine("Updating Splash Screen");
        }

        public void Render()
        {
            System.Console.WriteLine("Rendering Splash Screen");
        }
    }
}
