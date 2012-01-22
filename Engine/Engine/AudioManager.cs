using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenAL = OpenTK.Audio.OpenAL;

namespace Engine
{
    /* Managemenet class for sounds that are to be produced in game.  Stores all the sounds and will played based on triggers in game*/
    public class AudioManager
    {
        /* Hardware can only allow 256 sound channels to be processed at a single time.  Track the max channels and the channels that are being passed through*/
        readonly int maxSoundChannels = 256;
        List<int> soundChannels = new List<int>();
        
        public AudioManager()
        {
            // If there is no default device, return

        }

        public void LoadSound(string soundID, string path)
        {

        }

        internal Sound PlaySound(string soundID)
        {
            return null;
        }

        internal void StopSound(Sound sound)
        {
        }

        internal bool isSoundPlaying(Sound sound)
        {
            return false;
        }
    }
}
