using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using OpenTK;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;

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

            //string test = "Hydrate-Kenny_Beltrey.ogg";
            //AudioFile testFile = new AudioFile(test);
            //testFile.Play();
            // The 'using' idiom guarantees proper resource cleanup.
            // We request 30 UpdateFrame events per second, and unlimited
            // RenderFrame events (as fast as the computer can handle). (may change this in the future, or set it as an option)
            
            // Initialize the audio context and xram for sound
            AudioContext ac = new AudioContext();
            XRamExtension xr = new XRamExtension();
            using (GameEngine engine = new GameEngine())
            {
                engine.Run();
            }
        }
    }
}
