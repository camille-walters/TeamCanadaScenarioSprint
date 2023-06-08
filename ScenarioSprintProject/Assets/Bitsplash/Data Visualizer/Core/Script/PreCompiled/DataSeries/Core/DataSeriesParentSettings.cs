using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Data_Visualizer.Script.DataSeries.Core
{
    class DataSeriesParentSettings : IDataSeriesSettings
    {
        IDataSeriesSettings mParent, mChild;
        public DataSeriesParentSettings(IDataSeriesSettings parent, IDataSeriesSettings child)
        {
            mParent = parent;
            mChild = child;
        }

        public object GetSetting(string name)
        {
            if (name.Length == 0)
                return null;
            if(name[0] == '.')
                return mParent.GetSetting(name.Substring(1));
            return mChild.GetSetting(name);
        }
    }
}
