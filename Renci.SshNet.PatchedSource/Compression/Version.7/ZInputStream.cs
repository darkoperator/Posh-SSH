/*
Copyright (c) 2001 Lapo Luchini.

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
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS
OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
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
/* This file is a port of jzlib v1.0.7, com.jcraft.jzlib.ZInputStream.java
 */

using Renci.SshNet.Compression;
using System;
using System.Diagnostics;
using System.IO;

namespace Org.BouncyCastle.Utilities.Zlib
{
    public class ZInputStream : Stream
    {
        private const int BufferSize = 512;
        //private ZStream z = new ZStream();
        private ICompressor _compressor;

        // TODO Allow custom buf
        private byte[] _buffer = new byte[BufferSize];
        private CompressionMode _compressionMode;
        private Stream _input;
        private bool _isClosed;
        private bool _noMoreInput = false;

        public FlushType FlushMode { get; private set; }

        public ZInputStream()
        {
            this.FlushMode = FlushType.Z_PARTIAL_FLUSH;
        }

        public ZInputStream(Stream input)
            : this(input, false)
        {
        }

        public ZInputStream(Stream input, bool nowrap)
            : this()
        {
            Debug.Assert(input.CanRead);

            this._input = input;
            this._compressor = new Inflate(nowrap);
            //this._compressor.inflateInit(nowrap);
            this._compressionMode = CompressionMode.Decompress;
            this._compressor.next_in = _buffer;
            this._compressor.next_in_index = 0;
            this._compressor.avail_in = 0;
        }

        public ZInputStream(Stream input, CompressionLevel level)
            : this()
        {
            Debug.Assert(input.CanRead);

            this._input = input;
            this._compressor = new Deflate(level);
            //this._compressor.deflateInit(level);
            this._compressionMode = CompressionMode.Compress;
            this._compressor.next_in = _buffer;
            this._compressor.next_in_index = 0;
            this._compressor.avail_in = 0;
        }

        #region Stream implementation

        public sealed override bool CanRead
        {
            get { return !_isClosed; }
        }

        public sealed override bool CanSeek
        {
            get { return false; }
        }

        public sealed override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return 0;

            this._compressor.next_out = buffer;
            this._compressor.next_out_index = offset;
            this._compressor.avail_out = count;

            ZLibStatus err = ZLibStatus.Z_OK;
            do
            {
                if (this._compressor.avail_in == 0 && !_noMoreInput)
                {
                    // if buffer is empty and more input is available, refill it
                    this._compressor.next_in_index = 0;
                    this._compressor.avail_in = _input.Read(_buffer, 0, _buffer.Length); //(bufsize<z.avail_out ? bufsize : z.avail_out));

                    if (this._compressor.avail_in <= 0)
                    {
                        this._compressor.avail_in = 0;
                        _noMoreInput = true;
                    }
                }

                switch (_compressionMode)
                {
                    case CompressionMode.Compress:
                        err = this._compressor.deflate(this.FlushMode);
                        break;
                    case CompressionMode.Decompress:
                        err = this._compressor.inflate(this.FlushMode);
                        break;
                    default:
                        break;
                }

                if (_noMoreInput && err == ZLibStatus.Z_BUF_ERROR)
                    return 0;

                if (err != ZLibStatus.Z_OK && err != ZLibStatus.Z_STREAM_END)
                    //throw new IOException((compress ? "de" : "in") + "flating: " + z.msg);
                    throw new IOException(string.Format("{0}ion error: {1}", _compressionMode, this._compressor.msg));
                if ((_noMoreInput || err == ZLibStatus.Z_STREAM_END) && this._compressor.avail_out == count)
                    return 0;
            }
            while (this._compressor.avail_out == count && err == ZLibStatus.Z_OK);
            //Console.Error.WriteLine("("+(len-z.avail_out)+")");
            return count - this._compressor.avail_out;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        #endregion

        public override void Close()
        {
            if (!_isClosed)
            {
                _isClosed = true;
                _input.Close();
            }
        }
    }
}
