using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public abstract class StackedDataSeriesVisualFeature : DataSeriesVisualFeature
    {
        [HideInInspector]
        [SerializeField]
        int stackIndex;

        /// <summary>
        /// the stack index this visual feature will be applied on.
        /// </summary>
        public int StackIndex
        {
            get { return stackIndex; }
            set { stackIndex = value;
                DataChanged();
            }
        }
    }

}
