using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenAL = OpenTK.Audio.OpenAL;
using Audio = OpenTK.Audio;

namespace Engine
{
    /* Managemenet class for sounds that are to be produced in game.  Stores all the sounds and will played based on triggers in game*/
    public class AudioManager
    {
        /* Hardware can only allow 256 sound channels to be processed at a single time.  Track the max channels and the channels that are being passed through*/
        readonly int maxSoundChannels = 256;
        List<int> soundChannels = new List<int>();
        
        // Grab the default Audio Device from OpenAL
        public static string DefaultAudioDevice
        {
            get
            {
                try
                {
                    return Audio.AudioContext.DefaultDevice;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        static internal Audio.AudioContext CurrentAudioContext
        {
            get;
            private set;
        }


        public static List<string> AllAudioDevices
        {
            get
            {
                if (string.IsNullOrEmpty(DefaultAudioDevice))
                    return new List<string>();

                return new List<string>(Audio.AudioContext.AvailableDevices);
            }
        }

        public static bool isAudioDeviceInitialized
        {
            get;
            private set;
        }

        public AudioManager()
        {
            // If there is no default device, return
            if (!string.IsNullOrEmpty(DefaultAudioDevice))
            {
                return;
            }

        }

        /// <summary>
        /// Creates an audio context
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static bool CreateAudioDevice(string device)
        {
            // Release current audio context
            if (CurrentAudioContext != null)
            {
                ReleaseAudioDevice();
            }

            // Check  if device contains anything
            if (string.IsNullOrEmpty(device))
            {
                return false;
            }

            // Create a new audio context
            try
            {
                CurrentAudioContext = new Audio.AudioContext(device);
            }
            catch (Exception e)
            {
                // TO DO: WRITE TO FILE THE EXCEPTION
                return false;
            }
            return true;
        }

        /// <summary>
        /// Release the current audio context
        /// </summary>
        public static void ReleaseAudioDevice()
        {
            if (CurrentAudioContext != null)
            {
                CurrentAudioContext.Dispose();
            }
        }

        /// <summary>
        /// State of Audio.
        /// </summary>
        public enum AudioState
        {
            Audio_Initial = OpenAL.ALSourceState.Initial,
            Audio_Playing = OpenAL.ALSourceState.Playing,
            Audio_Paused = OpenAL.ALSourceState.Paused,
            Audio_Stopped = OpenAL.ALSourceState.Stopped
        }

        public static bool playMusic
        {
            get;
            set;
        }

        public static bool playSound
        {
            get;
            set;
        }
    }
}
