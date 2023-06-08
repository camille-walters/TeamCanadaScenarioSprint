using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DataVisualizer{
    [Serializable]
    public abstract class FixedDivisionAxisVisualFeature : DivisionAxisVisualFeature
    {

        /// <summary>
        /// the unit gap between each division in the axis
        /// </summary>
        ///         
        [SerializeField]
        [Tooltip("the unit gap between each division in the axis")]
        protected SpanningValue gapUnits;

        /// <summary>
        /// the unit gap between each division in the axis
        /// </summary>
        public double GapUnits
        {
            get { return gapUnits.Value; }
            set
            {
                gapUnits.Value = value;
                DataChanged();
            }
        }

        public TimeSpan GapUnitsTimeSpan

        {
            get { return DateUtility.ValueToTimeSpan(GapUnits); }
            set
            {
                GapUnits = DateUtility.TimeSpanToValue(value);
            }
        }
        /// <summary>
        /// the maximum amount of divisions shown at once
        /// </summary>         
        [SerializeField]
        [Tooltip("the maximum amount of divisions shown at once")]
        protected uint maxDivisions = 500;

        /// <summary>
        /// the maximum amount of divisions shown at once
        /// </summary>
        public uint MaxDivisions
        {
            get { return maxDivisions; }
            set
            {
                maxDivisions = value;
                DataChanged();
            }
        }

        /// <summary>
        /// the maximum amount of divisions shown at once
        /// </summary>         
        [SerializeField]
        [HideInInspector]
        [Tooltip("the log base for max divisions")]
        protected double maxDivisionsBase = 5;

        /// <summary>
        /// the maximum amount of divisions shown at once
        /// </summary>
        public double MaxDivisionsBase
        {
            get { return maxDivisionsBase; }
            set
            {
                maxDivisionsBase = value;
                DataChanged();
            }
        }

    }
}
