
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class GraphicMaterialManager
    {
        public readonly static string GraphicUvMapping = "_UVMapping";
        public readonly static string GraphicExtrusion = "_Extrusion";
        public readonly static string GraphicDirTransform = "_DirTransform";
        public readonly static string GraphicScale = "_LocalScale";
        public readonly static string GraphicRect = "_ClipRect";
        bool mEmpty = true;
        protected bool mSharedMaterial = false;
        public Material mCachedMaterial { get; private set; }

        Material mBaseMaterial;
        int mExtrusionName = -1;
        int mDirMatrixName = -1;
        int mScaleName = -1;
        int mClipRectName = -1;

        public int ExtrusionName { get { return mExtrusionName; } }
        public int DirectionName { get { return mDirMatrixName; } }
        public int ScaleName { get { return mScaleName; } }
        /// <summary>
        /// the texture property ids in the current material that should be affected by the Uv mapping set by SetUvMapping
        /// </summary>
        int[] mMaterialUvNames;

        /// <summary>
        /// these are the inital scale and offset values of the textures in the current material. these values will be transformed by the uv mapping to create the final values
        /// </summary>
        DoubleRect[] mMaterialInitialMapping;
        float mExtrusionAmount = 0f;
        protected MappingFunction mUvMappingFunction = MappingFunction.One;
        bool mMaintainAngles = true;
        Matrix4x4 mDirectionTransform = Matrix4x4.identity;
        Rect? mClipRect;
        MonoBehaviour mParent;

        public GraphicMaterialManager()
        {
            Awake();
        }

        public float ExtrusionAmount
        {
            get { return mExtrusionAmount; }
            set
            {
                mExtrusionAmount = value;
                if (mCachedMaterial != null && HasFeature(mExtrusionName))
                    mCachedMaterial.SetFloat(mExtrusionName, mExtrusionAmount);
            }
        }

        public MappingFunction UvMapping
        {
            get { return mUvMappingFunction; }
            set
            {
                mUvMappingFunction = value;
            }
        }

        public Matrix4x4 DirectionTranform
        {
            get { return mDirectionTransform; }
            set
            {
                mDirectionTransform = value;
                SetMaterialUniforms();
            }
        }

        public void LateUpdate()
        {
            if (mParent == null || mCachedMaterial == null)
                return;
            ChartCommon.PerformAtEndOfFrame(mParent, ApplyMappingToMaterial);
        }

        public void Update()
        {
            ChartIntegrity.Assert(mEmpty == true || mCachedMaterial != null);
            if (mSharedMaterial == false)
            {
                if (mCachedMaterial != null)
                {
                    if (mCachedMaterial != mBaseMaterial)
                    {
                        var tex = mCachedMaterial.mainTexture;
                        mCachedMaterial.CopyPropertiesFromMaterial(mBaseMaterial);       // TODO: check for perofrmance issues
                        mCachedMaterial.mainTexture = tex;
                    }
                    ExtrusionAmount = mExtrusionAmount;
                }
                // ApplyMappingToMaterial();
                SetMaterialUniforms();
            }
            else
            {
                SetClipRectUniform();
            }
            
        }

        public void OnDestory()
        {
            if (mSharedMaterial == false)
                ChartCommon.SafeDestroy(mCachedMaterial);
            mCachedMaterial = null;
        }

        public bool CheckExtrusion()
        {
#if DEBUG
            //    return false;
#endif
      //      ChartIntegrity.Assert(mCachedMaterial != null);
            if (mCachedMaterial == null)
            {

                return false;
            }
           
            //   ChartCommon.DevLog("has transfrom", mCachedMaterial.HasProperty(mDirMatrixName) + " " + mCachedMaterial.shader.name + " " + mCachedMaterial.GetInstanceID());
            return mCachedMaterial.HasProperty(mExtrusionName) && mCachedMaterial.HasProperty(mScaleName);//&& mCachedMaterial.HasProperty(mDirMatrixName);
        }

        public virtual bool HasFeature(string featureName)
        {
            if (featureName == GraphicUvMapping)
                return HasUvMapping();
            if (featureName == GraphicExtrusion)
                return CheckExtrusion();
            if (mCachedMaterial != null)
                return mCachedMaterial.HasProperty(featureName);
            return false;
        }

        public virtual bool HasFeature(int id)
        {
            if (id == mExtrusionName)
                return CheckExtrusion();
            if (mCachedMaterial != null)
                return mCachedMaterial.HasProperty(id);
            return false;
        }

        protected virtual void OnCachedMaterial()
        {
            ExtrusionAmount = mExtrusionAmount; // assign the current line thickness to the shader
            CheckMaterialOptimization();
            ApplyMappingToMaterial();
            SetMaterialUniforms();
        }

        public bool HasUvMapping()
        {
#if DEBUG
            //   return false;
#endif
            return (mMaterialUvNames != null) && (mMaterialInitialMapping != null);
        }

        void Awake()
        {
            mExtrusionName = Shader.PropertyToID(GraphicExtrusion);
            mScaleName = Shader.PropertyToID(GraphicScale);
            mClipRectName = Shader.PropertyToID(GraphicRect);
            mDirMatrixName = Shader.PropertyToID(GraphicDirTransform);
        }
        void ApplyMappingToMaterial()
        {
            ApplyMappingToMaterial(mCachedMaterial);
        }
        void ApplyMappingToMaterial(Material mat)
        {
            if (mSharedMaterial)
                return;
            if (mCachedMaterial == null)
                return;
            if (HasUvMapping() == false)
                return;
            if (mMaterialInitialMapping.Length != mMaterialUvNames.Length)
                return;

            //ChartCommon.DevLog(LogOptions.GraphicOptimization,"mapping","set");
            for (int i = 0; i < mMaterialInitialMapping.Length; i++)
            {
                int texId = mMaterialUvNames[i];
                DoubleRect texMap = mMaterialInitialMapping[i];
                DoubleVector3 add = ((texMap.Min) * mUvMappingFunction.Mult) + mUvMappingFunction.Add;
                DoubleVector3 mult = texMap.Size * mUvMappingFunction.Mult;

                add = mUvMappingFunction.Add;
                // add.y += 1f - mult.y;
                //  ChartCommon.DevLog("add", add);
                //  ChartCommon.DevLog("mult", mu lt);            
                mat.SetTextureOffset(texId, add.ToVector2());
                mat.SetTextureScale(texId, mult.ToVector2());
            }
        }
        void SetClipRectUniform()
        {
            if (mParent == null || mCachedMaterial == null || mClipRectName == -1)
                return;
            //if (mClipRect.HasValue)
            //{
            //    Rect rect = mClipRect.Value;
            //    Debug.Log(rect);
            //    mCachedMaterial.SetVector(mClipRectName, new Vector4(rect.x, rect.y,rect.x + rect.width, rect.y + rect.height));
            //    //mCachedMaterial.SetVector(mClipRectName, new Vector4(0,0,0,0));
            //}
            //else
            //{
            //    mCachedMaterial.SetVector(mClipRectName, new Vector4(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue));
            //}
        }
        public void SetMaterialUniforms()
        {
            if (mParent == null || mCachedMaterial == null || mScaleName == -1 || mDirMatrixName == -1 || mClipRectName == -1)
                return;


            if (mSharedMaterial)
                return;

            mCachedMaterial.SetMatrix(mDirMatrixName, mDirectionTransform);
            if (mMaintainAngles == false)
            {
                mCachedMaterial.SetVector(mScaleName, mParent.transform.localScale);
            }
            else
                mCachedMaterial.SetVector(mScaleName, new Vector4(1f, 1f, 1f, 1f));
        }

        void CheckMaterialOptimization()
        {
            mMaterialInitialMapping = null;
            if (mCachedMaterial != null)
            {
                mMaterialUvNames = MaterialOptimzationManager.Instance.getUvNames(mCachedMaterial);
                if (mMaterialUvNames == null)
                    return;
                mMaterialInitialMapping = new DoubleRect[mMaterialUvNames.Length];
                for (int i = 0; i < mMaterialInitialMapping.Length; i++)
                {
                    int texId = mMaterialUvNames[i];
                    DoubleVector2 offs = new DoubleVector2(mCachedMaterial.GetTextureOffset(texId));
                    DoubleVector2 scale = new DoubleVector2(mCachedMaterial.GetTextureScale(texId));
                    mMaterialInitialMapping[i] = new DoubleRect(offs.x, offs.y, scale.x, scale.y);
                }
            }
            else
            {
                mMaterialUvNames = null;
            }
        }


        public bool MaintainAngles
        {
            get { return mMaintainAngles; }
            set
            {
                mMaintainAngles = value;
                SetMaterialUniforms();
            }
        }   
        public void CopyTo(Material mat)
        {
            if (mCachedMaterial == null)
                return;
            mat.SetVector(mScaleName, mCachedMaterial.GetVector(mScaleName));
            mat.SetMatrix(mDirMatrixName, mDirectionTransform);
            ApplyMappingToMaterial(mat);
            mat.SetFloat(mExtrusionName, mExtrusionAmount);

        }
        public void SetClipRect(bool isValid,Rect rect)
        {
            if (isValid)
                mClipRect = rect;
            else
                mClipRect = null;
            SetClipRectUniform();
        }
        public void SetMaterial(MonoBehaviour parent,Material mat,bool shared,bool create = true)
        {
            mParent = parent;
            mSharedMaterial = shared;
            mBaseMaterial = mat;
            mEmpty = (mat == null);
            if (mSharedMaterial == true)
            {
                mCachedMaterial = mat;
                CheckMaterialOptimization();
                return;
            }

            ChartCommon.SafeDestroy(mCachedMaterial);
            if (mat == null)
            {
                mCachedMaterial = null;
                return;
            }

            if (create)
            {
                mCachedMaterial = new Material(mat);
                mCachedMaterial.hideFlags = HideFlags.DontSave;
            }
            else
            {
                mCachedMaterial = mat;
            }
            OnCachedMaterial();
        }
    }
}
