using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DataVisualizer
{    

    [Serializable]
    public struct ScrollingValue
    {
        public ScrollingValue(double value)
        {
            Value = value;
            Type = 0;
        }
        [SerializeField]
        public double Value;
        [SerializeField]
        [HideInInspector]
        public int Type;
    }
}
