using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    internal struct Line
    {
        public Line(DoubleVector3 from, DoubleVector3 to, double halfThickness, bool hasNext, bool hasPrev) : this()
        {
            HalfThickness = halfThickness;
            DoubleVector3 diff = (to - from);
            double magDec = 0;
            if (hasNext)
                magDec += halfThickness;
            if (hasPrev)
                magDec += halfThickness;
            Mag = diff.magnitude - magDec * 2;
            Degenerated = false;
            if (Mag <= 0)
                Degenerated = true;
            Dir = diff.normalized;
            DoubleVector3 add = halfThickness * 2 * Dir;
            if (hasPrev)
                from += add;
            if (hasNext)
                to -= add;
            From = from;
            To = to;
            Normal = new DoubleVector3(Dir.y, -Dir.x, Dir.z); // this part calculates the line inset and points based on thichkness
            P1 = From + Normal * halfThickness;
            P2 = from - Normal * halfThickness;
            P3 = to + Normal * halfThickness;
            P4 = to - Normal * halfThickness;
        }

        public double HalfThickness { get; private set; }
        public bool Degenerated { get; private set; }
        public DoubleVector3 P1 { get; private set; }
        public DoubleVector3 P2 { get; private set; }
        public DoubleVector3 P3 { get; private set; }
        public DoubleVector3 P4 { get; private set; }

        public DoubleVector3 From { get; private set; }
        public DoubleVector3 To { get; private set; }
        public DoubleVector3 Dir { get; private set; }
        public double Mag { get; private set; }
        public DoubleVector3 Normal { get; private set; }

        /// <summary>
        /// this method inflate one point in a line with the specified distantce. This enable the graphic to control the thickness of lines
        /// </summary>
        /// <param name="point"></param>
        /// <param name="dir"></param>
        /// <param name="normal"></param>
        /// <param name="dist"></param>
        /// <param name="size"></param>
        /// <param name="z"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        void GetSide(DoubleVector3 point, DoubleVector3 dir, DoubleVector3 normal, double dist, double size, double z, out DoubleVector3 p1, out DoubleVector3 p2)
        {
            point.z = z;
            point += dir * dist;
            normal *= size;
            p1 = point + normal;
            p2 = point - normal;
        }

        public void GetPrev(double halfThickness,out DoubleVector3 v1, out DoubleVector3 v2, out DoubleVector3 v3, out DoubleVector3 v4)
        {
            DoubleVector3 a1, a2;
            GetSide(To, Dir, Normal, halfThickness * 0.5f, HalfThickness * 0.6f, 0f, out a1, out a2);
            v1 = P3;
            v2 = P4;
            v3 = a1;
            v4 = a2;
        }

        public void GetNext(double halfThickness, out DoubleVector3 v1, out DoubleVector3 v2, out DoubleVector3 v3, out DoubleVector3 v4)
        {
            DoubleVector3 a1, a2;
            GetSide(From, -Dir, Normal, halfThickness * 0.5f, halfThickness * 0.6f, 0f, out a1, out a2);
            v1 = a1;
            v2 = a2;
            v3 = P1;
            v4 = P2;
        }
    }
}
