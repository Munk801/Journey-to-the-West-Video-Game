using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using AL = OpenTK.Audio.OpenAL.AL;
using csogg;
using csvorbis;
using AudioState = Engine.AudioManager.AudioState;

namespace Engine
{
    
    /* Represents a sound that is made in the game.*/
    public class AudioSource
    {
        #region Member Properties
        // Member variables
        static List<AudioSource> AudioSources;
        int[] audioBuffer;
        int sourceID;
        Dictionary<int, byte[]> bufferData;
        OggInputStream oggStream;
        Stream byteStream;
        string audioFileName;
        public bool isLooping;
        public string audioName
        {
            get;
            set;
        }


        public AudioManager.AudioState audioState
        {
            get
            {
                if (sourceID == 0) return AudioState.Audio_Initial;
                return (AudioState)AL.GetSourceState(sourceID);
            }
        }

        #endregion

        public AudioSource()
        {
            // If audio stream isn't initialized, initialize the list, and add this stream to the list
            if (AudioSources == null)
            {
                AudioSources = new List<AudioSource>();
                
            }
            AudioSources.Add(this);

            // Initialize buffers
            audioBuffer = AL.GenBuffers(2);

            // Initialize the source
            sourceID = AL.GenSource();

            // Stream the buffers to the buffer data
            bufferData = new Dictionary<int, byte[]>();
            bufferData[audioBuffer[0]] = new byte[44100];
            bufferData[audioBuffer[1]] = new byte[44100];

        }

        ~AudioSource()
        {

        }

        public void Release()
        {

        }

        public bool LoadSource(Stream stream)
        {
            if(!AudioManager.isAudioDeviceInitialized || byteStream == null)
            return false;

            byteStream = stream;

            oggStream = new OggInputStream(byteStream);

            return true;

        }

        public bool LoadSource(string fileName)
        {
            byte[] convString = ReadFile(fileName);
            Stream path = new MemoryStream(convString);

            return LoadSource(path);
        }

        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer;
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }




        public void PlaySource()
        {
            // Check to make sure the Audio Manager is initialized or the 
            // source is set or we actually have something in the stream
            if (!AudioManager.isAudioDeviceInitialized || audioState == AudioState.Audio_Playing || sourceID == null) return;
            
            // Fill buffers ????
            if (!AudioBuffer(audioBuffer[0], bufferData[audioBuffer[0]])) return;

            if (!AudioBuffer(audioBuffer[1], bufferData[audioBuffer[1]])) return;

            // Add buffers to the queue ????
            AL.SourceQueueBuffers(sourceID, audioBuffer.Length, audioBuffer);

            // Play Source
            AL.SourcePlay(sourceID);
        }

        public void StopSource()
        {
            // Check to make sure Audio Manager is initialized

            AL.SourceStop(sourceID);
            RewindSource();
        }

        public void PauseSource()
        {
            // Same as stop but no need to rewind
            AL.SourceStop(sourceID);
        }

        public void RewindSource()
        {

        }
        bool AudioBuffer(int id, byte[] data)
        {
            if (data == null | data.Length == 0)
                return false;

            int size = 0;
            while (size < data.Length)
            {
                int result = oggStream.Read(data, size, data.Length - size);
                if (result > 0)
                {
                    size += result;
                }
                else
                {
                    // End of stream
                    //if (Stream.Position == Stream.Length)
                    if (oggStream.Available == 0)
                    {
                        if (isLooping)
                        {
                            RewindSource();
                        }
                        else
                            StopSource();
                    }

                    return false;
                }
            }

            if (size == 0)
            {
                return false;
            }

            AL.BufferData(id, (OpenTK.Audio.OpenAL.ALFormat)oggStream.Format, data, data.Length, oggStream.Rate);
            return true;
        }
        internal void ProcessAudioBuffer()
        {
            int buffersProcessed = 0;
            AL.GetSource(sourceID, OpenTK.Audio.OpenAL.ALGetSourcei.BuffersProcessed, out buffersProcessed);

            while (buffersProcessed-- != 0)
            {
                // Enqueue Buffer
                int currentBuffer = AL.SourceUnqueueBuffer(sourceID);
                
                // Update Buffer
                AudioBuffer(currentBuffer, bufferData[currentBuffer]);

                // Queue up buffer
                AL.SourceQueueBuffer(sourceID, currentBuffer);
            }
        }

        public static void Update()
        {
            if (AudioSources == null) return;

            foreach (AudioSource source in AudioSources)
            {
                source.ProcessAudioBuffer();
            }
        }
    }
}
