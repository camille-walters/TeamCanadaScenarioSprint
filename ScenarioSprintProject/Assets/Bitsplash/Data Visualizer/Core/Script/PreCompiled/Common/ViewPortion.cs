using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer
{
    [Serializable]
    public struct ViewPortion
    {
        public static ViewPortion Zero { get; private set; }

        public DoubleVector3 From, To;
        public double ViewDiagonalBase;
        public bool OppositeX, OppositeY;

        public ViewPortion(double x ,double y, double width,double height,double diagonal,bool oppositeX, bool oppositeY)
        {
            ViewDiagonalBase = diagonal;
            From = new DoubleVector3(x, y, 0.0);
            To = new DoubleVector3(x + width, y + height, 0.0);
            OppositeX = oppositeX;
            OppositeY = oppositeY;
        }

        public override string ToString()
        {
            return "{From:" + From + ", To:" + To + ",OppX:" + OppositeX +",OppY:" + OppositeY + "}";
        }

        public double Width { get { return Math.Abs(To.x - From.x); } }
        public double Height { get { return Math.Abs(To.y - From.y); } }
        public void MakeIdentitySize()
        {
            To.x = From.x + 1.0;
            To.y = From.y + 1.0;
            To.z = From.z + 1.0;
        }
        public bool IsZeroSize()
        {
            const double error = 0.00001;
            if (Math.Abs(Width) < error && Math.Abs(Height) < error)
                return true;
            return false;

        }
        public static ViewPortion FromDoubleRect(DoubleRect portion)
        {
            return new ViewPortion()
            {
                From = portion.Min,
                To = portion.Max,
            };
        }

        public void InflateWithEpsilon()
        {
            DoubleVector3 epsilon = new DoubleVector3(1.0000001, 1.0000001, 0.0);
            From *= new DoubleVector3(1/epsilon.x,1/epsilon.y,0);
            To *= epsilon;
        }
        public double NormlizeWidth(double width)
        {
            return width / Width;
        }
        public double NormlizeHeight(double height)
        {
            return height / Height;
        }
        public DoubleVector3 Min
        {
            get { return new DoubleVector3(Math.Min(From.x, To.x), Math.Min(From.y, To.y), Math.Min(From.z, To.z)); }
        }
        public DoubleVector3 Max
        {
            get { return new DoubleVector3(Math.Max(From.x, To.x), Math.Max(From.y, To.y), Math.Max(From.z, To.z)); }
        }
        public DoubleRect ToRect()
        {
            return new DoubleRect(From.x, From.y, Width, Height);
        }
        public bool CompareWithError(ViewPortion other,double error)
        {
            if (OppositeX != other.OppositeX || OppositeY != other.OppositeY)
                return false;
            if (ChartCommon.CompareDoubleVector(From, other.From, error) == false)
                return false;
            if (ChartCommon.CompareDoubleVector(To, other.To, error) == false)
                return false;
            return ChartCommon.CompareDouble(ViewDiagonalBase, other.ViewDiagonalBase, error);

        }
    }
}
