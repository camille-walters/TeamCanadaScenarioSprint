using DataVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualizer
{
    public class UnspecifiedDataType : DataType
    {
        public UnspecifiedDataType()
        {
        }

        public override string Name { get { return "Unspecified"; } }


        public override DataType Clone()
        {
            return new UnspecifiedDataType();
        }

        private class EmptyDataView : DataView
        {
            public EmptyDataView(IDataViewerNotifier mainView) : base(mainView)
            {
            }

            protected override void FirstLoadData()
            {
                
            }
        }
        public override IDataView CreateView(IDataViewerNotifier mainView)
        {
            return new EmptyDataView(mainView);
        }
    }
}
