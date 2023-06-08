using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    interface IDataViewModifier : IDataViewerNotifier
    {
        void SetParent(DataSeriesBase parent);
        void SetView(double fromX, double fromY, double xSize, double ySize, double viewDiagonalBase);
        void FitInto(DoubleRect localRect, DoubleRect parentRect);
    }
}
