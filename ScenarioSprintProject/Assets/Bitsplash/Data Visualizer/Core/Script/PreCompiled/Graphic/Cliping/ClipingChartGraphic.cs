using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// a decorator of a chart graphic that applies clipping to it
    /// </summary>
    public abstract class ClipingChartGraphic : IChartSeriesGraphic
    {
        public IChartSeriesGraphic BaseGraphic { get; private set; }

        public ClipingChartGraphic(IChartSeriesGraphic baseGraphic)
        {
            BaseGraphic = baseGraphic;
            BaseGraphic.ParentGraphic = this;
        }

        public ClipingChartGraphic()
        {
        }

        public bool TringleStrip
        {
            get {return BaseGraphic.TringleStrip; }
            set { BaseGraphic.TringleStrip = value; }
        }

        public MappingFunction UvMapping
        {
            get { return BaseGraphic.UvMapping; }
            set
            { 
                BaseGraphic.UvMapping = value;
            }
        }

        public bool IsObjectActive
        {
            get { return BaseGraphic.IsObjectActive; }
        }

        public GameObject gameObject
        {
            get {
                if (((MonoBehaviour)BaseGraphic) == null)
                    return null;
                return BaseGraphic.gameObject; 
            }
        }

        public int VertexCapacity
        {
            get { return BaseGraphic.VertexCapacity; }
        }

        public int ItemSize
        {
            get { return BaseGraphic.ItemSize; }
        }

        public float ExtrusionAmount { get { return BaseGraphic.ExtrusionAmount; }  set { BaseGraphic.ExtrusionAmount = value; } }

        public bool MaintainAngles { get { return BaseGraphic.MaintainAngles; } set { BaseGraphic.MaintainAngles = value; } }

        public Matrix4x4 DirectionTranform { get { return BaseGraphic.DirectionTranform; } set { BaseGraphic.DirectionTranform = value; } }



        public bool HasColor { get { return BaseGraphic.HasColor; } set { BaseGraphic.HasColor = value; } }

        public bool HasTangent { get { return BaseGraphic.HasTangent; } set { BaseGraphic.HasTangent = value; } }

        public int VertexCount { get { return BaseGraphic.VertexCount; } }

        public IChartGraphic ParentGraphic { get; set; }

        public abstract bool AddEntity(SeriesObject entity);

        public abstract void Clear();

        public void ClearAndChangeProperties(ArrayManagerType arrayType, int itemSize)
        {
            BaseGraphic.ClearAndChangeProperties(arrayType, itemSize);
        }

        public bool HasFeature(string featureName)
        {
            return BaseGraphic.HasFeature(featureName);
        }

        public bool HasFeature(int id)
        {
            return BaseGraphic.HasFeature(id);
        }

        public void Invalidate()
        {
            BaseGraphic.Invalidate();
        }

        public abstract void MakeAllDirty(bool uvOnly,SimpleList<SeriesObject> takenOut);

        public abstract void RemoveEntity(SeriesObject entity);

        public abstract MakeDirtyResult MakeEntityDirty(SeriesObject entity,bool UvOnly);

        public abstract void OnSetView(ViewPortion view);

        
        public void RefreshHoverObjects()
        {
            BaseGraphic.RefreshHoverObjects();
        }

        public void SetDataLink(DataSeriesBase parent, ushort context)
        {
            BaseGraphic.SetDataLink(parent, context);
        }

        public void SetMaterial(Material mat, bool isShared)
        {
            BaseGraphic.SetMaterial(mat, isShared);
        }

        public void OnCreated(int targetVertexCount)
        {
            BaseGraphic.OnCreated(targetVertexCount);
        }

        public void SetVisible(bool enabled)
        {
            BaseGraphic.SetVisible(enabled);
        }
    }
}
