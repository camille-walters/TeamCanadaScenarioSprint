using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer
{
    /// <summary>
    /// 
    /// </summary>d
    public class CategoryDataHolder : IPrivateSetName, IPrivateSetParent<ChartDataSource>, IPrivateCategoryDataHolder
    {
        ChartDataSource mParent;
        StackedGenericDataHolder mHolder;
        DataSeriesCategory mParentCategory;

        public CategoryDataHolder(DataSeriesCategory parent)
        {
            mParentCategory = parent;
        }

        public string Name { get; private set; }

        /// <summary>
        /// advanced methods for category data holder. enables full control of all data channels and stacks.in most cases you would not need to use this
        /// </summary>
        protected StackedGenericDataHolder Advanced
        {
            get
            {
                if (mHolder == null)
                {
                    mHolder = new StackedGenericDataHolder(mParentCategory);
                    mHolder.AddChannels(ChannelType.Positions);
                }
                return mHolder;
            }
        }
        
        struct OHLC
        {

        }


        ChannelType mChannels;

        void EnsureChannels(GenericDataHolder holder)
        {

        }

        void EnsureChannels()
        {
            
        }

        public int Count
        {
            get { return Advanced.Count; }
        }

        StackedGenericDataHolder IPrivateCategoryDataHolder.InnerData
        {
            get
            {
                return Advanced;
            }
        }

        private void CommitChanges()
        {
            Advanced.CommitChanges();
        }

        /// <summary>
        /// clears the data holder , if releaseMemory is true , the memory used to hold the previous data will be realesed. (clear memory is good if you had a large amount of data that will be replaced by a small amount for example)
        /// </summary>
        /// <param name="releaseMemory"></param>s
        public void Clear()
        {
            Advanced.Clear();
        }

        private void RemoveValue(int index)
        {
            Advanced.Remove(index);
        }

        public DoubleVector3 GetPointAt(int index)
        {
            return Advanced.GetPointAt(index);
        }

        public int Pick(DoubleVector3 point)
        {
            return Advanced.Pick(point);
        }

        public int PickExact(DoubleVector3 point)
        {
            return Advanced.PickExact(point);
        }

        private void SetPositionValue(int index,double x,double y,double z= 0.0)
        {
            Advanced.SetPosition(0, index, new DoubleVector3(x, y, z));
        }

        public void Append(DateTime x,double y)
        {
            Append(DateUtility.DateToValue(x), y);
        }

        public void Append(double x,DateTime y)
        {
            Append(x,DateUtility.DateToValue(y));
        }
        public void Append(DateTime x, DateTime y)
        {
            Append(DateUtility.DateToValue(x), DateUtility.DateToValue(y));
        }
        public void Append(double x,double y ,double z=0.0)
        {
            Append(new DoubleVector3(x, y, z));
        }
        //protected GenericDataItem GetAbsoluteValue(int index)
        //{
        //    return mParent.GetAbsoluteValue(this, index);
        //}
        //private void Insert(int index,Color32 color, double x, double y, double z = 0.0)
        //{
        //    mParent.InsertSlideValue(this,index, new GenericDataItem()
        //    {
        //        Position = new DoubleVector3(x, y, z),
        //        Color = color
        //    }, 0.0, ChartDataSource.PositionInterpolator);
        //}

        DoubleVector3[] mTempAppend = new DoubleVector3[1];
        private void Append(Color32 color,double x, double y, double z = 0.0)
        {
            mTempAppend[0] = new DoubleVector3(x, y, z);
            Advanced.AppendPositionArray(0, mTempAppend, 1);
            //mParent.AppendSlideValue(this, new GenericDataItem()
            //{
            //    Position = new DoubleVector3(x, y, z),
            //    Color = color
            //}, slideTime, ChartDataSource.PositionInterpolator);
        }

        //private void AppendVector(Color32 color, DoubleVector3 position, DoubleVector3 endPosition, double slideTime)
        //{
        //    mParent.AppendSlideValue(this, new GenericDataItem()
        //    {
        //        Position = position,
        //        EndPosition = endPosition,
        //        Color = color
        //    }, new GenericDataItem()
        //    {
        //        Position = position,
        //        EndPosition = position,
        //        Color = color
        //    },slideTime, ChartDataSource.VectorInterpolator);
        //}

        //private void AppendVector(DoubleVector3 position,DoubleVector3 endPosition,double slideTime)
        //{
        //    mParent.AppendSlideValue(this, new GenericDataItem()
        //    {
        //        Position = position,
        //        EndPosition = endPosition,
        //    }, new GenericDataItem()
        //    {
        //        Position = position,
        //        EndPosition = position,
        //    }, slideTime, ChartDataSource.VectorInterpolator);
        //}
        public void SetLast(DoubleVector3 position)
        {
            CommitChanges();
            Advanced.SetPosition(0, Count-1, position);
        }
        public void SetPositionArray(DoubleVector3[] positions)
        {
            Advanced.SetPositionArray(0, positions, 0, positions.Length);
        }
        public void SetPositionArray(DoubleVector3[] positions,int arrayOffset,int count)
        {
            Advanced.SetPositionArray(0, positions, arrayOffset, count);
        }
        public void RemoveAllBefore(double x)
        {
            Advanced.RemoveAllBefore(x);
        }
        public void RemoveFromStart(int count)
        {
            Advanced.RemoveFromStart(count);
        }

        public void Append(DoubleVector3 position)
        {
            Advanced.CachedAppendPosition(0, position);
            //mParent.AppendSlideValue(this, GenericDataItem.FromPosition(position), slideTime, ChartDataSource.PositionInterpolator);
        }

        void IPrivateSetName.SetName(string name)
        {
            Name = name;
            Advanced.Name = name;
        }

        void IPrivateSetParent<ChartDataSource>.SetParent(ChartDataSource parent)
        {
            mParent = parent;
        }
    }
}
