using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThetaList
{
    /// <summary>
    /// a simple implementation of an array list 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleList<T> : IList<T>
    {
        /// <summary>
        /// the work queue is used when applying an operation tree to the list
        /// </summary>
        [ThreadStatic]
        static Queue<T> mInnerWorkQueue;

        static Queue<T> mWorkQueue
        {
            get
            {
                if (mInnerWorkQueue == null)
                    mInnerWorkQueue = new Queue<T>();
                return mInnerWorkQueue;
            }
        }

        int mCount = 0;
        T[] mData;
        
        public int MaxCapacity { get; set; }

        /// <summary>
        /// best not to access this. the underlying array of the list.
        /// </summary>
        public T[] RawArray
        {
            get { return mData; }
        }

        /// <summary>
        /// returns the raw array where the last item is copied and present twice in the array. this is useful when you want to access the array 2 items at a time , but don't want to have invalid index at the last item  ,this save performnace cost of if operation in highly optimizaed sections of the code
        /// the returned array is garenteed to have Count+1 items where the last two items are equeal
        /// </summary>
        public T[] RawArrayWithExtraLastItem
        {
            get
            {
                if(mCount > 0)
                    mData[mCount] = mData[mCount - 1];
                return mData;
            }
        }

        public void RemoveFromEnd(int count)
        {
            if (count > mCount)
                count = mCount;
            mCount -= count;
            if(ClearWithoutRelease == false)
            {
                for (int i = mCount; i < mData.Length; i++)
                    mData[i] = default(T);
            }
        }

        public void RemoveFromStart(int count)
        {
            if (count > mCount)
                count = mCount;
            for (int i = count; i < mCount; i++)
                mData[i - count] = mData[i];
            mCount -= count;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity">the inital capacity of the list</param>
        public SimpleList(int capacity)
        {
            MaxCapacity = int.MaxValue;
            mData = new T[capacity];
            GrowFactor = 2;
        }

        public SimpleList(bool clearWithoutRelease)
            : this(4)
        {
            ClearWithoutRelease = clearWithoutRelease;
        }
        public SimpleList(bool clearWithoutRelease,int initalCapacity)
    : this(initalCapacity)
        {
            ClearWithoutRelease = clearWithoutRelease;
        }
        public SimpleList()
            : this(4)
        {

        }
        public void SetCount(int count)
        {
            if (count == Count)
                return;
            if (count < Count)
                mCount = count;
            else
            {
                int preCount = mCount;
                AddEmpty(count - mCount);
                for (int i = preCount; i < mCount; i++)
                    this[i] = default(T);
            }
        }
        /// <summary>
        /// applies an operation tree to the list. this is done wihout reallicating the underlying array (unless it's capcity is exceeded)
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="operations">the operation tree</param>
        public void ApplyOperations(OperationTree<T> operations)
        {
            ApplyOperations(operations, x => x);

        }

        public bool ClearWithoutRelease { get; set; }

        void EmptyAction(T item)
        {

        }

        public void ApplyOperations<S>(OperationTree<S> operations, Func<S, T> convert)
        {
            ApplyOperations(operations, convert, EmptyAction);
        }

        /// <summary>
        /// applies an operation tree to the list. this is done wihout reallicating the underlying array (unless it's capcity is exceeded)
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="operations">thse operation tree</param>
        /// <param name="convert">converts an item in the operation tree to an item of the list</param>
        public void ApplyOperations<S>(OperationTree<S> operations, Func<S, T> convert, Action<T> release)
        {
            int newCount = mCount + operations.IndexBalance;
            EnsureCapacity(newCount + 1);
            mWorkQueue.Clear();
            int writeIndex = 0;
            int readIndex = 0;
            bool hasDequeValue = false;
            T dequeValue = default(T);
            OperationTree<S>.OperationNode lastOp = new OperationTree<S>.OperationNode(newCount, default(S), OperationTree<S>.OperationType.EndOp, 1);
            foreach (OperationTree<S>.OperationNode op in operations.OperationData(lastOp))
            {
                for (; writeIndex < op.Index; writeIndex++, readIndex++) // it is maintain that writeIndex <= readIndex
                {
                    if (readIndex < mCount)
                    {
                        mWorkQueue.Enqueue(mData[readIndex]);
                        mData[readIndex] = default(T);
                    }
                    if (hasDequeValue)
                    {
                        hasDequeValue = false;
                        release(mData[writeIndex]);
                        mData[writeIndex] = dequeValue;
                        if (mWorkQueue.Count > 0)
                            release(mWorkQueue.Dequeue());
                    }
                    else
                    {
                        if (mWorkQueue.Count > 0)
                        {
                            release(mData[writeIndex]);
                            mData[writeIndex] = mWorkQueue.Dequeue();
                        }
                    }
                }

                switch (op.Operation)
                {
                    case OperationTree<S>.OperationType.Insert:
                        if (readIndex < mCount)
                        {
                            mWorkQueue.Enqueue(mData[readIndex]);
                            mData[readIndex] = default(T);
                        }
                        release(mData[writeIndex]);
                        mData[writeIndex] = convert(op.Value);
                        writeIndex++;
                        readIndex++;
                        break;
                    case OperationTree<S>.OperationType.Set:
                        hasDequeValue = true;
                        dequeValue = convert(op.Value);
                        break;
                    case OperationTree<S>.OperationType.Remove:
                        int i = op.Count;
                        for (; mWorkQueue.Count > 0 && i > 0; --i)
                        {
                            release(mWorkQueue.Dequeue());
                        }
                        for (; i > 0 && readIndex < mCount; --i, readIndex++)
                        {
                            release(mData[readIndex]);
                            mData[readIndex] = default(T);
                        }
                        break;
                    default:
                        break;
                }
            }
            mCount = newCount;
            if (ClearWithoutRelease == false)
            {
                for (int i = mCount; i < mData.Length; i++)
                {
                    release(mData[i]);
                    mData[i] = default(T);
                }
            }
        }

        void ValidateIndex(int index)
        {
            if (index < 0 || index >= mCount)
                throw new IndexOutOfRangeException(String.Format("With index {0} and count {1}",index,mCount));
        }

        public T this[int index]
        {

            get
            {
                ValidateIndex(index);
                return mData[index];
            }
            set
            {
                ValidateIndex(index);
                mData[index] = value;
            }
        }

        public int Count { get { return mCount; } }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public double GrowFactor
        {
            get;
            private set;
        }
        /// <summary>
        /// this method ensures the capacity of the array. It is called with 1 extra item so RawArrayWithExtraLastItem is valid
        /// </summary>
        /// <param name="capacity"></param>
        void EnsureCapacity(int capacity)
        {
            if (capacity <= mData.Length)
                return;
            int newSize = Math.Max(capacity, (int)(mData.Length * GrowFactor));
            newSize = Math.Min(MaxCapacity, newSize);
            if (newSize == mData.Length) // can't grow anymore
                throw new Exception("list capacity reached "  + newSize);
            T[] arr = new T[newSize];
            Array.Copy(mData, arr, mCount);
            mData = arr;
        }

        T[] AddEmpty()
        {
            EnsureCapacity(mCount + 2);
            ++mCount;
            return mData;
        }

        public void Append(SimpleList<T> list)
        {
            EnsureCapacity(mCount + list.mCount + 1);
            Array.Copy(list.mData, 0, mData, mCount, list.mCount);
            mCount += list.mCount;
        }

        public void Add(T item)
        {
            EnsureCapacity(mCount + 2);
            mData[mCount] = item;
            ++mCount;
        }
        public T[] AddEmpty(int count)
        {
            EnsureCapacity(mCount + count + 1);
            mCount += count;
            return mData;
        }
        /// <summary>
        /// If T is a value type , sometimes it is not necessary to set all value the default value. So this clear method works at O(1)
        /// </summary>
        private void DoClearWithoutRelease()
        {
            mCount = 0;
        }

        public void Clear()
        {
            if (ClearWithoutRelease)
                DoClearWithoutRelease();
            else
            {
                Array.Clear(mData, 0, mCount);
                mCount = 0;
            }
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < mCount; i++)
                if (mData[i].Equals(item))
                    return true;
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(mData, 0, array, arrayIndex, mCount);
        }
        public void AddRange(IEnumerable<T> range)
        {
            foreach (T t in range)
                Add(t);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return mData.Take(mCount).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < mCount; i++)
                if (mData[i].Equals(item))
                    return i;
            return -1;
        }

        public void Insert(int index, T item)
        {
            EnsureCapacity(mCount + 2);
            if (index < mCount)
            {
                Array.Copy(mData, index, mData, index + 1, mCount - index);
            }
            mData[index] = item;
            mCount++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if (mCount == 0)
                throw new IndexOutOfRangeException();
            if(ClearWithoutRelease == false)
                mData[index] = default(T);
            mCount--;
            if (index < mCount)
                Array.Copy(mData, index + 1, mData, index, mCount - index);
        }
        public void RemoveAt(int index,int count)
        {
            if (mCount == 0)
                throw new IndexOutOfRangeException();
            if(index + count >mCount)
                throw new IndexOutOfRangeException();
            if (ClearWithoutRelease == false)
            {
                for(int i=0; i<count; i++)
                    mData[index + count] = default(T);
            }
            mCount-=count;
            if (index < mCount)
                Array.Copy(mData, index + count, mData, index, mCount - index);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
