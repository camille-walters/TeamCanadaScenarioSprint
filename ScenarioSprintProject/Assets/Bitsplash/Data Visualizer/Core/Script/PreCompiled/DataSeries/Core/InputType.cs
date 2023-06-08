using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public enum InputType
    {
        /// <summary>
        /// the data is changed at random places at random times
        /// </summary>
        GeneralPurpose,
        /// <summary>
        /// the data is not changed ofter
        /// </summary>
        Static,
        /// <summary>
        /// The data is changed but data is only appended to the end of the array , (such as stack graph)
        /// </summary>
        Streaming,
        /// <summary>
        /// all or most of the data is changed on a per frame basis
        /// </summary>
        Realtime
    }
}
