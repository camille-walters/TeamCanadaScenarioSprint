using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class ItemLabelsDataSeries : DataSeriesBase
    {
        public const string TextScaleableSetting = "scaleable";
        public const string TextFontSetting = "textFont";
        public const string TextFontSizeSetting = "fontSize";
        public const string TextFontStyleSetting = "fontStyle";
        public const string TextRichTextSetting = "richText";
        public const string TextLineSpacingSetting = "lineSpacing";
        public const string TextRotationSetting = "textRotation";
        public const string TextOffsetSetting = "textOffset";
        public const string TextScaleSetting = "textScale";
        public const string TextFormatSetting = "textFormat";
        public const string TextAlignToSizeSetting = "alignToSize";
        public const string TextMaterialSetting = "textMaterial";
        public const string TextAnchorSetting = "textAnchor";
        public const string TextAxisDirectionSetting = "axisDirection";

        IDataSeriesSettings mSettings;
        ItemLabelSeriesObject.ItemLabelSettings mLabelSettings = new ItemLabelSeriesObject.ItemLabelSettings();
        Material mSettingsMaterial;
        Material mMaterial;
        Font mDefaultFont;
        float mFontScale = 1f;
        float mFontRoatation = 0f;
        OffsetVector mFontOffset = new OffsetVector();
        AxisDimension mDirection = AxisDimension.X;
        public ItemLabelsDataSeries()
            : base( ArrayManagerType.Chunked,4)
        {

        }

        protected override ArrayManagerType GetArrayType(InputType type)
        {
            if (type == InputType.Static)
                return ArrayManagerType.Static;
            return ArrayManagerType.Chunked;
        }

        protected override bool IsCanvas
        {
            get { return true; }
        }

        public override Rect GetUvRect(int index, SeriesObject obj)
        {
            return new Rect(0f, 0f, 1f, 1f);
        }

        protected override void GenerateObjectsForIndex(int index, IList<SeriesObject> objects)
        {
            if (mSettings == null)
                return;
            var line = new ItemLabelSeriesObject();
            objects.Add(line);
        }

        protected override void SetData(IDataViewerNotifier data)
        {
            base.SetData(data);
            if (RawData != null)
                mLabelSettings.mParameters[StringFormatter.ParameterCategory] = RawData.Name;
        }

        Font DefaultFont
        {
            get
            {
                if (mDefaultFont == null)
                {
                    mDefaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    ChartCommon.DevLog("default font", mDefaultFont);
                }
                return mDefaultFont;
            }
        }

        public override object GraphicSettingsObject { get { return mLabelSettings; } }

        void SetExtrusion()
        {
            IChartGraphic graphic = Graphic;
            if (graphic != null)
            {
                if (ViewDiagonalBase > 0)
                {
                    double thickness = mFontScale;
                    if (mLabelSettings.Scalable)
                        thickness *= ViewDiagonalRatio;
                    graphic.ExtrusionAmount = (float)thickness;
                }
            }
        }

        public override void OnSetView(ViewPortion view)
        {
            base.OnSetView(view);
            SetExtrusion();
        }

        public override void FitInto(ViewPortion localView)
        {
            base.FitInto(localView);
            SetExtrusion();
        }
        
        protected override bool ValidateSettings(IDataSeriesSettings settings, out string error)
        {
            if (base.ValidateSettings(settings, out error) == false)
                return false;           
            mSettings = settings;
            mLabelSettings.Ensure();
            mLabelSettings.mParent = this;
            UnboxSetting(ref mLabelSettings.AlignToSize, mSettings, TextAlignToSizeSetting, DoubleVector3.zero);

            UnboxSetting(ref mLabelSettings.DefaultFormat, mSettings, TextFormatSetting, "<?x>:<?y>");
            mLabelSettings.mFormatter = StringFormatter.GetFormat(mLabelSettings.DefaultFormat);

            UnboxSetting(ref mLabelSettings.mSettings.font, mSettings, TextFontSetting, DefaultFont, DataSeriesRefreshType.FullRefresh);
            UnboxSetting(ref mLabelSettings.mSettings.fontSize, mSettings, TextFontSizeSetting, 10);
            UnboxSetting(ref mLabelSettings.mSettings.fontStyle, mSettings, TextFontStyleSetting, FontStyle.Normal);
            UnboxSetting(ref mLabelSettings.mSettings.richText, mSettings, TextRichTextSetting, false);
            UnboxSetting(ref mLabelSettings.mSettings.lineSpacing, mSettings, TextLineSpacingSetting, 1.0f);

            mLabelSettings.mSettings.generateOutOfBounds = true;
            mLabelSettings.mSettings.horizontalOverflow = HorizontalWrapMode.Overflow;
            mLabelSettings.mSettings.verticalOverflow = VerticalWrapMode.Overflow;

            UnboxSetting(ref mLabelSettings.mSettings.textAnchor, mSettings, TextAnchorSetting, TextAnchor.MiddleCenter);

            mLabelSettings.mSettings.scaleFactor = 1f;
            mLabelSettings.mSettings.color = Color.white;
            mLabelSettings.mSettings.resizeTextForBestFit = false;
            mLabelSettings.mSettings.pivot = Vector2.zero;

            bool updateMaterial = UnboxSetting(ref mSettingsMaterial, mSettings, TextMaterialSetting, null);

            UnboxSetting(ref mFontScale, mSettings, TextScaleSetting, 1f, DataSeriesRefreshType.None);
            UnboxSetting(ref mFontRoatation, mSettings, TextRotationSetting, 0f, DataSeriesRefreshType.None);
            UnboxSetting(ref mFontOffset, mSettings, TextOffsetSetting, null, DataSeriesRefreshType.None);
            if(mFontOffset == null)
                mFontOffset = new OffsetVector();
            UnboxSetting(ref mDirection, mSettings, TextAxisDirectionSetting,AxisDimension.X, DataSeriesRefreshType.None);

            ObjectOffset = mFontOffset.Clone();

            if (mDirection == AxisDimension.Y)
                ObjectOffset.Flip();
            if (mSettingsMaterial == null)
            {
                error = "Label material cannot be null";
                DestroyMaterial();
                return false;
            }

            if(mLabelSettings.mSettings.font == null || mLabelSettings.mSettings.font.material == null || mLabelSettings.mSettings.font.material.mainTexture == null)
            {
                error = "Label font material is invalid";
                DestroyMaterial();
                return false;
            }

            var graphic = Graphic;
            SetExtrusion();

            if (graphic != null)
            {
                graphic.MaintainAngles = false;
                graphic.DirectionTranform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, mFontRoatation), new Vector3(1f, 1f, 1f));
                if (updateMaterial)
                {
                    ChartCommon.DevLog("set texture", mLabelSettings.mSettings.font.material.mainTexture);
                    if (mMaterial != null)
                    {
                        ChartCommon.SafeDestroy(mMaterial);
                        mMaterial = null;
                    }
                    mMaterial = new Material(mSettingsMaterial);    // duplicate the material so we can set the font
                    mMaterial.mainTexture = mLabelSettings.mSettings.font.material.mainTexture;
                    graphic.SetMaterial(mMaterial, false);// mLabelSettings.mSettings.font.material;
                }
                graphic.TringleStrip = false;
            }
            MakeAllDirty(false);
            return true;
        }

        void DestroyMaterial()
        {
            if (mMaterial != null)
            {
                ChartCommon.SafeDestroy(mMaterial);
                mMaterial = null;
            }
            mSettingsMaterial = null;
        }

        public override void OnDestroy()
        {
            if (mMaterial != null)
            {
                DestroyMaterial();
                mMaterial = null;
            }
            base.OnDestroy();
        }

        public override void UniformUpdate()
        {
            base.UniformUpdate();
            if(mMaterial != null && mSettingsMaterial != null)
                mMaterial.CopyPropertiesFromMaterial(mSettingsMaterial);
        }

        protected override void GetConnectedInidices(int index, IList<int> related)
        {
            //   no indices connected to index
        }
    }
}
