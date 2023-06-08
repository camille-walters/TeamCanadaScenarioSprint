using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// Some materials may have features that allow them to be optimized for chart creation. This class manages information regarding which materials can be optimized and how.
    /// also check IMaterialOptimization.cs
    /// </summary>
    public class MaterialOptimzationManager
    {
        static MaterialOptimzationManager mInstance;

        /// <summary>
        /// holds a material optimizer entry
        /// </summary>
        class MaterialOptimizationHolder
        {
            public MaterialOptimizationHolder(IMaterialOptimization opt)
            {
                Optimization = opt;
            }

            public IMaterialOptimization Optimization
            {
                get;
                private set;
            }

            public int[] UvNames
            {
                get
                {
                    if(mUvNames == null)
                        mUvNames = Optimization.UvTextureNames.Select(x => Shader.PropertyToID(x)).ToArray();
                    return mUvNames;
                }
            }

            int[] mUvNames;
        }

        public static MaterialOptimzationManager Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new MaterialOptimzationManager();
                return mInstance;
            }
        }
        
        private MaterialOptimzationManager()
        {
           
        }

        public bool IsOptimizedForUv(Material mat)
        {
            EnsureMaterialOptimizationObject();
            return mData.ContainsKey(mat.shader.name);
        }

        public int[] getUvNames(Material mat)
        {
            EnsureMaterialOptimizationObject();
            MaterialOptimizationHolder holder;

            if (mData.TryGetValue(mat.shader.name, out holder) == false)
                return null;
            return holder.UvNames;
        }

        private Dictionary<string, MaterialOptimizationHolder> mData;
        void EnsureMaterialOptimizationObject()
        {
            if (mData != null)
                return;
            mData = new Dictionary<string, MaterialOptimizationHolder>();
            foreach (Type t in ChartCommon.GetDerivedTypes<IMaterialOptimization>())
            {
                ConstructorInfo inf = t.GetConstructor(Type.EmptyTypes);
                if (inf == null)
                    ChartCommon.RuntimeWarning("Type " + t.Name + " has no public empty constructor and therefore will not be used by MaterialOptimzationManager");
                IMaterialOptimization opt = (IMaterialOptimization)inf.Invoke(null);
                if (mData.ContainsKey(opt.ShaderName))
                    ChartCommon.RuntimeWarning("Shader " + opt.ShaderName + " already has another type defining optimization for it, and is therfore ignored");
                else
                {
                    mData.Add(opt.ShaderName, new MaterialOptimizationHolder(opt));
                }
            }
        }
    }
}
