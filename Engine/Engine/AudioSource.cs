using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Engine
{
    
    /* Represents a sound that is made in the game.*/
    public class AudioSource
    {
        static List<AudioSource> AudioSources;

        public AudioSource()
        {
            // TO DO
        }

        ~AudioSource()
        {

        }

        public void Release()
        {

        }

        public bool LoadSource(Stream stream)
        {
            return false;
        }

        public void PlaySource()
        {

        }

        public void StopSource()
        {

        }

        public void PauseSource()
        {

        }

        public void RewindSource()
        {

        }
        bool AudioBuffer(int id, byte[] data)
        {
            return false;
        }

        internal void ProcessAudioBuffer()
        {

        }

        static internal void Update()
        {

        }
    }
}
