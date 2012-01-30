//
//  VorbisFileInstance.cs
//  
//  Author:
//      Caleb Leak (04.05.2011)
//      caleb@embergames.net
//      www.EmberGames.net
//
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;

using csogg;

namespace csvorbis
{
    public class VorbisFileInstance
    {
        // Decoding working state local storage
        long pcm_offset;
        bool decode_ready = false;
        int current_serialno;
        int current_link;

        public float bittrack;
        public float samptrack;

        public StreamState os;
        public DspState vd;
        public Block vb;

        public VorbisFile vorbisFile;

        public long lastStreamPosition;

        public SyncState oy;

        public VorbisFileInstance(VorbisFile sourceFile)
        {
            vorbisFile = sourceFile;

            oy = new SyncState();
            bittrack = 0;
            samptrack = 0;

            os = new StreamState(); // take physical pages, weld into a logical
            // stream of packets
            vd = new DspState(); // central working state for 
            // the packet->PCM decoder
            vb = new Block(vd);     // local working space for packet->PCM decode

            raw_seek(0);
            lastStreamPosition = sourceFile.datasource.Position;
        }

        // fetch and process a packet.  Handles the case where we're at a
        // bitstream boundary and dumps the decoding machine.  If the decoding
        // machine is unloaded, it loads it.  It also keeps pcm_offset up to
        // date (seek and read both use this.  seek uses a special hack with
        // readp). 
        //
        // return: -1) hole in the data (lost packet) 
        //          0) need more date (only if readp==0)/eof
        //          1) got a packet 

        int process_packet(int readp)
        {
            Page og = new Page();

            // handle one packet.  Try to fetch it from current stream state
            // extract packets from page
            while (true)
            {
                // process a packet if we can.  If the machine isn't loaded,
                // neither is a page
                if (decode_ready)
                {
                    Packet op = new Packet();
                    int result = os.packetout(op);
                    long granulepos;
                    // if(result==-1)return(-1); // hole in the data. For now, swallow
                    // and go. We'll need to add a real
                    // error code in a bit.
                    if (result > 0)
                    {
                        // got a packet.  process it
                        granulepos = op.granulepos;
                        if (vb.synthesis(op) == 0)
                        { // lazy check for lazy
                            // header handling.  The
                            // header packets aren't
                            // audio, so if/when we
                            // submit them,
                            // vorbis_synthesis will
                            // reject them
                            // suck in the synthesis data and track bitrate
                            {
                                int oldsamples = vd.synthesis_pcmout(null, null);
                                vd.synthesis_blockin(vb);
                                samptrack += vd.synthesis_pcmout(null, null) - oldsamples;
                                bittrack += op.bytes * 8;
                            }

                            // update the pcm vorbisFile.offset.
                            if (granulepos != -1 && op.e_o_s == 0)
                            {
                                int link = (vorbisFile.skable ? current_link : 0);
                                int samples;
                                // this packet has a pcm_offset on it (the last packet
                                // completed on a page carries the vorbisFile.offset) After processing
                                // (above), we know the pcm position of the *last* sample
                                // ready to be returned. Find the vorbisFile.offset of the *first*
                                // 
                                // As an aside, this trick is inaccurate if we begin
                                // reading anew right at the last page; the end-of-stream
                                // granulepos declares the last frame in the stream, and the
                                // last packet of the last page may be a partial frame.
                                // So, we need a previous granulepos from an in-sequence page
                                // to have a reference point.  Thus the !op.e_o_s clause above

                                samples = vd.synthesis_pcmout(null, null);
                                granulepos -= samples;
                                for (int i = 0; i < link; i++)
                                {
                                    granulepos += vorbisFile.pcmlengths[i];
                                }
                                pcm_offset = granulepos;
                            }
                            return (1);
                        }
                    }
                }

                if (readp == 0) return (0);
                if (vorbisFile.get_next_page(og, -1, this) < 0) return (0); // eof. leave unitialized

                // bitrate tracking; add the header's bytes here, the body bytes
                // are done by packet above
                bittrack += og.header_len * 8;

                // has our decoding just traversed a bitstream boundary?
                if (decode_ready)
                {
                    if (current_serialno != og.serialno())
                    {
                        decode_clear();
                    }
                }

                // Do we need to load a new machine before submitting the page?
                // This is different in the seekable and non-seekable cases.  
                // 
                // In the seekable case, we already have all the header
                // information loaded and cached; we just initialize the machine
                // with it and continue on our merry way.
                // 
                // In the non-seekable (streaming) case, we'll only be at a
                // boundary if we just left the previous logical bitstream and
                // we're now nominally at the header of the next bitstream

                if (!decode_ready)
                {
                    int i;
                    if (vorbisFile.skable)
                    {
                        current_serialno = og.serialno();

                        // match the serialno to bitstream section.  We use this rather than
                        // vorbisFile.offset positions to avoid problems near logical bitstream
                        // boundaries
                        for (i = 0; i < vorbisFile.links; i++)
                        {
                            if (vorbisFile.serialnos[i] == current_serialno) break;
                        }
                        if (i == vorbisFile.links) return (-1); // sign of a bogus stream.  error out,
                        // leave machine uninitialized
                        current_link = i;

                        os.init(current_serialno);
                        os.reset();

                    }
                    else
                    {
                        // we're streaming
                        // fetch the three header packets, build the info struct
                        int[] foo = new int[1];
                        int ret = vorbisFile.fetch_headers(vorbisFile.vi[0], vorbisFile.vc[0], foo, og, this);
                        current_serialno = foo[0];
                        if (ret != 0) return ret;
                        current_link++;
                        i = 0;
                    }
                    make_decode_ready();
                }
                os.pagein(og);
            }
        }

        // returns the actual bitrate since last call.  returns -1 if no
        // additional data to offer since last call (or at beginning of stream)
        public int bitrate_instant()
        {
            int _link = (vorbisFile.skable ? current_link : 0);
            if (samptrack == 0) return (-1);
            int ret = (int)(bittrack / samptrack * vorbisFile.vi[_link].rate + .5);
            bittrack = 0.0f;
            samptrack = 0.0f;
            return (ret);
        }

        public int serialnumber(int i)
        {
            if (i >= vorbisFile.links) return (-1);
            if (!vorbisFile.skable && i >= 0) return (serialnumber(-1));
            if (i < 0)
            {
                return (current_serialno);
            }
            else
            {
                return (vorbisFile.serialnos[i]);
            }
        }

        int make_decode_ready()
        {
#if NET_2_1
			if(decode_ready) throw new Exception ("make_decode_ready: 1");
#else
            if (decode_ready) Environment.Exit(1);
#endif
            vd.synthesis_init(vorbisFile.vi[0]);
            vb.init(vd);
            decode_ready = true;
            return (0);
        }

        // clear out the current logical bitstream decoder
        void decode_clear()
        {
            os.clear();
            vd.clear();
            vb.clear();
            decode_ready = false;
            bittrack = 0.0f;
            samptrack = 0.0f;
        }

        // seek to an vorbisFile.offset relative to the *compressed* data. This also
        // immediately sucks in and decodes pages to update the PCM cursor. It
        // will cross a logical bitstream boundary, but only if it can't get
        // any packets out of the tail of the bitstream we seek to (so no
        // surprises). 
        // 
        // returns zero on success, nonzero on failure

        public int raw_seek(int pos)
        {
            if (!vorbisFile.skable) return (-1); // don't dump machine if we can't seek
            if (pos < 0 || pos > vorbisFile.offsets[vorbisFile.links])
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }

            // clear out decoding machine state
            pcm_offset = -1;
            decode_clear();

            // seek
            vorbisFile.seek_helper(pos, this);

            // we need to make sure the pcm_offset is set.  We use the
            // _fetch_packet helper to process one packet with readp set, then
            // call it until it returns '0' with readp not set (the last packet
            // from a page has the 'granulepos' field set, and that's how the
            // helper updates the vorbisFile.offset

            switch (process_packet(1))
            {
                case 0:
                    // oh, eof. There are no packets remaining.  Set the pcm vorbisFile.offset to
                    // the end of file
                    pcm_offset = vorbisFile.pcm_total(-1);
                    return (0);
                case -1:
                    // error! missing data or invalid bitstream structure
                    //goto seek_error;
                    pcm_offset = -1;
                    decode_clear();
                    return -1;
                default:
                    // all OK
                    break;
            }
            while (true)
            {
                switch (process_packet(0))
                {
                    case 0:
                        // the vorbisFile.offset is set.  If it's a bogus bitstream with no vorbisFile.offset
                        // information, it's not but that's not our fault.  We still run
                        // gracefully, we're just missing the vorbisFile.offset
                        return (0);
                    case -1:
                        // error! missing data or invalid bitstream structure
                        //goto seek_error;
                        pcm_offset = -1;
                        decode_clear();
                        return -1;
                    default:
                        // continue processing packets
                        break;
                }
            }

            // seek_error:
            // dump the machine so we're in a known state
            //pcm_offset=-1;
            //decode_clear();
            return -1;
        }

        // seek to a sample vorbisFile.offset relative to the decompressed pcm stream 
        // returns zero on success, nonzero on failure

        public int pcm_seek(long pos)
        {
            int link = -1;
            long total = vorbisFile.pcm_total(-1);

            if (!vorbisFile.skable) return (-1); // don't dump machine if we can't seek
            if (pos < 0 || pos > total)
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }

            // which bitstream section does this pcm vorbisFile.offset occur in?
            for (link = vorbisFile.links - 1; link >= 0; link--)
            {
                total -= vorbisFile.pcmlengths[link];
                if (pos >= total) break;
            }

            // search within the logical bitstream for the page with the highest
            // pcm_pos preceeding (or equal to) pos.  There is a danger here;
            // missing pages or incorrect frame number information in the
            // bitstream could make our task impossible.  Account for that (it
            // would be an error condition)
            {
                long target = pos - total;
                long end = vorbisFile.offsets[link + 1];
                long begin = vorbisFile.offsets[link];
                int best = (int)begin;

                Page og = new Page();
                while (begin < end)
                {
                    long bisect;
                    int ret;

                    if (end - begin < VorbisFile.CHUNKSIZE)
                    {
                        bisect = begin;
                    }
                    else
                    {
                        bisect = (end + begin) / 2;
                    }

                    vorbisFile.seek_helper(bisect, this);
                    ret = vorbisFile.get_next_page(og, end - bisect, this);

                    if (ret == -1)
                    {
                        end = bisect;
                    }
                    else
                    {
                        long granulepos = og.granulepos();
                        if (granulepos < target)
                        {
                            best = ret;  // raw vorbisFile.offset of packet with granulepos
                            begin = vorbisFile.offset; // raw vorbisFile.offset of next packet
                        }
                        else
                        {
                            end = bisect;
                        }
                    }
                }
                // found our page. seek to it (call raw_seek).
                if (raw_seek(best) != 0)
                {
                    //goto seek_error;
                    pcm_offset = -1;
                    decode_clear();
                    return -1;
                }
            }

            // verify result
            if (pcm_offset >= pos)
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }
            if (pos > vorbisFile.pcm_total(-1))
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }

            // discard samples until we reach the desired position. Crossing a
            // logical bitstream boundary with abandon is OK.
            while (pcm_offset < pos)
            {
                float[][] pcm;
                int target = (int)(pos - pcm_offset);
                float[][][] _pcm = new float[1][][];
                int[] _index = new int[getInfo(-1).channels];
                int samples = vd.synthesis_pcmout(_pcm, _index);
                pcm = _pcm[0];

                if (samples > target) samples = target;
                vd.synthesis_read(samples);
                pcm_offset += samples;

                if (samples < target)
                    if (process_packet(1) == 0)
                    {
                        pcm_offset = vorbisFile.pcm_total(-1); // eof
                    }
            }
            return 0;

            // seek_error:
            // dump machine so we're in a known state
            //pcm_offset=-1;
            //decode_clear();
            //return -1;
        }

        // seek to a playback time relative to the decompressed pcm stream 
        // returns zero on success, nonzero on failure
        public int time_seek(float seconds)
        {
            // translate time to PCM position and call pcm_seek

            int link = -1;
            long pcm_tot = vorbisFile.pcm_total(-1);
            float time_tot = vorbisFile.time_total(-1);

            if (!vorbisFile.skable) return (-1); // don't dump machine if we can't seek
            if (seconds < 0 || seconds > time_tot)
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }

            // which bitstream section does this time vorbisFile.offset occur in?
            for (link = vorbisFile.links - 1; link >= 0; link--)
            {
                pcm_tot -= vorbisFile.pcmlengths[link];
                time_tot -= vorbisFile.time_total(link);
                if (seconds >= time_tot) break;
            }

            // enough information to convert time vorbisFile.offset to pcm vorbisFile.offset
            {
                long target = (long)(pcm_tot + (seconds - time_tot) * vorbisFile.vi[link].rate);
                return (pcm_seek(target));
            }
        }

        // return PCM vorbisFile.offset (sample) of next PCM sample to be read
        public long pcm_tell()
        {
            return (pcm_offset);
        }

        // return time vorbisFile.offset (seconds) of next PCM sample to be read
        public float time_tell()
        {
            // translate time to PCM position and call pcm_seek

            int link = -1;
            long pcm_tot = 0;
            float time_tot = 0.0f;

            if (vorbisFile.skable)
            {
                pcm_tot = vorbisFile.pcm_total(-1);
                time_tot = vorbisFile.time_total(-1);

                // which bitstream section does this time vorbisFile.offset occur in?
                for (link = vorbisFile.links - 1; link >= 0; link--)
                {
                    pcm_tot -= vorbisFile.pcmlengths[link];
                    time_tot -= vorbisFile.time_total(link);
                    if (pcm_offset >= pcm_tot) break;
                }
            }

            return ((float)time_tot + (float)(pcm_offset - pcm_tot) / vorbisFile.vi[link].rate);
        }

        //  link:   -1) return the vorbis_info struct for the bitstream section
        //              currently being decoded
        //         0-n) to request information for a specific bitstream section
        //
        // In the case of a non-seekable bitstream, any call returns the
        // current bitstream.  NULL in the case that the machine is not
        // initialized

        public Info getInfo(int link)
        {
            if (vorbisFile.skable)
            {
                if (link < 0)
                {
                    if (decode_ready)
                    {
                        return vorbisFile.vi[current_link];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (link >= vorbisFile.links)
                    {
                        return null;
                    }
                    else
                    {
                        return vorbisFile.vi[link];
                    }
                }
            }
            else
            {
                if (decode_ready)
                {
                    return vorbisFile.vi[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public Comment getComment(int link)
        {
            if (vorbisFile.skable)
            {
                if (link < 0)
                {
                    if (decode_ready) { return vorbisFile.vc[current_link]; }
                    else { return null; }
                }
                else
                {
                    if (link >= vorbisFile.links) { return null; }
                    else { return vorbisFile.vc[link]; }
                }
            }
            else
            {
                if (decode_ready) { return vorbisFile.vc[0]; }
                else { return null; }
            }
        }

        // up to this point, everything could more or less hide the multiple
        // logical bitstream nature of chaining from the toplevel application
        // if the toplevel application didn't particularly care.  However, at
        // the point that we actually read audio back, the multiple-section
        // nature must surface: Multiple bitstream sections do not necessarily
        // have to have the same number of channels or sampling rate.
        // 
        // read returns the sequential logical bitstream number currently
        // being decoded along with the PCM data in order that the toplevel
        // application can take action on channel/sample rate changes.  This
        // number will be incremented even for streamed (non-seekable) streams
        // (for seekable streams, it represents the actual logical bitstream
        // index within the physical bitstream.  Note that the accessor
        // functions above are aware of this dichotomy).
        //
        // input values: buffer) a buffer to hold packed PCM data for return
        //               length) the byte length requested to be placed into buffer
        //               bigendianp) should the data be packed LSB first (0) or
        //                           MSB first (1)
        //               word) word size for output.  currently 1 (byte) or 
        //                     2 (16 bit short)
        // 
        // return values: -1) error/hole in data
        //                 0) EOF
        //                 n) number of bytes of PCM actually returned.  The
        //                    below works on a packet-by-packet basis, so the
        //                    return length is not related to the 'length' passed
        //                    in, just guaranteed to fit.
        // 
        // *section) set to the logical bitstream number

        public int read(byte[] buffer, int length,
            int bigendianp, int word, int sgned, int[] bitstream)
        {
            vorbisFile.datasource.Position = lastStreamPosition;
            //vorbisFile.oy = oy;

            int host_endian = vorbisFile.host_is_big_endian();
            int index = 0;

            while (true)
            {
                if (decode_ready)
                {
                    float[][] pcm;
                    float[][][] _pcm = new float[1][][];
                    int[] _index = new int[getInfo(-1).channels];
                    int samples = vd.synthesis_pcmout(_pcm, _index);
                    pcm = _pcm[0];
                    if (samples != 0)
                    {
                        // yay! proceed to pack data into the byte buffer
                        int channels = getInfo(-1).channels;
                        int bytespersample = word * channels;
                        if (samples > length / bytespersample) samples = length / bytespersample;

                        // a tight loop to pack each size
                        {
                            int val;
                            if (word == 1)
                            {
                                int off = (sgned != 0 ? 0 : 128);
                                for (int j = 0; j < samples; j++)
                                {
                                    for (int i = 0; i < channels; i++)
                                    {
                                        val = (int)(pcm[i][_index[i] + j] * 128.0 + 0.5);
                                        if (val > 127) val = 127;
                                        else if (val < -128) val = -128;
                                        buffer[index++] = (byte)(val + off);
                                    }
                                }
                            }
                            else
                            {
                                int off = (sgned != 0 ? 0 : 32768);

                                if (host_endian == bigendianp)
                                {
                                    if (sgned != 0)
                                    {
                                        for (int i = 0; i < channels; i++)
                                        { // It's faster in this order
                                            int src = _index[i];
                                            int dest = i * 2;
                                            for (int j = 0; j < samples; j++)
                                            {
                                                val = (int)(pcm[i][src + j] * 32767.0);
                                                if (val > 32767) val = 32767;
                                                else if (val < -32768) val = -32768;
                                                buffer[dest] = (byte)(val);
                                                buffer[dest + 1] = (byte)((uint)val >> 8);
                                                dest += bytespersample;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < channels; i++)
                                        {
                                            float[] src = pcm[i];
                                            int dest = i;
                                            for (int j = 0; j < samples; j++)
                                            {
                                                val = (int)(src[j] * 32768.0 + 0.5);
                                                if (val > 32767) val = 32767;
                                                else if (val < -32768) val = -32768;
                                                buffer[dest] = (byte)((uint)(val + off) >> 8);
                                                buffer[dest + 1] = (byte)(val + off);
                                                dest += channels * 2;
                                            }
                                        }
                                    }
                                }
                                else if (bigendianp != 0)
                                {
                                    for (int j = 0; j < samples; j++)
                                    {
                                        for (int i = 0; i < channels; i++)
                                        {
                                            val = (int)(pcm[i][j] * 32768.0 + 0.5);
                                            if (val > 32767) val = 32767;
                                            else if (val < -32768) val = -32768;
                                            val += off;
                                            buffer[index++] = (byte)((uint)val >> 8);
                                            buffer[index++] = (byte)val;
                                        }
                                    }
                                }
                                else
                                {
                                    //int val;
                                    for (int j = 0; j < samples; j++)
                                    {
                                        for (int i = 0; i < channels; i++)
                                        {
                                            val = (int)(pcm[i][j] * 32768.0 + 0.5);
                                            if (val > 32767) val = 32767;
                                            else if (val < -32768) val = -32768;
                                            val += off;
                                            buffer[index++] = (byte)val;
                                            buffer[index++] = (byte)((uint)val >> 8);
                                        }
                                    }
                                }
                            }
                        }

                        vd.synthesis_read(samples);
                        pcm_offset += samples;
                        if (bitstream != null) bitstream[0] = current_link;
                        lastStreamPosition = vorbisFile.datasource.Position;
                        return (samples * bytespersample);
                    }
                }

                // suck in another packet
                switch (process_packet(1))
                {
                    case 0:
                        lastStreamPosition = vorbisFile.datasource.Position;
                        return (0);
                    case -1:
                        lastStreamPosition = vorbisFile.datasource.Position;
                        return -1;
                    default:
                        break;
                }
            }
            lastStreamPosition = vorbisFile.datasource.Position;
            return -1;
        }
    }
}
