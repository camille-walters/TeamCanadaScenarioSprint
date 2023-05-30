using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataVisualizer{
    public class ItemLabelsVisualFeature : DataSeriesVisualFeature
    {
        protected static string mVisualFeatureTypeName = "Item Labels";
        [SerializeField]
        [Tooltip("If true , the line thickness would scale with zooming of the chart. Otherwise the line thickness remains constant when zooming")]
        private bool scalesWithView;

        /// <summary>
        /// If true , the text would scale with zooming of the chart. Otherwise the line thickness remains constant when zooming
        /// </summary>
        public bool ScalesWithView
        {
            get { return scalesWithView; }
            set
            {
                scalesWithView = value;
                DataChanged();
            }
        }

        [SerializeField]
        [Tooltip("the font of the item label")]
        private Font textFont;

        /// <summary>
        /// the font of the item labels
        /// </summary>
        public Font TextFont
        {
            get { return textFont; }
            set
            {
                textFont = value;
                DataChanged();
            }
        }

        [SerializeField]
        [Tooltip("the font size of the item label")]
        private int fontSize;

        public int FontSize
        {
            get { return fontSize; }
            set
            {
                fontSize = value;
                DataChanged();
            }
        }

        [SerializeField]
        [Tooltip("the font style of the item label")]
        private FontStyle fontStyle;

        public FontStyle FontStyle
        {
            get { return fontStyle; }
            set
            {
                fontStyle = value;
                DataChanged();
            }
        }

        /// <summary>
        /// allows adding rtf markups to the text
        /// </summary>
        [SerializeField]
        [HideInInspector]
        [Tooltip("allows adding rtf markups to the text")]
        private bool richText;
        public bool RichText
        {
            get { return richText; }
            set
            {
                richText = value;
                DataChanged();
            }
        }

        /// <summary>
        /// the line spacing multipler
        /// </summary>
        [SerializeField]
        [Tooltip("the line spacing multipler")]
        private float lineSpacing = 1f;

        /// <summary>
        /// the line spacing multipler
        /// </summary>
        public float LineSpacing
        {
            get { return lineSpacing; }
            set
            {
                lineSpacing = value;
                DataChanged();
            }
        }

        /// <summary>
        /// the rotation of the text object
        /// </summary>
        [SerializeField]
        [Tooltip("the rotation of the text object")]
        private float textRotation = 0f;
        /// <summary>
        /// the rotation of the text object
        /// </summary>
        public float TextRotation
        {
            get { return textRotation; }
            set
            {
                textRotation = value;
                DataChanged();
            }
        }

        /// <summary>
        /// the offset of the text from it's original position this can be used for animating text position
        /// </summary>
        [SerializeField]
        [Tooltip("the offset of the text from it's original position. this can be used for animating text position")]
        private OffsetVector textOffset;

        /// <summary>
        /// the offset of the text from it's original position this can be used for animating text position
        /// </summary>
        public OffsetVector TextOffset
        {
            get
            {
                return textOffset;
            }
        }


        [SerializeField]
        [HideInInspector]
        [Tooltip("scaling of the item label. this can be used for animating font size")]
        private float textScale = 1f;

        public float TextScale
        {
            get { return textScale; }
            set
            {
                textScale = value;
                DataChanged();
            }
        }

        [SerializeField]
        [Tooltip("The anchor of the text relative to the set point")]
        private TextAnchor textAnchor;

        public TextAnchor TextAnchor
        {
            get {return textAnchor;}
            set
            {
                textAnchor = value;
                DataChanged();
            }
        }

        [SerializeField]
        [HideInInspector]
        [Tooltip("Aligns the item label to the size of the data point with the specified direction and length. for tight match the length of the vector should be 1")]
        private DoubleVector3 alignToSize;

        /// <summary>
        /// Aligns the item label to the size of the data point with the specified direction and length. for tight match the length of the vector should be 1
        /// </summary>
        public DoubleVector3 AlignToSize
        {
            get { return alignToSize; }
            set
            {
                alignToSize = value;
                DataChanged();
            }
        }

        [SerializeField]
        [Tooltip("the format of the item text")]
        private string textFormat = "<?x>:<?y>";

        public string TextFormat
        {

            get { return textFormat; }
            set
            {
                textFormat = value;
                DataChanged();
            }
        }

        [SerializeField]
        [ChartSpecificMaterial]
        [FormerlySerializedAs("textMaterital")]
        [Tooltip("the material used for text drawing")]
        private Material textMaterial;

        /// <summary>
        /// the material used for text drawing
        /// </summary>
        public Material TextMaterial
        {
            get
            {
                return textMaterial;
            }
            set
            {
                textMaterial = value;
                DataChanged();
            }
        }

        public override string VisualFeatureTypeName
        {
            get { return mVisualFeatureTypeName; }
        }

        public override IDataSeries GenerateSeries(GameObject item)
        {
            return item.AddComponent<ItemLabelsDataSeries>();
        }
    }
}
