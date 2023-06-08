using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public abstract class VariableSeriesObject : CoreSeriesObject
    {
        int mItemCount = 0;

        public override int ItemCount
        {
            get { return mItemCount; }
        }

        
        /// <summary>
        /// calculates the current item count
        /// </summary>
        /// <returns></returns>
        protected abstract int CalculateItemCount(DataSeriesBase mapper);

        public override bool EnsureItemCount(DataSeriesBase mapper)
        {
            int newCount = CalculateItemCount(mapper);
            if (newCount == 0)
                throw new Exception("seriesobject can never have 0 items");
            if (mItemCount != newCount)
            {
                mItemCount = newCount;
                return true;
            }
            return false;
        }

    }
}
