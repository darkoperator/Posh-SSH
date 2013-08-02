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
/* This file is a port of jzlib v1.0.7, com.jcraft.jzlib.ZOutputStream.java
 */

using Renci.SshNet.Compression;
using System;
using System.Diagnostics;
using System.IO;

namespace Org.BouncyCastle.Utilities.Zlib
{
    public interface ICompressor
    {

        ZLibStatus inflateInit();

        ZLibStatus deflateInit(CompressionLevel level, bool nowrap);

        ZLibStatus deflate(byte[] bufer, int offset, int count, byte[] p1, int p2, int p3, FlushType flushType);

        ZLibStatus inflate(byte[] bufer, int offset, int count, byte[] p1, int p2, int p3, FlushType flushType);

        int avail_out { get; set; }

        int avail_in { get; set; }

        String msg { get; set; }

        ZLibStatus deflateEnd();

        ZLibStatus inflateEnd();

        void free();

        int next_out_index { get; set; }

        byte[] next_out { get; set; }

        ZLibStatus deflate(FlushType flushType);

        ZLibStatus inflate(FlushType flushType);

        ZLibStatus inflateInit(bool nowrap);

        byte[] next_in { get; set; }

        int next_in_index { get; set; }

        ZLibStatus deflateInit(CompressionLevel level);
    }

    public class ZOutputStream : Stream
    {
        private const int BufferSize = 512;

        //private ZStream z = new ZStream();
        private ICompressor _compressor;
        // TODO Allow custom buf    
        private byte[] _buffer = new byte[BufferSize];
        private CompressionMode _compressionMode;

        private Stream _output;
        private bool _isClosed;

        public virtual FlushType FlushMode { get; private set; }

        public ZOutputStream()
            : base()
        {
            this.FlushMode = FlushType.Z_PARTIAL_FLUSH;
        }

        public ZOutputStream(Stream output)
            : this()
        {
            this._output = output;
            this._compressor = new Inflate();
            this._compressor.inflateInit();
            this._compressionMode = CompressionMode.Decompress;
        }

        public ZOutputStream(Stream output, CompressionLevel level)
            : this(output, level, false)
        {
        }

        public ZOutputStream(Stream output, CompressionLevel level, bool nowrap)
            : this()
        {
            this._output = output;
            //  TODO:   Initialize deflate here when inherited
            this._compressor = new Deflate();
            this._compressor.deflateInit(level, nowrap);
            this._compressionMode = CompressionMode.Compress;
        }

        #region Stream Implemenation

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get
            { return !_isClosed; }
        }

        public override void Flush()
        {
            _output.Flush();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] bufer, int offset, int count)
        {
            if (count == 0)
                return;

            do
            {
                var err = ZLibStatus.Z_OK;
                switch (this._compressionMode)
                {
                    case CompressionMode.Compress:
                        err = this._compressor.deflate(bufer, offset, count, this._buffer, 0, this._buffer.Length, this.FlushMode);
                        break;
                    case CompressionMode.Decompress:
                        err = this._compressor.inflate(bufer, offset, count, this._buffer, 0, this._buffer.Length, this.FlushMode);
                        break;
                    default:
                        break;
                }

                //  TODO:   Move this error into inflate/deflate method
                if (err != ZLibStatus.Z_OK)
                    // TODO
                    //					throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                    //throw new IOException((_compressionMode ? "de" : "in") + "flating: " + z.msg);
                    throw new IOException(string.Format("{0}ion error: {1}", _compressionMode, this._compressor.msg));

                _output.Write(_buffer, 0, _buffer.Length - this._compressor.avail_out);
            }
            while (this._compressor.avail_in > 0 || this._compressor.avail_out == 0);
        }

        #endregion

        public override void Close()
        {
            if (this._isClosed)
                return;

            try
            {
                try
                {
                    Finish();
                }
                catch (IOException)
                {
                    // Ignore
                }
            }
            finally
            {
                this._isClosed = true;
                End();
                _output.Close();
                _output = null;
            }
        }

        private void End()
        {
            if (this._compressor == null)
                return;
            switch (this._compressionMode)
            {
                case CompressionMode.Compress:
                    this._compressor.deflateEnd();
                    break;
                case CompressionMode.Decompress:
                    this._compressor.inflateEnd();
                    break;
                default:
                    break;
            }

            this._compressor.free();
            this._compressor = null;
        }

        private void Finish()
        {
            do
            {
                this._compressor.next_out = _buffer;
                this._compressor.next_out_index = 0;
                this._compressor.avail_out = _buffer.Length;

                var err = ZLibStatus.Z_OK;
                switch (this._compressionMode)
                {
                    case CompressionMode.Compress:
                        err = this._compressor.deflate(FlushType.Z_FINISH);
                        break;
                    case CompressionMode.Decompress:
                        err = this._compressor.inflate(FlushType.Z_FINISH);
                        break;
                    default:
                        break;
                }

                if (err != ZLibStatus.Z_STREAM_END && err != ZLibStatus.Z_OK)
                    // TODO
                    //					throw new ZStreamException((compress?"de":"in")+"flating: "+z.msg);
                    //throw new IOException((_compressionMode ? "de" : "in") + "flating: " + z.msg);
                    throw new IOException(string.Format("{0}ion error: {1}", _compressionMode, this._compressor.msg));

                int count = _buffer.Length - this._compressor.avail_out;
                if (count > 0)
                {
                    _output.Write(_buffer, 0, count);
                }
            }
            while (this._compressor.avail_in > 0 || this._compressor.avail_out == 0);

            Flush();
        }
    }
}
