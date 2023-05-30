using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    /// <summary>
    /// flags indicating the composition of a Generic Data Holder. Each value is a channel that may be present or not in the generic data
    /// </summary>
    [Flags]
    public enum ChannelType
    {
        None = 0,
        Positions = 1,
        EndPositions = 2,
        StartEnd = 4,
        HighLow = 8,
        ErrorRange = 16,
        Sizes = 32,
        AlternativeBoundingVolume = 64,
        Name = 128,
        UserData = 256,
        Color = 512
    }
}
