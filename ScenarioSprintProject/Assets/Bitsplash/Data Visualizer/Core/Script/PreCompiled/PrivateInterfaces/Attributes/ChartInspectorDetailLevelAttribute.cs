using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Data_Visualizer.Script.Attributes
{
    class ChartInspectorDetailLevelAttribute
    {
        public enum Level
        {
            Simple,
            Advanced
        }
        public Level DetailLevel
        {
            get; private set;
        }
        public ChartInspectorDetailLevelAttribute(Level level)
        {
            DetailLevel = level;
        }
    }
}
