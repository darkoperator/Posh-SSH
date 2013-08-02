using System;
/*
 * $Id: InfBlocks.cs,v 1.2 2008-05-10 09:35:40 bouncy Exp $
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

    public enum InflateBlockMode
    {
        /// <summary>
        /// get type bits (3, including end bit)
        /// </summary>
        TYPE = 0,
        /// <summary>
        /// get lengths for stored
        /// </summary>
        LENS = 1,
        /// <summary>
        /// cessing stored block
        /// </summary>
        STORED = 2,
        /// <summary>
        /// get table lengths
        /// </summary>
        TABLE = 3,
        /// <summary>
        /// get bit lengths tree for a dynamic block
        /// </summary>
        BTREE = 4,
        /// <summary>
        /// get length, distance trees for a dynamic block
        /// </summary>
        DTREE = 5,
        /// <summary>
        /// processing fixed or dynamic block
        /// </summary>
        CODES = 6,
        /// <summary>
        /// output remaining window bytes
        /// </summary>
        DRY = 7,
        /// <summary>
        /// finished last block, done
        /// </summary>
        DONE = 8,
        /// <summary>
        /// ot a data error--stuck here
        /// </summary>
        BAD = 9
    }

    internal sealed class InflateBlocks : InflateCodes
    {
        private const int MANY = 1440;

        // And'ing with mask[n] masks the lower n bits
        private static readonly int[] inflate_mask = {
                                                0x00000000, 0x00000001, 0x00000003, 0x00000007, 0x0000000f,
                                                0x0000001f, 0x0000003f, 0x0000007f, 0x000000ff, 0x000001ff,
                                                0x000003ff, 0x000007ff, 0x00000fff, 0x00001fff, 0x00003fff,
                                                0x00007fff, 0x0000ffff
                                            };

        // Table for deflate from PKZIP's appnote.txt.
        static readonly int[] border = { // Order of the bit length code lengths
                                  16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15
                              };

        internal InflateBlockMode mode;            // current inflate_block mode 

        internal int left;            // if STORED, bytes left to copy 

        internal int table;           // table lengths (14 bits) 
        internal int index;           // index into blens (or border) 
        internal int[] blens;         // bit lengths of codes 
        internal int[] bb = new int[1]; // bit length tree depth 
        internal int[] tb = new int[1]; // bit length decoding tree 

        //internal InfCodes codes = new InfCodes();      // if CODES, current state 

        int last;            // true if this block is the last block 

        // mode independent information 
        internal int bitk;            // bits in bit buffer 
        internal int bitb;            // bit buffer 
        internal int[] hufts;         // single malloc for tree space 
        internal byte[] window;       // sliding window 
        internal int end;             // one byte after sliding window 
        internal int read;            // window read pointer 
        internal int write;           // window write pointer 
        internal Object checkfn;      // check function 
        internal long check;          // check on output 

        internal InflateTree inftree = new InflateTree();

        internal InflateBlocks(ZStream z, Object checkfn, int w)
        {
            hufts = new int[MANY * 3];
            window = new byte[w];
            end = w;
            this.checkfn = checkfn;
            mode = InflateBlockMode.TYPE;
            reset(z, null);
        }

        internal void reset(ZStream z, long[] c)
        {
            if (c != null) c[0] = check;
            if (mode == InflateBlockMode.BTREE || mode == InflateBlockMode.DTREE)
            {
            }
            if (mode == InflateBlockMode.CODES)
            {
                codesfree(z);
            }
            mode = InflateBlockMode.TYPE;
            bitk = 0;
            bitb = 0;
            read = write = 0;

            if (checkfn != null)
                z.adler = check = Adler32.adler32(0L, null, 0, 0);
        }

        internal ZLibStatus proc(ZStream z, ZLibStatus r)
        {
            int t;              // temporary storage
            ZLibStatus tt;
            int b;              // bit buffer
            int k;              // bits in bit buffer
            int p;              // input data pointer
            int n;              // bytes available there
            int q;              // output window write pointer
            int m;
            {              // bytes to end of window or read pointer

                // copy input/output information to locals (UPDATE macro restores)
                p = z.next_in_index; n = z.avail_in; b = bitb; k = bitk;
            }
            {
                q = write; m = (int)(q < read ? read - q - 1 : end - q);
            }

            // process input based on current state
            while (true)
            {
                switch (mode)
                {
                    case InflateBlockMode.TYPE:

                        while (k < (3))
                        {
                            if (n != 0)
                            {
                                r = ZLibStatus.Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                z.avail_in = n;
                                z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                            };
                            n--;
                            b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }
                        t = (int)(b & 7);
                        last = t & 1;

                        switch (t >> 1)
                        {
                            case 0:
                                {                         // stored 
                                    b >>= (3); k -= (3);
                                }
                                t = k & 7;
                                {                    // go to byte boundary

                                    b >>= (t); k -= (t);
                                }
                                mode = InflateBlockMode.LENS;                  // get length of stored block
                                break;
                            case 1:
                                {                         // fixed
                                    int[] bl = new int[1];
                                    int[] bd = new int[1];
                                    int[][] tl = new int[1][];
                                    int[][] td = new int[1][];

                                    InflateTree.InflateTreesFixed(bl, bd, tl, td, z);
                                    codesinit(bl[0], bd[0], tl[0], 0, td[0], 0, z);
                                }
                                {

                                    b >>= (3); k -= (3);
                                }

                                mode = InflateBlockMode.CODES;
                                break;
                            case 2:
                                {                         // dynamic

                                    b >>= (3); k -= (3);
                                }

                                mode = InflateBlockMode.TABLE;
                                break;
                            case 3:
                                {                         // illegal

                                    b >>= (3); k -= (3);
                                }
                                mode = InflateBlockMode.BAD;
                                z.msg = "invalid block type";
                                r = ZLibStatus.Z_DATA_ERROR;

                                bitb = b; bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                        }
                        break;
                    case InflateBlockMode.LENS:

                        while (k < (32))
                        {
                            if (n != 0)
                            {
                                r = ZLibStatus.Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                z.avail_in = n;
                                z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                            };
                            n--;
                            b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }

                        if ((((~b) >> 16) & 0xffff) != (b & 0xffff))
                        {
                            mode = InflateBlockMode.BAD;
                            z.msg = "invalid stored block lengths";
                            r = ZLibStatus.Z_DATA_ERROR;

                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }
                        left = (b & 0xffff);
                        b = k = 0;                       // dump bits
                        mode = left != 0 ? InflateBlockMode.STORED : (last != 0 ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
                        break;
                    case InflateBlockMode.STORED:
                        if (n == 0)
                        {
                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }

                        if (m == 0)
                        {
                            if (q == end && read != 0)
                            {
                                q = 0; m = (int)(q < read ? read - q - 1 : end - q);
                            }
                            if (m == 0)
                            {
                                write = q;
                                r = inflate_flush(z, r);
                                q = write; m = (int)(q < read ? read - q - 1 : end - q);
                                if (q == end && read != 0)
                                {
                                    q = 0; m = (int)(q < read ? read - q - 1 : end - q);
                                }
                                if (m == 0)
                                {
                                    bitb = b; bitk = k;
                                    z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    write = q;
                                    return inflate_flush(z, r);
                                }
                            }
                        }
                        r = ZLibStatus.Z_OK;

                        t = left;
                        if (t > n) t = n;
                        if (t > m) t = m;
                        System.Array.Copy(z.next_in, p, window, q, t);
                        p += t; n -= t;
                        q += t; m -= t;
                        if ((left -= t) != 0)
                            break;
                        mode = last != 0 ? InflateBlockMode.DRY : InflateBlockMode.TYPE;
                        break;
                    case InflateBlockMode.TABLE:

                        while (k < (14))
                        {
                            if (n != 0)
                            {
                                r = ZLibStatus.Z_OK;
                            }
                            else
                            {
                                bitb = b; bitk = k;
                                z.avail_in = n;
                                z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                            };
                            n--;
                            b |= (z.next_in[p++] & 0xff) << k;
                            k += 8;
                        }

                        table = t = (b & 0x3fff);
                        if ((t & 0x1f) > 29 || ((t >> 5) & 0x1f) > 29)
                        {
                            mode = InflateBlockMode.BAD;
                            z.msg = "too many length or distance symbols";
                            r = ZLibStatus.Z_DATA_ERROR;

                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }
                        t = 258 + (t & 0x1f) + ((t >> 5) & 0x1f);
                        if (blens == null || blens.Length < t)
                        {
                            blens = new int[t];
                        }
                        else
                        {
                            for (int i = 0; i < t; i++) { blens[i] = 0; }
                        }
                        {

                            b >>= (14); k -= (14);
                        }

                        index = 0;
                        mode = InflateBlockMode.BTREE;
                        goto case InflateBlockMode.BTREE;
                    case InflateBlockMode.BTREE:
                        while (index < 4 + (table >> 10))
                        {
                            while (k < (3))
                            {
                                if (n != 0)
                                {
                                    r = ZLibStatus.Z_OK;
                                }
                                else
                                {
                                    bitb = b; bitk = k;
                                    z.avail_in = n;
                                    z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    write = q;
                                    return inflate_flush(z, r);
                                };
                                n--;
                                b |= (z.next_in[p++] & 0xff) << k;
                                k += 8;
                            }

                            blens[border[index++]] = b & 7;
                            {

                                b >>= (3); k -= (3);
                            }
                        }

                        while (index < 19)
                        {
                            blens[border[index++]] = 0;
                        }

                        bb[0] = 7;
                        tt = inftree.InflateTreesBits(blens, bb, tb, hufts, z);
                        if (tt != ZLibStatus.Z_OK)
                        {
                            r = tt;
                            if (r == ZLibStatus.Z_DATA_ERROR)
                            {
                                blens = null;
                                mode = InflateBlockMode.BAD;
                            }

                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }

                        index = 0;
                        mode = InflateBlockMode.DTREE;
                        goto case InflateBlockMode.DTREE;
                    case InflateBlockMode.DTREE:
                        while (true)
                        {
                            t = table;
                            if (!(index < 258 + (t & 0x1f) + ((t >> 5) & 0x1f)))
                            {
                                break;
                            }

                            int i, j, c;

                            t = bb[0];

                            while (k < (t))
                            {
                                if (n != 0)
                                {
                                    r = ZLibStatus.Z_OK;
                                }
                                else
                                {
                                    bitb = b; bitk = k;
                                    z.avail_in = n;
                                    z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    write = q;
                                    return inflate_flush(z, r);
                                };
                                n--;
                                b |= (z.next_in[p++] & 0xff) << k;
                                k += 8;
                            }

                            if (tb[0] == -1)
                            {
                                //System.err.println("null...");
                            }

                            t = hufts[(tb[0] + (b & inflate_mask[t])) * 3 + 1];
                            c = hufts[(tb[0] + (b & inflate_mask[t])) * 3 + 2];

                            if (c < 16)
                            {
                                b >>= (t); k -= (t);
                                blens[index++] = c;
                            }
                            else
                            { // c == 16..18
                                i = c == 18 ? 7 : c - 14;
                                j = c == 18 ? 11 : 3;

                                while (k < (t + i))
                                {
                                    if (n != 0)
                                    {
                                        r = ZLibStatus.Z_OK;
                                    }
                                    else
                                    {
                                        bitb = b; bitk = k;
                                        z.avail_in = n;
                                        z.total_in += p - z.next_in_index; z.next_in_index = p;
                                        write = q;
                                        return inflate_flush(z, r);
                                    };
                                    n--;
                                    b |= (z.next_in[p++] & 0xff) << k;
                                    k += 8;
                                }

                                b >>= (t); k -= (t);

                                j += (b & inflate_mask[i]);

                                b >>= (i); k -= (i);

                                i = index;
                                t = table;
                                if (i + j > 258 + (t & 0x1f) + ((t >> 5) & 0x1f) ||
                                    (c == 16 && i < 1))
                                {
                                    blens = null;
                                    mode = InflateBlockMode.BAD;
                                    z.msg = "invalid bit length repeat";
                                    r = ZLibStatus.Z_DATA_ERROR;

                                    bitb = b; bitk = k;
                                    z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                    write = q;
                                    return inflate_flush(z, r);
                                }

                                c = c == 16 ? blens[i - 1] : 0;
                                do
                                {
                                    blens[i++] = c;
                                }
                                while (--j != 0);
                                index = i;
                            }
                        }

                        tb[0] = -1;
                        {
                            int[] bl = new int[1];
                            int[] bd = new int[1];
                            int[] tl = new int[1];
                            int[] td = new int[1];
                            bl[0] = 9;         // must be <= 9 for lookahead assumptions
                            bd[0] = 6;         // must be <= 9 for lookahead assumptions

                            t = table;
                            tt = inftree.InflateTreesDynamic(257 + (t & 0x1f),
                                1 + ((t >> 5) & 0x1f),
                                blens, bl, bd, tl, td, hufts, z);

                            if (tt != ZLibStatus.Z_OK)
                            {
                                if (tt == ZLibStatus.Z_DATA_ERROR)
                                {
                                    blens = null;
                                    mode = InflateBlockMode.BAD;
                                }
                                r = tt;

                                bitb = b; bitk = k;
                                z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                                write = q;
                                return inflate_flush(z, r);
                            }
                            codesinit(bl[0], bd[0], hufts, tl[0], hufts, td[0], z);
                        }
                        mode = InflateBlockMode.CODES;
                        goto case InflateBlockMode.CODES;
                    case InflateBlockMode.CODES:
                        bitb = b; bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        write = q;

                        if ((r = codesproc(this, z, r)) != ZLibStatus.Z_STREAM_END)
                        {
                            return inflate_flush(z, r);
                        }
                        r = ZLibStatus.Z_OK;
                        codesfree(z);

                        p = z.next_in_index; n = z.avail_in; b = bitb; k = bitk;
                        q = write; m = (int)(q < read ? read - q - 1 : end - q);

                        if (last == 0)
                        {
                            mode = InflateBlockMode.TYPE;
                            break;
                        }
                        mode = InflateBlockMode.DRY;
                        goto case InflateBlockMode.DRY;
                    case InflateBlockMode.DRY:
                        write = q;
                        r = inflate_flush(z, r);
                        q = write; m = (int)(q < read ? read - q - 1 : end - q);
                        if (read != write)
                        {
                            bitb = b; bitk = k;
                            z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                            write = q;
                            return inflate_flush(z, r);
                        }
                        mode = InflateBlockMode.DONE;
                        goto case InflateBlockMode.DONE;
                    case InflateBlockMode.DONE:
                        r = ZLibStatus.Z_STREAM_END;

                        bitb = b; bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        write = q;
                        return inflate_flush(z, r);
                    case InflateBlockMode.BAD:
                        r = ZLibStatus.Z_DATA_ERROR;

                        bitb = b; bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        write = q;
                        return inflate_flush(z, r);

                    default:
                        r = ZLibStatus.Z_STREAM_ERROR;

                        bitb = b; bitk = k;
                        z.avail_in = n; z.total_in += p - z.next_in_index; z.next_in_index = p;
                        write = q;
                        return inflate_flush(z, r);
                }
            }
        }

        internal void free(ZStream z)
        {
            reset(z, null);
            window = null;
            hufts = null;
            //ZFREE(z, s);
        }

        internal void set_dictionary(byte[] d, int start, int n)
        {
            System.Array.Copy(d, start, window, 0, n);
            read = write = n;
        }

        // Returns true if inflate is currently at the end of a block generated
        // by Z_SYNC_FLUSH or FlushType.Z_FULL_FLUSH. 
        internal ZLibStatus sync_point()
        {
            return mode == InflateBlockMode.LENS ? ZLibStatus.Z_STREAM_END : ZLibStatus.Z_OK;
        }

        // copy as much as possible from the sliding window to the output area
        internal ZLibStatus inflate_flush(ZStream z, ZLibStatus r)
        {
            int n;
            int p;
            int q;

            // local copies of source and destination pointers
            p = z.next_out_index;
            q = read;

            // compute number of bytes to copy as far as end of window
            n = (int)((q <= write ? write : end) - q);
            if (n > z.avail_out) n = z.avail_out;
            if (n != 0 && r == ZLibStatus.Z_BUF_ERROR) r = ZLibStatus.Z_OK;

            // update counters
            z.avail_out -= n;
            z.total_out += n;

            // update check information
            if (checkfn != null)
                z.adler = check = Adler32.adler32(check, window, q, n);

            // copy as far as end of window
            System.Array.Copy(window, q, z.next_out, p, n);
            p += n;
            q += n;

            // see if more to copy at beginning of window
            if (q == end)
            {
                // wrap pointers
                q = 0;
                if (write == end)
                    write = 0;

                // compute bytes to copy
                n = write - q;
                if (n > z.avail_out) n = z.avail_out;
                if (n != 0 && r == ZLibStatus.Z_BUF_ERROR) r = ZLibStatus.Z_OK;

                // update counters
                z.avail_out -= n;
                z.total_out += n;

                // update check information
                if (checkfn != null)
                    z.adler = check = Adler32.adler32(check, window, q, n);

                // copy
                System.Array.Copy(window, q, z.next_out, p, n);
                p += n;
                q += n;
            }

            // update pointers
            z.next_out_index = p;
            read = q;

            // done
            return r;
        }
    }
}