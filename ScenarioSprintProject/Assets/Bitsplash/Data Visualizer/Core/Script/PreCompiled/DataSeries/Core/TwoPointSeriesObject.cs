using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public abstract class TwoPointSeriesObject : FixedSeriesObject
    {

        public TwoPointSeriesObject()
        {

        }

        protected void AssignFromTo(DataSeriesBase mapper,ref DoubleVector3? from,ref DoubleVector3? to)
        {
            from = null;
            to = null;
            var rawFromArray = mapper.RawData.RawPositionArray;
            if (mapper.FromArray == VectorDataSource.EndPositions)
                rawFromArray = mapper.RawData.RawEndPositionArray;
            var rawToArray = mapper.RawData.RawPositionArray;
            if (mapper.ToArray == VectorDataSource.EndPositions)
                rawToArray = mapper.RawData.RawEndPositionArray;

            int next = MyIndex + mapper.ToOffset;
            
            if (mapper.RawData.Count <= next) // no next point so there's no line
                return;
            from = rawFromArray.Get(MyIndex).TrimZ();
            to = rawToArray.Get(next).TrimZ();
        }

        protected override double CalculateLength(DataSeriesBase mapper)
        {
            DoubleVector3? from = null, to = null;
            AssignFromTo(mapper,ref from,ref to);
            if (from.HasValue == false || to.HasValue == false)
                return 0.0;
            return (from.Value - to.Value).magnitude;
        }

        public override DoubleVector3 Center(DataSeriesBase mapper)
        {
            DoubleVector3? from = null, to = null;
            AssignFromTo(mapper,ref from, ref to);
            if (from.HasValue == false)
                return new DoubleVector3();
            return from.Value;
        }
    }
}
