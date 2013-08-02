using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Org.BouncyCastle.Utilities.Zlib;

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Implements Zlib compression algorithm.
    /// </summary>
    public class ZlibStream
    {
        private readonly Stream _baseStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZlibStream" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mode">The mode.</param>
        public ZlibStream(Stream stream, CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.Compress:
                    this._baseStream = new ZOutputStream(stream, CompressionLevel.Z_DEFAULT_COMPRESSION);
                    break;
                case CompressionMode.Decompress:
                    this._baseStream = new ZOutputStream(stream);
                    break;
                default:
                    break;
            }

            //this._baseStream.FlushMode = Ionic.Zlib.FlushType.Partial;
        }

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            this._baseStream.Write(buffer, offset, count);
        }
    }
}
