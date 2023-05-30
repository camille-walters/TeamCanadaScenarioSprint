using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ThetaList;

namespace DataVisualizer{
    public class ListViewer<T> : IEnumerable<T> where T:struct
    {
        SimpleList<T> mList;
        public ListViewer(SimpleList<T> list)
        {
            mList = list;
        }
        public T[] RawArray
        {
            get { return mList.RawArrayWithExtraLastItem; }
        }
        /// <summary>
        /// read the specified index in the list
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index] { get
            {
                return mList[index];
            } }
        /// <summary>
        /// get the item count in the list
        /// </summary>
        public int Count
        {
            get { return mList.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mList.GetEnumerator();
        }
    }
}
