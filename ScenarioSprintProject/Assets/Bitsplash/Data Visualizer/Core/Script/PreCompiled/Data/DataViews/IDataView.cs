using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualizer
{
    public interface IDataView : IDataViewerNotifier
    {
        void ApplySettings(IDataSeriesSettings settings);
    }
}
