using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Data_Visualizer.Script.DataSeries.Core
{
    public class DataSeriesOverrideSettings : IDataSeriesSettings
    {
        IDataSeriesSettings mSetting;
        string mOverrideName;
        object mOverrideValue;
        public DataSeriesOverrideSettings(IDataSeriesSettings settings,string overrideName,object overrideValue)
        {
            mOverrideName = overrideName;
            mOverrideValue = overrideValue;
            mSetting = settings;
        }
        public object GetSetting(string name)
        {
            if (name == mOverrideName)
                return mOverrideValue;
            return mSetting.GetSetting(name);
        }
    }
}
