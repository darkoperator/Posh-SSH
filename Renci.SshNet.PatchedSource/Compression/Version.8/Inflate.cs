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

    internal sealed class Inflate : ZStream, ICompressor
    {
        // preset dictionary flag in zlib header
        private const int PRESET_DICT = 0x20;

        private const int Z_DEFLATED = 8;

        private static readonly byte[] mark = { (byte)0, (byte)0, (byte)0xff, (byte)0xff };

        /// <summary>
        /// The current inflate mode
        /// </summary>
        private InflateMode _mode;

        /// <summary>
        /// if FLAGS, method byte
        /// </summary>
        private int method;

        /// <summary>
        /// Computed check value
        /// </summary>
        private long[] _was = new long[1];

        /// <summary>
        /// The stream check value
        /// </summary>
        private long _need;

        /// <summary>
        /// if BAD, inflateSync's marker bytes count
        /// </summary>
        private int _marker;

        // mode independent information
        /// <summary>
        /// Flag for no wrapper
        /// </summary>
        private int _nowrap;

        /// <summary>
        /// log2(window size)  (8..15, defaults to 15)
        /// </summary>
        private int _wbits;

        /// <summary>
        /// Current inflate_blocks state
        /// </summary>
        private InflateBlocks _blocks;

        public Inflate()
        {
            this.inflateInit();
        }

        public Inflate(bool nowrap)
        {
            this.inflateInit(nowrap);
        }

        private ZLibStatus InflateReset()
        {
            base.total_in = base.total_out = 0;
            base.msg = null;
            this._mode = this._nowrap != 0 ? InflateMode.BLOCKS : InflateMode.METHOD;
            this._blocks.reset(this, null);
            return ZLibStatus.Z_OK;
        }

        public ZLibStatus InflateEnd()
        {
            if (_blocks != null)
                _blocks.free(this);
            _blocks = null;
            //    ZFREE(z, z->state);
            return ZLibStatus.Z_OK;
        }

        public ZLibStatus InflateInit(int w)
        {
            base.msg = null;
            _blocks = null;

            // handle undocumented nowrap option (no zlib header or check)
            _nowrap = 0;
            if (w < 0)
            {
                w = -w;
                _nowrap = 1;
            }

            // set window size
            if (w < 8 || w > 15)
            {
                InflateEnd();
                return ZLibStatus.Z_STREAM_ERROR;
            }
            _wbits = w;

            this._blocks = new InflateBlocks(this, this._nowrap != 0 ? null : this, 1 << w);

            // reset state
            InflateReset();
            return ZLibStatus.Z_OK;
        }

        public ZLibStatus inflate(FlushType ff)
        {
            ZLibStatus r;
            int b;

            if (base.next_in == null)
                return ZLibStatus.Z_STREAM_ERROR;
            var f = ff == FlushType.Z_FINISH ? ZLibStatus.Z_BUF_ERROR : ZLibStatus.Z_OK;
            r = ZLibStatus.Z_BUF_ERROR;
            while (true)
            {
                //System.out.println("mode: "+this.mode);
                switch (this._mode)
                {
                    case InflateMode.METHOD:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        if (((this.method = base.next_in[base.next_in_index++]) & 0xf) != Z_DEFLATED)
                        {
                            this._mode = InflateMode.BAD;
                            base.msg = "unknown compression method";
                            this._marker = 5;       // can't try inflateSync
                            break;
                        }
                        if ((this.method >> 4) + 8 > this._wbits)
                        {
                            this._mode = InflateMode.BAD;
                            base.msg = "invalid window size";
                            this._marker = 5;       // can't try inflateSync
                            break;
                        }
                        this._mode = InflateMode.FLAG;
                        goto case InflateMode.FLAG;
                    case InflateMode.FLAG:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        b = (base.next_in[base.next_in_index++]) & 0xff;

                        if ((((this.method << 8) + b) % 31) != 0)
                        {
                            this._mode = InflateMode.BAD;
                            base.msg = "incorrect header check";
                            this._marker = 5;       // can't try inflateSync
                            break;
                        }

                        if ((b & PRESET_DICT) == 0)
                        {
                            this._mode = InflateMode.BLOCKS;
                            break;
                        }
                        this._mode = InflateMode.DICT4;
                        goto case InflateMode.DICT4;
                    case InflateMode.DICT4:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        this._need = ((base.next_in[base.next_in_index++] & 0xff) << 24) & 0xff000000L;
                        this._mode = InflateMode.DICT3;
                        goto case InflateMode.DICT3;
                    case InflateMode.DICT3:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        this._need += ((base.next_in[base.next_in_index++] & 0xff) << 16) & 0xff0000L;
                        this._mode = InflateMode.DICT2;
                        goto case InflateMode.DICT2;
                    case InflateMode.DICT2:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        this._need += ((base.next_in[base.next_in_index++] & 0xff) << 8) & 0xff00L;
                        this._mode = InflateMode.DICT1;
                        goto case InflateMode.DICT1;
                    case InflateMode.DICT1:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        this._need += (base.next_in[base.next_in_index++] & 0xffL);
                        base.adler = this._need;
                        this._mode = InflateMode.DICT0;
                        return ZLibStatus.Z_NEED_DICT;
                    case InflateMode.DICT0:
                        this._mode = InflateMode.BAD;
                        base.msg = "need dictionary";
                        this._marker = 0;       // can try inflateSync
                        return ZLibStatus.Z_STREAM_ERROR;
                    case InflateMode.BLOCKS:

                        r = this._blocks.proc(this, r);
                        if (r == ZLibStatus.Z_DATA_ERROR)
                        {
                            this._mode = InflateMode.BAD;
                            this._marker = 0;     // can try inflateSync
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
                        this._blocks.reset(this, this._was);
                        if (this._nowrap != 0)
                        {
                            this._mode = InflateMode.DONE;
                            break;
                        }
                        this._mode = InflateMode.CHECK4;
                        goto case InflateMode.CHECK4;
                    case InflateMode.CHECK4:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        this._need = ((base.next_in[base.next_in_index++] & 0xff) << 24) & 0xff000000L;
                        this._mode = InflateMode.CHECK3;
                        goto case InflateMode.CHECK3;
                    case InflateMode.CHECK3:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        this._need += ((base.next_in[base.next_in_index++] & 0xff) << 16) & 0xff0000L;
                        this._mode = InflateMode.CHECK2;
                        goto case InflateMode.CHECK2;
                    case InflateMode.CHECK2:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        this._need += ((base.next_in[base.next_in_index++] & 0xff) << 8) & 0xff00L;
                        this._mode = InflateMode.CHECK1;
                        goto case InflateMode.CHECK1;
                    case InflateMode.CHECK1:

                        if (base.avail_in == 0) return r; r = f;

                        base.avail_in--; base.total_in++;
                        this._need += (base.next_in[base.next_in_index++] & 0xffL);

                        if (((int)(this._was[0])) != ((int)(this._need)))
                        {
                            this._mode = InflateMode.BAD;
                            base.msg = "incorrect data check";
                            this._marker = 5;       // can't try inflateSync
                            break;
                        }

                        this._mode = InflateMode.DONE;
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

        public ZLibStatus InflateSetDictionary(byte[] dictionary, int dictLength)
        {
            int index = 0;
            int length = dictLength;
            if (this._mode != InflateMode.DICT0)
                return ZLibStatus.Z_STREAM_ERROR;

            if (Adler32.adler32(1L, dictionary, 0, dictLength) != base.adler)
            {
                return ZLibStatus.Z_DATA_ERROR;
            }

            base.adler = Adler32.adler32(0, null, 0, 0);

            if (length >= (1 << this._wbits))
            {
                length = (1 << this._wbits) - 1;
                index = dictLength - length;
            }
            this._blocks.set_dictionary(dictionary, index, length);
            this._mode = InflateMode.BLOCKS;
            return ZLibStatus.Z_OK;
        }

        public ZLibStatus InflateSync()
        {
            int n;       // number of bytes to look at
            int p;       // pointer to bytes
            int m;       // number of marker bytes found in a row
            long r, w;   // temporaries to save total_in and total_out

            // set up
            if (this._mode != InflateMode.BAD)
            {
                this._mode = InflateMode.BAD;
                this._marker = 0;
            }
            if ((n = base.avail_in) == 0)
                return ZLibStatus.Z_BUF_ERROR;
            p = base.next_in_index;
            m = this._marker;

            // search
            while (n != 0 && m < 4)
            {
                if (base.next_in[p] == mark[m])
                {
                    m++;
                }
                else if (base.next_in[p] != 0)
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
            base.total_in += p - base.next_in_index;
            base.next_in_index = p;
            base.avail_in = n;
            this._marker = m;

            // return no joy or set up to restart on a new block
            if (m != 4)
            {
                return ZLibStatus.Z_DATA_ERROR;
            }
            r = base.total_in; w = base.total_out;
            InflateReset();
            base.total_in = r; base.total_out = w;
            this._mode = InflateMode.BLOCKS;
            return ZLibStatus.Z_OK;
        }

        public ZLibStatus inflateInit()
        {
            return inflateInit(DEF_WBITS);
        }
        public ZLibStatus inflateInit(bool nowrap)
        {
            return inflateInit(DEF_WBITS, nowrap);
        }
        public ZLibStatus inflateInit(int w)
        {
            return inflateInit(w, false);
        }

        public ZLibStatus inflateInit(int w, bool nowrap)
        {
            return this.InflateInit(nowrap ? -w : w);
        }

        public ZLibStatus inflateEnd()
        {
            var ret = this.InflateEnd();
            return ret;
        }
        public ZLibStatus inflateSync()
        {
            return this.InflateSync();
        }

        public ZLibStatus inflateSetDictionary(byte[] dictionary, int dictLength)
        {
            return this.InflateSetDictionary(dictionary, dictLength);
        }

        public ZLibStatus inflate(byte[] inputBufer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset, int outputCount, FlushType flushType)
        {
            this.next_in = inputBufer;
            this.next_in_index = inputOffset;
            this.avail_in = inputCount;
            this.next_out = outputBuffer;
            this.next_out_index = outputOffset;
            this.avail_out = outputCount;
            return this.inflate(flushType);
        }

        //  TODO:   Dlete this
        public ZLibStatus deflate(byte[] bufer, int offset, int count, byte[] p1, int p2, int p3, FlushType flushType)
        {
            throw new NotImplementedException();
        }

        public ZLibStatus deflateEnd()
        {
            throw new NotImplementedException();
        }

        public ZLibStatus deflate(FlushType flushType)
        {
            throw new NotImplementedException();
        }
    }
}