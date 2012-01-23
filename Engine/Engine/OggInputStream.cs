using System;
using System.IO;
using csogg;
using csvorbis;

//taken from http://home.halden.net/tombr/ogg/ogg.html

/*
* Decompresses an Ogg file.
* <p>
* How to use:<br>
* 1. Create OggInputStream passing in the input stream of the packed ogg file<br>
* 2. Fetch format and sampling rate using getFormat() and getRate(). Use it to
*    initalize the sound player.<br>
* 3. Read the PCM data using one of the read functions, and feed it to your player.
* <p>
* OggInputStream provides a read(ByteBuffer, int, int) that can be used to read
* data directly into a native buffer.
*/


namespace Engine
{
	/// <summary>
	/// Ogg Vorbis streamer
	/// </summary>
	internal class OggInputStream
	{

		/// <summary>
		/// Creates an OggInputStream that decompressed the specified ogg file
		/// </summary>
		/// <param name="stream">Input stream</param>
		public OggInputStream(Stream stream)
		{
			Block = new Block(dspState);
			Stream = stream;
			InitVorbis();
			_index = new int[Info.channels];
		}


		/// <summary>
		/// Dispose resources
		/// </summary>
		public void Dispose()
		{
			if (Stream != null)
				Stream.Dispose();
			Stream = null;
		}


		/// <summary>
		/// Reads the next byte of data from this input stream. The value byte is
		/// returned as an int in the range 0 to 255. If no byte is available because
		/// the end of the stream has been reached, the value -1 is returned. This
		/// method blocks until input data is available, the end of the stream is
		/// detected, or an exception is thrown. 
		/// </summary>
		/// <returns>@return the next byte of data, or -1 if the end of the stream is reached</returns>
		public int Read()
		{
			int retVal = Read(ReadDummy, 0, 1);
			return (retVal == -1 ? -1 : ReadDummy[0]);
		}


		/// <summary>
		/// Reads up to len bytes of data from the input stream into an array of bytes.
		/// </summary>
		/// <param name="b">the buffer into which the data is read.</param>
		/// <param name="off">the start offset of the data.</param>
		/// <param name="len">the maximum number of bytes read</param>
		/// <returns>the total number of bytes read into the buffer, or -1 if there is no more data because the end of the stream has been reached.</returns>
		public int Read(byte[] b, int off, int len)
		{
			if (EndOfStream)
			{
				return 0;
			}

			int bytesRead = 0;
			while (!EndOfStream && (len > 0))
			{
				FillConvBuffer();

				if (!EndOfStream)
				{
					int bytesToCopy = Math.Min(len, convbufferSize-convbufferOff);
					Array.Copy(convbuffer, convbufferOff, b, off, bytesToCopy);
					convbufferOff += bytesToCopy;
					bytesRead += bytesToCopy;
					len -= bytesToCopy;
					off += bytesToCopy;
				}
			}

			return bytesRead;
		}


		/// <summary>
		/// Reads up to len bytes of data from the input stream into a ByteBuffer
		/// </summary>
		/// <param name="stream">the buffer into which the data is read</param>
		/// <param name="off">the start offset of the data</param>
		/// <param name="len">the maximum number of bytes read</param>
		/// <returns>the total number of bytes read into the buffer, or -1 if there is no more data because the end of the stream has been reached</returns>
		public int Read(MemoryStream stream, int off, int len)
		{
			if (EndOfStream)
			{
				return 0;
			}

			stream.Seek(off, SeekOrigin.Begin);
			int bytesRead = 0;
			while (!EndOfStream && (len > 0))
			{
				FillConvBuffer();

				if (!EndOfStream)
				{
					int bytesToCopy = Math.Min(len, convbufferSize-convbufferOff);
					stream.Write(convbuffer, convbufferOff, bytesToCopy);
					convbufferOff += bytesToCopy;
					bytesRead += bytesToCopy;
					len -= bytesToCopy;
				}
			}

			return bytesRead;
		}


		/// <summary>
		/// Helper function. Decodes a packet to the convbuffer if it is empty. 
		/// Updates convbufferSize, convbufferOff, and eos
		/// </summary>
		private void FillConvBuffer()
		{
			if (convbufferOff >= convbufferSize)
			{
				convbufferSize = LazyDecodePacket();
				convbufferOff = 0;
				if (convbufferSize == -1)
				{
					EndOfStream = true;
				}
			}
		}


		/// <summary>
		/// OggInputStream does not support mark and reset.
		/// This function does nothing
		/// </summary>
		public void Reset()
		{
		}


		/// <summary>
		/// Skips over and discards n bytes of data from the input stream. The skip
		/// method may, for a variety of reasons, end up skipping over some smaller
		/// number of bytes, possibly 0. The actual number of bytes skipped is returned
		/// </summary>
		/// <param name="length">the number of bytes to be skipped</param>
		/// <returns>return the actual number of bytes skipped</returns>
		public long Skip(long length)
		{
			int bytesRead = 0;
			while (bytesRead < length)
			{
				int res = Read();
				if (res == -1)
				{
					break;
				}

				bytesRead++;
			}

			return bytesRead;
		}


		/// <summary>
		/// Initalizes the vorbis stream. Reads the stream until info and comment are read.
		/// </summary>
		private void InitVorbis()
		{
			// Now we can read pages
			SyncState.init();

			// grab some data at the head of the stream.  We want the first page
			// (which is guaranteed to be small and only contain the Vorbis
			// stream initial header) We need the first page to get the stream
			// serialno.

			// submit a 4k block to libvorbis' Ogg layer
			int index = SyncState.buffer(4096);
			byte[] buffer = SyncState.data;
			int bytes = Stream.Read(buffer, 0, buffer.Length);
			SyncState.wrote(bytes);
			// Get the first page.
			if (SyncState.pageout(Page) != 1)
			{
				// have we simply run out of data?  If so, we're done.
				if (bytes < 4096)
					return;//break;

				// error case.  Must not be Vorbis data
				throw new Exception("Input does not appear to be an Ogg bitstream.");
			}

			// Get the serial number and set up the rest of decode.
			// serialno first; use it to set up a logical stream
			StreamState.init(Page.serialno());

			// extract the initial header from the first page and verify that the
			// Ogg bitstream is in fact Vorbis data

			// I handle the initial header first instead of just having the code
			// read all three Vorbis headers at once because reading the initial
			// header is an easy way to identify a Vorbis bitstream and it's
			// useful to see that functionality seperated out.

			Info.init();
			Comment.init();
			if (StreamState.pagein(Page) < 0)
			{
				// error; stream version mismatch perhaps
				throw new Exception("Error reading first page of Ogg bitstream data.");
			}

			if (StreamState.packetout(Packet) != 1)
			{
				// no page? must not be vorbis
				throw new Exception("Error reading initial header packet.");
			}

			if (Info.synthesis_headerin(Comment, Packet) < 0)
			{
				// error case; not a vorbis header
				throw new Exception("This Ogg bitstream does not contain Vorbis audio data.");
			}

			// At this point, we're sure we're Vorbis.  We've set up the logical
			// (Ogg) bitstream decoder.  Get the comment and codebook headers and
			// set up the Vorbis decoder

			// The next two packets in order are the comment and codebook headers.
			// They're likely large and may span multiple pages.  Thus we read
			// and submit data until we get our two packets, watching that no
			// pages are missing.  If a page is missing, error out; losing a
			// header page is the only place where missing data is fatal. 


			int i = 0;
			while (i < 2)
			{
				while (i < 2)
				{

					int result = SyncState.pageout(Page);
					if (result == 0)
						break; // Need more data
					// Don't complain about missing or corrupt data yet.  We'll
					// catch it at the packet output phase

					if (result == 1)
					{
						StreamState.pagein(Page); // we can ignore any errors here
						// as they'll also become apparent
						// at packetout
						while (i < 2)
						{
							result = StreamState.packetout(Packet);
							if (result == 0)
							{
								break;
							}

							if (result == -1)
							{
								// Uh oh; data at some point was corrupted or missing!
								// We can't tolerate that in a header.  Die.
								throw new Exception("Corrupt secondary header. Exiting.");
							}

							Info.synthesis_headerin(Comment, Packet);
							i++;
						}
					}
				}

				// no harm in not checking before adding more
				index = SyncState.buffer(4096);
				buffer = SyncState.data;
				bytes = Stream.Read(buffer, index, 4096);

				// NOTE: This is a bugfix. read will return -1 which will mess up syncState.
				if (bytes < 0)
				{
					bytes = 0;
				}

				if (bytes == 0 && i < 2)
				{
					throw new Exception("End of file before finding all Vorbis headers!");
				}

				SyncState.wrote(bytes);
			}

			convsize = 4096 / Info.channels;

			// OK, got and parsed all three headers. Initialize the Vorbis
			//  packet->PCM decoder.
			dspState.synthesis_init(Info); // central decode state
			Block.init(dspState); // local state for most of the decode
			// so multiple block decodes can
			// proceed in parallel.  We could init
			// multiple vorbis_block structures
			// for vd here
		}


		/// <summary>
		/// Decodes a packet
		/// </summary>
		/// <param name="packet"></param>
		/// <returns></returns>
		private int DecodePacket(Packet packet)
		{
			// check the endianes of the computer.
			bool bigEndian = !BitConverter.IsLittleEndian;

			if (Block.synthesis(packet) == 0)
			{
				// test for success!
				dspState.synthesis_blockin(Block);
			}

			// **pcm is a multichannel float vector.  In stereo, for
			// example, pcm[0] is left, and pcm[1] is right.  samples is
			// the size of each channel.  Convert the float values
			// (-1.<=range<=1.) to whatever PCM format and write it out
			int convOff = 0;
			int samples;
			while ((samples = dspState.synthesis_pcmout(_pcm, _index)) > 0)
			{
				//System.out.println("while() 4");
				float[][] pcm = _pcm[0];
				int bout = (samples < convsize ? samples : convsize);

				// convert floats to 16 bit signed ints (host order) and interleave
				for (int i = 0 ; i < Info.channels ; i++)
				{
					int ptr = (i << 1) + convOff;


					int mono = _index[i];

					for (int j = 0 ; j < bout ; j++)
					{
						int val = (int) (pcm[i][mono + j] * 32767);

						// might as well guard against clipping
						val = Math.Max(-32768, Math.Min(32767, val));
						val |= (val < 0 ? 0x8000 : 0);

						convbuffer[ptr + 0] = unchecked((byte) (bigEndian ? (int) ((uint) val >>  8) : val));
						convbuffer[ptr + 1] = unchecked((byte) (bigEndian ? val : (int) ((uint) val >>  8)));

						ptr += (Info.channels) << 1;
					}
				}

				convOff += 2 * Info.channels * bout;

				// Tell orbis how many samples were consumed
				dspState.synthesis_read(bout);
			}

			return convOff;
		}


		/// <summary>
		/// Decodes the next packet
		/// </summary>
		/// <returns>return bytes read into convbuffer of -1 if end of file</returns>
		private int LazyDecodePacket()
		{
			int result = GetNextPacket(Packet);
			if (result == -1)
			{
				return -1;
			}

			// we have a packet.  Decode it
			return DecodePacket(Packet);
		}


		/// <summary>
		/// packet where to put the packet
		/// </summary>
		/// <param name="packet"></param>
		/// <returns></returns>
		private int GetNextPacket(Packet packet)
		{
			// get next packet.
			bool fetchedPacket = false;
			while (!EndOfStream && !fetchedPacket)
			{
				int result1 = StreamState.packetout(packet);
				if (result1 == 0)
				{
					// no more packets in page. Fetch new page.
					int result2 = 0;
					while (!EndOfStream && result2 == 0)
					{
						result2 = SyncState.pageout(Page);
						if (result2 == 0)
						{
							FetchData();
						}
					}

					// return if we have reaced end of file.
					if ((result2 == 0) && (Page.eos() != 0))
					{
						return -1;
					}

					if (result2 == 0)
					{
						// need more data fetching page..
						FetchData();
					}
					else if (result2 == -1)
					{
						//throw new Exception("syncState.pageout(page) result == -1");
						Console.WriteLine("syncState.pageout(page) result == -1");
						return -1;
					}
					else
					{
						int result3 = StreamState.pagein(Page);
					}
				}
				else if (result1 == -1)
				{
					//throw new Exception("streamState.packetout(packet) result == -1");
					Console.WriteLine("streamState.packetout(packet) result == -1");
					return -1;
				}
				else
				{
					fetchedPacket = true;
				}
			}

			return 0;
		}


		/// <summary>
		/// Copys data from input stream to syncState
		/// </summary>
		private void FetchData()
		{
			if (!EndOfStream)
			{
				// copy 4096 bytes from compressed stream to syncState.
				int index = SyncState.buffer(4096);
				if (index < 0)
				{
					EndOfStream = true;
					return;
				}
				int bytes = Stream.Read(SyncState.data, index, 4096);
				SyncState.wrote(bytes);
				if (bytes == 0)
				{
					EndOfStream = true;
				}
			}
		}


		/// <summary>
		/// Gets information on the ogg
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			String s = "";
			s = s + "version         " + Info.version         + "\n";
			s = s + "channels        " + Info.channels        + "\n";
			s = s + "rate (hz)       " + Info.rate;
			return s;
		}



		#region Properties


		/// <summary>
		/// Gets the format of the ogg file. Is either AudioFormat.Mono16 or AudioFormat.Stereo16
		/// </summary>
        //public AudioFormat Format
        //{
        //    get
        //    {
        //        if (Info.channels == 1)
        //            return AudioFormat.Mono16;
        //        else
        //            return AudioFormat.Stereo16;
        //    }
        //}



		/// <summary>
		/// Returns 0 after EOF is reached, otherwise always return 1.
		/// Programs should not count on this method to return the actual number of
		/// bytes that could be read without blocking
		/// </summary>
		/// <returns>1 before EOF and 0 after EOF is reached.</returns>
		public int Available
		{
			get
			{
				return (EndOfStream ? 0 : 1);
			}
		}



		/// <summary>
		/// OggInputStream does not support mark and reset.
		/// </summary>
		/// <returns>false</returns>
		public bool MarkSupported
		{
			get
			{
				return false;
			}
		}
		
		
		/// <summary>
		/// Gets the rate of the pcm audio.
		/// </summary>
		/// <returns></returns>
		public int Rate
		{
			get
			{
				if (Info != null)
					return Info.rate;

				return 0;
			}
		}
		

		/// <summary>
		/// temp vars
		/// </summary>
		private float[][][] _pcm = new float[1][][];
		private int[] _index;

		/// <summary>
		/// End of stream
		/// </summary>
		private bool EndOfStream = false;

		/// <summary>
		/// sync and verify incoming physical bitstream
		/// </summary>
		private SyncState SyncState = new SyncState();

		/// <summary>
		/// take physical pages, weld into a logical stream of packets
		/// </summary>
		private StreamState StreamState = new StreamState();

		/// <summary>
		/// one Ogg bitstream page.  Vorbis packets are inside
		/// </summary>
		private Page Page = new Page();

		/// <summary>
		/// one raw packet of data for decode
		/// </summary>
		private Packet Packet = new Packet();

		/// <summary>
		/// struct that stores all the static vorbis bitstream settings
		/// </summary>
		private Info Info = new Info();

		/// <summary>
		/// struct that stores all the bitstream user comments
		/// </summary>
		private Comment Comment = new Comment();

		/// <summary>
		/// central working state for the packet->PCM decoder
		/// </summary>
		private DspState dspState = new DspState();

		/// <summary>
		/// local working space for packet->PCM decode
		/// </summary>
		private Block Block;

		/// <summary>
		/// Conversion buffer size
		/// </summary>
		private static int convsize = 4096 * 2;

		/// <summary>
		/// Conversion buffer
		/// </summary>
		private static byte[] convbuffer = new byte[convsize];

		/// <summary>
		///  where we are in the convbuffer
		/// </summary>
		private int convbufferOff = 0;

		/// <summary>
		/// bytes ready in convbuffer.
		/// </summary>
		private int convbufferSize = 0;

		/// <summary>
		/// A dummy used by read() to read 1 byte.
		/// </summary>
		private byte[] ReadDummy = new byte[1];

		/// <summary>
		/// 
		/// </summary>
		private Stream Stream;


		#endregion

	
	
	}
}
