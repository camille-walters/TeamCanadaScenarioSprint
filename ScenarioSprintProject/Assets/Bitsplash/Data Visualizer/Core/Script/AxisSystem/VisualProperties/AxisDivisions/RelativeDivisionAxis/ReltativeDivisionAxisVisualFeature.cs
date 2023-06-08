using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    abstract class ReltativeDivisionAxisVisualFeature : DivisionAxisVisualFeature
    {
        protected int TotalDivisions = 3;

        public override string VisualFeatureTypeName
        {
            get { return "relativeDivisionAxis"; }
        }
    }
}
