using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#pragma warning disable 0414
namespace DataVisualizer
{

    [Serializable]
    public struct SpanningValue
    {
        public SpanningValue(double value)
        {
            Value = value;
            Type = 0;
        }
        [SerializeField]
        public double Value;
        [SerializeField]
        [HideInInspector]
        private int Type;
    }
}
