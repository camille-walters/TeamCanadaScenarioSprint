using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThetaList;
using UnityEngine;

namespace DataVisualizer{
    public class ItemLabelSeriesObject : VariableSeriesObject
    {
        SimpleList<PreMappedVertex> mVertices = new SimpleList<PreMappedVertex>(true);
        string mFormattedString = null;

        public class ItemLabelSettings
        {
            /// <summary>
            /// a vector of the direction at which the text should be aligned to a sized point. 0 vector means center of a point , right vector means the text is aligned to the right of the sized point etc.
            /// </summary>
            public DoubleVector3 AlignToSize = DoubleVector3.zero;
            public StringFormatter mFormatter;
            public TextGenerator TextGenerator;
            public string DefaultFormat;
            public bool Scalable;
            public Matrix4x4 Rotation;
            public TextGenerationSettings mSettings;
            public DataSeriesBase mParent;
            /// <summary>
            /// this dictionary contains the format parameters and their values. The constant ones are set by the data series object . the ones that change between labels are set in this object
            /// </summary>
            public Dictionary<string, object> mParameters = new Dictionary<string, object>();
            public void Ensure()
            {
                mSettings = new TextGenerationSettings();
                TextGenerator = new TextGenerator();
            }
        }


        public ItemLabelSeriesObject()
        {
            
        }

        public override void MakeDirty()
        {
            //Add check to see if anything relevant to the format has changed. If not , then skip dirty
            base.MakeDirty();
            mFormattedString = null;
        }


        public override bool Is3D { get { return false; } }

        ItemLabelSettings GetSettings(DataSeriesBase mapper)
        {
            return (ItemLabelSettings)(mapper.GraphicSettingsObject);
        }

        void CreateString(DataSeriesBase mapper)
        {
            var settings = GetSettings(mapper);
            StackDataViewer.IDataArray<DoubleVector3> arr = mapper.RawData.RawPositionArray;
            StackDataViewer.IDataArray <string> names = mapper.RawData.RawNameArray;
            string name = null;
            DoubleVector3 pos = arr.Get(MyIndex);
            settings.mParameters[StringFormatter.ParameterXValue] = pos.x;
            settings.mParameters[StringFormatter.ParameterYValue] = pos.y;
            settings.mParameters[StringFormatter.ParameterZValue] = pos.z;
            if (names.IsNull == false)
                name = names.Get(MyIndex);
            
            StringFormatter format = settings.mFormatter;
            if (name != null)
                format = StringFormatter.GetFormat(name);
            StackDataViewer.IDataArray <double> sizeArr = mapper.RawData.RawSizeArray;
            if (sizeArr.IsNull == false)
            {
                double size = sizeArr.Get(MyIndex);
                settings.mParameters[StringFormatter.ParameterSize] = size;
            }
            arr = mapper.RawData.RawEndPositionArray;
            if (arr.IsNull == false)
            {
                DoubleVector3 endPos = arr.Get(MyIndex);
                settings.mParameters[StringFormatter.ParameterEndXValue] = endPos.x;
                settings.mParameters[StringFormatter.ParameterEndYValue] = endPos.y;
                settings.mParameters[StringFormatter.ParameterEndZValue] = endPos.z;
            }
            StackDataViewer.IDataArray < DoubleRange> rangeArr = mapper.RawData.RawHighLowArray;
            if (rangeArr.IsNull == false)
            {
                settings.mParameters[StringFormatter.ParameterHighValue] = rangeArr.Get(MyIndex).Max;
                settings.mParameters[StringFormatter.ParameterLowValue] = rangeArr.Get(MyIndex).Min;
            }
            rangeArr = mapper.RawData.RawStartEndArray;
            if (rangeArr.IsNull == false)
            {
                settings.mParameters[StringFormatter.ParameterStartValue] = rangeArr.Get(MyIndex).First;
                settings.mParameters[StringFormatter.ParameterEndValue] = rangeArr.Get(MyIndex).Last;
            }

            rangeArr = mapper.RawData.RawErrorRangeArray;
            if (rangeArr.IsNull == false)
            {
                settings.mParameters[StringFormatter.ParameterMaxErrorValue] = rangeArr.Get(MyIndex).Max;
                settings.mParameters[StringFormatter.ParameterMinErrorValue] = rangeArr.Get(MyIndex).Min;
            }
            StackDataViewer.IDataArray <object> userArr = mapper.RawData.RawUserDataArray;
            if (userArr.IsNull == false)
                settings.mParameters[StringFormatter.ParameterUserData] = userArr.Get(MyIndex);
            mFormattedString = format.FormatValues(settings.mParameters,(DataSeriesChart)settings.mParent.Parent);
        }

        DoubleVector3 GetPos(DataSeriesBase mapper)
        {
            var settings = GetSettings(mapper);
            StackDataViewer.IDataArray<DoubleVector3> arr = mapper.RawData.RawPositionArray;
            DoubleVector3 pos = arr.Get(MyIndex);
            StackDataViewer.IDataArray<double> sizeArr = mapper.RawData.RawSizeArray;
            if (sizeArr.IsNull == false)
                pos += sizeArr.Get(MyIndex) * settings.AlignToSize;
            return pos;
        }

        public override void WriteItemVertices(int itemIndex, int position, DataToArrayAdapter arrays)
        {
            var mapper = arrays.mMapper;
            EnsureVertices(mapper);
            int start = itemIndex * ItemSize;
            int end = start + ItemSize;
            //    ChartCommon.DevLog("item label verts", mVertices.Count);
            var array = mVertices.RawArrayWithExtraLastItem;
            Color32 color = ChartCommon.White;
            if (arrays.mMapper.HasColor)
                color = arrays.RawColorArray.Get(mMyIndex);
            for (int i = start; i < end; i++)
            {
                // ChartCommon.DevLog("vettex",mVertices.RawArray[i].tangent);
                array[i].color = color;
                arrays.MapVertexOptimizedFixedTangent(position,ref array[i]);
                position++;
            }
        }

        void EnsureVertices(DataSeriesBase mapper)
        {
            if (mFormattedString == null || mVertices == null)
            {
                CreateString(mapper);
                DoubleVector3 pos = GetPos(mapper);
                var settings = GetSettings(mapper);
                var generator = settings.TextGenerator;
                if (generator.PopulateWithErrors(mFormattedString, settings.mSettings, mapper.gameObject) == false)
                {
                    ChartCommon.DevLog("populate failed", "fail " + mFormattedString);
                }
                var center = (Vector3)generator.rectExtents.center;
                int index = 0;

                var generatedVerts = generator.verts;
                if (mVertices == null)
                {
                    mVertices = new SimpleList<PreMappedVertex>(generator.vertexCount);
                    mVertices.ClearWithoutRelease = true;
                }
                else
                    mVertices.Clear();
                if (generator.vertexCount > 0)
                {
                    var addTo = mVertices.AddEmpty(generator.vertexCount);
                    for (int i = generator.vertexCount - 1; i >= 0; i--)
                    {
                        Vector3 dist = generatedVerts[i].position - center;
                        dist.z = 0;
                        float mag = Math.Max(dist.magnitude, 0.00001f);
                        Vector3 tan = new Vector3(dist.x / mag, dist.y / mag, 0f);
                        ChartCommon.CreatePremappedVertex(ref addTo[index++], pos, generatedVerts[i].uv0, 0, tan, generatedVerts[i].color, mag, 0f);
                    }
                }
                else
                {
                    var addTo = mVertices.AddEmpty(4);
                    for (int i = 3; i >= 0; i--)
                        ChartCommon.CreatePremappedVertex(ref addTo[index++], new DoubleVector3(), new Vector2(), 0, new Vector4(), new Color(0f, 0f, 0f, 0f), 0f, 0f);
                }
                //    ChartCommon.DevLog("item label", mFormattedString);
                //    ChartCommon.DevLog("item label gen", generator.vertexCount);
                //   ChartCommon.DevLog("item label verts pre", mVertices.Count);

            }
        }

        protected override DoubleRect? CalcBoundingBox(DataSeriesBase mapper)
        {
            EnsureVertices(mapper);
            var settings = GetSettings(mapper);
            var generator = settings.TextGenerator;
            float halfWidth = generator.GetPreferredWidth(mFormattedString, settings.mSettings) * 0.5f;
            float halfHeight = generator.GetPreferredHeight(mFormattedString, settings.mSettings) * 0.5f;
            DoubleVector3 pos = GetPos(mapper);
            return new DoubleRect(pos.x - halfWidth, pos.y - halfHeight, halfWidth * 2, halfHeight * 2);
        }

        protected override double CalculateLength(DataSeriesBase mapper)
        {
            return 1.0;
        }

        public override void DiscardData()
        {
            mVertices = null;
        }

        protected override int CalculateItemCount(DataSeriesBase mapper)
        {
            EnsureVertices(mapper);
            return mVertices.Count / ItemSize;
        }

    }
}
