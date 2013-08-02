using System;
using System.IO;
/*
 * $Id: ZStream.cs,v 1.1 2006-07-31 13:59:26 bouncy Exp $
 *
Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

  1. Redistributions of source code must retain the above copyright notice,
     this list of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright 
     notice, this list of conditions and the following disclaimer in 
     the documentation and/or other materials provided with the distribution.

  3. The names of the authors may not be used to endorse or promote products
     derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/*
 * This program is based on zlib-1.1.3, so all credit should go authors
 * Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
 * and contributors of zlib.
 */

namespace Org.BouncyCastle.Utilities.Zlib
{

    public abstract class ZStream : Stream
    {
        private const int MAX_WBITS = 15;        // 32K LZ77 window
        private const int DEF_WBITS = MAX_WBITS;
        private const int MAX_MEM_LEVEL = 9;

        public byte[] next_in;     // next input byte
        public int next_in_index;
        public int avail_in;       // number of bytes available at next_in
        public long total_in;      // total nb of input bytes read so far

        public byte[] next_out;    // next output byte should be put there
        public int next_out_index;
        public int avail_out;      // remaining free space at next_out
        public long total_out;     // total nb of bytes output so far

        public String msg;

        internal Deflate dstate;
        internal Inflate istate;

        //internal int data_type; // best guess about the data type: ascii or binary

        //  TODO:   Make setter private
        public long Adler { get; set; }
        //internal Adler32 _adler = new Adler32();

        // largest prime smaller than 65536
        private const int ADLER32_BASE = 65521;
        // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
        private const int ADLER32_NMAX = 5552;

        internal long Adler32(long adler, byte[] buf, int index, int len)
        {
            if (buf == null) { return 1L; }

            long s1 = adler & 0xffff;
            long s2 = (adler >> 16) & 0xffff;
            int k;

            while (len > 0)
            {
                k = len < ADLER32_NMAX ? len : ADLER32_NMAX;
                len -= k;
                while (k >= 16)
                {
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    s1 += buf[index++] & 0xff; s2 += s1;
                    k -= 16;
                }
                if (k != 0)
                {
                    do
                    {
                        s1 += buf[index++] & 0xff; s2 += s1;
                    }
                    while (--k != 0);
                }
                s1 %= ADLER32_BASE;
                s2 %= ADLER32_BASE;
            }
            return (s2 << 16) | s1;
        }

        internal void UpdateAdler(long adler, byte[] buf, int index, int len)
        {
            this.Adler = this.Adler32(adler, buf, index, len);
        }
        internal void UpdateAdler(byte[] buf, int index, int len)
        {
            this.Adler = this.Adler32(this.Adler, buf, index, len);
        }


        protected ZLibStatus inflateInit()
        {
            return inflateInit(DEF_WBITS);
        }
        protected ZLibStatus inflateInit(bool nowrap)
        {
            return inflateInit(DEF_WBITS, nowrap);
        }
        protected ZLibStatus inflateInit(int w)
        {
            return inflateInit(w, false);
        }
        protected ZLibStatus inflateInit(int w, bool nowrap)
        {
            istate = new Inflate();
            return istate.InflateInit(this, nowrap ? -w : w);
        }
        protected ZLibStatus inflate(FlushType f)
        {
            if (istate == null) return ZLibStatus.Z_STREAM_ERROR;
            return istate.inflate(this, f);
        }
        protected ZLibStatus inflateEnd()
        {
            if (istate == null) return ZLibStatus.Z_STREAM_ERROR;
            var ret = istate.InflateEnd(this);
            istate = null;
            return ret;
        }
        protected ZLibStatus inflateSync()
        {
            if (istate == null)
                return ZLibStatus.Z_STREAM_ERROR;
            return istate.InflateSync(this);
        }
        //protected ZLibStatus inflateSetDictionary(byte[] dictionary, int dictLength)
        //{
        //    if (istate == null)
        //        return ZLibStatus.Z_STREAM_ERROR;
        //    return istate.InflateSetDictionary(this, dictionary, dictLength);
        //}

        protected ZLibStatus deflateInit(CompressionLevel level)
        {
            return deflateInit(level, MAX_WBITS);
        }
        protected ZLibStatus deflateInit(CompressionLevel level, bool nowrap)
        {
            return deflateInit(level, MAX_WBITS, nowrap);
        }
        protected ZLibStatus deflateInit(CompressionLevel level, int bits)
        {
            return deflateInit(level, bits, false);
        }
        protected ZLibStatus deflateInit(CompressionLevel level, int bits, bool nowrap)
        {
            dstate = new Deflate();
            return dstate.deflateInit(this, level, nowrap ? -bits : bits);
        }
        public ZLibStatus deflate(FlushType flush)
        {
            if (dstate == null)
            {
                return ZLibStatus.Z_STREAM_ERROR;
            }
            return dstate.deflate(this, flush);
        }
        protected ZLibStatus deflateEnd()
        {
            if (dstate == null) return ZLibStatus.Z_STREAM_ERROR;
            var ret = dstate.deflateEnd();
            dstate = null;
            return ret;
        }
        protected ZLibStatus deflateParams(CompressionLevel level, CompressionStrategy strategy)
        {
            if (dstate == null) return ZLibStatus.Z_STREAM_ERROR;
            return dstate.deflateParams(this, level, strategy);
        }
        protected ZLibStatus deflateSetDictionary(byte[] dictionary, int dictLength)
        {
            if (dstate == null)
                return ZLibStatus.Z_STREAM_ERROR;
            return dstate.deflateSetDictionary(this, dictionary, dictLength);
        }

        // Flush as much pending output as possible. All deflate() output goes
        // through this function so some applications may wish to modify it
        // to avoid allocating a large strm->next_out buffer and copying into it.
        // (See also read_buf()).
        internal void flush_pending()
        {
            int len = dstate.pending;

            if (len > avail_out) len = avail_out;
            if (len == 0) return;

            if (dstate.pending_buf.Length <= dstate.pending_out ||
                next_out.Length <= next_out_index ||
                dstate.pending_buf.Length < (dstate.pending_out + len) ||
                next_out.Length < (next_out_index + len))
            {
                //      System.out.println(dstate.pending_buf.length+", "+dstate.pending_out+
                //			 ", "+next_out.length+", "+next_out_index+", "+len);
                //      System.out.println("avail_out="+avail_out);
            }

            System.Array.Copy(dstate.pending_buf, dstate.pending_out,
                next_out, next_out_index, len);

            next_out_index += len;
            dstate.pending_out += len;
            total_out += len;
            avail_out -= len;
            dstate.pending -= len;
            if (dstate.pending == 0)
            {
                dstate.pending_out = 0;
            }
        }

        // Read a new buffer from the current input stream, update the adler32
        // and total number of bytes read.  All deflate() input goes through
        // this function so some applications may wish to modify it to avoid
        // allocating a large strm->next_in buffer and copying from it.
        // (See also flush_pending()).
        internal int read_buf(byte[] buf, int start, int size)
        {
            int len = avail_in;

            if (len > size) len = size;
            if (len == 0) return 0;

            avail_in -= len;

            if (dstate.noheader == 0)
            {
                Adler = this.Adler32(Adler, next_in, next_in_index, len);
            }
            System.Array.Copy(next_in, next_in_index, buf, start, len);
            next_in_index += len;
            total_in += len;
            return len;
        }
    }
}