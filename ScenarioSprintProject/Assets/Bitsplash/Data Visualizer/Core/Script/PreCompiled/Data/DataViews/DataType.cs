using Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualizer{
    [Serializable]
    public abstract class DataType : IDataSeriesSettings
    {        
        public abstract string Name { get; }
        public abstract DataType Clone();
        public abstract IDataView CreateView(IDataViewerNotifier mainView);

        Dictionary<string, object> mSettings = new Dictionary<string, object>();

        protected void AddSetting(string name,object obj)
        {
            mSettings.Add(name, obj);
        }

        public object GetSetting(string name)
        {
            object res = null;
            if (mSettings.TryGetValue(name, out res))
                return res;
            return null;
        }
    }
}
