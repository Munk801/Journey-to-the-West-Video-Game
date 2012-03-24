using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using AL = OpenTK.Audio.OpenAL.AL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using csogg;
using csvorbis;

namespace Engine
{
    /// <summary>
    /// Every audio source that is created is managed by Audio Manager 
    /// and will be placed on its own channel for buffering, updating, and playing.
    /// </summary>
    public class AudioSource : IDisposable
    {
        // Local variables
        public byte[] SegmentBuffer { get; private set; }
        public int[] Buffers { get; private set; }
        public int BufferCount { get; private set; }
        public int BufferSize { get; private set; }
        public int Source { get; private set; }
        public VorbisFileInstance SourceFile {get; private set;}

        public ALFormat AudioFormat {get; private set;}
        public int AudioRate {get; private set;}

        public bool FileHasEnded;
        public bool Replay;
        public bool IsFree
        {
            get
            {
                return SourceFile == null;
            }
        }

        private const int _BIGENDIANREADMODE = 0;
        private const int _WORDREADMODE = 2;
        private const int _SGNEDREADMODE = 1;

        // CONSTRUCTOR
        public AudioSource(int bufferCount, int bufferSize)
        {
            Buffers = new int[bufferCount];
            BufferCount = bufferCount;
            BufferSize = bufferSize;
            SourceFile = null;

            SegmentBuffer = new byte[bufferSize];

            // Create the source
            Source = AL.GenSource();

            // Create buffers
            for (int i = 0; i < BufferCount; i++)
            {
                Buffers[i] = AL.GenBuffer();
            }
        }

        // DESTRUCTOR
        ~AudioSource()
        {
            Dispose();
        }

        // DISPOSE METHOD FOR REMOVING UNUSED RESOURCES
        public void Dispose()
        {
            AL.SourceStop(Source);
            if (Buffers != null)
            {
                AL.DeleteBuffers(Buffers);
            }

            Buffers = null;
            SourceFile = null;
        }

        // REMOVE EMPTY BUFFERS FROM QUEUE
        protected void RemoveEmptyBuffers()
        {
            int buffersProcessed;
            AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out buffersProcessed);

            int[] buffersRemoved = new int[buffersProcessed];

            AL.SourceUnqueueBuffers(Source, buffersProcessed, buffersRemoved);

        }

        // DETERMINES THE ALFORMAT FOR A GIVEN SOURCE
        public ALFormat DetermineSourceFormat(VorbisFileInstance source)
        {
            Info sourceInfo = RetrieveSourceInfo(source);

            if (sourceInfo.channels == 1)
            {
                return ALFormat.Mono16;
            }
            else if (sourceInfo.channels == 2)
            {
                return ALFormat.Stereo16;
            }
            else
            {
                throw new NotImplementedException("Only mono and stereo are implemented.  Audio has too many channels.");
            }
        }

        // DETERMINE THE RATE OF A GIVEN SOURCE
        public int DetermineSourceRate(VorbisFileInstance source)
        {
            Info sourceInfo = RetrieveSourceInfo(source);

            return sourceInfo.rate;
        }

        /// <summary>
        /// Retrive the source info from a vorbis file
        /// </summary>
        /// <param name="source">the source file to retrieve data from</param>
        /// <returns></returns>
        public Info RetrieveSourceInfo(VorbisFileInstance source)
        {
            Info[] formatInfo = source.vorbisFile.getInfo();
            if (formatInfo.Length < 1 || formatInfo[0] == null)
                throw new ArgumentException("NO CLIP INFORMATION");

            Info sourceInfo = formatInfo[0];
            return sourceInfo;
        }

        // PLAY
        public void PlaySource(VorbisFileInstance source)
        {
            // Remove any empty buffers
            RemoveEmptyBuffers();

            // Set up all the source parameters
            AudioFormat = DetermineSourceFormat(source);
            AudioRate = DetermineSourceRate(source);

            // Initialize the source to play
            SourceFile = source;
            FileHasEnded = false;


            // Start buffer
            int processedBuffers = 0;
            for (int i = 0; i < BufferCount; i++)
            {
                int bytesRead = source.read(SegmentBuffer, SegmentBuffer.Length, _BIGENDIANREADMODE, _WORDREADMODE, _SGNEDREADMODE, null);
                if (bytesRead > 0)
                {
                    AL.BufferData(Buffers[i], AudioFormat, SegmentBuffer, bytesRead, AudioRate);
                    processedBuffers++;
                }
                else if (bytesRead == 0)
                {
                    break;
                }
                else
                {
                    throw new System.IO.IOException("Unable to open OGG File");
                }
            }

            // Play buffered clip
            AL.SourceQueueBuffers(Source, processedBuffers, Buffers);

            AL.SourcePlay(Source);
        }

        // PAUSE
        public void PauseSource(VorbisFileInstance source)
        {
            // TO DO
        }

        // UPDATE 
        public void Update()
        {
            // Make sure we are you trying to update a NULL Source
            if (SourceFile != null)
            {
                
                // Initialize our queued buffers and our process buffers
                int queuedBuffers;
                AL.GetSource(Source, ALGetSourcei.BuffersQueued, out queuedBuffers);

                int processedBuffers;
                AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out processedBuffers);

                // Check if we reached an end of file situation
                // CHANGE BACK TO TWO SEPARATE IF CALLS IF DOESN'T WORK

                if (FileHasEnded && queuedBuffers <= processedBuffers)
                {
                        // Source is done
                    AL.SourceStop(Source);
                    SourceFile = null;

                    RemoveEmptyBuffers();

                    return;
                }

                // Buffering isn't done so continue
                else
                {
                    if (queuedBuffers - processedBuffers > 0 && AL.GetError() == ALError.NoError)
                    {
                        // Continue playing
                        if (AL.GetSourceState(Source) != ALSourceState.Playing)
                        {
                            AL.SourcePlay(Source);
                        }
                    }

                    bool underFlow = (processedBuffers >= BufferCount);

                    // Continue through the buffers until we don't have anymore
                    while (processedBuffers > 0)
                    {
                        int removedBuffers = 0;

                        AL.SourceUnqueueBuffers(Source, 1, ref removedBuffers);

                        // If we reached the end of the clip, remove and continue
                        if (FileHasEnded)
                        {
                            processedBuffers--;
                            continue;
                        }

                        int bytesRead = SourceFile.read(SegmentBuffer, SegmentBuffer.Length, _BIGENDIANREADMODE, _WORDREADMODE, _SGNEDREADMODE, null);

                        if (bytesRead > 0)
                        {
                            AL.BufferData(removedBuffers, AudioFormat, SegmentBuffer, bytesRead, AudioRate);
                            AL.SourceQueueBuffer(Source, removedBuffers);
                        }

                        else if (bytesRead == 0)
                        {
                            FileHasEnded = true;
                            
                        }

                        else
                        {
                            
                            AL.SourceStop(Source);
                            SourceFile = null;
                            break;
                        }

                        if (AL.GetError() != ALError.NoError)
                        {
                            AL.SourceStop(Source);
                            SourceFile = null;
                            break;
                        }

                        processedBuffers--;
                    }
                }

            }
        }

        internal void SetReplayBool()
        {
            Replay = true;
        }
    }
}
