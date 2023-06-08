using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public struct VersionTrace<T> where T : struct
    {
        /// <summary>
        /// used for performance, do not change value
        /// </summary>
        internal T mRawValue;
        public int Version { get; private set; }

        public VersionTrace(T val)
        {
            mRawValue = val;
            Version = 1;
        }
        /// <summary>
        /// returns true if the VersionTrace has been assigned at least once
        /// </summary>
        public bool HasValue
        {
            get { return Version > 0; }
        }
        public T Value
        {
            get { return mRawValue; }
            set
            {
                mRawValue = value;
                checked
                {
                    ++Version;
                }
            }
        }
    }
}
