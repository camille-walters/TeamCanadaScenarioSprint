using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// A graphic object meant for use under canvas
    /// </summary>
    public abstract class PickableGraphicObject : SeriesObject
    {
        public class SeriesObjectData
        {
            public SeriesObjectData(PickableGraphicObject obj, int type,int index)
            {
                Object = obj;
                Type = type;
                Index = index;
            }
            public PickableGraphicObject Object { get; private set; }
            public int Type { get; private set; }
            public int Index { get; private set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">an integer representing the type of this graphic in relation to it's container. For example , for candle chart type 0 can be the candle thin part and type 1 can be the candle thick part ( use is up to implemntation ). The type is returned using GetSelectionData</param>
        /// <param name="userData">userdata to be returned using GetSelectionData</param>
        public PickableGraphicObject()
        {
        }

        public virtual byte Type { get { return 0; }  }
        /// <summary>
        /// takes the square distance between this object and the specified position
        /// </summary>
        /// <param name="mouse"></param>
        /// <returns></returns>
        public abstract double SqaureDist(DataSeriesBase mapper,DoubleVector3 mouse);

        /// <summary>
        /// returns the selection data for this graphic object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="userData"></param>
        public SeriesObjectData GetSelectionData()
        {
            return new SeriesObjectData(this, Type, MyIndex);
        }

        /// <summary>
        /// returns the bounding box of this object, or null if there is non
        /// </summary>
        public abstract DoubleRect? BoundingBox(DataSeriesBase mapper);


        /// <summary>
        /// for a 3d object this contains the z bounds. they are joind with mBoundingBox to create a 3D bound. This is done to perserve memory on 2d object and still benifit from polymorphism
        /// </summary>
        public virtual Range ZBounds { get { return Range.Empty; } }

    }
}
