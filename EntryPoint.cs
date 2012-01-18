using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs
{
    class EntryPoint
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (GameEngine engine = new GameEngine())
            {
                //Tells opentk side to run
                engine.Run(30.0);

                /** Initialize the Game Engine here **/
             //   engine.Init(); //OnLoad gets called when we call engine.run, same thing

                /** Load the Intro State **/
                // In onload we should set the first state.
               // MainMenuState ms = new MainMenuState();
               // engine.ChangeState(ms);
            }
        }
    }
}
