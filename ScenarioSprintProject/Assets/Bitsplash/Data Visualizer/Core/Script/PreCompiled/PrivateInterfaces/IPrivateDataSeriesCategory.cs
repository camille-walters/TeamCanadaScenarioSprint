using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    interface IPrivateDataSeriesCategory
    {
        DataBounds Bounds { get; }
        void ModifyMinMax(DoubleRect boundingVolume);
        void ModifyMinMax(DoubleVector3 point);
        void ClearMinMax();
        StackedGenericDataHolder InnerData { get; }

        void Validate();
    }
}
