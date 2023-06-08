//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using ThetaList;
//using UnityEngine;

//namespace DataVisualizer//{
//    class PageDataModifier : IDataViewModifier
//    {
//        public int StackCount => throw new NotImplementedException();

//        public int Count => throw new NotImplementedException();

//        public ChannelType CurrentChannels => throw new NotImplementedException();

//        public string Name => throw new NotImplementedException();

//        public event Action<object> OnChannelCompositionChanged;
//        public event Action<object, ChannelType> OnSetArray;
//        public event Action<object, int> OnBeforeSet;
//        public event Action<object, int> OnSet;
//        public event Action<object, int> OnAppend;
//        public event Action<object, int> OnInsert;
//        public event Action<object, int> OnBeforeInsert;
//        public event Action<object, int> OnBeforeRemove;
//        public event Action<object, int> OnRemove;
//        public event Action<object, OperationTree<int>> OnAfterCommit;
//        public event Action<object, OperationTree<int>> OnBeforeCommit;
//        public event Action<object> OnClear;
//        public event Action<object> OnUncomittedData;

//        public void CommitChanges()
//        {
//            throw new NotImplementedException();
//        }

//        public void FitInto(DoubleRect localRect, DoubleRect parentRect)
//        {
//            throw new NotImplementedException();
//        }

//        public DoubleRect?[] RawBoundingVolume(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public Color32[] RawColorArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public DoubleVector3[] RawEndPositionArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public DoubleRange[] RawErrorRangeArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public DoubleRange[] RawHighLowArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public string[] RawNameArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public DoubleVector3[] RawPositionArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public double[] RawSizeArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public DoubleRange[] RawStartEndArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public object[] RawUserDataArray(int stack)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetParent(DataSeriesBase parent)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetView(double fromX, double fromY, double xSize, double ySize, double viewDiagonalBase)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
