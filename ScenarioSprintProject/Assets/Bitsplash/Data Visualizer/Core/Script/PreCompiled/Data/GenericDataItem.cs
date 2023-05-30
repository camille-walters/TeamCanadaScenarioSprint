using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// a generic data item that can be used to add data to a GenericDataHolder instance
    /// </summary>
    public struct GenericDataItem
    {
        public static GenericDataItem FromPosition(DoubleVector3 position)
        {
            return new GenericDataItem()
            {
                Position = position
            };
        }

        /// <summary>
        /// the name of the object. Used in item lables
        /// </summary>
        public string Name;
        /// <summary>
        /// defines a position that can be used in different ways by different series
        /// </summary>
        public DoubleVector3 Position;
        /// <summary>
        /// defines an end position thatcan be used in different ways by different series
        /// </summary>
        public DoubleVector3 EndPosition;
        public DoubleRange HighLow, StartEnd, ErrorRange;
        public double Size;
        /// <summary>
        /// this can contain just about anything. This value is passed to child prefabs, use this to create custom behaviors such as a chart within a chart etc
        /// </summary>
        public object userData;
        public Color32 Color;

        public DoubleRect BoundingVolume(ChannelType channels)
        {
            DoubleRect volume = DoubleRect.CreateNan();
            if ((channels & ChannelType.Positions) != 0)
                volume.UnionVector( Position);
            if ((channels & ChannelType.EndPositions) != 0)
                volume.UnionVector(EndPosition);
            if ((channels & ChannelType.StartEnd) != 0)
                volume.UnionYRange(StartEnd);
            if ((channels & ChannelType.HighLow) != 0)
                volume.UnionYRange(HighLow);
            if ((channels & ChannelType.ErrorRange) != 0)
                volume.UnionYRange(ErrorRange);
            volume.NanToZero();
            if ((channels & ChannelType.Sizes) != 0)
                volume.Inflate(Size);
            return volume;
        }
    }
}
