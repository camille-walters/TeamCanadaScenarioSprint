using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DataVisualizer
{
    [Serializable]
    public class OffsetValue
    {
        public event Action Changed;
        public OffsetValue()
        {
        }
        [SerializeField]
        private double pixels;
        [SerializeField]
        private double percent;
        public double Pixels
        {
            get { return pixels; }
            set
            {
                pixels = value;
                if (Changed != null)
                    Changed();
            }
        }
        public double Percent
        {
            get { return percent; }
            set
            {
                percent = value;
                if (Changed != null)
                    Changed();
            }
        }
    }
}
