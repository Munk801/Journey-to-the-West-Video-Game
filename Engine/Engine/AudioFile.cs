using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using csogg;
using csvorbis;

namespace Engine
{

        public class AudioFile
        {
            VorbisFile sourceAudio;
            public AudioSource CurrentSource;
            bool IsOver = false;
            public AudioFile(string fileName)
            {
                sourceAudio = new VorbisFile(fileName);
                RemoveDelay(64 * 1024);
            }

            public AudioFile(Stream inputStream)
            {
                sourceAudio = new VorbisFile(inputStream);
                RemoveDelay(64 * 1024);
            }

            protected void RemoveDelay(int bytes)
            {
                VorbisFileInstance sourceInstance = sourceAudio.makeInstance();
                
                int totalBytes = 0;
                byte[] buffer = new byte[4096];

                while (totalBytes < bytes)
                {
                    int bytesRead = sourceInstance.read(buffer, buffer.Length, 0, 2, 1, null);

                    if (bytesRead <= 0)
                        break;

                    totalBytes += bytesRead;
                }
            }

            public void ReplayFile()
            {
                lock (AudioManager.Manager)
                {
                    
                    AudioManager.Manager.PlayFile(sourceAudio.makeInstance(), out CurrentSource);
                }
            }


            public void Play()
            {
                lock (AudioManager.Manager)
                {
                    AudioManager.Manager.PlayFile(sourceAudio.makeInstance());
                }
            }

            public void Pause()
            {
                lock (AudioManager.Manager)
                {
                    AudioManager.Manager.PauseFile(sourceAudio.makeInstance());
                    if (CurrentSource != null)
                        CurrentSource.Dispose();
                }
            }

            public void Stop()
            {
                lock (AudioManager.Manager)
                {
                    AudioManager.Manager.StopFile(sourceAudio.makeInstance());
                    if(CurrentSource != null)
                    CurrentSource.Dispose();
                }
            }
        }
}
