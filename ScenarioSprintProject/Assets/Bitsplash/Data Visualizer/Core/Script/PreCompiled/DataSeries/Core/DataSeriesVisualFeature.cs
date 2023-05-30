using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public abstract partial class DataSeriesVisualFeature : VisualFeatureBase
    {      
        [SerializeField]
        [HideInInspector]
        protected bool handleEvents;

        public bool HandleEvents
        {
            get { return handleEvents; }
            set
            {
                handleEvents = value;
                DataChanged();
            }
        }

      //  [SerializeField]
        protected bool useColorChannel = false;

        protected bool UseColorChannel
        {
            get { return useColorChannel; }
            set
            {
                useColorChannel = value;
                DataChanged();
            }
        }

        /// <summary>
        /// Generates a IDataSeries object that matches this Visual Feature
        /// </summary>
        /// <returns></returns>
        public abstract IDataSeries GenerateSeries(GameObject obj);

    }
}
