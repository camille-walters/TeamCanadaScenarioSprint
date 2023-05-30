using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public enum ArrayManagerType
    {
        /// <summary>
        /// static one time generation and discards arrays to preseve memory
        /// </summary>
        Static,
        /// <summary>
        /// fast fragmentation for objects of fixed size
        /// </summary>
        Compact,
        /// <summary>
        /// fast fragmentation for objects of fixed size . when setting all objects each frame (such as wave form)
        /// </summary>
        CompactRealtime,
        /// <summary>
        /// used for objects of undertermined size such as text objects
        /// </summary>
        Chunked,
        /// <summary>
        /// a compact array that is used mainly for streaming data
        /// </summary>
        StreamingCompact,
        /// <summary>
        /// a chunked array that is used mainly for streaming data
        /// </summary>
        StreamingChunked
    }
}
