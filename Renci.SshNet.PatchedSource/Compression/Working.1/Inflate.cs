using System;
/*
 * $Id: Inflate.cs,v 1.2 2008-05-10 09:35:40 bouncy Exp $
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

    public enum InflateMode : int
    {
        /// <summary>
        ///waiting for method byte
        /// </summary>
        METHOD = 0,
        /// <summary>
        /// waiting for flag byte
        /// </summary>
        FLAG = 1,
        /// <summary>
        /// four dictionary check bytes to go
        /// </summary>
        DICT4 = 2,
        /// <summary>
        /// three dictionary check bytes to go
        /// </summary>
        DICT3 = 3,
        /// <summary>
        /// two dictionary check bytes to go
        /// </summary>
        DICT2 = 4,
        /// <summary>
        /// one dictionary check byte to go
        /// </summary>
        DICT1 = 5,
        /// <summary>
        /// waiting for inflateSetDictionary
        /// </summary>
        DICT0 = 6,
        /// <summary>
        /// decompressing blocks
        /// </summary>
        BLOCKS = 7,
        /// <summary>
        /// four check bytes to go
        /// </summary>
        CHECK4 = 8,
        /// <summary>
        /// three check bytes to go
        /// </summary>
        CHECK3 = 9,
        /// <summary>
        /// two check bytes to go
        /// </summary>
        CHECK2 = 10,
        /// <summary>
        /// one check byte to go
        /// </summary>
        CHECK1 = 11,
        /// <summary>
        /// finished check, done
        /// </summary>
        DONE = 12,
        /// <summary>
        /// got an error--stay here
        /// </summary>
        BAD = 13
    }

    internal sealed class Inflate
    {
        /// <summary>
        /// 32K LZ77 window
        /// </summary>
        private const int MAX_WBITS = 15;

        /// <summary>
        /// Preset dictionary flag in zlib header
        /// </summary>
        private const int PRESET_DICT = 0x20;

        private const int Z_DEFLATED = 8;

        private static readonly byte[] mark = { (byte)0, (byte)0, (byte)0xff, (byte)0xff };

        /// <summary>
        /// if FLAGS, method byte
        /// </summary>
        private int method;

        /// <summary>
        /// Computed check value
        /// </summary>
        private long[] was = new long[1];

        /// <summary>
        /// The stream check value
        /// </summary>
        private long need;

        /// <summary>
        /// if BAD, inflateSync's marker bytes count
        /// </summary>
        private int marker;

        // mode independent information
        /// <summary>
        /// Flag for no wrapper
        /// </summary>
        private int nowrap;
        
        /// <summary>
        /// log2(window size)  (8..15, defaults to 15)
        /// </summary>
        private int wbits;

        /// <summary>
        /// Current inflate_blocks state
        /// </summary>
        private InfBlocks blocks;

        /// <summary>
        /// Gets the current inflate mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        internal InflateMode Mode { get; private set; }

        internal ZLibStatus InflateReset(ZStream z)
        {
            if (z == null || z.istate == null) return ZLibStatus.Z_STREAM_ERROR;

            z.total_in = z.total_out = 0;
            z.msg = null;
            z.istate.Mode = z.istate.nowrap != 0 ? InflateMode.BLOCKS : InflateMode.METHOD;
            z.istate.blocks.Reset(z, null);
            return ZLibStatus.Z_OK;
        }

        internal ZLibStatus InflateEnd(ZStream z)
        {
            if (blocks != null)
                blocks.free(z);
            blocks = null;
            //    ZFREE(z, z->state);
            return ZLibStatus.Z_OK;
        }

        internal ZLibStatus InflateInit(ZStream z, int w)
        {
            z.msg = null;
            blocks = null;

            // handle undocumented nowrap option (no zlib header or check)
            nowrap = 0;
            if (w < 0)
            {
                w = -w;
                nowrap = 1;
            }

            // set window size
            if (w < 8 || w > 15)
            {
                InflateEnd(z);
                return ZLibStatus.Z_STREAM_ERROR;
            }
            wbits = w;

            z.istate.blocks = new InfBlocks(z,
                z.istate.nowrap != 0 ? null : this,
                1 << w);

            // reset state
            InflateReset(z);
            return ZLibStatus.Z_OK;
        }

        internal ZLibStatus inflate(ZStream z, FlushType ff)
        {
            ZLibStatus r;
            int b;

            if (z == null || z.istate == null || z.next_in == null)
                return ZLibStatus.Z_STREAM_ERROR;
            var f = ff == FlushType.Z_FINISH ? ZLibStatus.Z_BUF_ERROR : ZLibStatus.Z_OK;
            r = ZLibStatus.Z_BUF_ERROR;
            while (true)
            {
                //System.out.println("mode: "+z.istate.mode);
                switch (z.istate.Mode)
                {
                    case InflateMode.METHOD:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        if (((z.istate.method = z.next_in[z.next_in_index++]) & 0xf) != Z_DEFLATED)
                        {
                            z.istate.Mode = InflateMode.BAD;
                            z.msg = "unknown compression method";
                            z.istate.marker = 5;       // can't try inflateSync
                            break;
                        }
                        if ((z.istate.method >> 4) + 8 > z.istate.wbits)
                        {
                            z.istate.Mode = InflateMode.BAD;
                            z.msg = "invalid window size";
                            z.istate.marker = 5;       // can't try inflateSync
                            break;
                        }
                        z.istate.Mode = InflateMode.FLAG;
                        goto case InflateMode.FLAG;
                    case InflateMode.FLAG:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        b = (z.next_in[z.next_in_index++]) & 0xff;

                        if ((((z.istate.method << 8) + b) % 31) != 0)
                        {
                            z.istate.Mode = InflateMode.BAD;
                            z.msg = "incorrect header check";
                            z.istate.marker = 5;       // can't try inflateSync
                            break;
                        }

                        if ((b & PRESET_DICT) == 0)
                        {
                            z.istate.Mode = InflateMode.BLOCKS;
                            break;
                        }
                        z.istate.Mode = InflateMode.DICT4;
                        goto case InflateMode.DICT4;
                    case InflateMode.DICT4:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need = ((z.next_in[z.next_in_index++] & 0xff) << 24) & 0xff000000L;
                        z.istate.Mode = InflateMode.DICT3;
                        goto case InflateMode.DICT3;
                    case InflateMode.DICT3:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += ((z.next_in[z.next_in_index++] & 0xff) << 16) & 0xff0000L;
                        z.istate.Mode = InflateMode.DICT2;
                        goto case InflateMode.DICT2;
                    case InflateMode.DICT2:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += ((z.next_in[z.next_in_index++] & 0xff) << 8) & 0xff00L;
                        z.istate.Mode = InflateMode.DICT1;
                        goto case InflateMode.DICT1;
                    case InflateMode.DICT1:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += (z.next_in[z.next_in_index++] & 0xffL);
                        z.Adler = z.istate.need;
                        z.istate.Mode = InflateMode.DICT0;
                        return ZLibStatus.Z_NEED_DICT;
                    case InflateMode.DICT0:
                        z.istate.Mode = InflateMode.BAD;
                        z.msg = "need dictionary";
                        z.istate.marker = 0;       // can try inflateSync
                        return ZLibStatus.Z_STREAM_ERROR;
                    case InflateMode.BLOCKS:

                        r = z.istate.blocks.ProcessBlocks(z, r);
                        if (r == ZLibStatus.Z_DATA_ERROR)
                        {
                            z.istate.Mode = InflateMode.BAD;
                            z.istate.marker = 0;     // can try inflateSync
                            break;
                        }
                        if (r == ZLibStatus.Z_OK)
                        {
                            r = f;
                        }
                        if (r != ZLibStatus.Z_STREAM_END)
                        {
                            return r;
                        }
                        r = f;
                        z.istate.blocks.Reset(z, z.istate.was);
                        if (z.istate.nowrap != 0)
                        {
                            z.istate.Mode = InflateMode.DONE;
                            break;
                        }
                        z.istate.Mode = InflateMode.CHECK4;
                        goto case InflateMode.CHECK4;
                    case InflateMode.CHECK4:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need = ((z.next_in[z.next_in_index++] & 0xff) << 24) & 0xff000000L;
                        z.istate.Mode = InflateMode.CHECK3;
                        goto case InflateMode.CHECK3;
                    case InflateMode.CHECK3:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += ((z.next_in[z.next_in_index++] & 0xff) << 16) & 0xff0000L;
                        z.istate.Mode = InflateMode.CHECK2;
                        goto case InflateMode.CHECK2;
                    case InflateMode.CHECK2:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += ((z.next_in[z.next_in_index++] & 0xff) << 8) & 0xff00L;
                        z.istate.Mode = InflateMode.CHECK1;
                        goto case InflateMode.CHECK1;
                    case InflateMode.CHECK1:

                        if (z.avail_in == 0) return r; r = f;

                        z.avail_in--; z.total_in++;
                        z.istate.need += (z.next_in[z.next_in_index++] & 0xffL);

                        if (((int)(z.istate.was[0])) != ((int)(z.istate.need)))
                        {
                            z.istate.Mode = InflateMode.BAD;
                            z.msg = "incorrect data check";
                            z.istate.marker = 5;       // can't try inflateSync
                            break;
                        }

                        z.istate.Mode = InflateMode.DONE;
                        goto case InflateMode.DONE;
                    case InflateMode.DONE:
                        return ZLibStatus.Z_STREAM_END;
                    case InflateMode.BAD:
                        return ZLibStatus.Z_DATA_ERROR;
                    default:
                        return ZLibStatus.Z_STREAM_ERROR;
                }
            }
        }

        //internal ZLibStatus InflateSetDictionary(ZStream z, byte[] dictionary, int dictLength)
        //{
        //    int index = 0;
        //    int length = dictLength;
        //    if (z == null || z.istate == null || z.istate.Mode != InflateMode.DICT0)
        //        return ZLibStatus.Z_STREAM_ERROR;

        //    if (z.Adler32(1L, dictionary, 0, dictLength) != z.Adler)
        //    {
        //        return ZLibStatus.Z_DATA_ERROR;
        //    }

        //    //z.adler = z.Adler32(0, null, 0, 0);
        //    z.UpdateAdler(0, null, 0, 0);

        //    if (length >= (1 << z.istate.wbits))
        //    {
        //        length = (1 << z.istate.wbits) - 1;
        //        index = dictLength - length;
        //    }
        //    z.istate.blocks.set_dictionary(dictionary, index, length);
        //    z.istate.Mode = InflateMode.BLOCKS;
        //    return ZLibStatus.Z_OK;
        //}

        internal ZLibStatus InflateSync(ZStream z)
        {
            int n;       // number of bytes to look at
            int p;       // pointer to bytes
            int m;       // number of marker bytes found in a row
            long r, w;   // temporaries to save total_in and total_out

            // set up
            if (z == null || z.istate == null)
                return ZLibStatus.Z_STREAM_ERROR;
            if (z.istate.Mode != InflateMode.BAD)
            {
                z.istate.Mode = InflateMode.BAD;
                z.istate.marker = 0;
            }
            if ((n = z.avail_in) == 0)
                return ZLibStatus.Z_BUF_ERROR;
            p = z.next_in_index;
            m = z.istate.marker;

            // search
            while (n != 0 && m < 4)
            {
                if (z.next_in[p] == mark[m])
                {
                    m++;
                }
                else if (z.next_in[p] != 0)
                {
                    m = 0;
                }
                else
                {
                    m = 4 - m;
                }
                p++; n--;
            }

            // restore
            z.total_in += p - z.next_in_index;
            z.next_in_index = p;
            z.avail_in = n;
            z.istate.marker = m;

            // return no joy or set up to restart on a new block
            if (m != 4)
            {
                return ZLibStatus.Z_DATA_ERROR;
            }
            r = z.total_in; w = z.total_out;
            InflateReset(z);
            z.total_in = r; z.total_out = w;
            z.istate.Mode = InflateMode.BLOCKS;
            return ZLibStatus.Z_OK;
        }

        // Returns true if inflate is currently at the end of a block generated
        // by FlushType.Z_SYNC_FLUSH or FlushType.Z_FULL_FLUSH. This function is used by one PPP
        // implementation to provide an additional safety check. PPP uses FlushType.Z_SYNC_FLUSH
        // but removes the length bytes of the resulting empty stored block. When
        // decompressing, PPP checks that at the end of input packet, inflate is
        // waiting for these length bytes.
        //internal ZLibStatus InflateSyncPoint(ZStream z)
        //{
        //    if (z == null || z.istate == null || z.istate.blocks == null)
        //        return ZLibStatus.Z_STREAM_ERROR;
        //    return z.istate.blocks.sync_point();
        //}
    }
}