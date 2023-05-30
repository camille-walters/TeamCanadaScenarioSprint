using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// A data series that maintains length for it's object. This is useful for uv calculation for a large continus graph line or fill for example.
    /// </summary>
    public abstract class UvLengthDataSeries : DataSeriesBase
    {
        public static readonly Rect SimpleRect = new Rect(0f, 0f, 1f, 1f);
        public const string MaterialTilingMethod = "materialTilingMethod";

        bool mOptimiziedLength = false;
        UvLengthVisualFeature.MaterialTilingMethodOptions mTilingMethod;
        ILengthCalculationMethod mLenghtMethod;
        double mCurrentTotalLength = 1.0;
        bool mDividerChange = false;
        protected UVRectMethod UVMethod
        { get; set; }

        public static Rect SimpleUVRectMethod(int index, SeriesObject obj)
        {
            return SimpleRect;
        }

        protected UvLengthVisualFeature.MaterialTilingMethodOptions TilingMethod{get{ return mTilingMethod; } }
        public delegate Rect UVRectMethod(int index, SeriesObject obj);

        /// <summary> 
        /// should be used by getUvRect to divide uv values. 
        /// </summary>
        protected double? UvDivider { get; private set; }

        public UvLengthDataSeries(ArrayManagerType arrayType, int itemSize, params IDataSeriesOptimizationCondition[] conditions) : base(arrayType, itemSize, conditions)
        {
            UVMethod = SimpleUVRectMethod;
            Canvas.willRenderCanvases += Canvas_willRenderCanvases;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Canvas.willRenderCanvases -= Canvas_willRenderCanvases;
        }

        private void Canvas_willRenderCanvases()
        {
            if(mDividerChange)
            {
                mDividerChange = false;

            }
        }

        /// <summary>
        /// Base class for length calculation method. This inteface allows maximal flexibillity and balance between performance and quallity
        /// </summary>
        interface ILengthCalculationMethod
        {
            /// <summary>
            /// called before rendering
            /// </summary>
            void PreRender();

            void InvalidateAll();
            /// <summary>
            /// called before the modification or removal of one index
            /// </summary>
            void StartModify();

            /// <summary>
            /// called before the data is inserted into the arrays
            /// </summary>
            /// <param name="index"></param>
            void OnBeforeInsert(int index);

            /// called when an index is inserted (just before the actual insert). this call will preceed a call to Modify(int index) with the newly inserted index
            void OnInsert(int index);
            /// <summary>
            /// called when an index is removed
            /// </summary>
            /// <param name="index"></param>
            void OnRemove(int index);

            /// <summary>
            /// called before an index is removed
            /// </summary>
            /// <param name="index"></param>
            void OnBeforeRemove(int index);
            /// <summary>
            /// called when an index is modified or inserted. If the object is inserted , this will be called just after the insert and set value
            /// </summary>
            /// <param name="index"></param>
            void OnBeforeSet(int index);

            /// <summary>
            /// called when an index is modified or inserted. If the object is inserted , this will be called just after the insert and set value
            /// </summary>
            /// <param name="index"></param>
            void Modify(int index);
            /// <summary>
            /// called after the modification or removal of one index
            /// </summary>
            void EndModify();
            double TotalLength { get; }
            void LengthOf(int index, out double length, out double accumilatedLength);
        }

        /// <summary>
        /// This method is less performant when modifing the data. However is performant in most casees when appending data only at the end of the chart
        /// </summary>
        class AcummilatedLengthCalculationMethod : ILengthCalculationMethod
        {
            public AcummilatedLengthCalculationMethod(UvLengthDataSeries parent)
            {
                Parent = parent;
            }

            public UvLengthDataSeries Parent { get; private set; }
            double totalLength = 0.0;
            List<double> mAcumilated = new List<double>();
            int mMinDirty;
            List<int> mTmpConnectedList = new List<int>();
            HashSet<int> mTmpConnectedSet = new HashSet<int>();
            int mInsertedIndex = -1;

            public double TotalLength
            {
                get
                {
                    if (totalLength <= 0.0)
                        return 1.0;
                    return totalLength;
                }
            }

            /// <summary>
            /// collect all acumilated values into an array.
            /// </summary>
            public void InvalidateAll()
            {
                ChartIntegrity.NotifyMethodCall((Action)InvalidateAll);
                ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "Invalidate All");
                mAcumilated.Clear();
                double total = 0.0;
                for (int i = 0; i < Parent.mEntries.Count; i++)
                {
                    mAcumilated.Add(total);
                    total += Parent.CalculateEntryLength(Parent.mEntries[i]);
                }
                totalLength = total;
                mMinDirty = int.MaxValue;
            }
            
            /// <summary>
            /// ensures that the specified index is not dirty and contains up to date data.
            /// </summary>
            /// <param name="index"></param>
            void EnsureAcummilated(int index)
            {
                if (index < mMinDirty)
                    return;
                if (index >= Parent.mEntries.Count)
                    return;
                ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name,"Ensure Acumilated");
                // make sure the list has enough items
                for (int i = mAcumilated.Count; i < Parent.mEntries.Count; i++)
                    mAcumilated.Add(0.0);
                mMinDirty = Math.Min(mMinDirty, mAcumilated.Count);

                int end = Parent.mEntries.Count;
                for (int i = mMinDirty; i < end; i++)
                {
                    double prevAcummilated = 0.0;
                    double prevLength = 0.0;
                    if (i > 0)
                    {
                        int prevIndex = i - 1;
                        prevAcummilated = mAcumilated[prevIndex];
                        prevLength = Parent.CalculateEntryLength(Parent.mEntries[prevIndex]);
                    }
                    mAcumilated[i] = prevAcummilated + prevLength;
                }
                mMinDirty = end;
            }

            void EnsureAccumilated()
            {
                EnsureAcummilated(mMinDirty);
                ChartIntegrity.Assert(() =>
                {
                    double total = 0.0;
                    for (int i = 0; i < Parent.mEntries.Count; i++)
                    {
                        if (mAcumilated[i] != total)
                        {
                            Debug.Log("accumilated at index " + i + " does not match. total entries : " + Parent.mEntries.Count);
                            return false;
                        }
                        total += Parent.CalculateEntryLength(Parent.mEntries[i]);
                    }
                    return true;
                });
            }

            public void PreRender()
            {
                //   Chartommon.DevLog("ensure accumillated","caled");
                EnsureAccumilated();
            }

            public void LengthOf(int index, out double length, out double accumilatedLength)
            {
                //     EnsureAcummilated(index);
                length = 0.0;
                double? ltmp = Parent.mEntries[index].EffectiveLength();
                if (ltmp.HasValue)
                    length = ltmp.Value;
                accumilatedLength = 0.0;
                if (index < mAcumilated.Count)
                    accumilatedLength = mAcumilated[index];
            }

            bool IsValidIndex(int index)
            {
                return index < Parent.mEntries.Count && index >= 0;
            }

            public void OnBeforeInsert(int index)
            {
                ChartIntegrity.AssertMethodCalled((Action)InvalidateAll); 
                ChartIntegrity.AssertFlagRaised(this,"StartModify"); // this method should be called only between start modify and end modify
                ChartIntegrity.AssertFlagNotRaised(this, "OnBeforeRemove", "OnBeforeInsert", "OnBeforeSet"); // these methods should only called once before EndModify
                ChartIntegrity.RaiseFlag(this, "OnBeforeInsert");
                ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "Before Insert", "Length:", totalLength);
                mInsertedIndex = index;
                Parent.GetAllIndices(index, mTmpConnectedList);
               
                for (int i = 0; i < mTmpConnectedList.Count; i++)       // deacrease all changed items from totalLength. They will be reapplied when EndModify is called
                {
                    int linked = mTmpConnectedList[i];
                    if (IsValidIndex(linked) == false)
                        continue;
                    double len = Parent.mEntries[linked].EffectiveLength(); // thisr 
                    ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "decrease", len, "index:", linked);
                    totalLength -= len;
                }
            }

            public void OnInsert(int index)
            {

            }

            public void OnBeforeRemove(int index)
            {
                ChartIntegrity.AssertMethodCalled((Action)InvalidateAll);
                ChartIntegrity.AssertFlagRaised(this, "StartModify"); // this method should be called only between start modify and end modify
                ChartIntegrity.AssertFlagNotRaised(this, "OnBeforeRemove", "OnBeforeInsert", "OnBeforeSet"); // these methods should only called once before EndModify
                ChartIntegrity.RaiseFlag(this, "OnBeforeRemove");
                ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "Before Remove", "Length:", totalLength);
                Parent.GetAllIndices(index, mTmpConnectedList);
                for (int i = 0; i < mTmpConnectedList.Count; i++)  // deacrease all changed items from totalLength. They will be reapplied when EndModify is called
                {
                    int linked = mTmpConnectedList[i];
                    double len = Parent.mEntries[linked].EffectiveLength();
                    ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "decrease", len, "index:", linked);
                    totalLength -= len;
                }
                mTmpConnectedList.Remove(index);    // remove the current index from list (not List(T).RemoveAt)
                for (int i = 0; i < mTmpConnectedList.Count; i++)
                {
                    if (mTmpConnectedList[i] > index)       // index is removed so anything above it should be deacresed
                        mTmpConnectedList[i] -= 1;
                }
                mMinDirty = Math.Min(index, mMinDirty);
            }

            public void OnRemove(int index)
            {

            }

            public void OnBeforeSet(int index)
            {
                ChartIntegrity.AssertMethodCalled((Action)InvalidateAll);
                ChartIntegrity.AssertFlagRaised(this, "StartModify"); // this method should be called only between start modify and end modify
                ChartIntegrity.AssertFlagNotRaised(this, "OnBeforeRemove", "OnBeforeInsert", "OnBeforeSet"); // these methods should only called once before EndModify
                ChartIntegrity.RaiseFlag(this, "OnBeforeSet");
                ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "Before Set", "Length:", totalLength);
                Parent.GetAllIndices(index, mTmpConnectedList);
                for (int i = 0; i < mTmpConnectedList.Count; i++)       // deacrease all changed items from totalLength. They will be reapplied when EndModify is called
                {
                    int linked = mTmpConnectedList[i];
                    if (IsValidIndex(linked) == false)
                        continue;
                    double len = Parent.mEntries[linked].EffectiveLength(); ;
                    ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "decrease", len, "index:", linked);
                    totalLength -= len;
                }
            }

            /// <summary>
            /// when a value changes the part of the array after it becomes dirty
            /// </summary>
            /// <param name="index"></param>
            public void Modify(int index)
            {
                ChartIntegrity.AssertFlagRaised(this, "StartModify"); // this method should be called only between start modify and end modify
                ChartIntegrity.AssertEitherFlagRaised(this, "OnBeforeRemove", "OnBeforeInsert", "OnBeforeSet"); // at least one of these method should have been called between startmodify and this method
                ChartIntegrity.AssertFlagNotRaised(this, "Modify"); // check that modify is not called twice before end modify
                ChartIntegrity.RaiseFlag(this, "Modify");
                ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "Modify");
                if (mInsertedIndex >= 0) // if an index was inserted then the length has already been reduced from the indieces. We just need to set all the indices that should be added
                {
                    ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "Modify");
                    Parent.GetAllIndices(index, mTmpConnectedList);
                    Parent.GetAllIndices(index+1, mTmpConnectedList, false);    // add all the indices that 
                    mTmpConnectedSet.Clear();                           // avoiding the use of IEnumerable.distinct because of unknown implementation (which ussually allocates an object). This implementation does not allocate any new objects
                    for (int i = 0; i < mTmpConnectedList.Count; i++)
                        mTmpConnectedSet.Add(mTmpConnectedList[i]);
                    mTmpConnectedList.Clear();
                    foreach (int item in mTmpConnectedSet)
                        mTmpConnectedList.Add(item);    
                }
                mMinDirty = Math.Min(index, mMinDirty);
            }

            public void EndModify()
            {
                for (int i = 0; i < mTmpConnectedList.Count; i++)       // reappend all changed items from totalLength
                {
                    int linked = mTmpConnectedList[i];
                    double len = Parent.CalculateEntryLength(Parent.mEntries[linked]);
                    ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "increase", len,"index:",linked);
                    totalLength += len;
                }
                ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "End Modify", totalLength);
                mTmpConnectedList.Clear();
                ChartIntegrity.Assert(() =>
                {
                    double total = 0.0;
                    for (int i = 0; i < Parent.mEntries.Count; i++)
                        total += Parent.CalculateEntryLength(Parent.mEntries[i]);
                    if(Math.Abs(total - totalLength) > 0.0001)
                    {
                        Debug.Log("total length : " + totalLength + " does not match real total length " + total);
                        return false;
                    }
                    return true;
                });

                // after end modify was called , all the previous methods are ok to be called again
                ChartIntegrity.RemoveFlag(this, "OnBeforeRemove");
                ChartIntegrity.RemoveFlag(this, "OnBeforeSet");
                ChartIntegrity.RemoveFlag(this, "OnBeforeInsert");
                ChartIntegrity.RemoveFlag(this, "Modify");
                ChartIntegrity.RemoveFlag(this,"StartModify");

            }

            public void StartModify()
            {
                ChartIntegrity.AssertFlagNotRaised(this, "StartModify");
                ChartIntegrity.RaiseFlag(this,"StartModify");
                ChartCommon.DevLog(LogOptions.UvDataSeries, "Acummilated", Parent.GetType().Name, "Start Modify");
                mInsertedIndex = -1;
                mTmpConnectedList.Clear();
            }
        }

        /// <summary>
        /// an optimzied length calculation method. It is good for solid colors , or materials with no horizontal change. Uv is streched 
        /// </summary>
        class OptimizedLengthCalculationMethod : ILengthCalculationMethod
        {
            public OptimizedLengthCalculationMethod(UvLengthDataSeries parent)
            {
                Parent = parent;
            }

            public UvLengthDataSeries Parent { get; private set; }
            double totalLength = 1.0;

            public double TotalLength
            {
                get
                {
                    return totalLength;
                }
            }

            public void EndModify()
            {
                
            }

            public void InvalidateAll()
            {
                totalLength = Math.Max(1,Parent.mEntries.Count - 1);
            }

            public void OnBeforeInsert(int index)
            {

            }

            public void LengthOf(int index, out double length, out double accumilatedLength)
            {
                length = 1.0;
                accumilatedLength = index;
            }

            public void Modify(int index)
            {
            }

            public void StartModify()
            {
               
            }

            public void OnInsert(int index)
            {
            }

            public void OnRemove(int index)
            {

            }

            public void PreRender()
            {

            }

            public void OnBeforeRemove(int index)
            {

            }

            public void OnBeforeSet(int index)
            {

            }
        }
        /// <summary>
        /// if true , Uvs are divided by the total length , this is done either by hardware (material st)
        /// </summary>
        protected bool DivideUvsWIthTotalLength { get { return true; } }

        public sealed override Rect GetUvRect(int index, SeriesObject obj)
        {
            Rect uv= UVMethod(index, obj);
            ChartCommon.DevLog(LogOptions.UvDataSeries, GetType().Name, "uv rect:", uv,"object:", obj.MyIndex);
            return uv;
        }

        /// <summary>
        /// the total length of the objects in this series (length is used for uv caculation along lines and countinuos objects).
        /// This value is 0 if TrackLength is false
        /// This value is calculated lazyly and cached for future calls. it will be recalculated only after InvalidateLength has been called
        /// </summary>
        public double TotalLength
        {
            get
            {
                if (mLenghtMethod == null)
                    return 1.0;
                return mLenghtMethod.TotalLength;
            }
        }

        protected override bool ValidateSettings(IDataSeriesSettings settings,out string error)
        {
            if (base.ValidateSettings(settings, out error) == false)
                return false;
            error = null;
            
            UnboxSetting(ref mTilingMethod, settings, MaterialTilingMethod, UvLengthVisualFeature.MaterialTilingMethodOptions.PerfectFit, DataSeriesRefreshType.FullRefresh);
            mOptimiziedLength = mTilingMethod != UvLengthVisualFeature.MaterialTilingMethodOptions.PerfectFit; 
            EnsureLengthMethod();
            return true;
        }

        /// <summary>
        /// ensure the length method mathces the current setting and is allocated and ready to use
        /// </summary>
        void EnsureLengthMethod()
        {
            if (mOptimiziedLength)
            {
                if (mLenghtMethod == null || !(mLenghtMethod is OptimizedLengthCalculationMethod))
                {
                    mLenghtMethod = new OptimizedLengthCalculationMethod(this);
                    mLenghtMethod.InvalidateAll();
                }
            }
            else
            {
                if(mLenghtMethod == null || !(mLenghtMethod is AcummilatedLengthCalculationMethod))
                {
                    mLenghtMethod = new AcummilatedLengthCalculationMethod(this);
                    mLenghtMethod.InvalidateAll();
                }
            }
        }

        public void GetLengthForIndex(int index,out double legnth,out double accumilatedLength)
        {
            //if (mLenghtMethod == null)
            //{
            //    legnth = accumilatedLength = 0.0;
            //    return;
            //}
            mLenghtMethod.LengthOf(index, out legnth, out accumilatedLength);
        }

        /// <summary>
        /// gets the length of the parameter entry. The length is used for UV calculations that require to accumilate the length of chart objects. (for example , uv along the line of a graph requires accumilating the length of the graph line)
        /// </summary>
        /// <param name="entry"></param>
        /// <returns>the length of the entry</returns>
        protected virtual double GetEntryLengthImpl(ObjectEntry entry)
        {
            if (entry == null || entry.InnerObjects.Count <= 0)
                return 0.0;
            return entry.InnerObjects[0].Legnth(this);
        }
        
  
        protected double CalculateEntryLength(ObjectEntry entry)
        {
            double len = GetEntryLengthImpl(entry);
            ChartCommon.DevLog(LogOptions.UvDataSeries, GetType().Name, "Entry Length:", len);
            //if (entry != null && entry.InnerObjects != null)
            //{
            //    for (int i = 0; i < entry.InnerObjects.Count; i++)
            //    {
            //        Graphic.MakeEntityDirty(entry.InnerObjects[i], true);
            //    }
            //}
            entry.NotifyLengthCalculated(len); // save the entry length for later use.
            return len;
        }

        public override void BeforeRemove(int index)
        {
            base.BeforeRemove(index);
            StartModifyLength();
            if (mLenghtMethod != null)
                mLenghtMethod.OnBeforeRemove(index);
        }

        public override void OnBeforeInsert(int index)
        {
            base.OnBeforeInsert(index);
            StartModifyLength();
            if (mLenghtMethod != null)
                mLenghtMethod.OnBeforeInsert(index);    // notify the length method of the releated entry that changed
        }

        public override void OnInsert(int index)
        {
            if (mLenghtMethod != null)
                mLenghtMethod.OnInsert(index);    // notify the length method of the releated entry that changed
            if (mLenghtMethod != null)
                mLenghtMethod.Modify(index);    // notify the length method of the releated entry that changed
            base.OnInsert(index);
            EndModifyLength();
        }

        protected override void Modified(int index, int entryIndex)
        {
            if (index == entryIndex)
            {
                if (mLenghtMethod != null)
                    mLenghtMethod.Modify(index);    // notify the length method of the releated entry that changed
            }
            base.Modified(index, entryIndex);
        }
        
        protected override void Destoryed(int index, int entryIndex)
        {
            if (index == entryIndex)
            {
                if (mLenghtMethod != null)
                    mLenghtMethod.OnRemove(entryIndex);    // notify the length method of the releated entry that changed
            }
            base.Destoryed(index, entryIndex);
        }

        public override bool OnRemove(int index)
        {
            bool res = base.OnRemove(index);
            EndModifyLength();
            return res;
        }

        void ApplyCurrentDivider()
        {
            if (mLenghtMethod != null)
                mCurrentTotalLength = mLenghtMethod.TotalLength;
            UvDivider = mCurrentTotalLength;
            mDividerChange = true;
            
        }

        protected override void MakeAllDirty(bool uvOnly)
        {
            base.MakeAllDirty(uvOnly);
            if (mLenghtMethod != null)
                mLenghtMethod.InvalidateAll();
            CheckTotalLength();
        }

        public override void OnAfterCommit(OperationTree<int> tree)
        {

            base.OnAfterCommit(tree);
            if (mLenghtMethod != null)
                mLenghtMethod.InvalidateAll();
            CheckTotalLength();
        }

        protected virtual void OnTotalLengthChanged()
        {
            if(DivideUvsWIthTotalLength)
            {
                if (UvDivider.HasValue && ChartCommon.OfEqualMagnitude(UvDivider.Value, mCurrentTotalLength, 10.0))  // if the UvDivider is of equal magnitude to the total legnth , the uv is changed only via texture st or quick invalidate
                {
                    double mult = UvDivider.Value / mCurrentTotalLength;
                    if (ChartCommon.IsValid(mult) == false)
                        mult = 1.0;
                    ChartCommon.DevLog(LogOptions.UvDataSeries, GetType().Name, "Length Changed", "set uv mapping", mult);
                    Graphic.UvMapping = Graphic.UvMapping.ModifyX(0, mult);           
                }
                else
                {
                    ChartCommon.DevLog(LogOptions.UvDataSeries, GetType().Name, "Length Changed", "full graphic invalidate");
                    ApplyCurrentDivider();
                    Graphic.UvMapping = Graphic.UvMapping.ModifyX(0, 1.0);
                    MakeAllDirty(true);
                    Invalidate();
                }
            }
        }

        void CheckTotalLength()
        {
            if (mLenghtMethod == null)
                return;
            ChartCommon.DevLog(LogOptions.UvDataSeries, "CheckTotalLength-", GetType().Name, "current:", mCurrentTotalLength, "new:", mLenghtMethod.TotalLength);
            if (mCurrentTotalLength != mLenghtMethod.TotalLength || UvDivider.HasValue == false)
            {
                mCurrentTotalLength = mLenghtMethod.TotalLength;
                OnTotalLengthChanged();
            }
        }
        
        public override void PrepareVertices()
        {
            base.PrepareVertices();
            if (mLenghtMethod != null)
                mLenghtMethod.PreRender();
        }

        protected override void PopulateAllEntries()
        {
            base.PopulateAllEntries();
            if (mLenghtMethod != null)
                mLenghtMethod.InvalidateAll();
        }

        public override void OnSet(IDataViewerNotifier data)
        {
            base.OnSet(data);
            if (mLenghtMethod != null)
                mLenghtMethod.InvalidateAll();
            CheckTotalLength();
        }
        
        protected override void OnBeforeSet(int index)
        {
            base.OnBeforeSet(index);
            StartModifyLength();
            if (mLenghtMethod != null)
                mLenghtMethod.OnBeforeSet(index);
        }

        protected override void OnSetArray(object obj, ChannelType type)
        {
            base.OnSetArray(obj, type);
            if (mLenghtMethod != null)
                mLenghtMethod.InvalidateAll();
            CheckTotalLength();
        }

        public override void OnClear()
        {
            base.OnClear();
            if (mLenghtMethod != null)
                mLenghtMethod.InvalidateAll();
            CheckTotalLength();
        }

        void StartModifyLength()
        {
            if (mLenghtMethod != null)
                mLenghtMethod.StartModify();
        }

        void EndModifyLength()
        {
            if (mLenghtMethod != null)
                mLenghtMethod.EndModify();
            CheckTotalLength();
        }
        
        public override void OnSetValue(int index)
        {
            base.OnSetValue(index);
            EndModifyLength();

        }
    }
}
