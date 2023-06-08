using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public partial class StackedGenericDataHolder
    {
        ViewPortion mViewPortion;
        static Dictionary<string, DataType> mDataTypes = new Dictionary<string, DataType>();
        static StackedGenericDataHolder()
        {
            AddDataType(new GraphDataType());
        }
        static void AddDataType(DataType t)
        {
            mDataTypes.Add(t.Name, t);
        }
        public virtual void SetView(ViewPortion view)
        {
            mViewPortion = view;
            RaiseParentViewSet(view);
        }
        public event Action<ViewPortion> ParentViewSet;
        public void ValidateViews()
        {
            foreach(var view in mActiveViews.Values)
            {
                view.ApplySettings(mParentCategory);
            }
        }
        public void RaiseParentViewSet(ViewPortion view)
        {
            if (ParentViewSet != null)
                ParentViewSet(view);
        }
        Dictionary<string, IDataView> mActiveViews = new Dictionary<string, IDataView>();

        IDataViewerNotifier TryGetView(string name)
        {
            IDataView res = null;
            if (mActiveViews.TryGetValue(name, out res) == false)
                return null;
            return res;
        }
        IDataViewerNotifier IDataViewerNotifier.GetDataView(string name)
        {
            IDataView res = null;
            if(mActiveViews.TryGetValue(name,out res) == false)
            {
                DataType type;
                if (mDataTypes.TryGetValue(name, out type) == false)
                    throw new NotSupportedException("view type not supported by data viewer");
                res = type.CreateView(this);
                res.ApplySettings(mParentCategory);
                RaiseParentViewSet(mViewPortion);
                mActiveViews.Add(name,res);
            }
            return res;
        }
    }
}
