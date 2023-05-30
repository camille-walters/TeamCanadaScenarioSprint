using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public interface IDataSeriesSettings
    {
        object GetSetting(string name);
    }
}
