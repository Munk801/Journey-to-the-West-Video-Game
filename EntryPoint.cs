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
            /** Create the StateManager that will manage the State of the Game **/
            GameEngine engine = new GameEngine();


            /** Initialize the Game Engine here **/
            engine.Init();

            /** Load the Intro State **/
            MainMenuState ms = new MainMenuState();
            engine.ChangeState(ms);
            
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle).
            using (Game game = new Game())
            {
                game.Run(30.0);
            }
        }
    }
}
