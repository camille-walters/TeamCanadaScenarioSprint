using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DataVisualizer
{
    [Serializable]
    public class OffsetVector
    {
        private event Action InnerChanged;

        public event Action Changed
        {
            add
            {
                InnerChanged += value;
                parallel.Changed += Item_Changed;
                orthogonal.Changed += Item_Changed;
                depth.Changed += Item_Changed;
            }
            remove
            {
                InnerChanged -= value;
                parallel.Changed -= Item_Changed;
                orthogonal.Changed -= Item_Changed;
                depth.Changed -= Item_Changed;
            }
        }

        private void Item_Changed()
        {
            if (InnerChanged != null)
                InnerChanged();
        }

        public OffsetVector Clone()
        {
            var offs = new OffsetVector();
            offs.parallel.Percent = parallel.Percent;
            offs.orthogonal.Percent = orthogonal.Percent;
            offs.parallel.Pixels = parallel.Pixels;
            offs.orthogonal.Pixels = orthogonal.Pixels;
            return offs;
        }

        public void Flip()
        {
            OffsetValue v;
            v = parallel;
            parallel = orthogonal;
            orthogonal = v;
        }
        [SerializeField]
        OffsetValue parallel = new OffsetValue();
        [SerializeField]
        OffsetValue orthogonal = new OffsetValue();
        [SerializeField]
        [HideInInspector]
        OffsetValue depth = new OffsetValue();

        
        public OffsetValue Parallel
        {
            get { return parallel; }
        }

        public OffsetValue Orthogonal
        {
            get { return orthogonal; }
        }

        public OffsetValue Depth
        {
            get { return depth; }
        }
        public DoubleVector3 Calculate(DoubleRect? fitRect)
        {
            if (fitRect.HasValue == false)
                return new DoubleVector3(Parallel.Pixels, Orthogonal.Pixels, depth.Pixels);
            return new DoubleVector3(Parallel.Pixels + fitRect.Value.Width * parallel.Percent/100.0, Orthogonal.Pixels + fitRect.Value.Height * Orthogonal.Percent / 100.0, depth.Pixels);
        }
    }
}
