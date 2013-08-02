using System;
/*
 * $Id: StaticTree.cs,v 1.2 2008-05-10 09:35:40 bouncy Exp $
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
    internal sealed class StaticTree
    {
        /// <summary>
        /// Gets the static tree or null
        /// </summary>
        /// <value>
        /// The static_tree.
        /// </value>
        public short[] TreeData { get; private set; }

        /// <summary>
        /// Gets the extra bits for each code or null.
        /// </summary>
        /// <value>
        /// The extra bits.
        /// </value>
        public int[] ExtraBits { get; private set; }

        /// <summary>
        /// Gets the base index for extra_bits.
        /// </summary>
        /// <value>
        /// The extra base.
        /// </value>
        public int ExtraBase { get; private set; }

        /// <summary>
        /// Gets the max number of elements in the tree.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        public int Elements { get; private set; }

        /// <summary>
        /// Gets the max bit length for the codes.
        /// </summary>
        /// <value>
        /// The length of the max.
        /// </value>
        public int MaxLength { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticTree" /> class.
        /// </summary>
        /// <param name="treeData">The tree data.</param>
        /// <param name="extraBits">The extra bits.</param>
        /// <param name="extraBase">The extra base.</param>
        /// <param name="elements">The elements.</param>
        /// <param name="maxLength">Length of the max.</param>
        public StaticTree(short[] treeData, int[] extraBits, int extraBase, int elements, int maxLength)
        {
            this.TreeData = treeData;
            this.ExtraBits = extraBits;
            this.ExtraBase = extraBase;
            this.Elements = elements;
            this.MaxLength = maxLength;
        }
    }
}