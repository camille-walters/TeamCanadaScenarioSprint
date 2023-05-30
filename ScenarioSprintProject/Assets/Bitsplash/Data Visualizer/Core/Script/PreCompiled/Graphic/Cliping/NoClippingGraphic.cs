using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;

namespace DataVisualizer{
    class NoClippingGraphic : ClipingChartGraphic
    {
        public NoClippingGraphic(IChartSeriesGraphic baseGraphic) : base(baseGraphic)
        {
        }

        public override bool AddEntity(SeriesObject entity)
        {
            return BaseGraphic.AddEntity(entity);
        }

        public override void Clear()
        {
            BaseGraphic.Clear();
        }

        public override void MakeAllDirty(bool uvOnly,SimpleList<SeriesObject> takenOut)
        {
            BaseGraphic.MakeAllDirty(uvOnly, takenOut);
        }

        public override MakeDirtyResult MakeEntityDirty(SeriesObject entity,bool uvOnly)
        {
            return BaseGraphic.MakeEntityDirty(entity, uvOnly);
        }
        public override void OnSetView(ViewPortion view)
        {
        }

        public override void RemoveEntity(SeriesObject entity)
        {
            BaseGraphic.RemoveEntity(entity);
        }
    }
}
