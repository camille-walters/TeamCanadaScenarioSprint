using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    [Serializable]
    public struct MappingFunction
    {
        DoubleVector3 mAdd;
        DoubleVector3 mMult;
        DoubleRect mFromRect;
        DoubleRect mToRect;

        public DoubleVector3 Add { get { return mAdd; } }
        public DoubleVector3 Mult { get { return mMult; } }

        public MappingFunction(DoubleRect from, DoubleRect to)
        {
            mFromRect = from;
            mToRect = to;
            CreateMappingVectors(from, to, out mMult, out mAdd);
        }
        public bool IsZero()
        {
            MappingFunction zero = Zero;
            if (mMult.x == 0 || mMult.y == 0)
                return true;
            return false;
        }
        public bool IsOne()
        {
            MappingFunction one = One;
            if (mAdd != one.mAdd || mMult != one.mMult)
                return false;
            return true;
        }
        private MappingFunction(DoubleVector3 mult, DoubleVector3 add, DoubleRect from, DoubleRect to)
        {
            mMult = mult;
            mAdd = add;
            mFromRect = from;
            mToRect = to;
        }

        public bool IsValid()
        {
            if (!ChartCommon.IsValid(Add) || !ChartCommon.IsValid(Mult))
                return false;
            if (Math.Abs(Mult.x) < 0.0000001)
                return false;
            if (Math.Abs(Mult.y) < 0.0000001)
                return false;
            return true;
        }

        private static void CreateMappingVectors(DoubleRect from, DoubleRect to, out DoubleVector3 mult, out DoubleVector3 add)
        {
            add = new DoubleVector3(-from.X * (to.Width / from.Width) + to.X,
                         -from.Y * (to.Height / from.Height) + to.Y, 0.0);
            mult = new DoubleVector3(to.Width / from.Width, to.Height / from.Height, 1.0);
        }

        static bool CheckMagnitudeValid(double magnitude, double threshold)
        {
            if (magnitude <= float.Epsilon)
                return false;
            if (magnitude < 1.0)
                magnitude = 1 / magnitude;
            if (magnitude < threshold)
                return true;
            return false;
        }

        public MappingFunction ModifyY(DoubleRect from, DoubleRect to)
        {
            MappingFunction res = this;
            DoubleVector3 mult, add;
            CreateMappingVectors(from, to, out mult, out add);
            res.mMult.y = mult.y;
            res.mAdd.y = add.y;
            return res;
        }
        public MappingFunction ModifyY(MappingFunction withData)
        {
            MappingFunction res = this;
            res.mMult.y = withData.Mult.y;
            res.mAdd.y = withData.Add.y;
            return res;
        }

        public MappingFunction ModifyX(MappingFunction withData)
        {
            MappingFunction res = this;
            res.mMult.x = withData.Mult.x;
            res.mAdd.x = withData.Add.x;
            return res;
        }

        public MappingFunction ModifyX(DoubleRect from, DoubleRect to)
        {
            MappingFunction res = this;
            DoubleVector3 mult, add;
            CreateMappingVectors(from, to, out mult, out add);
            res.mMult.x = mult.x;
            res.mAdd.x = add.x;
            return res;
        }

        public MappingFunction ModifyY(double add, double mult)
        {
            MappingFunction res = this;
            res.mMult.y = mult;
            res.mAdd.y = add;
            return res;
        }

        public MappingFunction ModifyX(double add,double mult)
        {
            MappingFunction res = this;
            res.mMult.x = mult;
            res.mAdd.x = add;
            return res;
        }
        /// <summary>
        /// returns the diffrence mapping between this mapping and the specified one. 
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public MappingFunction? CreateDifferenceMapping(DoubleRect from, DoubleRect to, double sizeThreshold, double addThreshold)
        {
            DoubleVector3 invertMult, invertAdd;
            DoubleVector3 newMult, newAdd;
            if (to != mToRect) 
                return null; 
            CreateMappingVectors(mToRect, mFromRect, out invertMult, out invertAdd);    // invert this trasform
            CreateMappingVectors(from, to, out newMult, out newAdd);
            DoubleVector3 add = invertAdd * newMult + newAdd;
            DoubleVector3 mult = invertMult * newMult;

            if (CheckMagnitudeValid(mult.x, sizeThreshold) == false)
                return null;
            if (CheckMagnitudeValid(mult.y, sizeThreshold) == false)
                return null;
            if (Math.Abs(add.x) > addThreshold)
                return null;
            if (Math.Abs(add.y) > addThreshold)
                return null;
            MappingFunction res = new MappingFunction(mult, add, from, to);
            if (res.IsValid() == false)
                return null;
            return res;
        }
        public override string ToString()
        {
            return "{Multiply:" + mMult + ",Add:" + mAdd + "}";
        }

        public static MappingFunction One
        {
            get { return new MappingFunction(new DoubleVector3(1.0, 1.0, 1.0), new DoubleVector3(), new DoubleRect(), new DoubleRect()); }
        }

        public static MappingFunction Zero
        {
            get { return new MappingFunction(); }
        }

        public Vector2 MapUv(Vector2 uv)
        {
            return new Vector2((float)(uv.x * mMult.x  + mAdd.x) , (float)(uv.y * mMult.y + mAdd.y));
        }

        public Vector4 MapDirectionToFloat(Vector4 v)
        {
            return new Vector4()
            {
                x = (float)(v.x * mMult.x),
                y = (float)(v.y * mMult.y),
                z = v.z,
            };
        }
        public Vector2 MapToFloat(DoubleVector3 v,float x)
        {
            return new Vector2(x, (float)(v.y * mMult.y + mAdd.y));
        }

        public Vector3 MapToFloat(DoubleVector3 v)
        {
            return new Vector3()
            {
                x = (float)(v.x * mMult.x + mAdd.x),
                y = (float)(v.y * mMult.y + mAdd.y),
                z = (float)v.z
            };
        }

        public DoubleVector3 MapVector(DoubleVector3 v)
        {
            return new DoubleVector3(v.x * mMult.x + mAdd.x, v.y * mMult.y + mAdd.y, v.z);
        }

    }
}
