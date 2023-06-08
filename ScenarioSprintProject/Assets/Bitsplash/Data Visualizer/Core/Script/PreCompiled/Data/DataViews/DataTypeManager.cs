//using Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews;
//using Assets.Data_Visualizer.Core.Script.PreCompiled.Data.DataViews.UnspecifiedDataType;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DataVisualizer//{
//    class DataTypeManager
//    {
//        static DataTypeManager mInstance;
//        static DataTypeManager Instance { get
//            {
//                if (mInstance == null)
//                    mInstance = new DataTypeManager();
//                return mInstance;
//            } };

//        Dictionary<DataTypeEnum, DataType> mDataTypes = new Dictionary<DataTypeEnum, DataType>();

//        private DataTypeManager()
//        {
//            AddDataType(new UnspecifiedDataType());
//            AddDataType(new GraphDataType());
//        }

//        public DataType CreateDataType(DataTypeEnum val)
//        {
//            return mDataTypes[val].Clone();
//        }

//        void AddDataType(DataType type)
//        {
//            mDataTypes.Add(type.EnumValue, type);
//        }

//    }
//}
