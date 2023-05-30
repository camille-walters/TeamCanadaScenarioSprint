
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ThetaList;
using UnityEngine;
using UnityEngine.UI;

namespace DataVisualizer{
    [ExecuteInEditMode]
    public abstract class DataSeriesBase : MonoBehaviour, IDataSeries
    {
        protected const int PopulateEntitiesEmpty = 0;
        protected const int PopulateEntitiesInProgress = 1;
        protected const int PopulateEntitiesDone = 2;

        public const double MultiplyThreshold = 10000;
        public const double AddThreshold = 10000;

        [ThreadStatic]
        static SimpleList<SeriesObject> mInnerTakenOut;

        static SimpleList<SeriesObject> mTakenOut
        {
            get
            {
                if (mInnerTakenOut == null)
                    mInnerTakenOut = new SimpleList<SeriesObject>();
                return mInnerTakenOut;
            }
        }

        bool mVisible = true;
        protected const string UseColorChannelSettings = "useColorChannel";
        public bool IsActive { get; private set; }

        protected virtual ArrayManagerType ArrayType { get; set; }
        protected virtual int ItemSize { get; private set; }

        static AsyncLoader mCurrentLoader = null;

        AsyncLoader mAsyncLoader;
        VectorDataSource mFrom = VectorDataSource.Positions, mTo = VectorDataSource.Positions;

        Coroutine mCoCheckUpdateMapping = null;
        /// <summary>
        /// obtains a uv rect for the specified object at the specified index
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract Rect GetUvRect(int index, SeriesObject obj);

        /// <summary>
        /// override in derived class to indicate that this object is using a canvas graphic
        /// </summary>
        protected abstract bool IsCanvas
        {
            get;
        }

        [Flags]
        enum ErrorValidations
        {
            None = 0,
            SettingsError = 1,
            ChannelError = 2
        }
        /// <summary>
        /// gets the raw data of the series
        /// </summary>
        public StackDataViewer RawData { get { return mDataViewer; } }

        public int StackIndex { get; set; }

        /// <summary>
        /// an entry that is associated with one index in the raw data array (mData). this entry contains all the objects that are used for visual representation of the data
        /// </summary>
        public class ObjectEntry
        {
            /// <summary>
            /// the length of the entry the last time it was taken by LenghtCalculation method. this way you can get the length that is reflected in the length calculation method instead of calculating another lenght that may reflect recent changes
            /// </summary>
            private double? Length;
            private DoubleRect? BoundingBox;
            public List<SeriesObject> InnerObjects = new List<SeriesObject>();

            public void InvalidateBoundingBox()
            {
                BoundingBox = null;
            }

            /// <summary>
            /// notifies this object that the length method has calculated a length for this entry. The length is stored and can be retrived using EffectiveLength
            /// </summary>
            /// <param name="length"></param>
            public void NotifyLengthCalculated(double length)
            {
                Length = length;
            }

            public void ClearLength()
            {
                Length = null;
            }

            public double EffectiveLength()
            {
                if (Length.HasValue == false)
                    return 0.0; // the length was never applied to any length method
                return Length.Value;
            }

            public DoubleRect? GetBoundingBox(int notifyIndex, DataSeriesBase mapper)
            {
                if (BoundingBox.HasValue)
                    return BoundingBox;
                if (InnerObjects == null)
                    return null;

                double minX = float.PositiveInfinity;
                double minY = float.PositiveInfinity;
                double maxX = float.NegativeInfinity;
                double maxY = float.NegativeInfinity;
                bool res = false;
                for (int i = 0; i < InnerObjects.Count; i++)
                {
                    PickableGraphicObject obj = InnerObjects[i] as PickableGraphicObject;

                    if (obj != null)
                    {
                        DoubleRect? r = obj.BoundingBox(mapper);
                        if (r.HasValue == false)
                            continue;
                        res = true;
                        DoubleVector3 tmp = r.Value.Min;
                        minX = Math.Min((float)tmp.x, minX);
                        minY = Math.Min((float)tmp.y, minY);
                        tmp = r.Value.Max;
                        maxX = Math.Max((float)tmp.x, maxX);
                        maxY = Math.Max((float)tmp.y, maxY);
                    }
                }
                if (res == false)
                    return null;
                return new DoubleRect(minX, minY, maxX - minX, maxY - minY);
            }

        }

        bool mInitialized = false;
        /// <summary>
        /// pool of free object entries
        /// </summary>
        List<ObjectEntry> mFreeEntries = new List<ObjectEntry>();
        /// <summary>s
        /// temporal list of connected indices. This is used for objects that are maintained along more then one index ( such as a line)
        /// </summary>
        List<int> mTmpConnectedList = new List<int>();
        HashSet<int> mTmpConnectedSet = new HashSet<int>();
        /// <summary>
        /// array of entries. Each entry is associated with one index in the raw data array, each entry can contain multiple objects
        /// </summary>
        protected SimpleList<ObjectEntry> mEntries = new SimpleList<ObjectEntry>(true);

        /// <summary>
        /// array of the raw data represented by this series object
        /// </summary>
        IDataViewerNotifier mData = null;
        IDataViewerNotifier mMainData = null;
        ErrorValidations mErrors = ErrorValidations.None;
        protected StackDataViewer mDataViewer;
        int mEntitesGenerated = PopulateEntitiesEmpty;
        DoubleRect mAxisBounds;
        ViewPortion mCurrentViewPortion;
        VersionTrace<ViewPortion> mCurrentView;
        DoubleRect? mFitInto;
        IPrivateDataSeriesChart mParent;
        double mNormalizeXSize, mNormalizeYSize;
        MappingFunction mMapping = MappingFunction.Zero;
        MappingFunction? mDifferenceMapping = null;
        ClipingChartGraphic mGraphic;
        double mViewDiagonalBase = 10.0;
        double mViewDiagonalRatio = 1.0;
        DataSeriesRefreshType mRefreshType = DataSeriesRefreshType.None;
        Vector3 mGraphicAdd = new Vector3();
        bool? mHasGraphicExtrusion = null;
        Vector3 mGraphicOffset;
        Vector3 mGraphicScale = new Vector3(1.0f, 1.0f, 1.0f);
        OffsetVector mObjectOffset;
        int mMapperAssigned;
        bool mHandleEvents = false;
        InputType mInputType;
        bool mInputTypeSet = false;
        bool mUseColorChannel = false;
        string mCategoryName = "";
        string mVisualFeatureName = "";
        bool mChannelsVerified = true;
        int mCanvasSortOrder = 0;

        public event Action<IDataSeries> DataStartLoading;
        public event Action<IDataSeries> DataDoneLoading;

        public void AssignMapper()
        {
            mMapperAssigned++;
        }

        public bool HasColor { get { return mUseColorChannel; } }

        public void UnassignMapper()
        {
            mMapperAssigned--;
            if (mMapperAssigned < 0)
                throw new InvalidOperationException("Invalid object state. The mapped was unassigned more then it has been assigned");
        }
        public MappingFunction Mapping
        {
            get { return mMapping; }
        }

        protected IChartSeriesGraphic Graphic
        {
            get { return mGraphic; }
        }

        public abstract object GraphicSettingsObject { get; }

        protected bool EntitesGenerated
        {
            get { return mEntitesGenerated == PopulateEntitiesDone; }
        }

        public virtual void OnDestroy()
        {
            UnhookDataEvents();
        }

        public virtual void UniformUpdate()
        {

        }

        public virtual int ToOffset
        {
            get { return (mFrom == mTo) ? 1 : 0; }
        }

        public virtual VectorDataSource ToArray
        {
            get { return mTo; }
        }

        public virtual VectorDataSource FromArray
        {
            get { return mFrom; }
        }

        protected virtual void LateUpdate()
        {
            if (mInitialized == false)
                return;

            UpdateGraphicExtrusion();
            EnsureDataIntegrity();
            if (mEntitesGenerated == PopulateEntitiesInProgress)
                mAsyncLoader.Update(10);

            if (mRefreshType != DataSeriesRefreshType.None)
            {
                if ((mRefreshType & DataSeriesRefreshType.RecreateEntries) != 0)     // refresh all entries
                {
                    mEntitesGenerated = PopulateEntitiesEmpty; 
                    EnsureEntites();
                }
                if (mGraphic != null)
                {
                    if ((mRefreshType & DataSeriesRefreshType.InvalidateGraphic) != 0)
                    {
                        ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "invalidating on LateUpdate");
                        // ChartIntegrity.Assert(mCurrentView.HasValue && (mCurrentView.Value.Height > 0 && mCurrentView.Value.Width > 0));
                        //CheckUpdateMapping();
                        if (mCoCheckUpdateMapping == null)
                            mCoCheckUpdateMapping = ChartCommon.PerformAtEndOfFrameOnCondition(this, mEntitesGenerated == PopulateEntitiesDone, CheckUpdateMapping);

                        mGraphic.Invalidate();
                        OnJustInvlidated();
                    }
                }
                mRefreshType = DataSeriesRefreshType.None;
            }
        }

        protected virtual void OnJustInvlidated()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conditions">conditions for optimzation, SortByX is already included so you should not specify it here </param>
        public DataSeriesBase(ArrayManagerType arrayType, int itemSize, params IDataSeriesOptimizationCondition[] conditions)
        {
            ArrayType = arrayType;
            ItemSize = itemSize;
            IsActive = true;
            mAsyncLoader = new AsyncLoader(this);
            //   mClipAlgorithm = clipAlgorithm;
        }



        public void Refresh()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Refresh");
            mRefreshType = DataSeriesRefreshType.FullRefresh;
            UnhookDataEvents(); // no need to update inner data if refresh is going to regenrate it in the next LateUpdate call
        }
        public void Invalidate()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Invalidate");
            mRefreshType |= DataSeriesRefreshType.InvalidateGraphic;
        }

        protected void DoneLoading()
        {
            HookDataEvents();
        }

        public void SetParent(IPrivateDataSeriesChart parent)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Set Parent");
            mParent = parent;
        }

        public IPrivateDataSeriesChart Parent
        {
            get { return mParent; }
        }

        void OnEnable()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "On Enable");
            RefreshHoverObjects();
        }

        protected void RefreshHoverObjects()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Refresh hover objects");
            // if (gameObject.activeInHierarchy == true)
            //     StartCoroutine(InnerRefreshHoverObjects()); //wait until the end of the frame so all the vertices has been recalculated (TODO: debug to make sure they are recalculated)
        }

        protected IEnumerator InnerRefreshHoverObjects()
        {
            yield return new WaitForEndOfFrame();
            if (mGraphic != null)
                mGraphic.RefreshHoverObjects();
        }

        void UpdateGraphicExtrusion()
        {
            mHasGraphicExtrusion = HasGraphicFeature(GraphicMaterialManager.GraphicExtrusion);

        }

        public bool HasGraphicExtrusion()
        {
            if (mHasGraphicExtrusion.HasValue == false)
                UpdateGraphicExtrusion();
            return mHasGraphicExtrusion.Value;

        }

        public bool HasGraphicFeature(string name)
        {
            if (mGraphic == null)
                return false;
            return mGraphic.HasFeature(name);
        }

        protected virtual IChartSeriesGraphic GenerateInnerGraphic(MultipleChartGraphic continer)
        {
            var graphic = ((IPrivateDataSeriesChart)mParent).CreateEdgeGraphic(continer.transform);
            //var canvas = graphic.gameObject.GetComponent<Canvas>();
            //if (canvas != null)
            //{
            //    //  canvas.overrideSorting = true;
            //    //  canvas.sortingOrder = mCanvasSortOrder;
            //}
            return graphic;
        }

        protected virtual ClipingChartGraphic GenerateGraphicObject()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Generate Graphic Object");
            GameObject obj = ((IPrivateDataSeriesChart)mParent).CreateChildObject(underlyingGameObject.transform);
            obj.name = "Composite Graphics";
            var multipleGraphic = obj.AddComponent<MultipleChartGraphic>();
            multipleGraphic.Init(GenerateInnerGraphic);
            multipleGraphic.OnCreated(ChartSettings.GraphicListSize);
            return new NoClippingGraphic(multipleGraphic);
        }

        protected virtual bool HasTangent { get { return true; } }

        public void OnInit()
        {
            if (mInitialized == true)
                return;
            ChartIntegrity.NotifyMethodCall((Action)OnInit);
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Init");
            mInitialized = true;
            if (IsCanvas)
            {
                //  ChartCommon.EnsureComponent<Canvas>(underlyingGameObject);
                if (mGraphic != null)
                {
                    ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Old Graphic destroyed");
                    ChartCommon.SafeDestroy(mGraphic.gameObject);
                }
                mGraphic = GenerateGraphicObject();
                mGraphic.HasTangent = HasTangent;
                mGraphic.HasColor = mUseColorChannel;
                mGraphic.ClearAndChangeProperties(ArrayType, ItemSize);
                mGraphic.SetDataLink(this, SeriesObject.EmptyContext);
            }
        }

        /// <summary>
        /// sets the draw properties for the graphic component of this data series.
        /// </summary>
        /// <param name="isStatic"></param>
        /// <param name="isCompact"></param>
        /// <param name="itemSize"></param>
        protected void SetGraphicProperties(ArrayManagerType arrayType, int itemSize)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Set Graphic Properties", "Array Type:", arrayType, "Item Size:", itemSize);
            ArrayType = arrayType;
            ItemSize = ItemSize;
            if (mGraphic != null)
            {
                mGraphic.ClearAndChangeProperties(ArrayType, ItemSize);
                AddAllEntitiesToGraphic();
                Invalidate();
            }
        }

        public virtual double GetDoubleArgument(int index)
        {
            return 0.0;
        }
        public int Count
        {
            get
            {
                if (mData == null)
                    return 0;
                return mData.Count;
            }
        }


        /// <summary>
        /// takes the entry object and pools it for future use
        /// </summary>
        /// <param name="entry"></param>
        void RecalimEntry(ObjectEntry entry)
        {
            entry.ClearLength();
            entry.InvalidateBoundingBox();
            entry.InnerObjects.Clear();
            mFreeEntries.Add(entry);
        }

        /// <summary>
        /// creates a new entry , either from the entry pool or by allocation
        /// </summary>
        /// <returns></returns>
        ObjectEntry CreateNewEntry()
        {
            if (mFreeEntries.Count == 0)
                return new ObjectEntry();
            ObjectEntry res = mFreeEntries[mFreeEntries.Count - 1];
            mFreeEntries.RemoveAt(mFreeEntries.Count - 1);
            return res;
        }


        /// <summary>
        /// mappes the uv of an object from unit space to presentation space.This can be used to add calcultion for the uv that are required per render , however still having the majority of uv calcultations cached in advance
        /// </summary>
        /// <param name="v"></param>
        /// <param name="mapped"></param>
        /// <returns></returns>
        public virtual bool MapUv(Vector2 v, out Vector2 mapped)
        {
            mapped = v;
            return true;
        }

        public virtual MappingFunction UVMappingFunction()
        {
            return MappingFunction.One;
        }

        /// <summary>
        /// returns the indices realted to the specified index , this can be adjacent points in a graph chart for example. no need to return the current index. It is always connected to itself
        /// </summary>
        /// <param name="index"></param>
        /// <param name="related"></param>
        protected abstract void GetConnectedInidices(int index, IList<int> related);

        /// <summary>
        /// generates an object for the specified index. in order to implement this you can call this[index] to get the data for the index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract void GenerateObjectsForIndex(int index, IList<SeriesObject> objects);

        public void GetAllIndices(int index, IList<int> related, bool clear = true)
        {
            if (clear)
                related.Clear();
            GetConnectedInidices(index, related);
            if (index >= 0 && index < Count)
                related.Add(index);
            ChartIntegrity.Assert(() =>
            {
                if (clear == false)
                    return true;
                if (related.Distinct().Count() == related.Count)
                    return true;
                return false;
            });
        }

        //public DataSeriesGenericValue this[int index]
        //{
        //    get
        //    {
        //        if(mData == null)
        //            return default(DataSeriesGenericValue);
        //        return mData[index];
        //    }
        //}

        public DoubleRect FitIntoRect
        {
            get
            {
                if (mFitInto.HasValue == false)
                    return new DoubleRect();
                return mFitInto.Value;
            }
        }
        public int CurrentViewVersion
        {
            get
            {
                return mCurrentView.Version;
            }
        }

        /// <summary>
        /// current view of the mapper in chart units
        /// </summary>
        public ViewPortion CurrentView
        {
            get
            {
                if (mCurrentView.HasValue == false)
                    return new ViewPortion();
                return mCurrentView.mRawValue;
            }
        }

        protected virtual void Destoryed(int index, int entryIndex)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Destoryed Event", index);
            if (entryIndex >= mEntries.Count)   // ignore indices that are out of range
                return;
            if (entryIndex < 0)
                return;
            var entry = mEntries[entryIndex];
            entry.InvalidateBoundingBox();
            for (int j = 0; j < entry.InnerObjects.Count; j++)
            {
                SeriesObject obj = entry.InnerObjects[j];
                obj.NotifyIndex(entryIndex);
                if (entryIndex == index)
                {
                    mGraphic.RemoveEntity(obj);
                    obj.Destory();
                }
                else
                    mGraphic.MakeEntityDirty(obj, false);
            }

        }

        protected virtual void MakeAllDirty(bool uvOnly)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Make all dirty", "Uv only:", uvOnly);
            mTakenOut.Clear();
            mGraphic.MakeAllDirty(uvOnly, mTakenOut);

            if (mTakenOut.Count > 0)
            {
                ChartCommon.RuntimeWarning("data series capacity exceeded : " + mCategoryName + "-" + mVisualFeatureName);     // These mean your materials or parameters are not set correctly and the DataSeries can't be drawn
                DisableOnError();
                mTakenOut.Clear();
            }
            //for (int i = 0; i < mEntries.Count; i++)
            //{
            //    var entry = mEntries[i];
            //    entry.InvalidateBoundingBox();
            //    for (int j = 0; j < entry.InnerObjects.Count; j++)
            //    {
            //        SeriesObject obj = entry.InnerObjects[j];
            //        obj.NotifyIndex(i);
            //        mGraphic.MakeEntityDirty(obj,uvOnly);
            //    }
            //}
        }

        protected virtual void Modified(int index, int entryIndex)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Modified event", index);
            if (entryIndex >= mEntries.Count)    // ignore indices that are out of range
                return;
            if (entryIndex < 0)
                return;
            var entry = mEntries[entryIndex];
            entry.InvalidateBoundingBox();
            for (int j = 0; j < entry.InnerObjects.Count; j++)
            {
                SeriesObject obj = entry.InnerObjects[j];
                obj.NotifyIndex(entryIndex);
                mGraphic.MakeEntityDirty(obj, false);
            }
        }

        public virtual void OnSetValue(int index)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "On Set Value", index);
            PrepareDataArrays();
            GetAllIndices(index, mTmpConnectedList);
            for (int i = 0; i < mTmpConnectedList.Count; i++)
                Modified(index, mTmpConnectedList[i]);
            RefreshHoverObjects();
            Invalidate();
        }

        /// <summary>
        /// notifies all entries about their current index in the data array. Starting from startIndex
        /// </summary>
        /// <param name="startIndex"></param>
        void NotifyAllIndices(int startIndex = 0)
        {
            for (int i = startIndex; i < mEntries.Count; i++) // notify all entries of their new index
            {
                var innerObjects = mEntries[i].InnerObjects;
                int innerCount = innerObjects.Count;
                for (int j = 0; j < innerCount; j++)
                    innerObjects[j].NotifyIndex(i);
            }
        }
        public virtual void OnRemoveFromEnd(int count)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name, name, "On Remove From end", count);
            PrepareDataArrays();
            if (mData.Count == 0 || count >= mEntries.Count)
            {
                OnClear();
                return;
            }
            if (count < 0)
                return;
            int start = mEntries.Count - count;
            for (int index = mEntries.Count-1; index >= start; index--)
            {
                Destoryed(index, index);
                var entry = mEntries[index];
                RecalimEntry(entry);
            }
            mEntries.RemoveFromEnd(count);
            RefreshHoverObjects();
            Invalidate();
        }
        public virtual void OnRemoveFromStart(int count)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "On Remove From Start", count);
            PrepareDataArrays();
            if (mData.Count == 0 || count >= mEntries.Count)
            {
                OnClear();
                return;
            }
            if (count < 0)
                return;
            for (int index = 0; index < count; index++)
            {
                Destoryed(index, index);
                var entry = mEntries[index];
                RecalimEntry(entry);
            }
            mEntries.RemoveFromStart(count);
            NotifyAllIndices();
            RefreshHoverObjects();
            Invalidate();
        }
        public virtual bool OnRemove(int index)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "On Remove", index);
            PrepareDataArrays();
            if (mData.Count == 0)
            {
                OnClear();
                return false;
            }
            if (index < 0 || index >= mEntries.Count)
                return false;
            GetAllIndices(index, mTmpConnectedList);
            for (int i = 0; i < mTmpConnectedList.Count; i++)
                Destoryed(index, mTmpConnectedList[i]);
            var entry = mEntries[index];
            RecalimEntry(entry);
            mEntries.RemoveAt(index);
            NotifyAllIndices(index);
            RefreshHoverObjects();
            Invalidate();
            return true;
        }

        void CreateEntryInnerObjects(int index, ObjectEntry entry)
        {
            GenerateObjectsForIndex(index, entry.InnerObjects);
            entry.InvalidateBoundingBox();
            for (int i = 0; i < entry.InnerObjects.Count; i++)
            {
                var obj = entry.InnerObjects[i];
                obj.NotifyIndex(index);
                bool res = mGraphic.AddEntity(obj);
                ChartIntegrity.Assert(res);
            }
        }

        void AddAllEntitiesToGraphic()
        {
            EnsureDataIntegrity();
            for (int i = 0; i < mEntries.Count; i++)
            {
                var entry = mEntries[i];
                for (int j = 0; j < entry.InnerObjects.Count; j++)
                    mGraphic.AddEntity(entry.InnerObjects[j]);
            }
        }

        private void InnerOnInsert(int index)
        {
            //     Debug.Log("inner insert entry");
            var entry = CreateNewEntry();
            entry.InnerObjects.Clear();
            CreateEntryInnerObjects(index, entry); 
            mEntries.Insert(index, entry);
            GetAllIndices(index, mTmpConnectedList);
            for (int i = 0; i < mTmpConnectedList.Count; i++)
            {
                int linked = mTmpConnectedList[i];
                if (index != linked)
                    Modified(index, mTmpConnectedList[i]);
            }
            NotifyAllIndices(index);
        }

        void ClearGraphic()
        {
            if (Graphic != null)
                Graphic.Clear();
        }

        void ClearInnerElements(ObjectEntry entry)
        {
            for (int i = 0; i < entry.InnerObjects.Count; i++)
            {
                var obj = entry.InnerObjects[i];
                mGraphic.RemoveEntity(obj);
                obj.Destory();
            }
        }

        void CommitRemoveEntry(ObjectEntry entry)
        {
            if (entry == null)
                return;
            ClearInnerElements(entry);
            RecalimEntry(entry);
        }

        public virtual void OnAfterCommit(OperationTree<int> tree)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "After Commit");
            if (mEntitesGenerated != PopulateEntitiesDone)
                return;
            ChartIntegrity.AsseetCollectionDistinct(mGraphic.BaseGraphic, "main", mEntries.SelectMany(x => x.InnerObjects).Cast<object>());
            ChartCommon.DevLog("commit", "data series");
            mTmpConnectedSet.Clear();
            mEntries.ApplyOperations(tree, x => CreateNewEntry(), CommitRemoveEntry);    // modify entries
            foreach (OperationTree<int>.OperationNode op in tree.OperationData()) // notify all the modified positions of changes
            {
                GetAllIndices(op.Index, mTmpConnectedList);
                if (op.Operation != OperationTree<int>.OperationType.Remove)
                {
                    var newEntry = mEntries[op.Index];
                    ChartIntegrity.Assert(newEntry.InnerObjects.Count == 0);
                    newEntry.InnerObjects.Clear();
                    CreateEntryInnerObjects(op.Index, newEntry);
                }
                for (int i = 0; i < mTmpConnectedList.Count; i++)
                {
                    int index = mTmpConnectedList[i];
                    mTmpConnectedSet.Add(index);
                }
            }

            for (int i = 0; i < mEntries.Count; i++)
            {
                var entry = mEntries[i];
                for (int j = 0; j < entry.InnerObjects.Count; j++)
                    entry.InnerObjects[j].NotifyIndex(i);
            }
            foreach (int index in mTmpConnectedSet)
            {
                var entry = mEntries[index];
                entry.InvalidateBoundingBox();
                for (int j = 0; j < entry.InnerObjects.Count; j++)
                {
                    SeriesObject obj = entry.InnerObjects[j];
                    mGraphic.MakeEntityDirty(obj, false);
                }
            }
            ChartIntegrity.AsseetCollectionDistinct(mGraphic.BaseGraphic, "main", mEntries.SelectMany(x => x.InnerObjects).Cast<object>()); // asset that the entries in the array match the entries in the graphic exacly
            PrepareDataArrays();
            NotifyAllIndices();
            RefreshHoverObjects();
            Invalidate();
        }

        public virtual void OnBeforeInsert(int index)
        {

        }

        public virtual void OnInsert(int index)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Insert", index);
            PrepareDataArrays();
            InnerOnInsert(index);
            RefreshHoverObjects();
            Invalidate();
        }

        void ClearAllEntries()
        {
            mEntitesGenerated = PopulateEntitiesEmpty;
            mGraphic.Clear();
            for (int i = 0; i < mEntries.Count; i++)
            {
                var entry = mEntries[i];
                entry.InvalidateBoundingBox();
                for (int j = 0; j < entry.InnerObjects.Count; j++)
                {
                    SeriesObject obj = entry.InnerObjects[j];
                    obj.Destory();
                }
                RecalimEntry(mEntries[i]);
            }
            mEntries.Clear();
        }
        void VerifyActive()
        {
            if (mErrors == ErrorValidations.None)
            {
                if (mGraphic != null && (mGraphic.IsObjectActive == false))
                    mGraphic.gameObject.SetActive(true);
            }
        }
        void DisableOnError(bool showError = true)
        {
            if (mGraphic.gameObject.activeSelf && showError)
                ChartCommon.RuntimeWarning("visual property " + mVisualFeatureName + " on category " + mCategoryName + " is disabled due to an error");
            if (mGraphic != null)
                mGraphic.gameObject.SetActive(false);
        }

        bool VerifyDataChanels()
        {
            string error;

            UnhookDataEvents();
            if (ValidateDataChannels(out error) == false) // not all channels are peresent in the 
            {
                bool showError = mData != null && mData.Count > 0;
                if (mChannelsVerified && showError)
                    ChartCommon.RuntimeWarning(error);
                mErrors = mErrors | ErrorValidations.ChannelError;
                mChannelsVerified = false;
                DisableOnError(showError);
                return false;
            }
            mErrors = mErrors & ~ErrorValidations.ChannelError;
            mChannelsVerified = true;
            HookDataEvents();
            VerifyActive();
            return true;
        }
        
        protected virtual void AppendEntries(ChannelType channel, int count)
        {
            if (VerifyDataChanels() == false) // not all channels are peresent in the 
                return;
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "append entries", "count:", count);
            EnsureDataIntegrity();
            int start = mEntries.Count;
            var rawEntries = mEntries.AddEmpty(count);

            for (int i = 0; i < count; i++)
            {
                int index = start + i;
                var entry = CreateNewEntry();
                entry.InnerObjects.Clear();
                CreateEntryInnerObjects(index, entry);
                for (int j = 0; j < entry.InnerObjects.Count; j++)
                    entry.InnerObjects[j].NotifyIndex(index);
                rawEntries[index] = entry;
            }

            /// modify the link between the appended array and t
            GetAllIndices(start, mTmpConnectedList);
            for (int i = 0; i < mTmpConnectedList.Count; i++)
            {
                int linked = mTmpConnectedList[i];
                if (start != linked)
                    Modified(start, mTmpConnectedList[i]);
            }

            RefreshHoverObjects();
            Invalidate();
        }

        protected virtual void PopulateAllEntries()
        {
            if (VerifyDataChanels() == false) // not all channels are peresent in the 
                return;
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Populate entries", "count:", Count);
            ClearAllEntries();
            ClearGraphic();
            mEntitesGenerated = PopulateEntitiesEmpty;
            HookDataEvents();
            mAsyncLoader.Restart();
        }

        //protected virtual void PopulateAllEntries()
        //{
        //    if (VerifyDataChanels() == false) // not all channels are peresent in the 
        //        return;
        //    ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name, "Populate entries", "count:", Count);
        //    ClearAllEntries();
        //    ClearGraphic();
        //    EnsureDataIntegrity();
        //    for (int i = 0; i < Count; i++)
        //    {
        //        var entry = CreateNewEntry();
        //        entry.InnerObjects.Clear();
        //        CreateEntryInnerObjects(i, entry);
        //        for (int j = 0; j < entry.InnerObjects.Count; j++)
        //            entry.InnerObjects[j].NotifyIndex(i);
        //        mEntries.Add(entry);
        //    }
        //    mEntitesGenerated = PopulateEntitiesDone;
        //    HookDataEvents();
        //    Invalidate();
        //}

        void EnsureEntites()
        {
            if (mEntitesGenerated != PopulateEntitiesEmpty)
                return;
            if (IsActive == false)
                return;
            if (mData != null && mFitInto.HasValue && mCurrentView.HasValue)
                PopulateAllEntries();
        }
        public void RaiseStartLoading()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name, name, "Start Loading");
            if (DataStartLoading != null)
                DataStartLoading(this);
        }
        public void RaiseDoneLoading()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name, name, "Done Loading");
            if (DataDoneLoading != null)
                DataDoneLoading(this);
        }
        public virtual void OnClear()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "On Clear");
            PrepareDataArrays();

            ClearAllEntries();
            ClearGraphic();
            mEntitesGenerated = PopulateEntitiesEmpty;
            //   mCurrentView = new VersionTrace<ViewPortion>();
            CreateMapping();
            Invalidate();
        }

        void UnhookDataEvents()
        {
            if (mData != null) // unhook
            {
                ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Unhook Events");
              //  if (unhookComposition)
              //      mData.OnChannelCompositionChanged -= MData_OnChannelCompositionChanged;
                mData.OnUncomittedData -= MData_OnUncomittedData;
                mData.OnClear -= MData_OnClear;
                mData.OnBeforeSet -= MData_OnBeforeSet;
                mData.OnSet -= MData_OnSet;
                mData.OnSetArray -= MData_OnSetArray;
                mData.OnAppendArray -= MData_OnAppendArray;
                mData.OnRemove -= MData_OnRemove;
                mData.OnInsert -= MData_OnInsert;
                mData.OnBeforeInsert -= MData_OnBeforeInsert;
                mData.OnAfterCommit -= MData_OnAfterCommit;
                mData.OnBeforeRemove -= MData_OnBeforeRemove;
                mData.OnRemoveFromStart -= MData_OnRemoveFromStart;
                mData.OnRemoveFromEnd -= MData_OnRemoveFromEnd;
                if (mAsyncLoader != null)
                {
                    //mData.OnChannelCompositionChanged -= MData_OnChannelCompositionChanged;
                    mData.OnUncomittedData -= mAsyncLoader.OnUncommitedData;
                    mData.OnClear -= mAsyncLoader.OnClear;
                    mData.OnBeforeSet -= mAsyncLoader.OnBeforeSet;
                    mData.OnSet -= mAsyncLoader.OnSet;
                    mData.OnSetArray -= mAsyncLoader.OnSetArray;
                    mData.OnAppendArray -= mAsyncLoader.OnAppendArray;
                    mData.OnRemove -= mAsyncLoader.OnRemove;
                    mData.OnInsert -= mAsyncLoader.OnInsert;
                    mData.OnBeforeInsert -= mAsyncLoader.OnBeforeInsert;
                    mData.OnAfterCommit -= mAsyncLoader.OnAfterCommit;
                    mData.OnBeforeRemove -= mAsyncLoader.OnBeforeRemove;
                    mData.OnRemoveFromStart -= mAsyncLoader.OnRemoveFromStart;
                    mData.OnRemoveFromEnd -= mAsyncLoader.OnRemoveFromEnd;
                }
            }
        }
    
        private void MData_OnAppendArray(object obj,ChannelType channel, int count)
        {
            PrepareDataArrays();
            AppendEntries(channel,count);
        }
        class AsyncLoader
        {
            DataSeriesBase mParent;
            public AsyncLoader(DataSeriesBase parent)
            {
                mParent = parent;
            }

            public void Update(long loadingMillis)
            {
                if (mCurrentLoader == null)
                    mCurrentLoader = this;
                else if (mCurrentLoader != this)
                    return;
                    
                mParent.EnsureDataIntegrity();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                bool done = true;
                int from = mParent.mEntries.Count;
                while (mParent.mEntries.Count < mParent.Count)
                {
                    if (watch.ElapsedMilliseconds > loadingMillis)
                    {
                        done = false;
                        break;
                    }
                    var entry = mParent.CreateNewEntry();
                    entry.InnerObjects.Clear();
                    int entryIndex = mParent.mEntries.Count;
                    mParent.CreateEntryInnerObjects(entryIndex, entry);
                    for (int j = 0; j < entry.InnerObjects.Count; j++)
                        entry.InnerObjects[j].NotifyIndex(entryIndex);
                    mParent.mEntries.Add(entry);
                }
                mParent.OnAsyncPageLoaded(from, mParent.mEntries.Count - from);
                if (done)
                {
                    OnDoneLoading();
                }
            }

            public void UnlockLoader()
            {
                if (mCurrentLoader == this)
                    mCurrentLoader = null;
            }

            public void OnDoneLoading()
            {
                UnlockLoader();
                mParent.RaiseDoneLoading();
                mParent.OnDoneAsyncLoading();

            }

            public void Restart()
            {
                Begin();
                mParent.OnRestartAsyncLoading();
            }

            public void OnClear(object obj)
            {
                Restart();
            }

            public void Begin()
            {
                ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,mParent.name, "Begin - Async");
                mParent.RaiseStartLoading();
                mParent.ClearAllEntries();
                mParent.ClearGraphic();
            }

            public void OnAfterCommit(object obj, OperationTree<int> opTree)
            {
                Restart();
            }

            public void OnAppendArray(object obj, ChannelType channel,int count)
            {

            }

            public void OnSetArray(object obj, ChannelType channel)
            {
                
            }

            public void OnRemoveFromEnd(object obj, int count)
            {
                mParent.PrepareDataArrays();
                if (count < 0)
                    return;
                if (mParent.mEntries.Count <= mParent.mData.Count)
                    return;
                for (int index = mParent.mData.Count; index < mParent.mEntries.Count; index++)
                {
                    mParent.Destoryed(index, index);
                    var entry = mParent.mEntries[index];
                    mParent.RecalimEntry(entry);
                }
                mParent.mEntries.RemoveFromEnd(mParent.mEntries.Count - mParent.mData.Count);
            }
            public void OnRemoveFromStart(object obj,int count)
            {
                mParent.PrepareDataArrays();

                if (mParent.mData.Count == 0 || count >= mParent.mEntries.Count)
                {
                    OnClear(obj);
                    return;
                }

                if (count < 0)
                    return;

                for (int index = 0; index < count; index++)
                {
                    mParent.Destoryed(index, index);
                    var entry = mParent.mEntries[index];
                    mParent.RecalimEntry(entry);
                }

                mParent.mEntries.RemoveFromStart(count);
                mParent.NotifyAllIndices();
            }

            private void CreateEntry(int index)
            {
                //     Debug.Log("inner insert entry");
                var entry = mParent.CreateNewEntry();
                entry.InnerObjects.Clear();
                if (index <= mParent.mEntries.Count)
                {
                    mParent.CreateEntryInnerObjects(index, entry);
                    mParent.mEntries.Insert(index, entry);
                    mParent.GetAllIndices(index, mParent.mTmpConnectedList);
                    for (int i = 0; i < mParent.mTmpConnectedList.Count; i++)
                    {
                        int linked = mParent.mTmpConnectedList[i];
                        if (index != linked && linked < mParent.mEntries.Count)
                            mParent.Modified(index, mParent.mTmpConnectedList[i]);
                    }
                    mParent.NotifyAllIndices(index);
                }
            }

            public void OnInsert(object obj, int index)
            {
                ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,mParent.name, "Insert - Async", index);
                if (index <= mParent.mEntries.Count)
                {
                    mParent.PrepareDataArrays();
                    CreateEntry(index);
                }
            }

            public void OnRemove(object obj, int index)
            {
                ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,mParent.name,     "On Remove - Async", index);
                mParent.PrepareDataArrays();
                if (index >= 0 && index <= mParent.mEntries.Count)
                {
                    mParent.GetAllIndices(index, mParent.mTmpConnectedList);
                    for (int i = 0; i < mParent.mTmpConnectedList.Count; i++)
                        mParent.Destoryed(index, mParent.mTmpConnectedList[i]);
                    if (index < mParent.mEntries.Count)
                    {
                        var entry = mParent.mEntries[index];
                        mParent.RecalimEntry(entry);
                        mParent.mEntries.RemoveAt(index);
                    }
                    mParent.NotifyAllIndices(index);
                }
            }

            public void OnSet(object obj, int index)
            {
                ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,mParent.name, "On Set Value - Async", index);
                if (index >= 0 && index <= mParent.mEntries.Count)
                { 
                    mParent.PrepareDataArrays();
                    mParent.GetAllIndices(index, mParent.mTmpConnectedList);
                    for (int i = 0; i < mParent.mTmpConnectedList.Count; i++)
                        mParent.Modified(index, mParent.mTmpConnectedList[i]);
                }
            }

            public void OnBeforeRemove(object obj, int index)
            {
                ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name, mParent.name, "Before remove - Asyn");
            }
            public void OnUncommitedData(object obj)
            {

            }
            public void OnBeforeSet(object obj, int index)
            {

            }
            public void OnBeforeInsert(object obj, int index)
            {

            }
        }

        void HookAsyncLoadingEvents()
        {
            UnhookDataEvents();
            if (IsActive == false)
                return;
            if (mData != null)
            {
                ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Hook Events");
                mData.OnChannelCompositionChanged -= MData_OnChannelCompositionChanged;
                mData.OnChannelCompositionChanged += MData_OnChannelCompositionChanged;
                mData.OnUncomittedData +=  mAsyncLoader.OnUncommitedData;
                mData.OnClear += mAsyncLoader.OnClear;
                mData.OnBeforeSet += mAsyncLoader.OnBeforeSet;
                mData.OnSet +=  mAsyncLoader.OnSet;
                mData.OnSetArray += mAsyncLoader.OnSetArray;
                mData.OnAppendArray += mAsyncLoader.OnAppendArray;
                mData.OnRemove +=  mAsyncLoader.OnRemove;
                mData.OnInsert +=  mAsyncLoader.OnInsert;
                mData.OnBeforeInsert +=  mAsyncLoader.OnBeforeInsert;
                mData.OnAfterCommit +=  mAsyncLoader.OnAfterCommit;
                mData.OnBeforeRemove +=  mAsyncLoader.OnBeforeRemove;
                mData.OnRemoveFromStart += mAsyncLoader.OnRemoveFromStart;
                mData.OnRemoveFromEnd += mAsyncLoader.OnRemoveFromEnd;

            }
        }



        void HookDataEvents()
        {
            if (mEntitesGenerated == PopulateEntitiesDone)
                HookSeriesDataEvents();
            else
                HookAsyncLoadingEvents();
        }

        void HookSeriesDataEvents()
        {
            UnhookDataEvents();
            if (IsActive == false)
                return;
            if (mData != null)
            {
                ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name,     "Hook Events");
                mData.OnChannelCompositionChanged -= MData_OnChannelCompositionChanged;
                mData.OnChannelCompositionChanged += MData_OnChannelCompositionChanged;
                mData.OnUncomittedData += MData_OnUncomittedData;
                mData.OnClear += MData_OnClear;
                mData.OnBeforeSet += MData_OnBeforeSet;
                mData.OnSet += MData_OnSet;
                mData.OnSetArray += MData_OnSetArray;
                mData.OnAppendArray += MData_OnAppendArray;
                mData.OnRemove += MData_OnRemove;
                mData.OnInsert += MData_OnInsert;
                mData.OnBeforeInsert += MData_OnBeforeInsert; ;
                mData.OnAfterCommit += MData_OnAfterCommit;
                mData.OnBeforeRemove += MData_OnBeforeRemove;
                mData.OnRemoveFromStart += MData_OnRemoveFromStart;
                mData.OnRemoveFromEnd += MData_OnRemoveFromEnd;
            }
        }

        private void MData_OnRemoveFromStart(object obj, int count)
        {
            OnRemoveFromStart(count);
        }

        private void MData_OnRemoveFromEnd(object obj, int count)
        {
            OnRemoveFromEnd(count);
        }

        private void MData_OnChannelCompositionChanged(object obj)
        {

            if(VerifyDataChanels())
                Refresh();
        }

        private void MData_OnBeforeSet(object arg1, int index)
        {
            OnBeforeSet(index);
        }

        private void MData_OnBeforeInsert(object arg1, int index)
        {
            OnBeforeInsert(index);
        }

        private void MData_OnSetArray(object obj, ChannelType type)
        {
            OnSetArray(obj, type);
        }

        protected virtual void OnBeforeSet(int index)
        {

        }

        protected virtual void OnRestartAsyncLoading()
        {
            mEntitesGenerated = PopulateEntitiesInProgress;
            mGraphic.SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            mVisible = visible;
            if(mEntitesGenerated == PopulateEntitiesDone)
                mGraphic.SetVisible(mVisible);
        }

        protected virtual void OnAsyncPageLoaded(int from, int count)
        {

        }

        protected virtual void OnDoneAsyncLoading()
        {

            mEntitesGenerated = PopulateEntitiesDone;
            HookDataEvents();
            mGraphic.SetVisible(mVisible);
        }

        protected virtual void OnSetArray(object obj, ChannelType type)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Set array", type);
            if (Graphic != null)
            {
                mTakenOut.Clear();
                Graphic.MakeAllDirty(false, mTakenOut);
                if (mTakenOut.Count > 0)
                {
                    ChartCommon.RuntimeWarning("data series capacity exceeded : " + mCategoryName + "-" + mVisualFeatureName);     // These mean your materials or parameters are not set correctly and the DataSeries can't be drawn
                    DisableOnError();
                    mTakenOut.Clear();
                }
            }

            if(mData.Count < mEntries.Count)
            {
                int removed = mEntries.Count - mData.Count;
                int start = mData.Count;
                for (int index = mEntries.Count - 1; index >= start; index--)
                {
                    Destoryed(index, index);
                    var entry = mEntries[index];
                    RecalimEntry(entry);
                }
                mEntries.RemoveFromEnd(removed);
            }

            else if (mData.Count >mEntries.Count)
            {
                int added = mData.Count - mEntries.Count;
                int start = mEntries.Count;
                var rawEntries = mEntries.AddEmpty(added);

                for (int i = 0; i < added; i++)
                {
                    int index = start + i;
                    var entry = CreateNewEntry();
                    entry.InnerObjects.Clear();
                    CreateEntryInnerObjects(index, entry);
                    for (int j = 0; j < entry.InnerObjects.Count; j++)
                        entry.InnerObjects[j].NotifyIndex(index);
                    rawEntries[index] = entry;
                }
            }
            var arr = mEntries.RawArrayWithExtraLastItem;
            int count = mEntries.Count;
            if (mHandleEvents)
            {
                for (int i = 0; i < count; i++)
                    arr[i].InvalidateBoundingBox();
            }
            Invalidate();
        }

        private void MData_OnUncomittedData(object obj)
        {
            Invalidate();
        }

        private void MData_OnClear(object obj)
        {
            OnClear();
        }

        private void MData_OnBeforeRemove(object arg1, int arg2)
        {
            BeforeRemove(arg2);
        }

        private void MData_OnAfterCommit(object sender, ThetaList.OperationTree<int> tree)
        {
            OnAfterCommit(tree);
        }

        private void MData_OnInsert(object sender, int index)
        {
            OnInsert(index);
        }

        private void MData_OnRemove(object sender, int index)
        {
            OnRemove(index);
        }

        private void MData_OnSet(object sender, int index)
        {
            OnSetValue(index);
        }

        protected virtual void SetData(IDataViewerNotifier data)
        {
            if (mMapperAssigned > 0)
                throw new Exception("Data cannot be set while mapper is assigned");
            UnhookDataEvents();
            mData = data;
            mDataViewer = new StackDataViewer(mData);
            HookDataEvents();
        }

        protected virtual void DoneLoadingAsync()
        {

        }
        protected virtual IDataViewerNotifier ModifyView(IDataViewerNotifier mainData)
        {
            return mainData;
        }
        public virtual void OnSet(IDataViewerNotifier data)
        {
            mMainData = data;
            data = ModifyView(data);
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name,     "Set Data Event");
            ClearAllEntries();
            SetData(data);
            EnsureDataIntegrity();
            mEntitesGenerated = PopulateEntitiesEmpty;
            if (VerifyDataChanels() == false)
                return;
            EnsureEntites();
        }

        public virtual void FitInto(ViewPortion localRect)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name,     "Fit into", "local:", localRect);
            mFitInto = new DoubleRect(localRect.From.x, localRect.From.y, localRect.To.x - localRect.From.x, localRect.To.y - localRect.From.y);
            if (mGraphic != null)
                mGraphicAdd = new Vector3((float)-mFitInto.Value.Width * 0.5f, (float)-mFitInto.Value.Height * 0.5f, 0f);
            if(mCoCheckUpdateMapping == null)
                mCoCheckUpdateMapping= ChartCommon.PerformAtEndOfFrameOnCondition(this, mEntitesGenerated == PopulateEntitiesDone, CheckUpdateMapping);
            
            RefreshHoverObjects();
            EnsureEntites();
        }

        /// <summary>
        /// do not call this if the view hasnt changed
        /// </summary>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        public virtual void OnSetView(ViewPortion view)
        {
            ChartIntegrity.NotifyMethodCall((Action<ViewPortion>)OnSetView);
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Set View");
            mCurrentView.Value = view;
            mViewDiagonalBase = view.ViewDiagonalBase;
            CalcViewDiagonalRatio();
            if (mCoCheckUpdateMapping == null)
                mCoCheckUpdateMapping = ChartCommon.PerformAtEndOfFrameOnCondition(this, mEntitesGenerated == PopulateEntitiesDone, CheckUpdateMapping);
            EnsureEntites();
            RefreshHoverObjects();
            Invalidate();
            mGraphic.OnSetView(view);
        }

        protected virtual OffsetVector ObjectOffset
        {
            get { return mObjectOffset; }
            set
            {
                mObjectOffset = value;
                ApplyDifferenceMap();
            }
        }

        public virtual Vector3 GraphicScale
        {
            get { return mGraphicScale; }
            set
            {
                mGraphicScale = value;
                ApplyDifferenceMap();
            }
        }

        public virtual Vector3 GraphicOffset
        {
            get { return mGraphicOffset; }
            set
            {
                mGraphicOffset = value;
                ApplyDifferenceMap();
            }
        }

        protected double ViewDiagonalBase
        {
            get { return mViewDiagonalBase; }
        }

        protected double ViewDiagonalRatio
        {
            get { return mViewDiagonalRatio; }
        }

        protected void CalcViewDiagonalRatio()
        {
            double viewDiagonal = new DoubleVector2(CurrentView.Width, CurrentView.Height).ToVector2().magnitude;
            mViewDiagonalRatio = mViewDiagonalBase / viewDiagonal;
        }

        public virtual void Destroy()
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name, "Destroy");
            UnhookDataEvents();
            mData = null;
            if (this != null)
                ChartCommon.SafeDestroy(gameObject);
            
        }

        /// <summary>
        /// returns true if the setting is different them the original one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <param name="settings"></param>
        /// <param name="settingName"></param>
        /// <param name="defValue"></param>
        /// <param name="refreshOnChange"></param>
        /// <returns></returns>
        public bool UnboxSetting<T>(ref T member, IDataSeriesSettings settings, string settingName, T defValue, DataSeriesRefreshType refreshOnChange = DataSeriesRefreshType.InvalidateGraphic)
        {
            T unboxed = ChartCommon.UnboxObject<T>(settings.GetSetting(settingName), defValue);
            if (((unboxed == null) && (member == null)) || unboxed.Equals(member))
                return false;
            member = unboxed;
            if (refreshOnChange != DataSeriesRefreshType.None)
            {
                MakeAllDirty(false); // when a setting is changed all objects are dirty
                mRefreshType |= refreshOnChange;
            }
            return true;
        }

        public virtual bool ValidateDataChannels(out string error)
        {

            error = null;

            if (mData == null)
            {
                error = "no data connected to the chart";
                return false;
            }

            if (FromArray == VectorDataSource.EndPositions || ToArray == VectorDataSource.EndPositions)
            {
                if ((mData.CurrentChannels & ChannelType.EndPositions) == 0)
                {
                    error = "The data does not contain a end position channel , but this channel is set as a data source";
                    return false;
                }
            }

            if (FromArray == VectorDataSource.Positions || ToArray == VectorDataSource.Positions)
            {
                if ((mData.CurrentChannels & ChannelType.Positions) == 0)
                {
                    error = "The data does not contain a position channel , but this channel is set as a data source";
                    return false;
                }
            }

            if (mUseColorChannel == true)
            {
                if ((mData.CurrentChannels & ChannelType.Color) == 0)   // no color channel to draw color data from
                {
                    error = "The data does not contain a color channel , but the setting " + UseColorChannelSettings + " is set to true";
                    return false;
                }
            }
            return true;
        }

        public void ApplySettings(IDataSeriesSettings settings, string categoryName, string VisualFeatureName)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Apply settings");

            mCategoryName = categoryName;
            mVisualFeatureName = VisualFeatureName;

            bool active = ChartCommon.UnboxObject<bool>(settings.GetSetting("active"), true);
            if (active != IsActive)
                SetActive(active);

            string error;

            bool clipable = ChartCommon.UnboxObject<bool>(settings.GetSetting("clip"), true);
            ((IPrivateDataSeriesChart)Parent).MakeClippingObject(gameObject, clipable);

            if (ValidateSettings(settings, out error) == false)
            {
                ChartCommon.RuntimeWarning("inscuffient settings : " + error);     // These mean your materials or parameters are not set correctly and the DataSeries can't be drawn
                mErrors = mErrors | ErrorValidations.SettingsError; // apply a setting error
                DisableOnError();
                return;
            }

            mErrors = mErrors & ~ErrorValidations.SettingsError; // disable a setting error
            UpdateGraphicExtrusion();
            VerifyActive();

            //    MakeAllDirty(false); // when a setting is changed all objects are dirty
            //   Invalidate();
        }

        protected abstract ArrayManagerType GetArrayType(InputType type);

        /// <summary>
        /// return true if the settings contain enough information to draw the series. False if not.
        /// When this method returns false , The underlaying graphic will be disabled and not shown.
        /// This method is also responsible for applying the settings to inherting types
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected virtual bool ValidateSettings(IDataSeriesSettings settings, out string error)
        {
            UnboxSetting<VectorDataSource>(ref mFrom, settings, "fromDataChannel", VectorDataSource.Positions, DataSeriesRefreshType.FullRefresh);
            UnboxSetting<VectorDataSource>(ref mTo, settings, "toDataChannel", VectorDataSource.Positions, DataSeriesRefreshType.FullRefresh);

            if (UnboxSetting(ref mCanvasSortOrder, settings, "canvasSortOrder", 0))
            {
                var canvases = GetComponentsInChildren<Canvas>();
                for (int i = 0; i < canvases.Length; i++)
                    canvases[i].sortingOrder = mCanvasSortOrder;
            }

            UnboxSetting<bool>(ref mHandleEvents, settings, "handleEvents", false, DataSeriesRefreshType.None);
            bool inputTypeChanged = UnboxSetting<InputType>(ref mInputType, settings, ".inputOptimization", InputType.GeneralPurpose, DataSeriesRefreshType.FullRefresh);
            if (inputTypeChanged || (mInputTypeSet == false))
            {
                mInputTypeSet = true;
                ArrayType = GetArrayType(mInputType);
                ChartCommon.DevLog(LogOptions.DataSeries, "series type:", GetType().Name, "array type:", ArrayType);
                mGraphic.ClearAndChangeProperties(ArrayType, ItemSize);
                Refresh();
            }
            UnboxSetting<bool>(ref mUseColorChannel, settings, UseColorChannelSettings, false, DataSeriesRefreshType.FullRefresh);
            if (mGraphic != null)
                mGraphic.HasColor = mUseColorChannel;
            error = null;
            return true;
        }

        public bool IsValidForOptimization(string name)
        {
            return false;
        }

        public virtual void BeforeRemove(int index)
        {
            ChartCommon.DevLog(LogOptions.DataSeries, GetType().Name,name, "Before remove");
        }

        public GameObject underlyingGameObject
        {
            get
            {
                return gameObject;
            }
        }

        public int CanvasViewOrder { get { return mCanvasSortOrder; } }

        public DoubleRect? GetBoundingBox(int index)
        {
            PrepareDataArrays();
            return mEntries[index].GetBoundingBox(index, this);
        }

        /// <summary>
        /// creates a mapping function for vertex mapping
        /// </summary>
        void CreateMapping()
        {
            if (mFitInto.HasValue && mCurrentView.HasValue)
            {

                ChartCommon.DevLog(LogOptions.Mapping, GetType().Name, "Current View",mCurrentView.Value.ToRect());
                ChartCommon.DevLog(LogOptions.Mapping, GetType().Name, "Fit Into", mFitInto.Value);
                mMapping = new MappingFunction(mCurrentView.Value.ToRect(), mFitInto.Value);
                ChartIntegrity.Assert(mMapping.IsValid);
            }
            else
                mMapping = MappingFunction.Zero;
            mDifferenceMapping = null;
            MakeAllDirty(false);
            Invalidate();
            ChartCommon.DevLog(LogOptions.Mapping,GetType().Name,"Mapping", mMapping);
        }

        void ApplyDifferenceMap()
        {
            if (mDifferenceMapping.HasValue == false)
                mDifferenceMapping = MappingFunction.One;
            ChartCommon.DevLog(LogOptions.Mapping, GetType().Name, "Apply Difference Map", mDifferenceMapping);
            if (mDifferenceMapping.Value.IsValid() == false)
                return;
            if (mGraphic != null)
            {
                
                Vector3 scale = mDifferenceMapping.Value.Mult.ToVector3();
                Vector3 add = mDifferenceMapping.Value.Add.ToVector3();
                Vector3 flip = new Vector3(1f, 1f, 0f);
                if (mCurrentView.Value.OppositeX)
                {
                    scale.x *= -1.0f;
                    flip.x = -1.0f;
                }
                if (mCurrentView.Value.OppositeY)
                {
                    scale.y *= -1.0f;
                    flip.y = -1.0f;
                }
                Vector3 offest = new Vector3();
                if (mObjectOffset != null)
                    offest = ObjectOffset.Calculate(mFitInto).ToVector3();
                gameObject.transform.localPosition = ChartCommon.MultiplayVector3(offest, flip);
                var rect = mGraphic.gameObject.GetComponent<RectTransform>();
                if(rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = ChartCommon.MultiplayVector3(ChartCommon.MultiplayVector3(mGraphicScale, add + mGraphicAdd) + mGraphicOffset,flip);
                    float xSize = 1000f, ySize = 1000f;
                    if (mDataViewer != null)
                    {
                        var bounds = mDataViewer.Bounds;
                        if (bounds != null)
                        {
                            if (bounds.MaxX.HasValue && bounds.MinX.HasValue)
                                xSize = (float)(Math.Max(Math.Abs(bounds.MaxX.Value), Math.Abs(bounds.MinX.Value)) * mMapping.Mult.x * 2 + 200);
                            if (bounds.MaxY.HasValue && bounds.MinY.HasValue)
                                ySize = (float)(Math.Max(Math.Abs(bounds.MaxY.Value), Math.Abs(bounds.MinY.Value)) * mMapping.Mult.y * 2 + 200);
                        }
                    }
                    if (ChartCommon.IsValid(xSize) && ChartCommon.IsValid(ySize))
                    {
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, xSize + Math.Abs(mGraphicAdd.x) * 2);
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ySize + Math.Abs(mGraphicAdd.y) * 2);
                    }
                }
                else
                    mGraphic.gameObject.transform.localPosition = ChartCommon.MultiplayVector3(mGraphicScale, mDifferenceMapping.Value.Add.ToVector3() + mGraphicAdd) + mGraphicOffset;
                mGraphic.gameObject.transform.localScale = ChartCommon.MultiplayVector3(mGraphicScale, scale);

            }
        }

        /// <summary>
        /// This method checks the current mapping relative to the ideal mapping , if they are too far from each other it will recreate the mapping (which will cause the entire data series to recalculate it's vertices)
        /// 
        /// </summary>
        void CheckUpdateMapping()
        {
            mCoCheckUpdateMapping = null;

            if (mMapping.IsZero())
            {
                ChartCommon.DevLog(LogOptions.Mapping, GetType().Name, "Mapping zero", mMapping);
                CreateMapping();
                ApplyDifferenceMap();
                return;
            }

            if (mMapping.IsValid() == false)
            {
                ChartCommon.DevLog(LogOptions.Mapping, GetType().Name, "Mapping invalid", mMapping);
                //ChartCommon.DevLog("Mapping", "invalid");
                CreateMapping();
                ApplyDifferenceMap();
                return;
            }

            if (mFitInto.HasValue && mCurrentView.HasValue)
            {
                mDifferenceMapping = MappingFunction.One;
                var map = mMapping.CreateDifferenceMapping(mCurrentView.Value.ToRect(), mFitInto.Value, MultiplyThreshold, AddThreshold);

                if (map == null)
                {
                    CreateMapping();
                }
                else
                    mDifferenceMapping = map;
                ApplyDifferenceMap();

            }
        }
        /// <summary>
        /// makes sure all data arrays are visible and readable in their most up to date state
        /// </summary>
        protected void PrepareDataArrays()
        {
            mDataViewer.SetStack(StackIndex);
        }

        /// <summary>
        /// ensure that all changes have been commited and the arrays are up to date with the latest data for this data series
        /// </summary>
        public void EnsureDataIntegrity()
        {
            if (mData != null)
            {
                mData.CommitChanges();
                PrepareDataArrays();
            }
        }

        public virtual void PrepareVertices()
        {
            if (mMapping.IsZero())
                CreateMapping();
            PrepareDataArrays();

        }
        public void SetActive(bool active)
        {
            IsActive = active;
            if(active)
            {
                HookDataEvents();
                EnsureEntites();
            }
            else
            {
                UnhookDataEvents();
                ClearAllEntries();
                ClearGraphic();
                if (mAsyncLoader != null)
                    mAsyncLoader.UnlockLoader();
               // if (mEntitesGenerated == PopulateEntitiesInProgress)
                    RaiseDoneLoading();
                mEntitesGenerated = PopulateEntitiesEmpty;
                mGraphic.SetVisible(false);
            }

            gameObject.SetActive(active);
        }

    }
}
