
using System;
using System.Collections.Generic;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public abstract class CoreSeriesObject : PickableGraphicObject
    {
        public CoreSeriesObject()
        {

        }

        public override DoubleRect? BoundingBox(DataSeriesBase mapper)
        {
            return CalcBoundingBox(mapper);
        }

        public override void Destory()
        {

        }

        protected virtual bool MapTangents { get { return true; } }

        /// <summary>
        /// 
        /// </summary>
        protected abstract DoubleRect? CalcBoundingBox(DataSeriesBase mapper);

        public virtual DoubleVector3 Center(DataSeriesBase mapper)
        {
            DoubleRect? box = BoundingBox(mapper);
            if (box.HasValue == false)
                return DoubleVector3.zero;
            return box.Value.Center;
        }

        public override double SqaureDist(DataSeriesBase mapper,DoubleVector3 mouse)
        {
            DoubleRect? box = BoundingBox(mapper);
            if (box.HasValue == false)
                return double.MaxValue;
            return box.Value.SqrDistance(mouse);
        }

        //public virtual void ApplyToDataHover(DataSeriesBase parent,DataSeriesHoverGraphic graphic)
        //{
        //    //graphic.SetVertices(paresnt, mVertices, Center);
        //}
        
        //public virtual void UpdateDataHover(DataSeriesBase parent,DataSeriesHoverGraphic graphic)
        //{
        //   // graphic.SetCenter(parent, Center);
        //}

        public override void MakeUvDirty()
        {
            UvDirty = true;
        }

        public override void MakeDirty()
        {
            Dirty = true;
            InvalidateLength();
        }

    }
}
