using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    [Serializable]
    public struct DoubleRect
    {
        public double X, Y, Width, Height;

        public static DoubleRect FromTwoPoints(DoubleVector2 a, DoubleVector2 b)
        {
            double minX = Math.Min(a.x, b.x);
            double minY = Math.Min(a.y, b.y);
            double maxX = Math.Max(a.x, b.x);
            double maxY = Math.Max(a.y, b.y);
            return new DoubleRect(minX, minY, maxX - minX, maxY - minY);
        }
        public static DoubleRect CreateNan()
        {
            return new DoubleRect(double.NaN, double.NaN, double.NaN, double.NaN);
        }


        public DoubleRect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override bool Equals(object obj)
        {
            if (obj is DoubleRect == false)
                return base.Equals(obj);
            DoubleRect other = (DoubleRect)obj;
            return this == other;
        }

        public double SqrDistance(DoubleVector3 point)
        {
            if (Contains(point))
                return 0.0;
            DoubleVector3 min = Min;
            DoubleVector3 max = Max;
            DoubleVector3 minXmaxY = new DoubleVector3(min.x, max.y);
            DoubleVector3 maxXminY = new DoubleVector3(max.x, min.y);
            double sqrDist = ChartCommon.SegmentPointSqrDistance(min, minXmaxY, point);
            sqrDist = Math.Min(sqrDist, ChartCommon.SegmentPointSqrDistance(min, maxXminY, point));
            sqrDist = Math.Min(sqrDist, ChartCommon.SegmentPointSqrDistance(max, minXmaxY, point));
            return Math.Min(sqrDist, ChartCommon.SegmentPointSqrDistance(max, maxXminY, point));
        }

        public DoubleVector3 Center
        {
            get { return FromRectCoords(new DoubleVector3(0.5, 0.5, 0.0)); }
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
        }

        public static bool operator !=(DoubleRect a, DoubleRect b)
        {
            return ((a.X != b.X) || (a.Y != b.Y) || (a.Width != b.Width) || (a.Height != b.Height));
        }

        public static bool operator ==(DoubleRect a, DoubleRect b)
        {
            return ((a.X == b.X) && (a.Y == b.Y) && (a.Width == b.Width) && (a.Height == b.Height));
        }

        public override string ToString()
        {
            return string.Format("x:{0},y:{1},w:{2},h:{3}", X, Y, Width, Height);
        }

        public bool Contains(DoubleVector3 point)
        {
            if (point.x < X || point.y < Y || point.x > X + Width || point.y > Y + Height)
                return false;
            return true;
        }
        public bool Contains(Vector3 point)
        {
            if (point.x < X || point.y < Y || point.x > X + Width || point.y > Y + Height)
                return false;
            return true;
        }
        public bool Intersect(DoubleRect rect)
        {
            DoubleVector3 min1, min2, max1, max2;
            min1 = rect.Min;
            max1 = rect.Max;
            min2 = Min;
            max2 = Max;
            return (max2.x >= min1.x && min2.x <= max1.x) && (max2.y >= min1.y && min2.y <= max1.y);
        }

        public DoubleVector3 FromRectCoords(DoubleVector3 v)
        {
            double x = (v.x * Width) + X;
            double y = (v.y * Height) + Y;
            return new DoubleVector3(x, y, v.z);
        }

        public DoubleVector3 ToRectCoords(DoubleVector3 v)
        {
            double x = (v.x - X) / Width;
            double y = (v.y - Y) / Height;
            return new DoubleVector3(x, y, v.z);
        }

        public Rect ToRect()
        {
            return new Rect((float)X, (float)Y, (float)Width, (float)Height);
        }

        public bool IsNan
        {
            get { return double.IsNaN(X)|| double.IsNaN(Y) || double.IsNaN(Width)|| double.IsNaN(Height); }
        }

        public void NanToZero()
        {
            if (double.IsNaN(X))
                X = 0.0;
            if (double.IsNaN(Y))
                Y = 0.0;
            if (double.IsNaN(Width))
                Width = 0.0;
            if (double.IsNaN(Height))
                Height = 0.0;
        }
        public void Inflate(double size)
        {
            double half = size * 0.5;
            X -= half;
            Y -= half;
            Width += size;
            Height += size;
        }
        public void UnionVector(DoubleVector3 v)
        {
            if (IsNan)
            {
                X = v.x;
                Y = v.y;
                Width = 0.0;
                Height = 0.0;
                return;
            }
            double endX = X + Width;
            if (X < v.x)
            {
                Width = endX - v.x;
                X = v.x;
            }
            else if (endX > v.x)
                Width = v.x - X;

            double endY = Y + Height;
            if (Y < v.y)
            {
                Height = endY - v.y;
                Y = v.y;
            }
            else if (endY > v.y)
                Height = v.y - Y;
        }

        public void UnionYRange(DoubleRange range)
        {
            if (IsNan)
            {
                X = 0.0;
                Y = range.Min ;
                Width = 0.0;
                Height = range.Size;
                return;
            }
            double end = Y + Height;
            if(Y < range.Min)
            {
                Height = end - range.Min;
                Y = range.Min;
            }
            if(end > range.Max)
                Height = range.Max - Y;
                
        }
        public static DoubleRect Lerp(DoubleRect from, DoubleRect to,double t)
        {
            double x = ChartCommon.DoubleLerp(from.X, to.X, t);
            double y = ChartCommon.DoubleLerp(from.Y, to.Y, t);
            double x1 = ChartCommon.DoubleLerp(from.X + from.Width, to.X + to.Width, t);
            double y1 = ChartCommon.DoubleLerp(from.Y + from.Height, to.Y + to.Height, t);
            return new DoubleRect(x, y, x1 - x, y1 - y);
        }

        public DoubleVector3 Min { get { return new DoubleVector3(X, Y); } }
        public DoubleVector3 Max { get { return new DoubleVector3(X + Width, Y + Height); } }
        public DoubleVector3 Size { get { return new DoubleVector3(Width, Height); } }

    }
}
