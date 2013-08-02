using System;
using System.Collections.Generic;
/*
 * $Id: Deflate.cs,v 1.2 2008-05-10 09:35:40 bouncy Exp $
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

    public enum BlockType : byte
    {
        Z_BINARY = 0,
        Z_ASCII = 1,
        Z_UNKNOWN = 2
    }

    public sealed class Deflate
    {
        /// <summary>
        /// The Bit length codes must not exceed MAX_BL_BITS bits
        /// </summary>
        private const int MAX_BL_BITS = 7;

        private const int MAX_MEM_LEVEL = 9;

        #region Private static declarations

        private static readonly byte[] _lengthCode ={
                                                0,  1,  2,  3,  4,  5,  6,  7,  8,  8,  9,  9, 10, 10, 11, 11, 12, 12, 12, 12,
                                                13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 16, 16, 16, 16, 16,
                                                17, 17, 17, 17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19,
                                                19, 19, 19, 19, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
                                                21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 22, 22, 22, 22,
                                                22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23,
                                                23, 23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
                                                24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
                                                25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
                                                25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 26,
                                                26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
                                                26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
                                                27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28
                                            };

        private static readonly int[] _baseLength = {
                                               0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 12, 14, 16, 20, 24, 28, 32, 40, 48, 56,
                                               64, 80, 96, 112, 128, 160, 192, 224, 0
                                           };

        private static readonly int[] _baseDistance = {
                                             0,   1,      2,     3,     4,    6,     8,    12,    16,     24,
                                             32,  48,     64,    96,   128,  192,   256,   384,   512,    768,
                                             1024, 1536,  2048,  3072,  4096,  6144,  8192, 12288, 16384, 24576
                                         };

        private static readonly short[] static_ltree = {
                                                   12,  8, 140,  8,  76,  8, 204,  8,  44,  8,
                                                   172,  8, 108,  8, 236,  8,  28,  8, 156,  8,
                                                   92,  8, 220,  8,  60,  8, 188,  8, 124,  8,
                                                   252,  8,   2,  8, 130,  8,  66,  8, 194,  8,
                                                   34,  8, 162,  8,  98,  8, 226,  8,  18,  8,
                                                   146,  8,  82,  8, 210,  8,  50,  8, 178,  8,
                                                   114,  8, 242,  8,  10,  8, 138,  8,  74,  8,
                                                   202,  8,  42,  8, 170,  8, 106,  8, 234,  8,
                                                   26,  8, 154,  8,  90,  8, 218,  8,  58,  8,
                                                   186,  8, 122,  8, 250,  8,   6,  8, 134,  8,
                                                   70,  8, 198,  8,  38,  8, 166,  8, 102,  8,
                                                   230,  8,  22,  8, 150,  8,  86,  8, 214,  8,
                                                   54,  8, 182,  8, 118,  8, 246,  8,  14,  8,
                                                   142,  8,  78,  8, 206,  8,  46,  8, 174,  8,
                                                   110,  8, 238,  8,  30,  8, 158,  8,  94,  8,
                                                   222,  8,  62,  8, 190,  8, 126,  8, 254,  8,
                                                   1,  8, 129,  8,  65,  8, 193,  8,  33,  8,
                                                   161,  8,  97,  8, 225,  8,  17,  8, 145,  8,
                                                   81,  8, 209,  8,  49,  8, 177,  8, 113,  8,
                                                   241,  8,   9,  8, 137,  8,  73,  8, 201,  8,
                                                   41,  8, 169,  8, 105,  8, 233,  8,  25,  8,
                                                   153,  8,  89,  8, 217,  8,  57,  8, 185,  8,
                                                   121,  8, 249,  8,   5,  8, 133,  8,  69,  8,
                                                   197,  8,  37,  8, 165,  8, 101,  8, 229,  8,
                                                   21,  8, 149,  8,  85,  8, 213,  8,  53,  8,
                                                   181,  8, 117,  8, 245,  8,  13,  8, 141,  8,
                                                   77,  8, 205,  8,  45,  8, 173,  8, 109,  8,
                                                   237,  8,  29,  8, 157,  8,  93,  8, 221,  8,
                                                   61,  8, 189,  8, 125,  8, 253,  8,  19,  9,
                                                   275,  9, 147,  9, 403,  9,  83,  9, 339,  9,
                                                   211,  9, 467,  9,  51,  9, 307,  9, 179,  9,
                                                   435,  9, 115,  9, 371,  9, 243,  9, 499,  9,
                                                   11,  9, 267,  9, 139,  9, 395,  9,  75,  9,
                                                   331,  9, 203,  9, 459,  9,  43,  9, 299,  9,
                                                   171,  9, 427,  9, 107,  9, 363,  9, 235,  9,
                                                   491,  9,  27,  9, 283,  9, 155,  9, 411,  9,
                                                   91,  9, 347,  9, 219,  9, 475,  9,  59,  9,
                                                   315,  9, 187,  9, 443,  9, 123,  9, 379,  9,
                                                   251,  9, 507,  9,   7,  9, 263,  9, 135,  9,
                                                   391,  9,  71,  9, 327,  9, 199,  9, 455,  9,
                                                   39,  9, 295,  9, 167,  9, 423,  9, 103,  9,
                                                   359,  9, 231,  9, 487,  9,  23,  9, 279,  9,
                                                   151,  9, 407,  9,  87,  9, 343,  9, 215,  9,
                                                   471,  9,  55,  9, 311,  9, 183,  9, 439,  9,
                                                   119,  9, 375,  9, 247,  9, 503,  9,  15,  9,
                                                   271,  9, 143,  9, 399,  9,  79,  9, 335,  9,
                                                   207,  9, 463,  9,  47,  9, 303,  9, 175,  9,
                                                   431,  9, 111,  9, 367,  9, 239,  9, 495,  9,
                                                   31,  9, 287,  9, 159,  9, 415,  9,  95,  9,
                                                   351,  9, 223,  9, 479,  9,  63,  9, 319,  9,
                                                   191,  9, 447,  9, 127,  9, 383,  9, 255,  9,
                                                   511,  9,   0,  7,  64,  7,  32,  7,  96,  7,
                                                   16,  7,  80,  7,  48,  7, 112,  7,   8,  7,
                                                   72,  7,  40,  7, 104,  7,  24,  7,  88,  7,
                                                   56,  7, 120,  7,   4,  7,  68,  7,  36,  7,
                                                   100,  7,  20,  7,  84,  7,  52,  7, 116,  7,
                                                   3,  8, 131,  8,  67,  8, 195,  8,  35,  8,
                                                   163,  8,  99,  8, 227,  8
                                               };

        private static readonly short[] static_dtree = {
                                                   0, 5, 16, 5,  8, 5, 24, 5,  4, 5,
                                                   20, 5, 12, 5, 28, 5,  2, 5, 18, 5,
                                                   10, 5, 26, 5,  6, 5, 22, 5, 14, 5,
                                                   30, 5,  1, 5, 17, 5,  9, 5, 25, 5,
                                                   5, 5, 21, 5, 13, 5, 29, 5,  3, 5,
                                                   19, 5, 11, 5, 27, 5,  7, 5, 23, 5
                                               };


        /// <summary>
        /// The extra bits for each length code
        /// </summary>
        private static int[] ExtraLBits = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0 };

        /// <summary>
        /// The extra bits for each distance code
        /// </summary>
        private static int[] ExtraDBits = { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };

        /// <summary>
        /// The extra bits for each bit length code
        /// </summary>
        private static int[] ExtraBLBits = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 7 };


        private static byte[] BLOrder = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

        #endregion

        private const int MAX_WBITS = 15;            // 32K LZ77 window
        private const int DEF_MEM_LEVEL = 8;

        private const int STORED = 0;
        private const int FAST = 1;
        private const int SLOW = 2;

        private static readonly IDictionary<CompressionLevel, Config> _configurationTable;

        static Deflate()
        {
            //config_table = new Config[10];
            _configurationTable = new Dictionary<CompressionLevel, Config>();
            //  TODO:   Improve array initialization
            //                         good  lazy  nice  chain
            _configurationTable[(CompressionLevel)0] = new Config(0, 0, 0, 0, STORED);
            _configurationTable[(CompressionLevel)1] = new Config(4, 4, 8, 4, FAST);
            _configurationTable[(CompressionLevel)2] = new Config(4, 5, 16, 8, FAST);
            _configurationTable[(CompressionLevel)3] = new Config(4, 6, 32, 32, FAST);

            _configurationTable[(CompressionLevel)4] = new Config(4, 4, 16, 16, SLOW);
            _configurationTable[(CompressionLevel)5] = new Config(8, 16, 32, 32, SLOW);
            _configurationTable[(CompressionLevel)6] = new Config(8, 16, 128, 128, SLOW);
            _configurationTable[(CompressionLevel)7] = new Config(8, 32, 128, 256, SLOW);
            _configurationTable[(CompressionLevel)8] = new Config(32, 128, 258, 1024, SLOW);
            _configurationTable[(CompressionLevel)9] = new Config(32, 258, 258, 4096, SLOW);
        }

        private static readonly String[] z_errmsg = {
                                               "need dictionary",     // ZLibStatus.Z_NEED_DICT       2
                                               "stream end",          // ZLibStatus.Z_STREAM_END      1
                                               "",                    // ZLibStatus.Z_OK              0
                                               "file error",          // ZLibStatus.Z_ERRNO         (-1)
                                               "stream error",        // ZLibStatus.Z_STREAM_ERROR  (-2)
                                               "data error",          // ZLibStatus.Z_DATA_ERROR    (-3)
                                               "insufficient memory", // ZLibStatus.Z_MEM_ERROR     (-4)
                                               "buffer error",        // ZLibStatus.Z_BUF_ERROR     (-5)
                                               "incompatible version",// ZLibStatus.Z_VERSION_ERROR (-6)
                                               ""
                                           };


        private static readonly StaticTree static_l_desc = new StaticTree(static_ltree, Deflate.ExtraLBits, LITERALS + 1, L_CODES, MAX_BITS);

        private static readonly StaticTree static_d_desc = new StaticTree(static_dtree, Deflate.ExtraDBits, 0, D_CODES, MAX_BITS);

        private static readonly StaticTree static_bl_desc = new StaticTree(null, Deflate.ExtraBLBits, 0, BL_CODES, MAX_BL_BITS);


        /// <summary>
        /// block not completed, need more input or more output
        /// </summary>
        private const int NeedMore = 0;

        /// <summary>
        // The block flush performed
        /// </summary>
        private const int BlockDone = 1;

        /// <summary>
        /// finish started, need only more output at next deflate
        /// </summary>
        private const int FinishStarted = 2;

        /// <summary>
        /// finish done, accept no more input or output
        /// </summary>
        private const int FinishDone = 3;

        /// <summary>
        /// preset dictionary flag in zlib header
        /// </summary>
        private const int PRESET_DICT = 0x20;

        private const int INIT_STATE = 42;
        private const int BUSY_STATE = 113;
        private const int FINISH_STATE = 666;

        /// <summary>
        /// The deflate compression method
        /// </summary>
        private const int Z_DEFLATED = 8;

        private const int STORED_BLOCK = 0;
        private const int STATIC_TREES = 1;
        private const int DYN_TREES = 2;

        private const int Buf_size = 8 * 2;

        /// <summary>
        /// The repeat previous bit length 3-6 times (2 bits of repeat count)
        /// </summary>
        private const int REP_3_6 = 16;

        /// <summary>
        /// repeat a zero length 3-10 times  (3 bits of repeat count)
        /// </summary>
        private const int REPZ_3_10 = 17;

        /// <summary>
        /// repeat a zero length 11-138 times  (7 bits of repeat count)
        /// </summary>
        private const int REPZ_11_138 = 18;

        private const int MIN_MATCH = 3;
        private const int MAX_MATCH = 258;
        private const int MIN_LOOKAHEAD = (MAX_MATCH + MIN_MATCH + 1);

        private const int MAX_BITS = 15;
        private const int D_CODES = 30;
        private const int BL_CODES = 19;
        private const int LENGTH_CODES = 29;
        private const int LITERALS = 256;
        private const int L_CODES = (LITERALS + 1 + LENGTH_CODES);
        private const int HEAP_SIZE = (2 * L_CODES + 1);

        private const int END_BLOCK = 256;

        /// <summary>
        /// pointer back to this zlib stream
        /// </summary>
        private ZStream strm;

        /// <summary>
        /// as the name implies
        /// </summary>
        private int status;

        private BlockType _dataType;

        /// <summary>
        /// STORED (for zip only) or DEFLATED
        /// </summary>
        private byte method;

        /// <summary>
        /// size of pending_buf
        /// </summary>
        private int pending_buf_size;

        /// <summary>
        /// LZ77 window size (32K by default)
        /// </summary>
        private int w_size;

        /// <summary>
        /// log2(w_size)  (8..16)
        /// </summary>
        private int w_bits;

        /// <summary>
        /// w_size - 1
        /// </summary>
        private int w_mask;

        /// <summary>
        /// Sliding window. Input bytes are read into the second half of the window,
        /// and move to the first half later to keep a dictionary of at least wSize
        /// bytes. With this organization, matches are limited to a distance of
        /// wSize-MAX_MATCH bytes, but this ensures that IO is always
        /// performed with a length multiple of the block size. Also, it limits
        /// the window size to 64K, which is quite useful on MSDOS.
        /// To do: use the user input buffer as sliding window.
        /// </summary>
        private byte[] window;

        /// <summary>
        /// Actual size of window: 2*wSize, except when the user input buffer
        /// is directly used as sliding window.
        /// </summary>
        private int window_size;

        /// <summary>
        /// Link to older string with same hash index. To limit the size of this
        /// array to 64K, this link is maintained only for the last 32K strings.
        /// An index in this array is thus a window index modulo 32K.
        /// </summary>
        private short[] prev;

        /// <summary>
        /// Heads of the hash chains or NIL.
        /// </summary>
        private short[] head;

        /// <summary>
        /// hash index of string to be inserted
        /// </summary>
        private int ins_h;

        /// <summary>
        /// number of elements in hash table
        /// </summary>
        private int hash_size;

        /// <summary>
        /// log2(hash_size)
        /// </summary>
        private int hash_bits;

        /// <summary>
        /// hash_size-1
        /// </summary>
        private int hash_mask;

        /// <summary>
        /// Number of bits by which ins_h must be shifted at each input
        /// step. It must be such that after MIN_MATCH steps, the oldest
        /// byte no longer takes part in the hash key, that is:
        /// hash_shift * MIN_MATCH >= hash_bits
        /// </summary>
        private int hash_shift;

        /// <summary>
        /// Window position at the beginning of the current output block. Gets
        /// negative when the window is moved backwards.
        /// </summary>
        private int block_start;

        /// <summary>
        /// length of best match
        /// </summary>
        private int match_length;

        /// <summary>
        /// previous match
        /// </summary>
        private int prev_match;

        /// <summary>
        /// set if previous match exists
        /// </summary>
        private int match_available;

        /// <summary>
        /// start of string to insert
        /// </summary>
        private int strstart;

        /// <summary>
        /// start of matching string
        /// </summary>
        private int match_start;

        /// <summary>
        /// number of valid bytes ahead in window
        /// </summary>
        private int lookahead;

        /// <summary>
        /// Length of the best match at previous step. Matches not greater than this
        /// are discarded. This is used in the lazy match evaluation.
        /// </summary>
        private int prev_length;

        /// <summary>
        /// To speed up deflation, hash chains are never searched beyond this
        /// length.  A higher limit improves compression ratio but degrades the speed.
        /// </summary>
        private int max_chain_length;

        /// <summary>
        /// Attempt to find a better match only when the current match is strictly
        /// smaller than this value. This mechanism is used only for compression
        /// levels >= 4.
        /// </summary>
        private int max_lazy_match;

        // Insert new strings in the hash table only if the match length is not
        // greater than this length. This saves time but degrades compression.
        // max_insert_length is used only for compression levels <= 3.

        /// <summary>
        /// The compression level (1..9)
        /// </summary>
        private CompressionLevel level;

        /// <summary>
        /// The favor or force Huffman coding
        /// </summary>
        private CompressionStrategy strategy;

        /// <summary>
        /// Use a faster search when the previous match is longer than this
        /// </summary>
        private int good_match;

        /// <summary>
        /// Stop searching when current match exceeds this
        /// </summary>
        private int nice_match;

        /// <summary>
        /// literal and length tree
        /// </summary>
        private short[] dyn_ltree;

        /// <summary>
        /// The distance tree
        /// </summary>
        private short[] dyn_dtree;

        /// <summary>
        /// The Huffman tree for bit lengths
        /// </summary>
        private short[] bl_tree;

        /// <summary>
        /// The desc for literal tree
        /// </summary>
        private Tree l_desc = new Tree();

        /// <summary>
        /// The desc for distance tree
        /// </summary>
        private Tree d_desc = new Tree();

        /// <summary>
        /// The desc for bit length tree
        /// </summary>
        private Tree bl_desc = new Tree();

        /// <summary>
        /// index for literals or lengths
        /// </summary>
        private int l_buf;

        /// <summary>
        /// Size of match buffer for literals/lengths.  There are 4 reasons for
        /// limiting lit_bufsize to 64K:
        ///   - frequencies can be kept in 16 bit counters
        ///   - if compression is not successful for the first block, all input
        ///     data is still in the window so we can still emit a stored block even
        ///     when input comes from standard input.  (This can also be done for
        ///     all blocks if lit_bufsize is not greater than 32K.)
        ///   - if compression is not successful for a file smaller than 64K, we can
        ///     even emit a stored file instead of a stored block (saving 5 bytes).
        ///     This is applicable only for zip (not gzip or zlib).
        ///   - creating new Huffman trees less frequently may not provide fast
        ///     adaptation to changes in the input data statistics. (Take for
        ///     example a binary file with poorly compressible code followed by
        ///     a highly compressible string table.) Smaller buffer sizes give
        ///     fast adaptation but have of course the overhead of transmitting
        ///     trees more frequently.
        ///   - I can't count above 4
        /// </summary>
        private int lit_bufsize;

        /// <summary>
        /// running index in l_buf
        /// </summary>
        private int last_lit;

        // Buffer for distances. To simplify the code, d_buf and l_buf have
        // the same number of elements. To use different lengths, an extra flag
        // array would be necessary.

        /// <summary>
        /// The index of pendig_buf
        /// </summary>
        private int d_buf;

        /// <summary>
        /// The number of string matches in current block
        /// </summary>
        private int matches;

        /// <summary>
        /// The bit length of EOB code for last block
        /// </summary>
        private int last_eob_len;

        /// <summary>
        /// Output buffer. bits are inserted starting at the bottom (least
        /// significant bits).
        /// </summary>
        private uint bi_buf;

        /// <summary>
        /// Number of valid bits in bi_buf.  All bits above the last valid bit
        /// are always zero.
        /// </summary>
        private int bi_valid;

        /// <summary>
        /// value of flush param for previous deflate call
        /// </summary>
        private FlushType last_flush;

        /// <summary>
        /// bit length of current block with optimal trees
        /// </summary>
        internal int opt_len;

        /// <summary>
        /// bit length of current block with static trees
        /// </summary>
        internal int static_len;

        /// <summary>
        /// The output still pending
        /// </summary>
        internal byte[] pending_buf;

        /// <summary>
        /// next pending byte to output to the stream
        /// </summary>
        internal int pending_out;

        /// <summary>
        /// nb of bytes in the pending buffer
        /// </summary>
        internal int pending;

        /// <summary>
        /// suppress zlib header and adler32
        /// </summary>
        internal int noheader;

        /// <summary>
        /// number of codes at each bit length for an optimal tree
        /// </summary>
        internal short[] bl_count = new short[MAX_BITS + 1];

        /// <summary>
        /// heap used to build the Huffman trees
        /// </summary>
        internal int[] heap = new int[2 * L_CODES + 1];

        /// <summary>
        /// The number of elements in the heap
        /// </summary>
        internal int heap_len;
        /// <summary>
        /// The element of largest frequency
        /// </summary>
        internal int heap_max;

        // The sons of heap[n] are heap[2*n] and heap[2*n+1]. heap[0] is not used.
        // The same heap array is used to build all trees.

        /// <summary>
        /// Depth of each subtree used as tie breaker for trees of equal frequency
        /// </summary>
        internal byte[] depth = new byte[2 * L_CODES + 1];

        public Deflate()
        {
            dyn_ltree = new short[HEAP_SIZE * 2];
            dyn_dtree = new short[(2 * D_CODES + 1) * 2]; // distance tree
            bl_tree = new short[(2 * BL_CODES + 1) * 2];  // Huffman tree for bit lengths
        }

        public ZLibStatus deflateInit(ZStream strm, CompressionLevel level, int bits)
        {
            return deflateInit2(strm, level, Z_DEFLATED, bits, DEF_MEM_LEVEL,
                CompressionStrategy.Z_DEFAULT_STRATEGY);
        }

        public ZLibStatus deflateEnd()
        {
            if (status != INIT_STATE && status != BUSY_STATE && status != FINISH_STATE)
            {
                return ZLibStatus.Z_STREAM_ERROR;
            }
            // Deallocate in reverse order of allocations:
            pending_buf = null;
            head = null;
            prev = null;
            window = null;
            // free
            // dstate=null;
            return status == BUSY_STATE ? ZLibStatus.Z_DATA_ERROR : ZLibStatus.Z_OK;
        }

        public ZLibStatus deflateParams(ZStream strm, CompressionLevel _level, CompressionStrategy _strategy)
        {
            var err = ZLibStatus.Z_OK;

            if (_level == CompressionLevel.Z_DEFAULT_COMPRESSION)
            {
                _level = (CompressionLevel)6;
            }
            if (_level < 0 || _level > (CompressionLevel)9 ||
                _strategy < 0 || _strategy > CompressionStrategy.Z_HUFFMAN_ONLY)
            {
                return ZLibStatus.Z_STREAM_ERROR;
            }

            if (_configurationTable[level].Function != _configurationTable[_level].Function &&
                strm.total_in != 0)
            {
                // Flush the last buffer:
                err = strm.deflate(FlushType.Z_PARTIAL_FLUSH);
            }

            if (level != _level)
            {
                level = _level;
                max_lazy_match = _configurationTable[level].MaxLazy;
                good_match = _configurationTable[level].GoodLength;
                nice_match = _configurationTable[level].NiceLength;
                max_chain_length = _configurationTable[level].MaxChain;
            }
            strategy = _strategy;
            return err;
        }

        public ZLibStatus deflateSetDictionary(ZStream strm, byte[] dictionary, int dictLength)
        {
            int length = dictLength;
            int index = 0;

            if (dictionary == null || status != INIT_STATE)
                return ZLibStatus.Z_STREAM_ERROR;

            //strm.adler = strm.Adler32(strm.adler, dictionary, 0, dictLength);
            strm.UpdateAdler(dictionary, 0, dictLength);


            if (length < MIN_MATCH) return ZLibStatus.Z_OK;
            if (length > w_size - MIN_LOOKAHEAD)
            {
                length = w_size - MIN_LOOKAHEAD;
                index = dictLength - length; // use the tail of the dictionary
            }
            System.Array.Copy(dictionary, index, window, 0, length);
            strstart = length;
            block_start = length;

            // Insert all strings in the hash table (except for the last two bytes).
            // s->lookahead stays null, so s->ins_h will be recomputed at the next
            // call of fill_window.

            ins_h = window[0] & 0xff;
            ins_h = (((ins_h) << hash_shift) ^ (window[1] & 0xff)) & hash_mask;

            for (int n = 0; n <= length - MIN_MATCH; n++)
            {
                ins_h = (((ins_h) << hash_shift) ^ (window[(n) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                prev[n & w_mask] = head[ins_h];
                head[ins_h] = (short)n;
            }
            return ZLibStatus.Z_OK;
        }

        public ZLibStatus deflate(ZStream strm, FlushType flush)
        {
            FlushType old_flush;

            if (flush > FlushType.Z_FINISH || flush < 0)
            {
                return ZLibStatus.Z_STREAM_ERROR;
            }

            if (strm.next_out == null ||
                (strm.next_in == null && strm.avail_in != 0) ||
                (status == FINISH_STATE && flush != FlushType.Z_FINISH))
            {
                strm.msg = z_errmsg[ZLibStatus.Z_NEED_DICT - (ZLibStatus.Z_STREAM_ERROR)];
                return ZLibStatus.Z_STREAM_ERROR;
            }
            if (strm.avail_out == 0)
            {
                strm.msg = z_errmsg[ZLibStatus.Z_NEED_DICT - (ZLibStatus.Z_BUF_ERROR)];
                return ZLibStatus.Z_BUF_ERROR;
            }

            this.strm = strm; // just in case
            old_flush = last_flush;
            last_flush = flush;

            // Write the zlib header
            if (status == INIT_STATE)
            {
                int header = (Z_DEFLATED + ((w_bits - 8) << 4)) << 8;
                int level_flags = (((int)level - 1) & 0xff) >> 1;

                if (level_flags > 3) level_flags = 3;
                header |= (level_flags << 6);
                if (strstart != 0) header |= PRESET_DICT;
                header += 31 - (header % 31);

                status = BUSY_STATE;
                putShortMSB(header);


                // Save the adler32 of the preset dictionary:
                if (strstart != 0)
                {
                    putShortMSB((int)(strm.Adler >> 16));
                    putShortMSB((int)(strm.Adler & 0xffff));
                }
                //strm.adler = strm.Adler32(0, null, 0, 0);
                strm.UpdateAdler(0, null, 0, 0);
            }

            // Flush as much pending output as possible
            if (pending != 0)
            {
                strm.flush_pending();
                if (strm.avail_out == 0)
                {
                    //System.out.println("  avail_out==0");
                    // Since avail_out is 0, deflate will be called again with
                    // more output space, but possibly with both pending and
                    // avail_in equal to zero. There won't be anything to do,
                    // but this is not an error situation so make sure we
                    // return OK instead of BUF_ERROR at next call of deflate:
                    last_flush = (FlushType)(-1);
                    return ZLibStatus.Z_OK;
                }

                // Make sure there is something to do and avoid duplicate consecutive
                // flushes. For repeated and useless calls with FlushType.Z_FINISH, we keep
                // returning ZLibStatus.Z_STREAM_END instead of Z_BUFF_ERROR.
            }
            else if (strm.avail_in == 0 && flush <= old_flush &&
                flush != FlushType.Z_FINISH)
            {
                strm.msg = z_errmsg[ZLibStatus.Z_NEED_DICT - (ZLibStatus.Z_BUF_ERROR)];
                return ZLibStatus.Z_BUF_ERROR;
            }

            // User must not provide more input after the first FINISH:
            if (status == FINISH_STATE && strm.avail_in != 0)
            {
                strm.msg = z_errmsg[ZLibStatus.Z_NEED_DICT - (ZLibStatus.Z_BUF_ERROR)];
                return ZLibStatus.Z_BUF_ERROR;
            }

            // Start a new block or continue the current one.
            if (strm.avail_in != 0 || lookahead != 0 ||
                (flush != FlushType.Z_NO_FLUSH && status != FINISH_STATE))
            {
                int bstate = -1;
                switch (_configurationTable[level].Function)
                {
                    case STORED:
                        bstate = deflate_stored(flush);
                        break;
                    case FAST:
                        bstate = deflate_fast(flush);
                        break;
                    case SLOW:
                        bstate = deflate_slow(flush);
                        break;
                    default:
                        break;
                }

                if (bstate == FinishStarted || bstate == FinishDone)
                {
                    status = FINISH_STATE;
                }
                if (bstate == NeedMore || bstate == FinishStarted)
                {
                    if (strm.avail_out == 0)
                    {
                        last_flush = (FlushType)(-1); // avoid BUF_ERROR next call, see above
                    }
                    return ZLibStatus.Z_OK;
                    // If flush != FlushType.Z_NO_FLUSH && avail_out == 0, the next call
                    // of deflate should use the same flush parameter to make sure
                    // that the flush is complete. So we don't have to output an
                    // empty block here, this will be done at next call. This also
                    // ensures that for a very small output buffer, we emit at most
                    // one empty block.
                }

                if (bstate == BlockDone)
                {
                    if (flush == FlushType.Z_PARTIAL_FLUSH)
                    {
                        _tr_align();
                    }
                    else
                    { // FULL_FLUSH or SYNC_FLUSH
                        _tr_stored_block(0, 0, false);
                        // For a full flush, this empty block will be recognized
                        // as a special marker by inflate_sync().
                        if (flush == FlushType.Z_FULL_FLUSH)
                        {
                            //state.head[s.hash_size-1]=0;
                            for (int i = 0; i < hash_size/*-1*/; i++)  // forget history
                                head[i] = 0;
                        }
                    }
                    strm.flush_pending();
                    if (strm.avail_out == 0)
                    {
                        last_flush = (FlushType)(-1); // avoid BUF_ERROR at next call, see above
                        return ZLibStatus.Z_OK;
                    }
                }
            }

            if (flush != FlushType.Z_FINISH) return ZLibStatus.Z_OK;
            if (noheader != 0) return ZLibStatus.Z_STREAM_END;

            // Write the zlib trailer (adler32)
            putShortMSB((int)(strm.Adler >> 16));
            putShortMSB((int)(strm.Adler & 0xffff));
            strm.flush_pending();

            // If avail_out is zero, the application will call deflate again
            // to flush the rest.
            noheader = -1; // write the trailer only once!
            return pending != 0 ? ZLibStatus.Z_OK : ZLibStatus.Z_STREAM_END;
        }

        private void lm_init()
        {
            window_size = 2 * w_size;

            head[hash_size - 1] = 0;
            for (int i = 0; i < hash_size - 1; i++)
            {
                head[i] = 0;
            }

            // Set the default configuration parameters:
            max_lazy_match = Deflate._configurationTable[level].MaxLazy;
            good_match = Deflate._configurationTable[level].GoodLength;
            nice_match = Deflate._configurationTable[level].NiceLength;
            max_chain_length = Deflate._configurationTable[level].MaxChain;

            strstart = 0;
            block_start = 0;
            lookahead = 0;
            match_length = prev_length = MIN_MATCH - 1;
            match_available = 0;
            ins_h = 0;
        }

        /// <summary>
        /// Initialize the tree data structures for a new zlib stream.
        /// </summary>
        private void tr_init()
        {

            //l_desc.dyn_tree = dyn_ltree;
            //l_desc.stat_desc = Deflate.static_l_desc;
            l_desc.Init(dyn_ltree, Deflate.static_l_desc);

            //d_desc.dyn_tree = dyn_dtree;
            //d_desc.stat_desc = Deflate.static_d_desc;
            d_desc.Init(dyn_ltree, Deflate.static_l_desc);

            //bl_desc.dyn_tree = bl_tree;
            //bl_desc.stat_desc = Deflate.static_bl_desc;
            bl_desc.Init(dyn_ltree, Deflate.static_l_desc);

            bi_buf = 0;
            bi_valid = 0;
            last_eob_len = 8; // enough lookahead for inflate

            // Initialize the first block of the first file:
            init_block();
        }

        private void init_block()
        {
            // Initialize the trees.
            for (int i = 0; i < L_CODES; i++) dyn_ltree[i * 2] = 0;
            for (int i = 0; i < D_CODES; i++) dyn_dtree[i * 2] = 0;
            for (int i = 0; i < BL_CODES; i++) bl_tree[i * 2] = 0;

            dyn_ltree[END_BLOCK * 2] = 1;
            opt_len = static_len = 0;
            last_lit = matches = 0;
        }

        /// <summary>
        /// Restore the heap property by moving down the tree starting at node k,
        /// exchanging a node with the smallest of its two sons if necessary, stopping
        /// when the heap property is re-established (each father smaller than its
        /// two sons).
        /// </summary>
        /// <param name="tree">The tree to restore.</param>
        /// <param name="k">The node to move down.</param>
        public void pqdownheap(short[] tree, int k)
        {
            int v = heap[k];
            int j = k << 1;  // left son of k
            while (j <= heap_len)
            {
                // Set j to the smallest of the two sons:
                if (j < heap_len &&
                    smaller(tree, heap[j + 1], heap[j], depth))
                {
                    j++;
                }
                // Exit if v is smaller than both sons
                if (smaller(tree, v, heap[j], depth)) break;

                // Exchange v with the smallest son
                heap[k] = heap[j]; k = j;
                // And continue down the tree, setting j to the left son of k
                j <<= 1;
            }
            heap[k] = v;
        }

        private static bool smaller(short[] tree, int n, int m, byte[] depth)
        {
            short tn2 = tree[n * 2];
            short tm2 = tree[m * 2];
            return (tn2 < tm2 ||
                (tn2 == tm2 && depth[n] <= depth[m]));
        }

        /// <summary>
        /// Scan a literal or distance tree to determine the frequencies of the codes
        /// in the bit length tree.
        /// </summary>
        /// <param name="tree">The tree to be scanned.</param>
        /// <param name="max_code">The largest code of non zero frequency.</param>
        private void scan_tree(short[] tree, int max_code)
        {
            int n;                     // iterates over all tree elements
            int prevlen = -1;          // last emitted length
            int curlen;                // length of current code
            int nextlen = tree[0 * 2 + 1]; // length of next code
            int count = 0;             // repeat count of the current code
            int max_count = 7;         // max repeat count
            int min_count = 4;         // min repeat count

            if (nextlen == 0) { max_count = 138; min_count = 3; }
            tree[(max_code + 1) * 2 + 1] = -1; // guard

            for (n = 0; n <= max_code; n++)
            {
                curlen = nextlen; nextlen = tree[(n + 1) * 2 + 1];
                if (++count < max_count && curlen == nextlen)
                {
                    continue;
                }
                else if (count < min_count)
                {
                    bl_tree[curlen * 2] += (short)count;
                }
                else if (curlen != 0)
                {
                    if (curlen != prevlen) bl_tree[curlen * 2]++;
                    bl_tree[REP_3_6 * 2]++;
                }
                else if (count <= 10)
                {
                    bl_tree[REPZ_3_10 * 2]++;
                }
                else
                {
                    bl_tree[REPZ_11_138 * 2]++;
                }
                count = 0; prevlen = curlen;
                if (nextlen == 0)
                {
                    max_count = 138; min_count = 3;
                }
                else if (curlen == nextlen)
                {
                    max_count = 6; min_count = 3;
                }
                else
                {
                    max_count = 7; min_count = 4;
                }
            }
        }

        /// <summary>
        /// Construct the Huffman tree for the bit lengths and return the index in
        /// bl_order of the last bit length code to send.
        /// </summary>
        /// <returns></returns>
        private int build_bl_tree()
        {
            int max_blindex;  // index of last bit length code of non zero freq

            // Determine the bit length frequencies for literal and distance trees
            scan_tree(dyn_ltree, l_desc.LargestCode);
            scan_tree(dyn_dtree, d_desc.LargestCode);

            // Build the bit length tree:
            bl_desc.BuildTree(this);
            // opt_len now includes the length of the tree representations, except
            // the lengths of the bit lengths codes and the 5+5+4 bits for the counts.

            // Determine the number of bit length codes to send. The pkzip format
            // requires that at least 4 bit length codes be sent. (appnote.txt says
            // 3 but the actual value used is 4.)
            for (max_blindex = BL_CODES - 1; max_blindex >= 3; max_blindex--)
            {
                if (bl_tree[Deflate.BLOrder[max_blindex] * 2 + 1] != 0) break;
            }
            // Update opt_len to include the bit length tree and counts
            opt_len += 3 * (max_blindex + 1) + 5 + 5 + 4;

            return max_blindex;
        }

        /// <summary>
        /// Send the header for a block using dynamic Huffman trees: the counts, the
        /// lengths of the bit length codes, the literal tree and the distance tree.
        /// </summary>
        /// <param name="lcodes">The lcodes.</param>
        /// <param name="dcodes">The dcodes.</param>
        /// <param name="blcodes">The blcodes.</param>
        private void send_all_trees(int lcodes, int dcodes, int blcodes)
        {
            int rank;                    // index in bl_order

            send_bits(lcodes - 257, 5); // not +255 as stated in appnote.txt
            send_bits(dcodes - 1, 5);
            send_bits(blcodes - 4, 4); // not -3 as stated in appnote.txt
            for (rank = 0; rank < blcodes; rank++)
            {
                send_bits(bl_tree[Deflate.BLOrder[rank] * 2 + 1], 3);
            }
            send_tree(dyn_ltree, lcodes - 1); // literal tree
            send_tree(dyn_dtree, dcodes - 1); // distance tree
        }

        /// <summary>
        /// Send a literal or distance tree in compressed form, using the codes in
        /// bl_tree.
        /// </summary>
        /// <param name="tree">The tree to be sent.</param>
        /// <param name="max_code">The largest code of non zero frequency.</param>
        private void send_tree(short[] tree, int max_code)
        {
            int n;                     // iterates over all tree elements
            int prevlen = -1;          // last emitted length
            int curlen;                // length of current code
            int nextlen = tree[0 * 2 + 1]; // length of next code
            int count = 0;             // repeat count of the current code
            int max_count = 7;         // max repeat count
            int min_count = 4;         // min repeat count

            if (nextlen == 0) { max_count = 138; min_count = 3; }

            for (n = 0; n <= max_code; n++)
            {
                curlen = nextlen; nextlen = tree[(n + 1) * 2 + 1];
                if (++count < max_count && curlen == nextlen)
                {
                    continue;
                }
                else if (count < min_count)
                {
                    do { send_code(curlen, bl_tree); } while (--count != 0);
                }
                else if (curlen != 0)
                {
                    if (curlen != prevlen)
                    {
                        send_code(curlen, bl_tree); count--;
                    }
                    send_code(REP_3_6, bl_tree);
                    send_bits(count - 3, 2);
                }
                else if (count <= 10)
                {
                    send_code(REPZ_3_10, bl_tree);
                    send_bits(count - 3, 3);
                }
                else
                {
                    send_code(REPZ_11_138, bl_tree);
                    send_bits(count - 11, 7);
                }
                count = 0; prevlen = curlen;
                if (nextlen == 0)
                {
                    max_count = 138; min_count = 3;
                }
                else if (curlen == nextlen)
                {
                    max_count = 6; min_count = 3;
                }
                else
                {
                    max_count = 7; min_count = 4;
                }
            }
        }

        /// <summary>
        /// Output a byte on the stream.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="start">The start.</param>
        /// <param name="len">The len.</param>
        private void put_byte(byte[] p, int start, int len)
        {
            System.Array.Copy(p, start, pending_buf, pending, len);
            pending += len;
        }

        private void put_short(int w)
        {
            pending_buf[pending++] = (byte)(w/*&0xff*/);
            pending_buf[pending++] = (byte)(w >> 8);
        }

        private void putShortMSB(int b)
        {
            pending_buf[pending++] = (byte)(b >> 8);
            pending_buf[pending++] = (byte)(b/*&0xff*/);
        }

        private void send_code(int c, short[] tree)
        {
            int c2 = c * 2;
            send_bits((tree[c2] & 0xffff), (tree[c2 + 1] & 0xffff));
        }

        private void send_bits(int val, int length)
        {
            if (bi_valid > Buf_size - length)
            {
                bi_buf |= (uint)(val << bi_valid);
                pending_buf[pending++] = (byte)(bi_buf/*&0xff*/);
                pending_buf[pending++] = (byte)(bi_buf >> 8);
                bi_buf = ((uint)val) >> (Buf_size - bi_valid);
                bi_valid += length - Buf_size;
            }
            else
            {
                bi_buf |= (uint)(val << bi_valid);
                bi_valid += length;
            }
            //            int len = length;
            //            if (bi_valid > (int)Buf_size - len) {
            //                int val = value;
            //                //      bi_buf |= (val << bi_valid);
            //                bi_buf = (short)((ushort)bi_buf | (ushort)((val << bi_valid)&0xffff));
            //                put_short(bi_buf);
            //                bi_buf = (short)(((uint)val) >> (Buf_size - bi_valid));
            //                bi_valid += len - Buf_size;
            //            } else {
            //                //      bi_buf |= (value) << bi_valid;
            //                bi_buf = (short)((ushort)bi_buf | (ushort)(((value) << bi_valid)&0xffff));
            //                bi_valid += len;
            //            }
        }

        /// <summary>
        /// Send one empty static block to give enough lookahead for inflate.
        /// This takes 10 bits, of which 7 may remain in the bit buffer.
        /// The current inflate code requires 9 bits of lookahead. If the
        /// last two codes for the previous block (real code plus EOB) were coded
        /// on 5 bits or less, inflate may have only 5+3 bits of lookahead to decode
        /// the last real code. In this case we send two empty static blocks instead
        /// of one. (There are no problems if the previous block is stored or fixed.)
        /// To simplify the code, we assume the worst case of last real code encoded
        /// on one bit only.
        /// </summary>
        private void _tr_align()
        {
            send_bits(STATIC_TREES << 1, 3);
            send_code(END_BLOCK, Deflate.static_ltree);

            bi_flush();

            // Of the 10 bits for the empty block, we have already sent
            // (10 - bi_valid) bits. The lookahead for the last real code (before
            // the EOB of the previous block) was thus at least one plus the length
            // of the EOB plus what we have just sent of the empty static block.
            if (1 + last_eob_len + 10 - bi_valid < 9)
            {
                send_bits(STATIC_TREES << 1, 3);
                send_code(END_BLOCK, Deflate.static_ltree);
                bi_flush();
            }
            last_eob_len = 7;
        }

        /// <summary>
        /// Save the match info and tally the frequency counts. Return true if
        /// the current block must be flushed.
        /// </summary>
        /// <param name="dist">The distance of matched string.</param>
        /// <param name="lc">The match length-MIN_MATCH or unmatched char (if dist==0).</param>
        /// <returns></returns>
        private bool _tr_tally(int dist, int lc)
        {

            pending_buf[d_buf + last_lit * 2] = (byte)(dist >> 8);
            pending_buf[d_buf + last_lit * 2 + 1] = (byte)dist;

            pending_buf[l_buf + last_lit] = (byte)lc; last_lit++;

            if (dist == 0)
            {
                // lc is the unmatched char
                dyn_ltree[lc * 2]++;
            }
            else
            {
                matches++;
                // Here, lc is the match length - MIN_MATCH
                dist--;             // dist = match distance - 1
                dyn_ltree[(_lengthCode[lc] + LITERALS + 1) * 2]++;
                dyn_dtree[Tree.DistanceCode(dist) * 2]++;
            }

            if ((last_lit & 0x1fff) == 0 && level > (CompressionLevel)2)
            {
                // Compute an upper bound for the compressed length
                int out_length = last_lit * 8;
                int in_length = strstart - block_start;
                int dcode;
                for (dcode = 0; dcode < D_CODES; dcode++)
                {
                    out_length += (int)((int)dyn_dtree[dcode * 2] *
                        (5L + Deflate.ExtraDBits[dcode]));
                }
                out_length >>= 3;
                if ((matches < (last_lit / 2)) && out_length < in_length / 2) return true;
            }

            return (last_lit == lit_bufsize - 1);
            // We avoid equality with lit_bufsize because of wraparound at 64K
            // on 16 bit machines and because stored blocks are restricted to
            // 64K-1 bytes.
        }

        /// <summary>
        /// Send the block data compressed using the given Huffman trees
        /// </summary>
        /// <param name="ltree">The ltree.</param>
        /// <param name="dtree">The dtree.</param>
        private void compress_block(short[] ltree, short[] dtree)
        {
            int dist;      // distance of matched string
            int lc;         // match length or unmatched char (if dist == 0)
            int lx = 0;     // running index in l_buf
            int code;       // the code to send
            int extra;      // number of extra bits to send

            if (last_lit != 0)
            {
                do
                {
                    dist = ((pending_buf[d_buf + lx * 2] << 8) & 0xff00) |
                        (pending_buf[d_buf + lx * 2 + 1] & 0xff);
                    lc = (pending_buf[l_buf + lx]) & 0xff; lx++;

                    if (dist == 0)
                    {
                        send_code(lc, ltree); // send a literal byte
                    }
                    else
                    {
                        // Here, lc is the match length - MIN_MATCH
                        code = _lengthCode[lc];

                        send_code(code + LITERALS + 1, ltree); // send the length code
                        extra = Deflate.ExtraLBits[code];
                        if (extra != 0)
                        {
                            lc -= _baseLength[code];
                            send_bits(lc, extra);       // send the extra length bits
                        }
                        dist--; // dist is now the match distance - 1
                        code = Tree.DistanceCode(dist);

                        send_code(code, dtree);       // send the distance code
                        extra = Deflate.ExtraDBits[code];
                        if (extra != 0)
                        {
                            dist -= _baseDistance[code];
                            send_bits(dist, extra);   // send the extra distance bits
                        }
                    } // literal or match pair ?

                    // Check that the overlay between pending_buf and d_buf+l_buf is ok:
                }
                while (lx < last_lit);
            }

            send_code(END_BLOCK, ltree);
            last_eob_len = ltree[END_BLOCK * 2 + 1];
        }

        /// <summary>
        /// Set the data type to ASCII or BINARY, using a crude approximation:
        /// binary if more than 20% of the bytes are <= 6 or >= 128, ascii otherwise.
        /// IN assertion: the fields freq of dyn_ltree are set and the total of all
        /// frequencies does not exceed 64K (to fit in an int on 16 bit machines).
        /// </summary>
        private void set_data_type()
        {
            int n = 0;
            int ascii_freq = 0;
            int bin_freq = 0;
            while (n < 7) { bin_freq += dyn_ltree[n * 2]; n++; }
            while (n < 128) { ascii_freq += dyn_ltree[n * 2]; n++; }
            while (n < LITERALS) { bin_freq += dyn_ltree[n * 2]; n++; }
            _dataType = (bin_freq > (ascii_freq >> 2) ? BlockType.Z_BINARY : BlockType.Z_ASCII);
        }

        /// <summary>
        /// Flush the bit buffer, keeping at most 7 bits in it.
        /// </summary>
        private void bi_flush()
        {
            if (bi_valid == 16)
            {
                pending_buf[pending++] = (byte)(bi_buf/*&0xff*/);
                pending_buf[pending++] = (byte)(bi_buf >> 8);
                bi_buf = 0;
                bi_valid = 0;
            }
            else if (bi_valid >= 8)
            {
                pending_buf[pending++] = (byte)(bi_buf);
                bi_buf >>= 8;
                bi_buf &= 0x00ff;
                bi_valid -= 8;
            }
        }

        /// <summary>
        /// Flush the bit buffer and align the output on a byte boundary
        /// </summary>
        private void bi_windup()
        {
            if (bi_valid > 8)
            {
                pending_buf[pending++] = (byte)(bi_buf);
                pending_buf[pending++] = (byte)(bi_buf >> 8);
            }
            else if (bi_valid > 0)
            {
                pending_buf[pending++] = (byte)(bi_buf);
            }
            bi_buf = 0;
            bi_valid = 0;
        }

        /// <summary>
        /// Copy a stored block, storing first the length and its
        /// one's complement if requested.
        /// </summary>
        /// <param name="buf">The input data.</param>
        /// <param name="len">The length.</param>
        /// <param name="header">if set to <c>true</c> block header must be written.</param>
        private void copy_block(int buf, int len, bool header)
        {
            //int index=0;
            bi_windup();      // align on byte boundary
            last_eob_len = 8; // enough lookahead for inflate

            if (header)
            {
                put_short((short)len);
                put_short((short)~len);
            }

            //  while(len--!=0) {
            //    put_byte(window[buf+index]);
            //    index++;
            //  }
            put_byte(window, buf, len);
        }

        private void flush_block_only(bool eof)
        {
            _tr_flush_block(block_start >= 0 ? block_start : -1,
                strstart - block_start,
                eof);
            block_start = strstart;
            strm.flush_pending();
        }

        /// <summary>
        /// Copy without compression as much as possible from the input stream, return
        /// the current block state.
        /// This function does not insert new strings in the dictionary since
        /// uncompressible data is probably not useful. This function is used
        /// only for the level=0 compression option.
        /// </summary>
        /// <param name="flush">The flush.</param>
        /// <returns></returns>
        private int deflate_stored(FlushType flush)
        {
            // TODO: this function should be optimized to avoid extra copying from window to pending_buf.

            // Stored blocks are limited to 0xffff bytes, pending_buf is limited
            // to pending_buf_size, and each stored block has a 5 byte header:

            int max_block_size = 0xffff;
            int max_start;

            if (max_block_size > pending_buf_size - 5)
            {
                max_block_size = pending_buf_size - 5;
            }

            // Copy as much as possible from input to output:
            while (true)
            {
                // Fill the window as much as possible:
                if (lookahead <= 1)
                {
                    fill_window();
                    if (lookahead == 0 && flush == FlushType.Z_NO_FLUSH) return NeedMore;
                    if (lookahead == 0) break; // flush the current block
                }

                strstart += lookahead;
                lookahead = 0;

                // Emit a stored block if pending_buf will be full:
                max_start = block_start + max_block_size;
                if (strstart == 0 || strstart >= max_start)
                {
                    // strstart == 0 is possible when wraparound on 16-bit machine
                    lookahead = (int)(strstart - max_start);
                    strstart = (int)max_start;

                    flush_block_only(false);
                    if (strm.avail_out == 0) return NeedMore;

                }

                // Flush if we may have to slide, otherwise block_start may become
                // negative and the data will be gone:
                if (strstart - block_start >= w_size - MIN_LOOKAHEAD)
                {
                    flush_block_only(false);
                    if (strm.avail_out == 0) return NeedMore;
                }
            }

            flush_block_only(flush == FlushType.Z_FINISH);
            if (strm.avail_out == 0)
                return (flush == FlushType.Z_FINISH) ? FinishStarted : NeedMore;

            return flush == FlushType.Z_FINISH ? FinishDone : BlockDone;
        }

        /// <summary>
        /// Send a stored block
        /// </summary>
        /// <param name="buf">The input block.</param>
        /// <param name="stored_len">The length of input block.</param>
        /// <param name="eof">if set to <c>true</c> last block for a file.</param>
        private void _tr_stored_block(int buf, int stored_len, bool eof)
        {
            send_bits((STORED_BLOCK << 1) + (eof ? 1 : 0), 3);  // send block type
            copy_block(buf, stored_len, true);          // with header
        }

        /// <summary>
        /// Determine the best encoding for the current block: dynamic trees, static
        /// trees or store, and output the encoded block to the zip file.
        /// </summary>
        /// <param name="buf">The input block, or NULL if too old.</param>
        /// <param name="stored_len">The length of input block.</param>
        /// <param name="eof">if set to <c>true</c> the last block for a file.</param>
        private void _tr_flush_block(int buf, int stored_len, bool eof)
        {
            int opt_lenb, static_lenb;// opt_len and static_len in bytes
            int max_blindex = 0;      // index of last bit length code of non zero freq

            // Build the Huffman trees unless a stored block is forced
            if (level > 0)
            {
                // Check if the file is ascii or binary
                if (_dataType == BlockType.Z_UNKNOWN) set_data_type();

                // Construct the literal and distance trees
                l_desc.BuildTree(this);

                d_desc.BuildTree(this);

                // At this point, opt_len and static_len are the total bit lengths of
                // the compressed block data, excluding the tree representations.

                // Build the bit length tree for the above two trees, and get the index
                // in bl_order of the last bit length code to send.
                max_blindex = build_bl_tree();

                // Determine the best encoding. Compute first the block length in bytes
                opt_lenb = (opt_len + 3 + 7) >> 3;
                static_lenb = (static_len + 3 + 7) >> 3;

                if (static_lenb <= opt_lenb) opt_lenb = static_lenb;
            }
            else
            {
                opt_lenb = static_lenb = stored_len + 5; // force a stored block
            }

            if (stored_len + 4 <= opt_lenb && buf != -1)
            {
                // 4: two words for the lengths
                // The test buf != NULL is only necessary if LIT_BUFSIZE > WSIZE.
                // Otherwise we can't have processed more than WSIZE input bytes since
                // the last block flush, because compression would have been
                // successful. If LIT_BUFSIZE <= WSIZE, it is never too late to
                // transform a block into a stored block.
                _tr_stored_block(buf, stored_len, eof);
            }
            else if (static_lenb == opt_lenb)
            {
                send_bits((STATIC_TREES << 1) + (eof ? 1 : 0), 3);
                compress_block(Deflate.static_ltree, Deflate.static_dtree);
            }
            else
            {
                send_bits((DYN_TREES << 1) + (eof ? 1 : 0), 3);
                send_all_trees(l_desc.LargestCode + 1, d_desc.LargestCode + 1, max_blindex + 1);
                compress_block(dyn_ltree, dyn_dtree);
            }

            // The above check is made mod 2^32, for files larger than 512 MB
            // and uLong implemented on 32 bits.

            init_block();

            if (eof)
            {
                bi_windup();
            }
        }

        /// <summary>
        /// Fill the window when the lookahead becomes insufficient.
        /// Updates strstart and lookahead.
        /// </summary>
        private void fill_window()
        {
            int n, m;
            int p;
            int more;    // Amount of free space at the end of the window.

            do
            {
                more = (window_size - lookahead - strstart);

                // Deal with !@#$% 64K limit:
                if (more == 0 && strstart == 0 && lookahead == 0)
                {
                    more = w_size;
                }
                else if (more == -1)
                {
                    // Very unlikely, but possible on 16 bit machine if strstart == 0
                    // and lookahead == 1 (input done one byte at time)
                    more--;

                    // If the window is almost full and there is insufficient lookahead,
                    // move the upper half to the lower one to make room in the upper half.
                }
                else if (strstart >= w_size + w_size - MIN_LOOKAHEAD)
                {
                    System.Array.Copy(window, w_size, window, 0, w_size);
                    match_start -= w_size;
                    strstart -= w_size; // we now have strstart >= MAX_DIST
                    block_start -= w_size;

                    // Slide the hash table (could be avoided with 32 bit values
                    // at the expense of memory usage). We slide even when level == 0
                    // to keep the hash table consistent if we switch back to level > 0
                    // later. (Using level 0 permanently is not an optimal usage of
                    // zlib, so we don't care about this pathological case.)

                    n = hash_size;
                    p = n;
                    do
                    {
                        m = (head[--p] & 0xffff);
                        head[p] = (short)(m >= w_size ? (m - w_size) : 0);
                    }
                    while (--n != 0);

                    n = w_size;
                    p = n;
                    do
                    {
                        m = (prev[--p] & 0xffff);
                        prev[p] = (short)(m >= w_size ? (m - w_size) : 0);
                        // If n is not on any hash chain, prev[n] is garbage but
                        // its value will never be used.
                    }
                    while (--n != 0);
                    more += w_size;
                }

                if (strm.avail_in == 0) return;

                // If there was no sliding:
                //    strstart <= WSIZE+MAX_DIST-1 && lookahead <= MIN_LOOKAHEAD - 1 &&
                //    more == window_size - lookahead - strstart
                // => more >= window_size - (MIN_LOOKAHEAD-1 + WSIZE + MAX_DIST-1)
                // => more >= window_size - 2*WSIZE + 2
                // In the BIG_MEM or MMAP case (not yet supported),
                //   window_size == input_size + MIN_LOOKAHEAD  &&
                //   strstart + s->lookahead <= input_size => more >= MIN_LOOKAHEAD.
                // Otherwise, window_size == 2*WSIZE so more >= 2.
                // If there was sliding, more >= WSIZE. So in all cases, more >= 2.

                n = strm.read_buf(window, strstart + lookahead, more);
                lookahead += n;

                // Initialize the hash value now that we have some input:
                if (lookahead >= MIN_MATCH)
                {
                    ins_h = window[strstart] & 0xff;
                    ins_h = (((ins_h) << hash_shift) ^ (window[strstart + 1] & 0xff)) & hash_mask;
                }
                // If the whole input has less than MIN_MATCH bytes, ins_h is garbage,
                // but this is not important since only literal bytes will be emitted.
            }
            while (lookahead < MIN_LOOKAHEAD && strm.avail_in != 0);
        }

        /// <summary>
        /// Compress as much as possible from the input stream, return the current
        /// block state.
        /// This function does not perform lazy evaluation of matches and inserts
        /// new strings in the dictionary only for unmatched strings or for short
        /// matches. It is used only for the fast compression options.
        /// </summary>
        /// <param name="flush">The flush.</param>
        /// <returns></returns>
        private int deflate_fast(FlushType flush)
        {
            //    short hash_head = 0; // head of the hash chain
            int hash_head = 0; // head of the hash chain
            bool bflush;      // set if current block must be flushed

            while (true)
            {
                // Make sure that we always have enough lookahead, except
                // at the end of the input file. We need MAX_MATCH bytes
                // for the next match, plus MIN_MATCH bytes to insert the
                // string following the next match.
                if (lookahead < MIN_LOOKAHEAD)
                {
                    fill_window();
                    if (lookahead < MIN_LOOKAHEAD && flush == FlushType.Z_NO_FLUSH)
                    {
                        return NeedMore;
                    }
                    if (lookahead == 0) break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:
                if (lookahead >= MIN_MATCH)
                {
                    ins_h = (((ins_h) << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;

                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (head[ins_h] & 0xffff);
                    prev[strstart & w_mask] = head[ins_h];
                    head[ins_h] = (short)strstart;
                }

                // Find the longest match, discarding those <= prev_length.
                // At this point we have always match_length < MIN_MATCH

                if (hash_head != 0L &&
                    ((strstart - hash_head) & 0xffff) <= w_size - MIN_LOOKAHEAD
                    )
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).
                    if (strategy != CompressionStrategy.Z_HUFFMAN_ONLY)
                    {
                        match_length = longest_match(hash_head);
                    }
                    // longest_match() sets match_start
                }
                if (match_length >= MIN_MATCH)
                {
                    //        check_match(strstart, match_start, match_length);

                    bflush = _tr_tally(strstart - match_start, match_length - MIN_MATCH);

                    lookahead -= match_length;

                    // Insert new strings in the hash table only if the match length
                    // is not too large. This saves time but degrades compression.
                    if (match_length <= max_lazy_match &&
                        lookahead >= MIN_MATCH)
                    {
                        match_length--; // string at strstart already in hash table
                        do
                        {
                            strstart++;

                            ins_h = ((ins_h << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                            //      prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (head[ins_h] & 0xffff);
                            prev[strstart & w_mask] = head[ins_h];
                            head[ins_h] = (short)strstart;

                            // strstart never exceeds WSIZE-MAX_MATCH, so there are
                            // always MIN_MATCH bytes ahead.
                        }
                        while (--match_length != 0);
                        strstart++;
                    }
                    else
                    {
                        strstart += match_length;
                        match_length = 0;
                        ins_h = window[strstart] & 0xff;

                        ins_h = (((ins_h) << hash_shift) ^ (window[strstart + 1] & 0xff)) & hash_mask;
                        // If lookahead < MIN_MATCH, ins_h is garbage, but it does not
                        // matter since it will be recomputed at next deflate call.
                    }
                }
                else
                {
                    // No match, output a literal byte

                    bflush = _tr_tally(0, window[strstart] & 0xff);
                    lookahead--;
                    strstart++;
                }
                if (bflush)
                {

                    flush_block_only(false);
                    if (strm.avail_out == 0) return NeedMore;
                }
            }

            flush_block_only(flush == FlushType.Z_FINISH);
            if (strm.avail_out == 0)
            {
                if (flush == FlushType.Z_FINISH) return FinishStarted;
                else return NeedMore;
            }
            return flush == FlushType.Z_FINISH ? FinishDone : BlockDone;
        }

        /// <summary>
        /// Same as above, but achieves better compression. We use a lazy
        /// evaluation for matches: a match is finally adopted only if there is
        /// no better match at the next window position.
        /// </summary>
        /// <param name="flush">The flush.</param>
        /// <returns></returns>
        private int deflate_slow(FlushType flush)
        {
            //    short hash_head = 0;    // head of hash chain
            int hash_head = 0;    // head of hash chain
            bool bflush;         // set if current block must be flushed

            // Process the input block.
            while (true)
            {
                // Make sure that we always have enough lookahead, except
                // at the end of the input file. We need MAX_MATCH bytes
                // for the next match, plus MIN_MATCH bytes to insert the
                // string following the next match.

                if (lookahead < MIN_LOOKAHEAD)
                {
                    fill_window();
                    if (lookahead < MIN_LOOKAHEAD && flush == FlushType.Z_NO_FLUSH)
                    {
                        return NeedMore;
                    }
                    if (lookahead == 0) break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:

                if (lookahead >= MIN_MATCH)
                {
                    ins_h = (((ins_h) << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (head[ins_h] & 0xffff);
                    prev[strstart & w_mask] = head[ins_h];
                    head[ins_h] = (short)strstart;
                }

                // Find the longest match, discarding those <= prev_length.
                prev_length = match_length; prev_match = match_start;
                match_length = MIN_MATCH - 1;

                if (hash_head != 0 && prev_length < max_lazy_match &&
                    ((strstart - hash_head) & 0xffff) <= w_size - MIN_LOOKAHEAD
                    )
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).

                    if (strategy != CompressionStrategy.Z_HUFFMAN_ONLY)
                    {
                        match_length = longest_match(hash_head);
                    }
                    // longest_match() sets match_start

                    if (match_length <= 5 && (strategy == CompressionStrategy.Z_FILTERED ||
                        (match_length == MIN_MATCH &&
                        strstart - match_start > 4096)))
                    {

                        // If prev_match is also MIN_MATCH, match_start is garbage
                        // but we will ignore the current match anyway.
                        match_length = MIN_MATCH - 1;
                    }
                }

                // If there was a match at the previous step and the current
                // match is not better, output the previous match:
                if (prev_length >= MIN_MATCH && match_length <= prev_length)
                {
                    int max_insert = strstart + lookahead - MIN_MATCH;
                    // Do not insert strings in hash table beyond this.

                    //          check_match(strstart-1, prev_match, prev_length);

                    bflush = _tr_tally(strstart - 1 - prev_match, prev_length - MIN_MATCH);

                    // Insert in hash table all strings up to the end of the match.
                    // strstart-1 and strstart are already inserted. If there is not
                    // enough lookahead, the last two strings are not inserted in
                    // the hash table.
                    lookahead -= prev_length - 1;
                    prev_length -= 2;
                    do
                    {
                        if (++strstart <= max_insert)
                        {
                            ins_h = (((ins_h) << hash_shift) ^ (window[(strstart) + (MIN_MATCH - 1)] & 0xff)) & hash_mask;
                            //prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (head[ins_h] & 0xffff);
                            prev[strstart & w_mask] = head[ins_h];
                            head[ins_h] = (short)strstart;
                        }
                    }
                    while (--prev_length != 0);
                    match_available = 0;
                    match_length = MIN_MATCH - 1;
                    strstart++;

                    if (bflush)
                    {
                        flush_block_only(false);
                        if (strm.avail_out == 0) return NeedMore;
                    }
                }
                else if (match_available != 0)
                {

                    // If there was no match at the previous position, output a
                    // single literal. If there was a match but the current match
                    // is longer, truncate the previous match to a single literal.

                    bflush = _tr_tally(0, window[strstart - 1] & 0xff);

                    if (bflush)
                    {
                        flush_block_only(false);
                    }
                    strstart++;
                    lookahead--;
                    if (strm.avail_out == 0) return NeedMore;
                }
                else
                {
                    // There is no previous match to compare with, wait for
                    // the next step to decide.

                    match_available = 1;
                    strstart++;
                    lookahead--;
                }
            }

            if (match_available != 0)
            {
                bflush = _tr_tally(0, window[strstart - 1] & 0xff);
                match_available = 0;
            }
            flush_block_only(flush == FlushType.Z_FINISH);

            if (strm.avail_out == 0)
            {
                if (flush == FlushType.Z_FINISH) return FinishStarted;
                else return NeedMore;
            }

            return flush == FlushType.Z_FINISH ? FinishDone : BlockDone;
        }

        private int longest_match(int cur_match)
        {
            int chain_length = max_chain_length; // max hash chain length
            int scan = strstart;                 // current string
            int match;                           // matched string
            int len;                             // length of current match
            int best_len = prev_length;          // best match length so far
            int limit = strstart > (w_size - MIN_LOOKAHEAD) ?
                strstart - (w_size - MIN_LOOKAHEAD) : 0;
            int nice_match = this.nice_match;

            // Stop when cur_match becomes <= limit. To simplify the code,
            // we prevent matches with the string of window index 0.

            int wmask = w_mask;

            int strend = strstart + MAX_MATCH;
            byte scan_end1 = window[scan + best_len - 1];
            byte scan_end = window[scan + best_len];

            // The code is optimized for HASH_BITS >= 8 and MAX_MATCH-2 multiple of 16.
            // It is easy to get rid of this optimization if necessary.

            // Do not waste too much time if we already have a good match:
            if (prev_length >= good_match)
            {
                chain_length >>= 2;
            }

            // Do not look for matches beyond the end of the input. This is necessary
            // to make deflate deterministic.
            if (nice_match > lookahead) nice_match = lookahead;

            do
            {
                match = cur_match;

                // Skip to next match if the match length cannot increase
                // or if the match length is less than 2:
                if (window[match + best_len] != scan_end ||
                    window[match + best_len - 1] != scan_end1 ||
                    window[match] != window[scan] ||
                    window[++match] != window[scan + 1]) continue;

                // The check at best_len-1 can be removed because it will be made
                // again later. (This heuristic is not always a win.)
                // It is not necessary to compare scan[2] and match[2] since they
                // are always equal when the other bytes match, given that
                // the hash keys are equal and that HASH_BITS >= 8.
                scan += 2; match++;

                // We check for insufficient lookahead only every 8th comparison;
                // the 256th check will be made at strstart+258.
                do
                {
                } while (window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    scan < strend);

                len = MAX_MATCH - (int)(strend - scan);
                scan = strend - MAX_MATCH;

                if (len > best_len)
                {
                    match_start = cur_match;
                    best_len = len;
                    if (len >= nice_match) break;
                    scan_end1 = window[scan + best_len - 1];
                    scan_end = window[scan + best_len];
                }

            } while ((cur_match = (prev[cur_match & wmask] & 0xffff)) > limit
                && --chain_length != 0);

            if (best_len <= lookahead) return best_len;
            return lookahead;
        }
        
        private ZLibStatus deflateInit2(ZStream strm, CompressionLevel level, int method, int windowBits, int memLevel, CompressionStrategy strategy)
        {
            int noheader = 0;
            //    byte[] my_version=ZLIB_VERSION;

            //
            //  if (version == null || version[0] != my_version[0]
            //  || stream_size != sizeof(z_stream)) {
            //  return ZLibStatus.Z_VERSION_ERROR;
            //  }

            strm.msg = null;

            if (level == CompressionLevel.Z_DEFAULT_COMPRESSION) level = (CompressionLevel)6;

            if (windowBits < 0)
            { // undocumented feature: suppress zlib header
                noheader = 1;
                windowBits = -windowBits;
            }

            if (memLevel < 1 || memLevel > MAX_MEM_LEVEL ||
                method != Z_DEFLATED ||
                windowBits < 9 || windowBits > 15 || level < 0 || level > (CompressionLevel)9 ||
                strategy < 0 || strategy > CompressionStrategy.Z_HUFFMAN_ONLY)
            {
                return ZLibStatus.Z_STREAM_ERROR;
            }

            strm.dstate = (Deflate)this;

            this.noheader = noheader;
            w_bits = windowBits;
            w_size = 1 << w_bits;
            w_mask = w_size - 1;

            hash_bits = memLevel + 7;
            hash_size = 1 << hash_bits;
            hash_mask = hash_size - 1;
            hash_shift = ((hash_bits + MIN_MATCH - 1) / MIN_MATCH);

            window = new byte[w_size * 2];
            prev = new short[w_size];
            head = new short[hash_size];

            lit_bufsize = 1 << (memLevel + 6); // 16K elements by default

            // We overlay pending_buf and d_buf+l_buf. This works since the average
            // output size for (length,distance) codes is <= 24 bits.
            pending_buf = new byte[lit_bufsize * 4];
            pending_buf_size = lit_bufsize * 4;

            d_buf = lit_bufsize / 2;
            l_buf = (1 + 2) * lit_bufsize;

            this.level = level;

            //System.out.println("level="+level);

            this.strategy = strategy;
            this.method = (byte)method;

            return deflateReset(strm);
        }

        private ZLibStatus deflateReset(ZStream strm)
        {
            strm.total_in = strm.total_out = 0;
            strm.msg = null; //
            //strm.data_type = Z_UNKNOWN;

            pending = 0;
            pending_out = 0;

            if (noheader < 0)
            {
                noheader = 0; // was set to -1 by deflate(..., FlushType.Z_FINISH);
            }
            status = (noheader != 0) ? BUSY_STATE : INIT_STATE;
            //strm.adler = strm.Adler32(0, null, 0, 0);
            strm.UpdateAdler(0L, null, 0, 0);


            last_flush = FlushType.Z_NO_FLUSH;

            tr_init();
            lm_init();
            return ZLibStatus.Z_OK;
        }

        private class Config
        {
            /// <summary>
            /// Reduce lazy search above this match length
            /// </summary>
            internal int GoodLength { get; private set; }
            /// <summary>
            /// Gets or sets whether not to perform lazy search above this match length
            /// </summary>
            /// <value>
            /// The max lazy.
            /// </value>
            internal int MaxLazy { get; private set; }
            /// <summary>
            /// Gets or sets whether to quit search above this match length
            /// </summary>
            /// <value>
            /// The length of the nice.
            /// </value>
            internal int NiceLength { get; private set; }
            internal int MaxChain { get; private set; }
            internal int Function { get; private set; }
            internal Config(int goodLength, int maxLazy, int niceLength, int maxChain, int function)
            {
                this.GoodLength = goodLength;
                this.MaxLazy = maxLazy;
                this.NiceLength = niceLength;
                this.MaxChain = maxChain;
                this.Function = function;
            }
        }

    }
}