using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;

namespace DataVisualizer{
    public interface IChartSeriesGraphic : IChartGraphic
    {
        bool AddEntity(SeriesObject entity);

        void RemoveEntity(SeriesObject entity);

        /// <summary>
        /// make all entites dirty
        /// </summary>
        void MakeAllDirty(bool uvOnly, SimpleList<SeriesObject> takenOut);

        MakeDirtyResult MakeEntityDirty(SeriesObject entity, bool uvOnly);

        /// <summary>
        /// clears the entities within the graphic object
        /// </summary>
        void Clear();

        /// <summary>
        /// if true no memory is held for the chart vertices. This will reduce memory used , but will require full regeneration of the mesh on change
        /// </summary>
        int ItemSize { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="staticGeneration"></param>
        /// <param name="isCompact"></param>
        void ClearAndChangeProperties(ArrayManagerType arrayType, int itemSize);

        /// <summary>
        /// 
        /// </summary>
        void RefreshHoverObjects();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="context"></param>
        void SetDataLink(DataSeriesBase parent, ushort context);
    }
}
