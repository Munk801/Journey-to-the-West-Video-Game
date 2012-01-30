/* csvorbis
 * Copyright (C) 2000 ymnk, JCraft,Inc.
 *  
 * Written by: 2000 ymnk<ymnk@jcraft.com>
 * Ported to C# from JOrbis by: Mark Crichton <crichton@gimp.org> 
 *   
 * Thanks go to the JOrbis team, for licencing the code under the
 * LGPL, making my job a lot easier.
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public License
 * as published by the Free Software Foundation; either version 2 of
 * the License, or (at your option) any later version.
   
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Library General Public License for more details.
 * 
 * You should have received a copy of the GNU Library General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

// Caleb Leak: Many changes.  Pushed as many state variables as I could recognize into VorbisFileInstance so that multiple instances can be
// instantiated without the overhead of extra streams, copies of the headers, etc.  Check the DragonOgg svn repository to revert.

// Alteration of FileShare mode on line 97 by Matthew Harris for use as part of the DragonOgg library. Remove the following to revert:
// , FileShare.ReadWrite
// Be aware that this does introduce potential difficulties with the file being written between writing operations, but is needed to allow
// the DragonOgg library to work properly w.r.t writing tags.
// Also added IDisposable implementation to properly close down the filestream


using System;
using System.IO;
using csogg;

namespace csvorbis 
{
	public class VorbisFile : IDisposable 
	{
		public static int CHUNKSIZE=8500;
		public static int SEEK_SET=0;
		public static int SEEK_CUR=1;
		public static int SEEK_END=2;

		public static int OV_FALSE=-1;
		public static int OV_EOF=-2;
		public static int OV_HOLE=-3;

		public static int OV_EREAD=-128;
		public static int OV_EFAULT=-129;
		public static int OV_EIMPL=-130;
		public static int OV_EINVAL=-131;
		public static int OV_ENOTVORBIS=-132;
		public static int OV_EBADHEADER=-133;
		public static int OV_EVERSION=-134;
		public static int OV_ENOTAUDIO=-135;
		public static int OV_EBADPACKET=-136;
		public static int OV_EBADLINK=-137;
		public static int OV_ENOSEEK=-138;

		//FileStream datasource;
        public Stream datasource;
        public bool skable = false;
        public long offset;
        public long end;

        public int links;
        public long[] offsets;
        public long[] dataoffsets;
        public int[] serialnos;
        public long[] pcmlengths;
        public Info[] vi;
        public Comment[] vc;

		// Decoding working state local storage


		private VorbisFile()
		{
		}

		public VorbisFile(String file) : this()
		{
			FileStream inst=null;
			
			try{ inst=new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);}
			catch(Exception e)
			{
				throw new csorbisException("VorbisFile: "+e.Message);
			}
			int ret=open(inst, null, 0);
			if(ret==-1)
			{
				throw new csorbisException("VorbisFile: open return -1");
			}
		}

        public VorbisFile(Stream inst)
            : this()
        {
            int ret = open(inst, null, 0);
        }

		public VorbisFile(Stream inst, byte[] initial, int ibytes) : this()
		{
			int ret=open(inst, initial, ibytes);
		}

		public int get_data(VorbisFileInstance instance)
		{
			int index=instance.oy.buffer(CHUNKSIZE);
            byte[] buffer = instance.oy.data;
			//  int bytes=callbacks.read_func(buffer, index, 1, CHUNKSIZE, datasource);
			int bytes=0;
			try
			{
				bytes=datasource.Read(buffer, index, CHUNKSIZE);
			}
			catch(Exception e)
			{
				Console.Error.WriteLine(e.Message);
				return OV_EREAD;
			}
            instance.oy.wrote(bytes);
			if(bytes==-1)
			{
				bytes=0;
			}
			return bytes;
		}

		public void seek_helper(long offst, VorbisFileInstance instance)
		{
			//callbacks.seek_func(datasource, offst, SEEK_SET);
			fseek(datasource, offst, SEEK_SET);
			this.offset=offst;
			instance.oy.reset();
		}

		public int get_next_page(Page page, long boundary, VorbisFileInstance instance)
		{
			if(boundary>0) boundary+=offset;
			while(true)
			{
				int more;
				if(boundary>0 && offset>=boundary)return OV_FALSE;
                more = instance.oy.pageseek(page);
				if(more<0){offset-=more;}
				else
				{
					if(more==0)
					{
						if(boundary==0)return OV_FALSE;
						//	  if(get_data()<=0)return -1;
						int ret=get_data(instance);
						if(ret==0) return OV_EOF;
						if(ret<0) return OV_EREAD; 
					}
					else
					{
						int ret=(int)offset; //!!!
						offset+=more;
						return ret;
					}
				}
			}
		}

        private int get_prev_page(Page page, VorbisFileInstance instance)
		{
			long begin=offset; //!!!
			int ret;
			int offst=-1;
			while(offst==-1)
			{
				begin-=CHUNKSIZE;
				if(begin<0)
					begin=0;
				seek_helper(begin, instance);
				while(offset<begin+CHUNKSIZE)
				{
					ret=get_next_page(page, begin+CHUNKSIZE-offset, instance);
					if(ret==OV_EREAD){ return OV_EREAD; }
					if(ret<0){ break; }
					else{ offst=ret; }
				}
			}
            seek_helper(offst, instance); //!!!
			ret=get_next_page(page, CHUNKSIZE, instance);
			if(ret<0)
			{
				//System.err.println("Missed page fencepost at end of logical bitstream Exiting");
				//System.exit(1);
				return OV_EFAULT;
			}
			return offst;
		}

		public int bisect_forward_serialno(long begin, long searched, long end, int currentno, int m, VorbisFileInstance instance)
		{
			long endsearched=end;
			long next=end;
			Page page=new Page();
			int ret;

			while(searched<endsearched)
			{
				long bisect;
				if(endsearched-searched<CHUNKSIZE)
				{
					bisect=searched;
				}
				else
				{
					bisect=(searched+endsearched)/2;
				}

                seek_helper(bisect, instance);
                ret = get_next_page(page, -1, instance);
				if(ret==OV_EREAD) return OV_EREAD;
				if(ret<0 || page.serialno()!=currentno)
				{
					endsearched=bisect;
					if(ret>=0)next=ret;
				}
				else
				{
					searched=ret+page.header_len+page.body_len;
				}
			}
            seek_helper(next, instance);
            ret = get_next_page(page, -1, instance);
			if(ret==OV_EREAD) return OV_EREAD;

			if(searched>=end || ret==-1)
			{
				links=m+1;
				offsets=new long[m+2];
				offsets[m+1]=searched;
			}
			else
			{
				ret=bisect_forward_serialno(next, offset, end, page.serialno(), m+1, instance);
				if(ret==OV_EREAD)return OV_EREAD;
			}
			offsets[m]=begin;
			return 0;  
		}

		// uses the local ogg_stream storage in vf; this is important for
		// non-streaming input sources
        public int fetch_headers(Info vi, Comment vc, int[] serialno, Page og_ptr, VorbisFileInstance instance)
		{
			//System.err.println("fetch_headers");
			Page og=new Page();
			Packet op=new Packet();
			int ret;

			if(og_ptr==null)
			{
                ret = get_next_page(og, CHUNKSIZE, instance);
				if(ret==OV_EREAD)return OV_EREAD;
				if(ret<0) return OV_ENOTVORBIS;
				og_ptr=og;
			}
  
			if(serialno!=null)serialno[0]=og_ptr.serialno();

			instance.os.init(og_ptr.serialno());
  
			// extract the initial header from the first page and verify that the
			// Ogg bitstream is in fact Vorbis data
  
			vi.init();
			vc.init();
  
			int i=0;
			while(i<3)
			{
                instance.os.pagein(og_ptr);
				while(i<3)
				{
                    int result = instance.os.packetout(op);
					if(result==0)break;
					if(result==-1)
					{
						Console.Error.WriteLine("Corrupt header in logical bitstream.");
						//goto bail_header;
						vi.clear();
						vc.clear();
                        instance.os.clear();
						return -1;
					}
					if(vi.synthesis_headerin(vc, op)!=0)
					{
						Console.Error.WriteLine("Illegal header in logical bitstream.");
						//goto bail_header;
						vi.clear();
						vc.clear();
                        instance.os.clear();
						return -1;
					}
					i++;
				}
				if(i<3)
					if(get_next_page(og_ptr, 1, instance)<0)
					{
						Console.Error.WriteLine("Missing header in logical bitstream.");
						//goto bail_header;
						vi.clear();
						vc.clear();
                        instance.os.clear();
						return -1;
					}
			}
			return 0; 
		}

		// last step of the OggVorbis_File initialization; get all the
		// vorbis_info structs and PCM positions.  Only called by the seekable
		// initialization (local stream storage is hacked slightly; pay
		// attention to how that's done)
		public void prefetch_all_headers(Info first_i,Comment first_c, int dataoffset,
            VorbisFileInstance instance)
		{
			Page og=new Page();
			int ret;
  
			vi=new Info[links];
			vc=new Comment[links];
			dataoffsets=new long[links];
			pcmlengths=new long[links];
			serialnos=new int[links];
  
			for(int i=0;i<links;i++)
			{
				if(first_i!=null && first_c!=null && i==0)
				{
					// we already grabbed the initial header earlier.  This just
					// saves the waste of grabbing it again
					// !!!!!!!!!!!!!
					vi[i]=first_i;
					//memcpy(vf->vi+i,first_i,sizeof(vorbis_info));
					vc[i]=first_c;
					//memcpy(vf->vc+i,first_c,sizeof(vorbis_comment));
					dataoffsets[i]=dataoffset;
				}
				else
				{
					// seek to the location of the initial header
                    seek_helper(offsets[i], instance); //!!!
                    if (fetch_headers(vi[i], vc[i], null, null, instance) == -1)
					{
						Console.Error.WriteLine("Error opening logical bitstream #"+(i+1)+"\n");
						dataoffsets[i]=-1;
					}
					else
					{
						dataoffsets[i]=offset;
						instance.os.clear();
					}
				}

				// get the serial number and PCM length of this link. To do this,
				// get the last page of the stream
				long end=offsets[i+1]; //!!!
                seek_helper(end, instance);

				while(true)
				{
					ret=get_prev_page(og, instance);
					if(ret==-1)
					{
						// this should not be possible
						Console.Error.WriteLine("Could not find last page of logical "+
							"bitstream #"+(i)+"\n");
						vi[i].clear();
						vc[i].clear();
						break;
					}
					if(og.granulepos()!=-1)
					{
						serialnos[i]=og.serialno();
						pcmlengths[i]=og.granulepos();
						break;
					}
				}
			}
		}

		int open_seekable()
		{
            VorbisFileInstance instance = makeInstance();
			Info initial_i=new Info();
			Comment initial_c=new Comment();
			int serialno;
			long end;
			int ret;
			int dataoffset;
			Page og=new Page();
			// is this even vorbis...?
			int[] foo=new int[1];
            ret = fetch_headers(initial_i, initial_c, foo, null, instance);
			serialno=foo[0];
			dataoffset=(int)offset; //!!
			instance.os.clear();
			if(ret==-1)return(-1);
			// we can seek, so set out learning all about this file
			skable=true;
			//(callbacks.seek_func)(datasource, 0, SEEK_END);
			fseek(datasource, 0, SEEK_END);
			//offset=end=(callbacks.tell_func)(datasource);
			offset=ftell(datasource);
			end=offset;
			// We get the offset for the last page of the physical bitstream.
			// Most OggVorbis files will contain a single logical bitstream
			end=get_prev_page(og, instance);
			// moer than one logical bitstream?
			if(og.serialno()!=serialno)
			{
				// Chained bitstream. Bisect-search each logical bitstream
				// section.  Do so based on serial number only
				if(bisect_forward_serialno(0,0,end+1,serialno,0, instance)<0)
				{
					clear();
					return OV_EREAD;
				}
			}
			else
			{
				// Only one logical bitstream
				if(bisect_forward_serialno(0,end,end+1,serialno,0, instance)<0)
				{
					clear();
					return OV_EREAD;
				}
			}
			prefetch_all_headers(initial_i, initial_c, dataoffset, instance);
			//return(raw_seek(0));
            return 0;
		}

        public VorbisFileInstance makeInstance()
        {
            VorbisFileInstance instance = new VorbisFileInstance(this);
            return instance;
        }

        // CWL: This doesn't appear to be needed for most applications.  It
        // complicates having multiple instances, and so was removed.
		/*int open_nonseekable()
		{
			//System.err.println("open_nonseekable");
			// we cannot seek. Set up a 'single' (current) logical bitstream entry
			links=1;
			vi=new Info[links]; vi[0]=new Info(); // ??
			vc=new Comment[links]; vc[0]=new Comment(); // ?? bug?

			// Try to fetch the headers, maintaining all the storage
			int[]foo=new int[1];
			if(fetch_headers(vi[0], vc[0], foo, null)==-1)return(-1);
			current_serialno=foo[0];
			make_decode_ready();
			return 0;
		}*/

		//The helpers are over; it's all toplevel interface from here on out
		// clear out the OggVorbis_File struct
		int clear()
		{
			/*vb.clear();
			vd.clear();
			os.clear();*/
    
			if(vi!=null && links!=0)
			{
				for(int i=0;i<links;i++)
				{
					vi[i].clear();
					vc[i].clear();
				}
				vi=null;
				vc=null;
			}
			if(dataoffsets!=null)dataoffsets=null;
			if(pcmlengths!=null)pcmlengths=null;
			if(serialnos!=null)serialnos=null;
			if(offsets!=null)offsets=null;
			//oy.clear();
			//if(datasource!=null)(vf->callbacks.close_func)(vf->datasource);
			//memset(vf,0,sizeof(OggVorbis_File));
			return(0);
		}

		public static int fseek(Stream fis,
			//int64_t off,
			long off,
			int whence)
		{
			if(fis.CanSeek == true)
			{
				try
				{
					if(whence==SEEK_SET)
					{
						fis.Seek(off, 0);
					}
					else if(whence==SEEK_END)
					{
						fis.Seek(fis.Length - off, 0);
					}
					else
					{
						Console.Error.WriteLine("seek: "+whence+" is not supported");
					}
				}
				catch(Exception e)
				{
					Console.Error.WriteLine(e.Message);
				}
				return 0;
			}
			try
			{
				if(whence==0){ fis.Seek(0, 0); }
				fis.Seek(off, 0);
			}
			catch(Exception e)
			{
				Console.Error.WriteLine(e.Message);
				return -1;
			}
			return 0;
		}

		static long ftell(Stream fis)
		{
			try
			{
				if(fis.CanSeek == true)
				{
					return (fis.Position);
				}
			}
			catch(Exception e)
			{
				Console.Error.WriteLine(e.Message);
			}
			return 0;
		}

		// inspects the OggVorbis file and finds/documents all the logical
		// bitstreams contained in it.  Tries to be tolerant of logical
		// bitstream sections that are truncated/woogie. 
		//
		// return: -1) error
		//          0) OK

		int open(Stream iis, byte[] initial, int ibytes)
		{
			return open_callbacks(iis, initial, ibytes);
		}

		int open_callbacks(Stream iis, byte[] initial, int ibytes)
		{
			int ret;
			datasource=iis;
			//callbacks = _callbacks;
			// init the framing state
            //instance.oy.init();

			// perhaps some data was previously read into a buffer for testing
			// against other stream types.  Allow initialization from this
			// previously read data (as we may be reading from a non-seekable
			// stream)
			if(initial!=null)
			{
                // CWL: Doesn't seem needed
                /*int index = instance.oy.buffer(ibytes);
                Array.Copy(initial, 0, instance.oy.data, index, ibytes);
                instance.oy.wrote(ibytes);*/
			}
			// can we seek? Stevens suggests the seek test was portable
			if(iis.CanSeek == true)
            { 
                ret=open_seekable(); 
            }
			else
            {
                // CWL: Disabled since they're no longer used
                throw new NotImplementedException("Non-seekable streams are not implemented");
                //ret=open_nonseekable(); 
            }
			if(ret!=0)
			{
				datasource=null;
				clear();
			}
			return(ret);
		}

		// How many logical bitstreams in this physical bitstream?
		public int streams()
		{
			return links;
		}

		// Is the FILE * associated with vf seekable?
		public bool seekable()
		{
			return skable;
		}

		// returns the bitrate for a given logical bitstream or the entire
		// physical bitstream.  If the file is open for random access, it will
		// find the *actual* average bitrate.  If the file is streaming, it
		// returns the nominal bitrate (if set) else the average of the
		// upper/lower bounds (if set) else -1 (unset).
		// 
		// If you want the actual bitrate field settings, get them from the
		// vorbis_info structs

		public int bitrate(int i)
		{
			if(i>=links)return(-1);
			if(!skable && i!=0)return(bitrate(0));
			if(i<0)
			{
				long bits=0;
				for(int j=0;j<links;j++)
				{
					bits+=(offsets[j+1]-dataoffsets[j])*8;
				}
				return((int)Math.Round(bits/time_total(-1)));
			}
			else
			{
				if(skable)
				{
					// return the actual bitrate
					return((int)Math.Round((offsets[i+1]-dataoffsets[i])*8/time_total(i)));
				}
				else
				{
					// return nominal if set
					if(vi[i].bitrate_nominal>0)
					{
						return vi[i].bitrate_nominal;
					}
					else
					{
						if(vi[i].bitrate_upper>0)
						{
							if(vi[i].bitrate_lower>0)
							{
								return (vi[i].bitrate_upper+vi[i].bitrate_lower)/2;
							}
							else
							{
								return vi[i].bitrate_upper;
							}
						}
						return(-1);
					}
				}
			}
		}

		// returns: total raw (compressed) length of content if i==-1
		//          raw (compressed) length of that logical bitstream for i==0 to n
		//          -1 if the stream is not seekable (we can't know the length)

		public long raw_total(int i)
		{
			if(!skable || i>=links)return(-1);
			if(i<0)
			{
				long acc=0;               // bug?
				for(int j=0;j<links;j++)
				{
					acc+=raw_total(j);
				}
				return(acc);
			}
			else
			{
				return(offsets[i+1]-offsets[i]);
			}
		}

		// returns: total PCM length (samples) of content if i==-1
		//          PCM length (samples) of that logical bitstream for i==0 to n
		//          -1 if the stream is not seekable (we can't know the length)
		public long pcm_total(int i)
		{
			if(!skable || i>=links)return(-1);
			if(i<0)
			{
				long acc=0;
				for(int j=0;j<links;j++)
				{
					acc+=pcm_total(j);
				}
				return(acc);
			}
			else
			{
				return(pcmlengths[i]);
			}
		}

		// returns: total seconds of content if i==-1
		//          seconds in that logical bitstream for i==0 to n
		//          -1 if the stream is not seekable (we can't know the length)
		public float time_total(int i)
		{
			if(!skable || i>=links)return(-1);
			if(i<0)
			{
				float acc=0;
				for(int j=0;j<links;j++)
				{
					acc+=time_total(j);
				}
				return(acc);
			}
			else
			{
				return((float)(pcmlengths[i])/vi[i].rate);
			}
		}

		// tell the current stream offset cursor.  Note that seek followed by
		// tell will likely not give the set offset due to caching
		public long raw_tell()
		{
			return(offset);
		}

		public int host_is_big_endian() 
		{
			return 0;
			//the above isn't really right...
		}

		public Info[] getInfo(){return vi;}
		public Comment[] getComment(){return vc;}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			datasource.Close();
			datasource.Dispose();
			datasource = null;
		}
		
		#endregion
	}
}
