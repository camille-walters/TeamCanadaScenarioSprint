using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    /// <summary>
    /// A manager class for helper arrays. This class holds array with a specific size. these arrays can be used for intermidate tasks that require arrays of an exact size
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class HelperArrayManager<T>
    {
        static HelperArrayManager<T> mInstance = new HelperArrayManager<T>();
        public static HelperArrayManager<T> Instance
        {
            get
            {
                return mInstance;
            }
        }

        const int MaxSizeCount = 3;
        const int MaxArrayCount = 2;
        
        Dictionary<int, List<T[]>> mArrays = new Dictionary<int, List<T[]>>();
        HashSet<T[]> mLocked = new HashSet<T[]>();

        public T[] LockArray(int count)
        {
            List<T[]> items;
            if(mArrays.TryGetValue(count,out items) == false)
            {
                items = new List<T[]>();
                if (mArrays.Count >= MaxSizeCount)
                    throw new Exception("To many helper arrays");
                mArrays.Add(count, items);
            }
            for(int i=0; i<items.Count; i++)
            {
                var arr = items[i];
                if (mLocked.Contains(arr))
                    continue;
                mLocked.Add(arr);
                ChartIntegrity.Assert(arr.Length == count);
                return arr;
            }
            // no free array found
            if(items.Count >= MaxArrayCount)
                throw new Exception("To many helper arrays");
            T[] newArr = new T[count];
            items.Add(newArr);
            mLocked.Add(newArr);
            return newArr;
        }

        public void UnlockArray(T[] array)
        {
            if (mLocked.Remove(array) == false)
                throw new Exception("array was never locked");
        }
    }
}
