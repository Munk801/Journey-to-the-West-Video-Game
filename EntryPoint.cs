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
            // RenderFrame events (as fast as the computer can handle). (may change this in the future, or set it as an option)
            using (GameEngine engine = new GameEngine())
            {
                engine.Run(30.0);
            }
        }
    }
}
