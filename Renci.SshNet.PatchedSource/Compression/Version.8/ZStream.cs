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

    public interface ICompressor
    {
        ZLibStatus deflate(FlushType flushType);

        ZLibStatus deflate(byte[] bufer, int offset, int count, byte[] p1, int p2, int p3, FlushType flushType);

        ZLibStatus deflateEnd();

        ZLibStatus inflate(byte[] bufer, int offset, int count, byte[] p1, int p2, int p3, FlushType flushType);

        ZLibStatus inflateEnd();

        ZLibStatus inflate(FlushType flushType);

        int avail_out { get; set; }

        int avail_in { get; set; }

        int next_out_index { get; set; }

        byte[] next_out { get; set; }

        byte[] next_in { get; set; }

        int next_in_index { get; set; }
    }


    public abstract class ZStream
    {

        protected const int MAX_WBITS = 15;        // 32K LZ77 window
        protected const int DEF_WBITS = MAX_WBITS;

        private const int MAX_MEM_LEVEL = 9;

        public byte[] next_in { get; set; }     // next input byte
        public int next_in_index { get; set; }
        public int avail_in { get; set; }       // number of bytes available at next_in
        public long total_in { get; set; }      // total nb of input bytes read so far

        public byte[] next_out { get; set; }    // next output byte should be put there
        public int next_out_index { get; set; }
        public int avail_out { get; set; }      // remaining free space at next_out
        public long total_out { get; set; }     // total nb of bytes output so far

        public String msg { get; set; }

        internal BlockType data_type; // best guess about the data type: ascii or binary

        public long adler;

    }
}