using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenAL = OpenTK.Audio.OpenAL;
using Audio = OpenTK.Audio;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using csogg;
using csvorbis;

namespace Engine
{
    /// <summary>
    /// Audio manager provides all the resources for queueing audio sources into 
    /// appropriate channels and handles playing them.  There will  only be one 
    /// audio manager that runs when the application is run but many channels can
    /// be created in this manager instance.
    /// </summary>
    public class AudioManager
    {
        // Public variables needed for Audio Manager.  The number of channels, buffers per channel, and bytes per buffer
        int NumOfChannels { get;  set; }
        int BuffersPerChannel { get;  set; }
        int BytesPerBuffer { get;  set; }

        // Stores all the channels we have
        AudioSource[] AudioSources { get; set; }

        // Audio threading
        Thread ThreadCall { get; set; }

        // Checks if we need updating
        public bool needsUpdate { get; set; }

        // Initialize our audio manager
        private static AudioManager _manager = null;

        public static AudioManager Manager
        {
            get
            {
                if (_manager == null) _manager = new AudioManager();

                return _manager;
            }

            set
            {
                _manager = value;
            }
        }
        // Constructor.
        /// <summary>
        /// Constructor for Audio Manager
        /// </summary>
        /// <param name="numOfChannels">Number of channels</param>
        /// <param name="buffersPerChannel">Number of buffers to use per channel</param>
        /// <param name="bytesPerBuffer">Number of bytes to use per buffer</param>
        /// <param name="threadCall">Determines whether to thread audio channels</param>
        public AudioManager(int numOfChannels, int buffersPerChannel, int bytesPerBuffer, bool threadCall)
        {
            InitializeManager(numOfChannels, buffersPerChannel, bytesPerBuffer, threadCall);
        }
        
        /// <summary>
        /// Default Constructor.  Initializes with a 16 channels, 32 buffer, 4 kb threaded call
        /// </summary>
        public AudioManager()
        {
            InitializeManager(16, 32, 4096, true);
        }


        public void StartAudioServices()
        {
            AudioContext ac = new AudioContext();
            XRamExtension xram = new XRamExtension();
        }

        private void InitializeManager(int numOfChannels, int buffersPerChannel, int bytesPerBuffer, bool threadCall)
        {
            // Set the local variables to parameters
            NumOfChannels = numOfChannels;
            //BuffersPerChannel = buffersPerChannel;
            //BytesPerBuffer = bytesPerBuffer;
            needsUpdate = threadCall;
            AudioSources = new AudioSource[numOfChannels];

            // Create a new Audio channel for every channel specified in local array
            for (int i = 0; i < numOfChannels; i++)
            {
                AudioSources[i] = new AudioSource(buffersPerChannel, bytesPerBuffer);
            }
            
            Manager = this;

            // If we are looking for a threaded call, create a new thread
            if (threadCall)
            {
                // Create a new thread
                ThreadCall = new Thread(Update);
                // Thread should be background
                ThreadCall.IsBackground = true;
                // Start the thread
                ThreadCall.Start();
            }

            else
            {
                // No thread so NULL
                ThreadCall = null;
            }
        }

        // Update
        public void Update()
        {
            // While we need to update
            while (needsUpdate)
            {
                // For every source in our sources, lock the source and update it.
                foreach (AudioSource source in AudioSources)
                {
                    lock (this)
                    {
                        // POSSIBLE THERE NEEDS TO BE A SLEEP CALL IN THREAD
                        source.Update();
                        Thread.Sleep(1);
                    }
                }
            }
        }


        // Play Call
        public void PlayFile(VorbisFileInstance audioFile)
        {
            foreach (AudioSource source in AudioSources)
            {
                try
                {
                    if (source.IsFree)
                    {
                        source.PlaySource(audioFile);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        // Destructor
        ~AudioManager()
        {
            DisposeResources();
        }


        /// <summary>
        /// Removes any unused audio sources
        /// </summary>
        public void DisposeResources()
        {
            // TO DO:  REMOVE AUDIO RESOURCES
        }
    }
}
