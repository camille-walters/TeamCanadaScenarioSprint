using Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews;
using Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews.GraphDataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThetaList;
using UnityEngine;

namespace DataVisualizer
{
    public class GraphDataType : DataType
    {
        public const string ViewName = "Graph Data";
        public override string Name { get { return ViewName; } }


        public override DataType Clone()
        {
            return new GraphDataType();
        }

        public override IDataView CreateView(IDataViewerNotifier mainView)
        {
            return new GraphDataView( new GraphPagerView(mainView));
        }


    }
}
