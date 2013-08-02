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

using System;
using System.Diagnostics;
using System.IO;

namespace Org.BouncyCastle.Utilities.Zlib
{
    public class ZOutputStream : ZStream
    {
        private const int BufferSize = 512;

        //private ZStream _z = new ZStream();

        private byte[] _bufffer = new byte[BufferSize];
        private bool compress;

        private Stream _output;
        private bool _isDisposed;

        public virtual FlushType FlushMode { get; private set; }

        public ZOutputStream(Stream output)
            : base()
        {
            this.FlushMode = FlushType.Z_PARTIAL_FLUSH;
            this._output = output;
            this.inflateInit();
            this.compress = false;
        }

        public ZOutputStream(Stream output, CompressionLevel level)
            : this(output, level, false)
        {
        }

        public ZOutputStream(Stream output, CompressionLevel level, bool nowrap)
            : base()
        {
            this.FlushMode = FlushType.Z_PARTIAL_FLUSH;
            this._output = output;
            this.deflateInit(level, nowrap);
            this.compress = true;
        }

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
            get { return !_isDisposed; }
        }

        public override void Flush()
        {
            this._output.Flush();
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            this.next_in = buffer;
            this.next_in_index = offset;
            this.avail_in = count;

            do
            {
                this.next_out = this._bufffer;
                this.next_out_index = 0;
                this.avail_out = this._bufffer.Length;

                ZLibStatus err = compress ? this.deflate(this.FlushMode) : this.inflate(this.FlushMode);

                if (err != ZLibStatus.Z_OK)
                    throw new IOException((compress ? "de" : "in") + "flating: " + this.msg);

                this._output.Write(this._bufffer, 0, this._bufffer.Length - this.avail_out);
            }
            while (this.avail_in > 0 || this.avail_out == 0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this._isDisposed)
                return;

            try
            {
                do
                {
                    this.next_out = this._bufffer;
                    this.next_out_index = 0;
                    this.avail_out = this._bufffer.Length;

                    var err = compress? this.deflate(FlushType.Z_FINISH) : this.inflate(FlushType.Z_FINISH);

                    if (err != ZLibStatus.Z_STREAM_END && err != ZLibStatus.Z_OK)
                        throw new IOException((compress ? "de" : "in") + "flating: " + this.msg);

                    int count = this._bufffer.Length - this.avail_out;
                    if (count > 0)
                    {
                        this._output.Write(this._bufffer, 0, count);
                    }
                }
                while (this.avail_in > 0 || this.avail_out == 0);

                this.Flush();
            }
            finally
            {
                this._isDisposed = true;
                if (compress)
                    this.deflateEnd();
                else
                    this.inflateEnd();
                this._output.Close();
                this._output = null;
            }
        }
    }
}
