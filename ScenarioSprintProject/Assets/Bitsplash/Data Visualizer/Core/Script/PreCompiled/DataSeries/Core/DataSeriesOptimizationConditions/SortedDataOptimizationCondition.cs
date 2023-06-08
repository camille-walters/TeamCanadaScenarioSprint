using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    public class SortedDataOptimizationCondition //: IDataSeriesOptimizationCondition
    {
        public static readonly Selector BoundingBoxSorted = new Selector("BoundingBoxSorted", BoundingBoxCompare);
        DataSeriesBase mMapper;

        public class Selector
        {
            public Selector(string name, CompareDelegate del)
            {
                Name = name;
                Delegate = del;
            }
            public CompareDelegate Delegate { get; private set; }
            public string Name { get; private set; }
        }

        private static bool BoundingBoxCompare(int index1, int index2, DataSeriesBase mapper)
        {
            DoubleRect? r1 = mapper.GetBoundingBox(index1);
            DoubleRect? r2 = mapper.GetBoundingBox(index2);
            if (r1.HasValue == false || r2.HasValue == false)
                return false;
            if (r1.Value.Max.x < r2.Value.Min.x)
                return true;
            return false;
        }

        /// <summary>
        /// a refrence to the data of the data series
        /// </summary>
        IDataViewerNotifier mData;
        Selector mSelector;
        bool mIsSorted = false;

        bool mCheckOnRemove = false;

        /// <summary>
        /// compares index1 and index2 to see if they are sorted
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public delegate bool CompareDelegate(int index1,int index2,DataSeriesBase mapper);
        
        public SortedDataOptimizationCondition(Selector selector,DataSeriesBase mapper)
        {
            mMapper = mapper;
            mSelector = selector;
        }

        public bool DataCanBeOptimized
        {
            get
            {
                return mIsSorted;
            }
        }

        public string Name
        {
            get
            {
                if (mSelector == null)
                    return "";
                return mSelector.Name;
            }
        }

        bool CheckIndexSorted(int index)
        {
            if (mData == null || mSelector == null)
                return false;
            if (index > 0)
            {
                if (mSelector.Delegate(index - 1, index, mMapper) == false)
                    return false;
            }
            int nextIndex = index + 1;
            if(nextIndex < mData.Count)
            {
                if (mSelector.Delegate(index, nextIndex, mMapper) == false)
                    return false;
            }
            return true;
        }

        bool CheckSorted()
        {
            if (mData == null || mSelector == null)
                return false;
            if (mData.Count == 0)
                return true;
            for(int i=1; i<mData.Count; i++)
            {
                if(mSelector.Delegate(i-1, i, mMapper))
                    return false;
            }
            return true;
        }

        public void OnAppend(int index)
        {
            OnInsert(index);
        }

        public void OnInit()
        {

        }

        public void OnClear()
        {
            mIsSorted = true;
        }

        public void OnInsert(int index)
        {
            if(mIsSorted)
                 mIsSorted = CheckIndexSorted(index);
        }
        
        public void OnValueModified(int index)
        {
            if(mIsSorted)
                 mIsSorted = CheckIndexSorted(index);
        }

        public void OnBeforeRemove(int index)
        {
            if (CheckIndexSorted(index) == false)   // if the item index that is being removed is sorted , then removing it will keep the array sorted or unsorted ( the array state will not change)
                mCheckOnRemove = false;
            else
                mCheckOnRemove = true; // if the item was not ordered we must check if the array has just become sorted 
        }

        public void OnRemove(int index)
        {
            if (mIsSorted == true) // removing from a sorted array will keep it sorted
                return;
            if(mCheckOnRemove)
            {
                // possibillity to add a check to see if the array has became sorted after not being sorted
                // however the requirement currently is that the array will not break order even once during insertion removal and data manipulation
            }
            mCheckOnRemove = false;
        }

        public void OnSet(IDataViewerNotifier data)
        {
            mData = data;
            mIsSorted = CheckSorted();
        }

    }
}
