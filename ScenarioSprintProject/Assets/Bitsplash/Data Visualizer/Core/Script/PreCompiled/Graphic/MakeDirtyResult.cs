using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public enum MakeDirtyResult
    {
        /// <summary>
        /// The object was marked as dirty successfuly
        /// </summary>
        Succeeded,
        /// <summary>
        /// The entity was marked as dirty and was removed from the array manager. This means there was not enough space for the new data in the array manager
        /// </summary>
        Removed
    }
}
