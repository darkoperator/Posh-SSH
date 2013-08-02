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

    public enum DefalteFlavor : int
    {
        STORED = 0,
        FAST = 1,
        SLOW = 2
    }

    public sealed class Deflate : ZStream, ICompressor
    {
        private const int MAX_MEM_LEVEL = 9;

        private const int Z_DEFAULT_COMPRESSION = -1;

        private const int MAX_WBITS = 15;            // 32K LZ77 window

        private const int DEF_MEM_LEVEL = 8;

        /// <summary>
        /// The Bit length codes must not exceed MAX_BL_BITS bits
        /// </summary>
        private const int MAX_BL_BITS = 7;

        // block not completed, need more input or more output
        private const int NeedMore = 0;

        // block flush performed
        private const int BlockDone = 1;

        // finish started, need only more output at next deflate
        private const int FinishStarted = 2;

        // finish done, accept no more input or output
        private const int FinishDone = 3;

        // preset dictionary flag in zlib header
        private const int PRESET_DICT = 0x20;

        private const int Z_VERSION_ERROR = -6;

        private const int INIT_STATE = 42;
        private const int BUSY_STATE = 113;
        private const int FINISH_STATE = 666;

        // The deflate compression method
        private const int Z_DEFLATED = 8;

        private const int STORED_BLOCK = 0;
        private const int STATIC_TREES = 1;
        private const int DYN_TREES = 2;

        private const int Buf_size = 8 * 2;

        // repeat previous bit length 3-6 times (2 bits of repeat count)
        private const int REP_3_6 = 16;

        // repeat a zero length 3-10 times  (3 bits of repeat count)
        private const int REPZ_3_10 = 17;

        // repeat a zero length 11-138 times  (7 bits of repeat count)
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

        private static readonly IDictionary<CompressionLevel, Config> config_table = new Dictionary<CompressionLevel, Config>() { 
            {(CompressionLevel)0, new Config(0, 0, 0, 0, DefalteFlavor.STORED)},
            {(CompressionLevel)1, new Config(4, 4, 8, 4, DefalteFlavor.FAST)},
            {(CompressionLevel)2, new Config(4, 5, 16, 8, DefalteFlavor.FAST)},
            {(CompressionLevel)3, new Config(4, 6, 32, 32, DefalteFlavor.FAST)},

            {(CompressionLevel)4, new Config(4, 4, 16, 16, DefalteFlavor.SLOW)},
            {(CompressionLevel)5, new Config(8, 16, 32, 32, DefalteFlavor.SLOW)},
            {(CompressionLevel)6, new Config(8, 16, 128, 128, DefalteFlavor.SLOW)},
            {(CompressionLevel)7, new Config(8, 32, 128, 256, DefalteFlavor.SLOW)},
            {(CompressionLevel)8, new Config(32, 128, 258, 1024, DefalteFlavor.SLOW)},
            {(CompressionLevel)9, new Config(32, 258, 258, 4096, DefalteFlavor.SLOW)},
        };

        private static readonly StaticTree static_l_desc = new StaticTree(static_ltree, Deflate.extra_lbits, LITERALS + 1, L_CODES, MAX_BITS);

        private static readonly StaticTree static_d_desc = new StaticTree(static_dtree, Deflate.extra_dbits, 0, D_CODES, MAX_BITS);

        private static readonly StaticTree static_bl_desc = new StaticTree(null, Deflate.extra_blbits, 0, BL_CODES, MAX_BL_BITS);

        #region Static definitions

        private static readonly byte[] _length_code ={
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

        private static readonly int[] base_length = {
                                               0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 12, 14, 16, 20, 24, 28, 32, 40, 48, 56,
                                               64, 80, 96, 112, 128, 160, 192, 224, 0
                                           };

        private static readonly int[] base_dist = {
                                             0,   1,      2,     3,     4,    6,     8,    12,    16,     24,
                                             32,  48,     64,    96,   128,  192,   256,   384,   512,    768,
                                             1024, 1536,  2048,  3072,  4096,  6144,  8192, 12288, 16384, 24576
                                         };

        // extra bits for each length code
        private static readonly int[] extra_lbits ={
                                             0,0,0,0,0,0,0,0,1,1,1,1,2,2,2,2,3,3,3,3,4,4,4,4,5,5,5,5,0
                                         };

        // extra bits for each distance code
        private static readonly int[] extra_dbits ={
                                             0,0,0,0,1,1,2,2,3,3,4,4,5,5,6,6,7,7,8,8,9,9,10,10,11,11,12,12,13,13
                                         };

        // extra bits for each bit length code
        private static readonly int[] extra_blbits ={
                                              0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,3,7
                                          };

        private static readonly byte[] bl_order ={
                                            16,17,18,0,8,7,9,6,10,5,11,4,12,3,13,2,14,1,15};

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
        #endregion

        private static readonly String[] z_errmsg = {
                                               "need dictionary",     // ZLibStatus.Z_NEED_DICT       2
                                               "stream end",          // ZLibStatus.Z_STREAM_END      1
                                               "",                    // ZLibStatus.Z_OK              0
                                               "file error",          // ZLibStatus.Z_ERRNO         (-1)
                                               "stream error",        // ZLibStatus.Z_STREAM_ERROR  (-2)
                                               "data error",          // ZLibStatus.Z_DATA_ERROR    (-3)
                                               "insufficient memory", // Z_MEM_ERROR     (-4)
                                               "buffer error",        // ZLibStatus.Z_BUF_ERROR     (-5)
                                               "incompatible version",// Z_VERSION_ERROR (-6)
                                               ""
                                           };



        /// <summary>
        /// as the name implies
        /// </summary>
        private int _status;

        private BlockType _dataType;

        /// <summary>
        /// STORED (for zip only) or DEFLATED
        /// </summary>
        private byte _method;

        /// <summary>
        /// size of pending_buf
        /// </summary>
        private int _pendingBufferSize;

        /// <summary>
        /// LZ77 window size (32K by default)
        /// </summary>
        private int _windowSize;

        /// <summary>
        /// log2(w_size)  (8..16)
        /// </summary>
        private int _windowBits;

        /// <summary>
        /// w_size - 1
        /// </summary>
        private int _windowMask;

        /// <summary>
        /// Sliding window. Input bytes are read into the second half of the window,
        /// and move to the first half later to keep a dictionary of at least wSize
        /// bytes. With this organization, matches are limited to a distance of
        /// wSize-MAX_MATCH bytes, but this ensures that IO is always
        /// performed with a length multiple of the block size. Also, it limits
        /// the window size to 64K, which is quite useful on MSDOS.
        /// To do: use the user input buffer as sliding window.
        /// </summary>
        private byte[] _window;

        /// <summary>
        /// Actual size of window: 2*wSize, except when the user input buffer
        /// is directly used as sliding window.
        /// </summary>
        private int _windowActualSize;

        /// <summary>
        /// Link to older string with same hash index. To limit the size of this
        /// array to 64K, this link is maintained only for the last 32K strings.
        /// An index in this array is thus a window index modulo 32K.
        /// </summary>
        private short[] _previous;

        /// <summary>
        /// Heads of the hash chains or NIL.
        /// </summary>
        private short[] _head;

        /// <summary>
        /// hash index of string to be inserted
        /// </summary>
        private int _insertedHashIndex;

        /// <summary>
        /// number of elements in hash table
        /// </summary>
        private int _hashSize;

        /// <summary>
        /// log2(hash_size)
        /// </summary>
        private int _hashBits;

        /// <summary>
        /// hash_size-1
        /// </summary>
        private int _hashMask;

        /// <summary>
        /// Number of bits by which ins_h must be shifted at each input
        /// step. It must be such that after MIN_MATCH steps, the oldest
        /// byte no longer takes part in the hash key, that is:
        /// hash_shift * MIN_MATCH >= hash_bits
        /// </summary>
        private int _hashShift;

        /// <summary>
        /// Window position at the beginning of the current output block. Gets
        /// negative when the window is moved backwards.
        /// </summary>
        private int _blockStart;

        /// <summary>
        /// length of best match
        /// </summary>
        private int _matchLength;

        /// <summary>
        /// previous match
        /// </summary>
        private int _previousMatch;

        /// <summary>
        /// set if previous match exists
        /// </summary>
        private bool _matchAvailable;

        /// <summary>
        /// start of string to insert
        /// </summary>
        private int _startInsertString;

        /// <summary>
        /// start of matching string
        /// </summary>
        private int _startMatchString;

        /// <summary>
        /// number of valid bytes ahead in window
        /// </summary>
        private int _validBytesAhead;

        /// <summary>
        /// Length of the best match at previous step. Matches not greater than this
        /// are discarded. This is used in the lazy match evaluation.
        /// </summary>
        private int _previousLength;

        /// <summary>
        /// To speed up deflation, hash chains are never searched beyond this
        /// length.  A higher limit improves compression ratio but degrades the speed.
        /// </summary>
        private int _maxChainLength;

        /// <summary>
        /// Attempt to find a better match only when the current match is strictly
        /// smaller than this value. This mechanism is used only for compression
        /// levels >= 4.
        /// </summary>
        private int _maxLazyMatch;

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
        private uint _bitBuffer;

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

        public Deflate(CompressionLevel level)
            : this()
        {
            this.deflateInit(level);
        }

        public Deflate(CompressionLevel level, bool nowrap)
            : this()
        {
            this.deflateInit(level, nowrap);
        }


        public ZLibStatus deflate(byte[] inputBufer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset, int outputCount, FlushType flushType)
        {
            base.next_in = inputBufer;
            base.next_in_index = inputOffset;
            base.avail_in = inputCount;
            base.next_out = outputBuffer;
            base.next_out_index = outputOffset;
            base.avail_out = outputCount;
            return this.deflate(flushType);
        }

        public ZLibStatus deflate(byte[] outputBuffer, int outputOffset, int outputCount, FlushType flushType)
        {
            this.next_out = outputBuffer;
            this.next_out_index = outputOffset;
            this.avail_out = outputCount;
            return this.deflate(flushType);
        }

        public ZLibStatus deflateEnd()
        {
            if (_status != INIT_STATE && _status != BUSY_STATE && _status != FINISH_STATE)
            {
                return ZLibStatus.Z_STREAM_ERROR;
            }
            // Deallocate in reverse order of allocations:
            pending_buf = null;
            _head = null;
            _previous = null;
            _window = null;
            // free
            // dstate=null;
            return _status == BUSY_STATE ? ZLibStatus.Z_DATA_ERROR : ZLibStatus.Z_OK;
        }

        private ZLibStatus deflate(FlushType flush)
        {
            FlushType old_flush;

            if (flush > FlushType.Z_FINISH || flush < 0)
            {
                return ZLibStatus.Z_STREAM_ERROR;
            }

            if (base.next_out == null ||
                (base.next_in == null && base.avail_in != 0) ||
                (_status == FINISH_STATE && flush != FlushType.Z_FINISH))
            {
                base.msg = z_errmsg[ZLibStatus.Z_NEED_DICT - (ZLibStatus.Z_STREAM_ERROR)];
                return ZLibStatus.Z_STREAM_ERROR;
            }
            if (base.avail_out == 0)
            {
                base.msg = z_errmsg[ZLibStatus.Z_NEED_DICT - (ZLibStatus.Z_BUF_ERROR)];
                return ZLibStatus.Z_BUF_ERROR;
            }

            old_flush = last_flush;
            last_flush = flush;

            // Write the zlib header
            if (_status == INIT_STATE)
            {
                int header = (Z_DEFLATED + ((this._windowBits - 8) << 4)) << 8;
                int level_flags = (((int)this.level - 1) & 0xff) >> 1;

                if (level_flags > 3) level_flags = 3;
                header |= (level_flags << 6);
                if (this._startInsertString != 0) header |= PRESET_DICT;
                header += 31 - (header % 31);

                _status = BUSY_STATE;
                PutShortMSB(header);


                // Save the adler32 of the preset dictionary:
                if (this._startInsertString != 0)
                {
                    PutShortMSB((int)(base.adler >> 16));
                    PutShortMSB((int)(base.adler & 0xffff));
                }
                base.adler = Adler32.adler32(0, null, 0, 0);
            }

            // Flush as much pending output as possible
            if (pending != 0)
            {
                this.FlushPending();
                if (base.avail_out == 0)
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
                // flushes. For repeated and useless calls with Z_FINISH, we keep
                // returning Z_STREAM_END instead of Z_BUFF_ERROR.
            }
            else if (base.avail_in == 0 && flush <= old_flush &&
                flush != FlushType.Z_FINISH)
            {
                base.msg = z_errmsg[ZLibStatus.Z_NEED_DICT - (ZLibStatus.Z_BUF_ERROR)];
                return ZLibStatus.Z_BUF_ERROR;
            }

            // User must not provide more input after the first FINISH:
            if (_status == FINISH_STATE && base.avail_in != 0)
            {
                base.msg = z_errmsg[ZLibStatus.Z_NEED_DICT - (ZLibStatus.Z_BUF_ERROR)];
                return ZLibStatus.Z_BUF_ERROR;
            }

            // Start a new block or continue the current one.
            if (base.avail_in != 0 || this._validBytesAhead != 0 ||
                (flush != FlushType.Z_NO_FLUSH && _status != FINISH_STATE))
            {
                int bstate = -1;
                switch (config_table[level].Function)
                {
                    case DefalteFlavor.STORED:
                        bstate = DeflateStored(flush);
                        break;
                    case DefalteFlavor.FAST:
                        bstate = DeflateFast(flush);
                        break;
                    case DefalteFlavor.SLOW:
                        bstate = DeflateSlow(flush);
                        break;
                    default:
                        break;
                }

                if (bstate == FinishStarted || bstate == FinishDone)
                {
                    _status = FINISH_STATE;
                }
                if (bstate == NeedMore || bstate == FinishStarted)
                {
                    if (base.avail_out == 0)
                    {
                        last_flush = (FlushType)(-1); // avoid BUF_ERROR next call, see above
                    }
                    return ZLibStatus.Z_OK;
                    // If flush != Z_NO_FLUSH && avail_out == 0, the next call
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
                        SendStoredBlock(0, 0, false);
                        // For a full flush, this empty block will be recognized
                        // as a special marker by inflate_sync().
                        if (flush == FlushType.Z_FULL_FLUSH)
                        {
                            //state.head[s.hash_size-1]=0;
                            for (int i = 0; i < this._hashSize/*-1*/; i++)  // forget history
                                this._head[i] = 0;
                        }
                    }
                    this.FlushPending();
                    if (base.avail_out == 0)
                    {
                        last_flush = (FlushType)(-1); // avoid BUF_ERROR at next call, see above
                        return ZLibStatus.Z_OK;
                    }
                }
            }

            if (flush != FlushType.Z_FINISH) return ZLibStatus.Z_OK;
            if (noheader != 0) return ZLibStatus.Z_STREAM_END;

            // Write the zlib trailer (adler32)
            PutShortMSB((int)(base.adler >> 16));
            PutShortMSB((int)(base.adler & 0xffff));
            this.FlushPending();

            // If avail_out is zero, the application will call deflate again
            // to flush the rest.
            noheader = -1; // write the trailer only once!
            return pending != 0 ? ZLibStatus.Z_OK : ZLibStatus.Z_STREAM_END;
        }

        /// <summary>
        /// Restore the heap property by moving down the tree starting at node k,
        /// exchanging a node with the smallest of its two sons if necessary, stopping
        /// when the heap property is re-established (each father smaller than its
        /// two sons).
        /// </summary>
        /// <param name="tree">The tree to restore.</param>
        /// <param name="k">The node to move down.</param>
        internal void RestoreHeap(short[] tree, int k)
        {
            int v = heap[k];
            int j = k << 1;  // left son of k
            while (j <= heap_len)
            {
                // Set j to the smallest of the two sons:
                if (j < heap_len &&
                    Smaller(tree, heap[j + 1], heap[j], depth))
                {
                    j++;
                }
                // Exit if v is smaller than both sons
                if (Smaller(tree, v, heap[j], depth)) break;

                // Exchange v with the smallest son
                heap[k] = heap[j]; k = j;
                // And continue down the tree, setting j to the left son of k
                j <<= 1;
            }
            heap[k] = v;
        }

        private ZLibStatus deflateInit(CompressionLevel level)
        {
            return deflateInit(level, MAX_WBITS);
        }

        private ZLibStatus deflateInit(CompressionLevel level, bool nowrap)
        {
            return deflateInit(level, MAX_WBITS, nowrap);
        }

        private ZLibStatus deflateInit(CompressionLevel level, int bits, bool nowrap)
        {
            return this.deflateInit(level, nowrap ? -bits : bits);
        }

        private ZLibStatus deflateInit(CompressionLevel level, int bits)
        {
            return deflateInit2(level, Z_DEFLATED, bits, DEF_MEM_LEVEL, CompressionStrategy.Z_DEFAULT_STRATEGY);
        }

        private ZLibStatus deflateInit2(CompressionLevel level, int method, int windowBits, int memLevel, CompressionStrategy strategy)
        {
            int noheader = 0;
            //    byte[] my_version=ZLIB_VERSION;

            //
            //  if (version == null || version[0] != my_version[0]
            //  || stream_size != sizeof(z_stream)) {
            //  return Z_VERSION_ERROR;
            //  }

            base.msg = null;

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

            this.noheader = noheader;
            _windowBits = windowBits;
            _windowSize = 1 << _windowBits;
            _windowMask = _windowSize - 1;

            _hashBits = memLevel + 7;
            _hashSize = 1 << _hashBits;
            _hashMask = _hashSize - 1;
            _hashShift = ((_hashBits + MIN_MATCH - 1) / MIN_MATCH);

            _window = new byte[_windowSize * 2];
            _previous = new short[_windowSize];
            _head = new short[_hashSize];

            lit_bufsize = 1 << (memLevel + 6); // 16K elements by default

            // We overlay pending_buf and d_buf+l_buf. This works since the average
            // output size for (length,distance) codes is <= 24 bits.
            pending_buf = new byte[lit_bufsize * 4];
            _pendingBufferSize = lit_bufsize * 4;

            d_buf = lit_bufsize / 2;
            l_buf = (1 + 2) * lit_bufsize;

            this.level = level;

            //System.out.println("level="+level);

            this.strategy = strategy;
            this._method = (byte)method;

            return deflateReset();
        }

        private ZLibStatus deflateReset()
        {
            base.total_in = base.total_out = 0;
            base.msg = null; //
            base.data_type = BlockType.Z_UNKNOWN;

            pending = 0;
            pending_out = 0;

            if (noheader < 0)
            {
                noheader = 0; // was set to -1 by deflate(..., FlushType.Z_FINISH);
            }
            _status = (noheader != 0) ? BUSY_STATE : INIT_STATE;
            base.adler = Adler32.adler32(0, null, 0, 0);

            last_flush = FlushType.Z_NO_FLUSH;

            InitializeTreeData();
            lm_init();
            return ZLibStatus.Z_OK;
        }

        private ZLibStatus deflateParams(CompressionLevel _level, CompressionStrategy _strategy)
        {
            ZLibStatus err = ZLibStatus.Z_OK;

            if (_level == CompressionLevel.Z_DEFAULT_COMPRESSION)
            {
                _level = (CompressionLevel)6;
            }
            if (_level < 0 || _level > (CompressionLevel)9 ||
                _strategy < 0 || _strategy > CompressionStrategy.Z_HUFFMAN_ONLY)
            {
                return ZLibStatus.Z_STREAM_ERROR;
            }

            if (config_table[level].Function != config_table[_level].Function &&
                base.total_in != 0)
            {
                // Flush the last buffer:
                err = this.deflate(FlushType.Z_PARTIAL_FLUSH);
            }

            if (level != _level)
            {
                level = _level;
                _maxLazyMatch = config_table[level].MaxLazy;
                good_match = config_table[level].GoodLength;
                nice_match = config_table[level].NiceLength;
                _maxChainLength = config_table[level].MaxChain;
            }
            strategy = _strategy;
            return err;
        }

        private ZLibStatus deflateSetDictionary(byte[] dictionary, int dictLength)
        {
            int length = dictLength;
            int index = 0;

            if (dictionary == null || _status != INIT_STATE)
                return ZLibStatus.Z_STREAM_ERROR;

            base.adler = Adler32.adler32(base.adler, dictionary, 0, dictLength);

            if (length < MIN_MATCH) return ZLibStatus.Z_OK;
            if (length > _windowSize - MIN_LOOKAHEAD)
            {
                length = _windowSize - MIN_LOOKAHEAD;
                index = dictLength - length; // use the tail of the dictionary
            }
            System.Array.Copy(dictionary, index, _window, 0, length);
            _startInsertString = length;
            _blockStart = length;

            // Insert all strings in the hash table (except for the last two bytes).
            // s->lookahead stays null, so s->ins_h will be recomputed at the next
            // call of fill_window.

            _insertedHashIndex = _window[0] & 0xff;
            _insertedHashIndex = (((_insertedHashIndex) << _hashShift) ^ (_window[1] & 0xff)) & _hashMask;

            for (int n = 0; n <= length - MIN_MATCH; n++)
            {
                _insertedHashIndex = (((_insertedHashIndex) << _hashShift) ^ (_window[(n) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;
                _previous[n & _windowMask] = _head[_insertedHashIndex];
                _head[_insertedHashIndex] = (short)n;
            }
            return ZLibStatus.Z_OK;
        }

        private void lm_init()
        {
            _windowActualSize = 2 * _windowSize;

            _head[_hashSize - 1] = 0;
            for (int i = 0; i < _hashSize - 1; i++)
            {
                _head[i] = 0;
            }

            // Set the default configuration parameters:
            _maxLazyMatch = Deflate.config_table[level].MaxLazy;
            good_match = Deflate.config_table[level].GoodLength;
            nice_match = Deflate.config_table[level].NiceLength;
            _maxChainLength = Deflate.config_table[level].MaxChain;

            _startInsertString = 0;
            _blockStart = 0;
            _validBytesAhead = 0;
            _matchLength = _previousLength = MIN_MATCH - 1;
            _matchAvailable = false;
            _insertedHashIndex = 0;
        }

        /// <summary>
        /// Initialize the tree data structures for a new zlib stream.
        /// </summary>
        private void InitializeTreeData()
        {

            l_desc.Init(dyn_ltree, Deflate.static_l_desc);

            d_desc.Init(dyn_dtree, Deflate.static_d_desc);

            bl_desc.Init(bl_tree, Deflate.static_bl_desc);


            _bitBuffer = 0;
            bi_valid = 0;
            last_eob_len = 8; // enough lookahead for inflate

            // Initialize the first block of the first file:
            InitializeBlock();
        }

        private void InitializeBlock()
        {
            // Initialize the trees.
            for (int i = 0; i < L_CODES; i++) dyn_ltree[i * 2] = 0;
            for (int i = 0; i < D_CODES; i++) dyn_dtree[i * 2] = 0;
            for (int i = 0; i < BL_CODES; i++) bl_tree[i * 2] = 0;

            dyn_ltree[END_BLOCK * 2] = 1;
            opt_len = static_len = 0;
            last_lit = matches = 0;
        }

        private static bool Smaller(short[] tree, int n, int m, byte[] depth)
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
        /// <param name="max_code">The tree's largest code of non zero frequency.</param>
        private void ScanTree(short[] tree, int max_code)
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
        /// Construct the Huffman tree for the bit lengths and return the index in bl_order of the last bit length code to send.
        /// </summary>
        /// <returns></returns>
        private int BuildBlTree()
        {
            int max_blindex;  // index of last bit length code of non zero freq

            // Determine the bit length frequencies for literal and distance trees
            ScanTree(dyn_ltree, l_desc.LargestCode);
            ScanTree(dyn_dtree, d_desc.LargestCode);

            // Build the bit length tree:
            bl_desc.BuildTree(this);
            // opt_len now includes the length of the tree representations, except
            // the lengths of the bit lengths codes and the 5+5+4 bits for the counts.

            // Determine the number of bit length codes to send. The pkzip format
            // requires that at least 4 bit length codes be sent. (appnote.txt says
            // 3 but the actual value used is 4.)
            for (max_blindex = BL_CODES - 1; max_blindex >= 3; max_blindex--)
            {
                if (bl_tree[Deflate.bl_order[max_blindex] * 2 + 1] != 0) break;
            }
            // Update opt_len to include the bit length tree and counts
            opt_len += 3 * (max_blindex + 1) + 5 + 5 + 4;

            return max_blindex;
        }

        /// <summary>
        /// Send the header for a block using dynamic Huffman trees: the counts, the lengths of the bit length codes, the literal tree and the distance tree.
        /// </summary>
        /// <param name="lcodes">The lcodes.</param>
        /// <param name="dcodes">The dcodes.</param>
        /// <param name="blcodes">The blcodes.</param>
        private void SendAllTrees(int lcodes, int dcodes, int blcodes)
        {
            int rank;                    // index in bl_order

            SendBits(lcodes - 257, 5); // not +255 as stated in appnote.txt
            SendBits(dcodes - 1, 5);
            SendBits(blcodes - 4, 4); // not -3 as stated in appnote.txt
            for (rank = 0; rank < blcodes; rank++)
            {
                SendBits(bl_tree[Deflate.bl_order[rank] * 2 + 1], 3);
            }
            SendTree(dyn_ltree, lcodes - 1); // literal tree
            SendTree(dyn_dtree, dcodes - 1); // distance tree
        }

        /// <summary>
        /// Send a literal or distance tree in compressed form, using the codes in bl_tree.
        /// </summary>
        /// <param name="tree">The tree to be sent.</param>
        /// <param name="max_code">The tree's largest code of non zero frequency.</param>
        private void SendTree(short[] tree, int max_code)
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
                    do { SendCode(curlen, bl_tree); } while (--count != 0);
                }
                else if (curlen != 0)
                {
                    if (curlen != prevlen)
                    {
                        SendCode(curlen, bl_tree); count--;
                    }
                    SendCode(REP_3_6, bl_tree);
                    SendBits(count - 3, 2);
                }
                else if (count <= 10)
                {
                    SendCode(REPZ_3_10, bl_tree);
                    SendBits(count - 3, 3);
                }
                else
                {
                    SendCode(REPZ_11_138, bl_tree);
                    SendBits(count - 11, 7);
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
        /// Puts  a bytes into the stream.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="start">The start.</param>
        /// <param name="len">The len.</param>
        private void PutByte(byte[] p, int start, int len)
        {
            System.Array.Copy(p, start, pending_buf, pending, len);
            pending += len;
        }

        /// <summary>
        /// Puts  a byte into the stream.
        /// </summary>
        /// <param name="c">The c.</param>
        private void PutByte(byte c)
        {
            pending_buf[pending++] = c;
        }

        /// <summary>
        /// Puts a short into the stream.
        /// </summary>
        /// <param name="w">The w.</param>
        private void PutShort(int w)
        {
            pending_buf[pending++] = (byte)(w/*&0xff*/);
            pending_buf[pending++] = (byte)(w >> 8);
        }

        /// <summary>
        /// Puts the short MSB.
        /// </summary>
        /// <param name="b">The b.</param>
        private void PutShortMSB(int b)
        {
            pending_buf[pending++] = (byte)(b >> 8);
            pending_buf[pending++] = (byte)(b/*&0xff*/);
        }

        private void SendCode(int code, short[] tree)
        {
            int c2 = code * 2;
            SendBits((tree[c2] & 0xffff), (tree[c2 + 1] & 0xffff));
        }

        private void SendBits(int bits, int length)
        {
            if (bi_valid > Buf_size - length)
            {
                _bitBuffer |= (uint)(bits << bi_valid);
                pending_buf[pending++] = (byte)(_bitBuffer/*&0xff*/);
                pending_buf[pending++] = (byte)(_bitBuffer >> 8);
                _bitBuffer = ((uint)bits) >> (Buf_size - bi_valid);
                bi_valid += length - Buf_size;
            }
            else
            {
                _bitBuffer |= (uint)(bits << bi_valid);
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
            SendBits(STATIC_TREES << 1, 3);
            SendCode(END_BLOCK, Deflate.static_ltree);

            FlushBitBuffer();

            // Of the 10 bits for the empty block, we have already sent
            // (10 - bi_valid) bits. The lookahead for the last real code (before
            // the EOB of the previous block) was thus at least one plus the length
            // of the EOB plus what we have just sent of the empty static block.
            if (1 + last_eob_len + 10 - bi_valid < 9)
            {
                SendBits(STATIC_TREES << 1, 3);
                SendCode(END_BLOCK, Deflate.static_ltree);
                FlushBitBuffer();
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
                dyn_ltree[(Deflate._length_code[lc] + LITERALS + 1) * 2]++;
                dyn_dtree[Tree.DistanceCode(dist) * 2]++;
            }

            if ((last_lit & 0x1fff) == 0 && level > (CompressionLevel)2)
            {
                // Compute an upper bound for the compressed length
                int out_length = last_lit * 8;
                int in_length = _startInsertString - _blockStart;
                int dcode;
                for (dcode = 0; dcode < D_CODES; dcode++)
                {
                    out_length += (int)((int)dyn_dtree[dcode * 2] *
                        (5L + Deflate.extra_dbits[dcode]));
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
        private void CompressBlock(short[] ltree, short[] dtree)
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
                        SendCode(lc, ltree); // send a literal byte
                    }
                    else
                    {
                        // Here, lc is the match length - MIN_MATCH
                        code = Deflate._length_code[lc];

                        SendCode(code + LITERALS + 1, ltree); // send the length code
                        extra = Deflate.extra_lbits[code];
                        if (extra != 0)
                        {
                            lc -= Deflate.base_length[code];
                            SendBits(lc, extra);       // send the extra length bits
                        }
                        dist--; // dist is now the match distance - 1
                        code = Tree.DistanceCode(dist);

                        SendCode(code, dtree);       // send the distance code
                        extra = Deflate.extra_dbits[code];
                        if (extra != 0)
                        {
                            dist -= Deflate.base_dist[code];
                            SendBits(dist, extra);   // send the extra distance bits
                        }
                    } // literal or match pair ?

                    // Check that the overlay between pending_buf and d_buf+l_buf is ok:
                }
                while (lx < last_lit);
            }

            SendCode(END_BLOCK, ltree);
            last_eob_len = ltree[END_BLOCK * 2 + 1];
        }

        /// <summary>
        /// Set the data type to ASCII or BINARY, using a crude approximation:
        /// binary if more than 20% of the bytes are <= 6 or >= 128, ascii otherwise.
        /// IN assertion: the fields freq of dyn_ltree are set and the total of all
        /// frequencies does not exceed 64K (to fit in an int on 16 bit machines).
        /// </summary>
        private void SetDataType()
        {
            int n = 0;
            int ascii_freq = 0;
            int bin_freq = 0;
            while (n < 7) { bin_freq += dyn_ltree[n * 2]; n++; }
            while (n < 128) { ascii_freq += dyn_ltree[n * 2]; n++; }
            while (n < LITERALS) { bin_freq += dyn_ltree[n * 2]; n++; }
            this._dataType = (bin_freq > (ascii_freq >> 2) ? BlockType.Z_BINARY : BlockType.Z_ASCII);
        }

        /// <summary>
        /// Flush the bit buffer, keeping at most 7 bits in it.
        /// </summary>
        private void FlushBitBuffer()
        {
            if (bi_valid == 16)
            {
                pending_buf[pending++] = (byte)(_bitBuffer/*&0xff*/);
                pending_buf[pending++] = (byte)(_bitBuffer >> 8);
                _bitBuffer = 0;
                bi_valid = 0;
            }
            else if (bi_valid >= 8)
            {
                pending_buf[pending++] = (byte)(_bitBuffer);
                _bitBuffer >>= 8;
                _bitBuffer &= 0x00ff;
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
                pending_buf[pending++] = (byte)(_bitBuffer);
                pending_buf[pending++] = (byte)(_bitBuffer >> 8);
            }
            else if (bi_valid > 0)
            {
                pending_buf[pending++] = (byte)(_bitBuffer);
            }
            _bitBuffer = 0;
            bi_valid = 0;
        }

        /// <summary>
        /// Copy a stored block, storing first the length and its one's complement if requested.
        /// </summary>
        /// <param name="buf">The input data.</param>
        /// <param name="len">The length.</param>
        /// <param name="header">if set to <c>true</c> block header must be written.</param>
        private void CopyBlock(int buf, int len, bool header)
        {
            //int index=0;
            bi_windup();      // align on byte boundary
            last_eob_len = 8; // enough lookahead for inflate

            if (header)
            {
                PutShort((short)len);
                PutShort((short)~len);
            }

            //  while(len--!=0) {
            //    put_byte(window[buf+index]);
            //    index++;
            //  }
            PutByte(_window, buf, len);
        }

        private void FlushBlockOnly(bool eof)
        {
            FlushBlock(_blockStart >= 0 ? _blockStart : -1,
                _startInsertString - _blockStart,
                eof);
            _blockStart = _startInsertString;
            this.FlushPending();
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
        private int DeflateStored(FlushType flush)
        {
            //  TODO:   this function should be optimized to avoid extra copying from window to pending_buf.

            // Stored blocks are limited to 0xffff bytes, pending_buf is limited
            // to pending_buf_size, and each stored block has a 5 byte header:

            int max_block_size = 0xffff;
            int max_start;

            if (max_block_size > _pendingBufferSize - 5)
            {
                max_block_size = _pendingBufferSize - 5;
            }

            // Copy as much as possible from input to output:
            while (true)
            {
                // Fill the window as much as possible:
                if (_validBytesAhead <= 1)
                {
                    FillWindow();
                    if (_validBytesAhead == 0 && flush == FlushType.Z_NO_FLUSH) return NeedMore;
                    if (_validBytesAhead == 0) break; // flush the current block
                }

                _startInsertString += _validBytesAhead;
                _validBytesAhead = 0;

                // Emit a stored block if pending_buf will be full:
                max_start = _blockStart + max_block_size;
                if (_startInsertString == 0 || _startInsertString >= max_start)
                {
                    // strstart == 0 is possible when wraparound on 16-bit machine
                    _validBytesAhead = (int)(_startInsertString - max_start);
                    _startInsertString = (int)max_start;

                    FlushBlockOnly(false);
                    if (base.avail_out == 0) return NeedMore;

                }

                // Flush if we may have to slide, otherwise block_start may become
                // negative and the data will be gone:
                if (_startInsertString - _blockStart >= _windowSize - MIN_LOOKAHEAD)
                {
                    FlushBlockOnly(false);
                    if (base.avail_out == 0) return NeedMore;
                }
            }

            FlushBlockOnly(flush == FlushType.Z_FINISH);
            if (base.avail_out == 0)
                return (flush == FlushType.Z_FINISH) ? FinishStarted : NeedMore;

            return flush == FlushType.Z_FINISH ? FinishDone : BlockDone;
        }

        /// <summary>
        /// Send a stored block
        /// </summary>
        /// <param name="buf">The input block.</param>
        /// <param name="stored_len">The length of input block.</param>
        /// <param name="eof">if set to <c>true</c> then this is the last block for a file.</param>
        private void SendStoredBlock(int buf, int stored_len, bool eof)
        {
            SendBits((STORED_BLOCK << 1) + (eof ? 1 : 0), 3);  // send block type
            CopyBlock(buf, stored_len, true);          // with header
        }

        /// <summary>
        /// Determine the best encoding for the current block: dynamic trees, static
        /// trees or store, and output the encoded block to the zip file.
        /// </summary>
        /// <param name="buf">The input block, or NULL if too old.</param>
        /// <param name="stored_len">The length of input block.</param>
        /// <param name="eof">if set to <c>true</c> then this is the last block for a file.</param>
        private void FlushBlock(int buf, int stored_len, bool eof)
        {
            int opt_lenb, static_lenb;// opt_len and static_len in bytes
            int max_blindex = 0;      // index of last bit length code of non zero freq

            // Build the Huffman trees unless a stored block is forced
            if (level > 0)
            {
                // Check if the file is ascii or binary
                if (this._dataType == BlockType.Z_UNKNOWN) SetDataType();

                // Construct the literal and distance trees
                l_desc.BuildTree(this);

                d_desc.BuildTree(this);

                // At this point, opt_len and static_len are the total bit lengths of
                // the compressed block data, excluding the tree representations.

                // Build the bit length tree for the above two trees, and get the index
                // in bl_order of the last bit length code to send.
                max_blindex = BuildBlTree();

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
                SendStoredBlock(buf, stored_len, eof);
            }
            else if (static_lenb == opt_lenb)
            {
                SendBits((STATIC_TREES << 1) + (eof ? 1 : 0), 3);
                CompressBlock(Deflate.static_ltree, Deflate.static_dtree);
            }
            else
            {
                SendBits((DYN_TREES << 1) + (eof ? 1 : 0), 3);
                SendAllTrees(l_desc.LargestCode + 1, d_desc.LargestCode + 1, max_blindex + 1);
                CompressBlock(dyn_ltree, dyn_dtree);
            }

            // The above check is made mod 2^32, for files larger than 512 MB
            // and uLong implemented on 32 bits.

            InitializeBlock();

            if (eof)
            {
                bi_windup();
            }
        }

        /// <summary>
        /// Fill the window when the lookahead becomes insufficient.
        /// Updates strstart and lookahead.
        /// </summary>
        private void FillWindow()
        {
            int n, m;
            int p;
            int more;    // Amount of free space at the end of the window.

            do
            {
                more = (_windowActualSize - _validBytesAhead - _startInsertString);

                // Deal with !@#$% 64K limit:
                if (more == 0 && _startInsertString == 0 && _validBytesAhead == 0)
                {
                    more = _windowSize;
                }
                else if (more == -1)
                {
                    // Very unlikely, but possible on 16 bit machine if strstart == 0
                    // and lookahead == 1 (input done one byte at time)
                    more--;

                    // If the window is almost full and there is insufficient lookahead,
                    // move the upper half to the lower one to make room in the upper half.
                }
                else if (_startInsertString >= _windowSize + _windowSize - MIN_LOOKAHEAD)
                {
                    System.Array.Copy(_window, _windowSize, _window, 0, _windowSize);
                    _startMatchString -= _windowSize;
                    _startInsertString -= _windowSize; // we now have strstart >= MAX_DIST
                    _blockStart -= _windowSize;

                    // Slide the hash table (could be avoided with 32 bit values
                    // at the expense of memory usage). We slide even when level == 0
                    // to keep the hash table consistent if we switch back to level > 0
                    // later. (Using level 0 permanently is not an optimal usage of
                    // zlib, so we don't care about this pathological case.)

                    n = _hashSize;
                    p = n;
                    do
                    {
                        m = (_head[--p] & 0xffff);
                        _head[p] = (short)(m >= _windowSize ? (m - _windowSize) : 0);
                    }
                    while (--n != 0);

                    n = _windowSize;
                    p = n;
                    do
                    {
                        m = (_previous[--p] & 0xffff);
                        _previous[p] = (short)(m >= _windowSize ? (m - _windowSize) : 0);
                        // If n is not on any hash chain, prev[n] is garbage but
                        // its value will never be used.
                    }
                    while (--n != 0);
                    more += _windowSize;
                }

                if (base.avail_in == 0) return;

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

                n = this.ReadBuffer(_window, _startInsertString + _validBytesAhead, more);
                _validBytesAhead += n;

                // Initialize the hash value now that we have some input:
                if (_validBytesAhead >= MIN_MATCH)
                {
                    _insertedHashIndex = _window[_startInsertString] & 0xff;
                    _insertedHashIndex = (((_insertedHashIndex) << _hashShift) ^ (_window[_startInsertString + 1] & 0xff)) & _hashMask;
                }
                // If the whole input has less than MIN_MATCH bytes, ins_h is garbage,
                // but this is not important since only literal bytes will be emitted.
            }
            while (_validBytesAhead < MIN_LOOKAHEAD && base.avail_in != 0);
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
        private int DeflateFast(FlushType flush)
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
                if (_validBytesAhead < MIN_LOOKAHEAD)
                {
                    FillWindow();
                    if (_validBytesAhead < MIN_LOOKAHEAD && flush == FlushType.Z_NO_FLUSH)
                    {
                        return NeedMore;
                    }
                    if (_validBytesAhead == 0) break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:
                if (_validBytesAhead >= MIN_MATCH)
                {
                    _insertedHashIndex = (((_insertedHashIndex) << _hashShift) ^ (_window[(_startInsertString) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;

                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (_head[_insertedHashIndex] & 0xffff);
                    _previous[_startInsertString & _windowMask] = _head[_insertedHashIndex];
                    _head[_insertedHashIndex] = (short)_startInsertString;
                }

                // Find the longest match, discarding those <= prev_length.
                // At this point we have always match_length < MIN_MATCH

                if (hash_head != 0L &&
                    ((_startInsertString - hash_head) & 0xffff) <= _windowSize - MIN_LOOKAHEAD
                    )
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).
                    if (strategy != CompressionStrategy.Z_HUFFMAN_ONLY)
                    {
                        _matchLength = LongestMatch(hash_head);
                    }
                    // longest_match() sets match_start
                }
                if (_matchLength >= MIN_MATCH)
                {
                    //        check_match(strstart, match_start, match_length);

                    bflush = _tr_tally(_startInsertString - _startMatchString, _matchLength - MIN_MATCH);

                    _validBytesAhead -= _matchLength;

                    // Insert new strings in the hash table only if the match length
                    // is not too large. This saves time but degrades compression.
                    if (_matchLength <= _maxLazyMatch &&
                        _validBytesAhead >= MIN_MATCH)
                    {
                        _matchLength--; // string at strstart already in hash table
                        do
                        {
                            _startInsertString++;

                            _insertedHashIndex = ((_insertedHashIndex << _hashShift) ^ (_window[(_startInsertString) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;
                            //      prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (_head[_insertedHashIndex] & 0xffff);
                            _previous[_startInsertString & _windowMask] = _head[_insertedHashIndex];
                            _head[_insertedHashIndex] = (short)_startInsertString;

                            // strstart never exceeds WSIZE-MAX_MATCH, so there are
                            // always MIN_MATCH bytes ahead.
                        }
                        while (--_matchLength != 0);
                        _startInsertString++;
                    }
                    else
                    {
                        _startInsertString += _matchLength;
                        _matchLength = 0;
                        _insertedHashIndex = _window[_startInsertString] & 0xff;

                        _insertedHashIndex = (((_insertedHashIndex) << _hashShift) ^ (_window[_startInsertString + 1] & 0xff)) & _hashMask;
                        // If lookahead < MIN_MATCH, ins_h is garbage, but it does not
                        // matter since it will be recomputed at next deflate call.
                    }
                }
                else
                {
                    // No match, output a literal byte

                    bflush = _tr_tally(0, _window[_startInsertString] & 0xff);
                    _validBytesAhead--;
                    _startInsertString++;
                }
                if (bflush)
                {

                    FlushBlockOnly(false);
                    if (base.avail_out == 0) return NeedMore;
                }
            }

            FlushBlockOnly(flush == FlushType.Z_FINISH);
            if (base.avail_out == 0)
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
        private int DeflateSlow(FlushType flush)
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

                if (_validBytesAhead < MIN_LOOKAHEAD)
                {
                    FillWindow();
                    if (_validBytesAhead < MIN_LOOKAHEAD && flush == FlushType.Z_NO_FLUSH)
                    {
                        return NeedMore;
                    }
                    if (_validBytesAhead == 0) break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:

                if (_validBytesAhead >= MIN_MATCH)
                {
                    _insertedHashIndex = (((_insertedHashIndex) << _hashShift) ^ (_window[(_startInsertString) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;
                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (_head[_insertedHashIndex] & 0xffff);
                    _previous[_startInsertString & _windowMask] = _head[_insertedHashIndex];
                    _head[_insertedHashIndex] = (short)_startInsertString;
                }

                // Find the longest match, discarding those <= prev_length.
                _previousLength = _matchLength; _previousMatch = _startMatchString;
                _matchLength = MIN_MATCH - 1;

                if (hash_head != 0 && _previousLength < _maxLazyMatch &&
                    ((_startInsertString - hash_head) & 0xffff) <= _windowSize - MIN_LOOKAHEAD
                    )
                {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).

                    if (strategy != CompressionStrategy.Z_HUFFMAN_ONLY)
                    {
                        _matchLength = LongestMatch(hash_head);
                    }
                    // longest_match() sets match_start

                    if (_matchLength <= 5 && (strategy == CompressionStrategy.Z_FILTERED ||
                        (_matchLength == MIN_MATCH &&
                        _startInsertString - _startMatchString > 4096)))
                    {

                        // If prev_match is also MIN_MATCH, match_start is garbage
                        // but we will ignore the current match anyway.
                        _matchLength = MIN_MATCH - 1;
                    }
                }

                // If there was a match at the previous step and the current
                // match is not better, output the previous match:
                if (_previousLength >= MIN_MATCH && _matchLength <= _previousLength)
                {
                    int max_insert = _startInsertString + _validBytesAhead - MIN_MATCH;
                    // Do not insert strings in hash table beyond this.

                    //          check_match(strstart-1, prev_match, prev_length);

                    bflush = _tr_tally(_startInsertString - 1 - _previousMatch, _previousLength - MIN_MATCH);

                    // Insert in hash table all strings up to the end of the match.
                    // strstart-1 and strstart are already inserted. If there is not
                    // enough lookahead, the last two strings are not inserted in
                    // the hash table.
                    _validBytesAhead -= _previousLength - 1;
                    _previousLength -= 2;
                    do
                    {
                        if (++_startInsertString <= max_insert)
                        {
                            _insertedHashIndex = (((_insertedHashIndex) << _hashShift) ^ (_window[(_startInsertString) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;
                            //prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (_head[_insertedHashIndex] & 0xffff);
                            _previous[_startInsertString & _windowMask] = _head[_insertedHashIndex];
                            _head[_insertedHashIndex] = (short)_startInsertString;
                        }
                    }
                    while (--_previousLength != 0);
                    _matchAvailable = false;
                    _matchLength = MIN_MATCH - 1;
                    _startInsertString++;

                    if (bflush)
                    {
                        FlushBlockOnly(false);
                        if (base.avail_out == 0) return NeedMore;
                    }
                }
                else if (_matchAvailable != false)
                {

                    // If there was no match at the previous position, output a
                    // single literal. If there was a match but the current match
                    // is longer, truncate the previous match to a single literal.

                    bflush = _tr_tally(0, _window[_startInsertString - 1] & 0xff);

                    if (bflush)
                    {
                        FlushBlockOnly(false);
                    }
                    _startInsertString++;
                    _validBytesAhead--;
                    if (base.avail_out == 0) return NeedMore;
                }
                else
                {
                    // There is no previous match to compare with, wait for
                    // the next step to decide.

                    _matchAvailable = true;
                    _startInsertString++;
                    _validBytesAhead--;
                }
            }

            if (_matchAvailable != false)
            {
                bflush = _tr_tally(0, _window[_startInsertString - 1] & 0xff);
                _matchAvailable = false;
            }
            FlushBlockOnly(flush == FlushType.Z_FINISH);

            if (base.avail_out == 0)
            {
                if (flush == FlushType.Z_FINISH) return FinishStarted;
                else return NeedMore;
            }

            return flush == FlushType.Z_FINISH ? FinishDone : BlockDone;
        }

        private int LongestMatch(int cur_match)
        {
            int chain_length = _maxChainLength; // max hash chain length
            int scan = _startInsertString;                 // current string
            int match;                           // matched string
            int len;                             // length of current match
            int best_len = _previousLength;          // best match length so far
            int limit = _startInsertString > (_windowSize - MIN_LOOKAHEAD) ?
                _startInsertString - (_windowSize - MIN_LOOKAHEAD) : 0;
            int nice_match = this.nice_match;

            // Stop when cur_match becomes <= limit. To simplify the code,
            // we prevent matches with the string of window index 0.

            int wmask = _windowMask;

            int strend = _startInsertString + MAX_MATCH;
            byte scan_end1 = _window[scan + best_len - 1];
            byte scan_end = _window[scan + best_len];

            // The code is optimized for HASH_BITS >= 8 and MAX_MATCH-2 multiple of 16.
            // It is easy to get rid of this optimization if necessary.

            // Do not waste too much time if we already have a good match:
            if (_previousLength >= good_match)
            {
                chain_length >>= 2;
            }

            // Do not look for matches beyond the end of the input. This is necessary
            // to make deflate deterministic.
            if (nice_match > _validBytesAhead) nice_match = _validBytesAhead;

            do
            {
                match = cur_match;

                // Skip to next match if the match length cannot increase
                // or if the match length is less than 2:
                if (_window[match + best_len] != scan_end ||
                    _window[match + best_len - 1] != scan_end1 ||
                    _window[match] != _window[scan] ||
                    _window[++match] != _window[scan + 1]) continue;

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
                } while (_window[++scan] == _window[++match] &&
                    _window[++scan] == _window[++match] &&
                    _window[++scan] == _window[++match] &&
                    _window[++scan] == _window[++match] &&
                    _window[++scan] == _window[++match] &&
                    _window[++scan] == _window[++match] &&
                    _window[++scan] == _window[++match] &&
                    _window[++scan] == _window[++match] &&
                    scan < strend);

                len = MAX_MATCH - (int)(strend - scan);
                scan = strend - MAX_MATCH;

                if (len > best_len)
                {
                    _startMatchString = cur_match;
                    best_len = len;
                    if (len >= nice_match) break;
                    scan_end1 = _window[scan + best_len - 1];
                    scan_end = _window[scan + best_len];
                }

            } while ((cur_match = (_previous[cur_match & wmask] & 0xffff)) > limit
                && --chain_length != 0);

            if (best_len <= _validBytesAhead) return best_len;
            return _validBytesAhead;
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
            internal DefalteFlavor Function { get; private set; }
            internal Config(int goodLength, int maxLazy, int niceLength, int maxChain, DefalteFlavor function)
            {
                this.GoodLength = goodLength;
                this.MaxLazy = maxLazy;
                this.NiceLength = niceLength;
                this.MaxChain = maxChain;
                this.Function = function;
            }
        }

        /// <summary>
        /// Flush as much pending output as possible. All deflate() output goes
        /// through this function so some applications may wish to modify it
        /// to avoid allocating a large strm->next_out buffer and copying into it.
        /// (See also ReadBuffer()).
        /// </summary>
        private void FlushPending()
        {
            int len = this.pending;

            if (len > avail_out) len = avail_out;
            if (len == 0) return;

            if (this.pending_buf.Length <= this.pending_out ||
                next_out.Length <= next_out_index ||
                this.pending_buf.Length < (this.pending_out + len) ||
                next_out.Length < (next_out_index + len))
            {
                //      System.out.println(this.pending_buf.length+", "+this.pending_out+
                //			 ", "+next_out.length+", "+next_out_index+", "+len);
                //      System.out.println("avail_out="+avail_out);
            }

            System.Array.Copy(this.pending_buf, this.pending_out,
                next_out, next_out_index, len);

            next_out_index += len;
            this.pending_out += len;
            total_out += len;
            avail_out -= len;
            this.pending -= len;
            if (this.pending == 0)
            {
                this.pending_out = 0;
            }
        }

        /// <summary>
        /// Reads a new buffer from the current input stream, update the adler32
        /// and total number of bytes read.  All deflate() input goes through
        /// this function so some applications may wish to modify it to avoid
        /// allocating a large strm->next_in buffer and copying from it.
        /// (See also FlushPending()).
        /// </summary>
        /// <param name="buf">The buf.</param>
        /// <param name="start">The start.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        private int ReadBuffer(byte[] buf, int start, int size)
        {
            int len = avail_in;

            if (len > size) len = size;
            if (len == 0) return 0;

            avail_in -= len;

            if (this.noheader == 0)
            {
                adler = Adler32.adler32(adler, next_in, next_in_index, len);
            }
            System.Array.Copy(next_in, next_in_index, buf, start, len);
            next_in_index += len;
            total_in += len;
            return len;
        }

        //  TODO:   Delete this
        public ZLibStatus inflateEnd()
        {
            throw new NotImplementedException();
        }

        public ZLibStatus inflate(byte[] bufer, int offset, int count, byte[] p1, int p2, int p3, FlushType flushType)
        {
            throw new NotImplementedException();
        }

        public ZLibStatus inflate(byte[] outputBuffer, int outputOffset, int outputCount, FlushType flushType)
        {
            throw new NotImplementedException();
        }
    }
}