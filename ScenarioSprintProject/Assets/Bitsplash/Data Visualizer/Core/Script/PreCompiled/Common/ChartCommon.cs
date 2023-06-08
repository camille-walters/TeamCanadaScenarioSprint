
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DataVisualizer{
    public class ChartCommon
    {
        class IntComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return x == y;
            }

            public int GetHashCode(int obj)
            {
                return obj.GetHashCode();
            }
        }

        public static readonly Vector2 ZeroUV = new Vector2(0f, 0f);
        public static readonly Vector3 BackVector = new Vector3(0f, 0f, -1f);
        private static Material mDefaultMaterial;
        public static readonly Color32 White = new Color32(255, 255, 255, 255);
        public static readonly Color32 Transparent = new Color32(0, 0, 0, 0);

        public static double Select(double a,double? b, bool isB,double defValue = 0.0)
        {
            if (!isB)
                return a;
            if (b.HasValue == false)
                return defValue;
            return b.Value;
        }

        public static IEnumerable<Type> GetDerivedTypes<T>()
        {
            Type t = typeof(T);
            Assembly assembly = t.Assembly;
            return assembly.GetTypes().Where(x => x.IsAbstract == false && t.IsAssignableFrom(x));
        }

        static ChartCommon()
        {
            DefaultIntComparer = new IntComparer();
        }
        /// <summary>
        /// return true if a and b are of the same magnitude. For example if magnitude = 2 , the this method returns true 
        /// if b < a*2 && a < b*2
        /// </summary>
        /// <returns></returns>
        public static bool OfEqualMagnitude(double a, double b, double magnitude = 2.0)
        {
            if (a > b * magnitude || b > a * magnitude)
                return false;
            return true;
        }
        public static void ZeroLocalTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        /// <summary>
        /// tries to unbox an object . returning defValue on failure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static T UnboxObject<T>(object obj, T defValue)
        {
            if (obj == null || !(obj is T))
                return defValue;
            return (T)obj;
        }
        /// <summary>
        /// returns true if x and y are equal with an error margin
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static bool CompareDouble(double x, double y, double error)
        {
            return Math.Abs(x - y) < error;
        }

        public static bool CompareDoubleVector(DoubleVector3 a, DoubleVector3 b, double error)
        {
            return (Math.Abs(a.x - b.x) < error) && (Math.Abs(a.y - b.y) < error) && (Math.Abs(a.z - b.z) < error);
        }
        public static void ClearChildren(GameObject gameObj)
        {
            var trans = gameObj.transform;
            for (int i = 0; i < trans.childCount; i++)
                SafeDestroy(trans.GetChild(i).gameObject);
        }
        public static bool IsValid(float number)
        {
            return (!float.IsNaN(number)) && (!float.IsInfinity(number));
        }
        public static bool IsValid(double number)
        {
            return (!double.IsNaN(number)) && (!double.IsInfinity(number));
        }
        public static bool IsValid(DoubleVector3 v)
        {
            return IsValid(v.x) && IsValid(v.y) && IsValid(v.z);
        }
        /// <summary>
        /// checks if a point is contained within a polygon
        /// </summary>
        /// <param name="point"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static double PointPolySquareDist(DoubleVector3 point, IList<PreMappedVertex> vertices)
        {
            int count = 0;
            double minSqrDist = double.MaxValue;
            for (int i = 0; i < vertices.Count; i++)
            {
                DoubleVector3 current = vertices[i].preMapped;
                DoubleVector3 next = vertices[(i + 1) % vertices.Count].preMapped;
                minSqrDist = Math.Min(minSqrDist, SegmentPointSqrDistance(current, next, point));
                if (current.y <= point.y)
                {
                    if (next.y > point.y && IsLeft(current, next, point) > 0)
                        ++count;  // have a valid up intersect
                }
                else
                {
                    if (next.y <= point.y && IsLeft(current, next, point) < 0)
                        --count;
                }
            }
            if (count != 0)
                return 0.0;
            return minSqrDist;
        }

        public static float InvertValue(float v)
        {
            if (Math.Abs(v) <= 0.00001f)
                return v;
            return 1f / v;
        }
        public static double InvertValue(double v)
        {
            if (Math.Abs(v) <= 0.00001f)
                return v;
            return 1f / v;
        }
        public static DoubleVector3 InvertScale(DoubleVector3 scale)
        {
            return new DoubleVector3(InvertValue(scale.x), InvertValue(scale.y), InvertValue(scale.z));
        }
        public static Vector3 InvertScale(Vector3 scale)
        {
            return new Vector3(InvertValue(scale.x), InvertValue(scale.y), InvertValue(scale.z));
        }
        /// <summary>
        /// helper method for PointInPoly
        /// </summary>
        /// <param name="segA"></param>
        /// <param name="segB"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static int IsLeft(DoubleVector3 segA, DoubleVector3 segB, DoubleVector3 point)
        {
            return (int)((segB.x - segA.x) * (point.y - segA.y) - (point.x - segA.x) * (segB.y - segA.y));
        }

        public static Vector3 MultiplayVector3(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static void EnsureMinMax(ref double min, ref double max)
        {
            if (min < max)
                return;
            double tmp = min;
            min = max;
            max = tmp;
        }

        public static void Swap(ref double a, ref double b)
        {
            double tmp = a;
            a = b;
            b = tmp;
        }

        public static double? DoubleLerp(double? from, double? to, double factor)
        {
            if (from.HasValue == false || to.HasValue == false)
                return null;
            return DoubleLerp(from.Value, to.Value, factor);
        }

        public static double DoubleLerp(double from, double to, double factor)
        {
            return (from * (1.0 - factor)) + (to * factor);
        }

        public static float SmoothLerp(float from, float to, float factor)
        {
            return (from * (1f - factor)) + (to * factor);
        }

        public static GameObject CreateCanvasChartItem()
        {
            GameObject obj = new GameObject("item", typeof(RectTransform));
            obj.AddComponent<ChartItem>();
            return obj;
        }
        public static double? Max(double? x, double? y)
        {
            if (x.HasValue == false)
                return y;
            if (y.HasValue == false)
                return x;
            return Math.Max(x.Value, y.Value);
        }

        public static double? Min(double? x, double? y)
        {
            if (x.HasValue == false)
                return y;
            if (y.HasValue == false)
                return x;
            return Math.Min(x.Value, y.Value);
        }

        public static double Max(double? x, double y)
        {
            if (x.HasValue == false)
                return y;
            return Math.Max(x.Value, y);
        }

        public static double Min(double? x, double y)
        {
            if (x.HasValue == false)
                return y;
            return Math.Min(x.Value, y);
        }

        public static double Clamp(double val)
        {
            if (val > 1.0)
                return 1.0;
            if (val < 0.0)
                return 0.0;
            return val;
        }

        public static GameObject CreateChartItem()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<ChartItem>();
            return obj;
        }

        public static double normalizeInRangeX(double value, DoubleVector3 min, DoubleVector3 size)
        {
            return normalizeInRange(value, min.x, size.x);
        }

        public static double normalizeInRangeY(double value, DoubleVector3 min, DoubleVector3 size)
        {
            return normalizeInRange(value, min.y, size.y);
        }

        public static double normalizeInRange(double value, double min, double size)
        {
            return (value - min) / size;
        }

        public static double interpolateInRectX(Rect rect, double x)
        {
            return rect.x + rect.width * x;
        }

        public static double interpolateInRectY(Rect rect, double y)
        {
            return rect.y + rect.height * y;
        }

        public static Rect RectFromCenter(float centerX, float sizeX, float top, float bottom)
        {
            float half = sizeX * 0.5f;
            return new Rect(centerX - half, top, sizeX, bottom - top);
        }

        public static DoubleVector4 interpolateInRect(Rect rect, DoubleVector3 point)
        {
            double x = rect.x + rect.width * point.x;
            double y = rect.y + rect.height * point.y;
            return new DoubleVector4(x, y, point.z, 0.0);

        }

        public static void HideObject(GameObject obj)
        {
#if DONTHIDEINNEROBJECTS
            obj.hideFlags = obj.hideFlags & ~HideFlags.HideInHierarchy & ~HideFlags.HideInInspector;
#else
            obj.hideFlags = obj.hideFlags | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
#endif
        }

        public static IEnumerator CoPerformAtEndOfFrame(Action perform)
        {
            yield return 0;// new WaitForEndOfFrame();
            perform();
        }

        public static Coroutine PerformAtEndOfFrame(MonoBehaviour b, Action perform)
        {
            if (!IsInEditMode && b.isActiveAndEnabled)
                return b.StartCoroutine(CoPerformAtEndOfFrame(perform));
            perform();
            return null;
        }

        /// <summary>
        /// performs an action either at the end of the frame if "performImidiateIfFalse" is true. otherwise performs the action imidiatly
        /// </summary>
        /// <param name="b"></param>
        /// <param name="endOfFrameIfTrue"></param>
        /// <param name="perform"></param>
        public static Coroutine PerformAtEndOfFrameOnCondition(MonoBehaviour b, bool performImidiateIfFalse, Action perform)
        {
            if (ChartCommon.IsInEditMode || performImidiateIfFalse == false || b.isActiveAndEnabled == false)
            {
                perform();
                return null;
            }
            return b.StartCoroutine(CoPerformAtEndOfFrame(perform));
        }

        public static Vector3 Perpendicular(Vector3 v)
        {
            return new Vector3(v.y, -v.x, v.z);
        }

        public static Vector2 Perpendicular(Vector2 v)
        {
            return new Vector2(v.y, -v.x);
        }

        public static bool SegmentIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
        {
            intersection = new Vector2();

            Vector2 dirA = a2 - a1;
            Vector2 dirB = b2 - b1;

            float dotA = Vector2.Dot(dirA, Perpendicular(dirB));
            if (dotA == 0)
                return false;

            Vector2 dirAB = b1 - a1;
            float t = Vector2.Dot(dirAB, Perpendicular(dirB)) / dotA;
            if (t < 0 || t > 1)
                return false;

            float s = Vector2.Dot(dirAB, Perpendicular(dirA)) / dotA;
            if (s < 0 || s > 1)
                return false;

            intersection = a1 + t * dirA;

            return true;
        }

        public static Vector2 FromPolarRadians(float angleDeg, float radius)
        {
            float x = radius * Mathf.Cos(angleDeg);
            float y = radius * Mathf.Sin(angleDeg);
            return new Vector2(x, y);
        }

        public static Vector2 FromPolar(float angleDeg, float radius)
        {
            angleDeg *= Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angleDeg);
            float y = radius * Mathf.Sin(angleDeg);
            return new Vector2(x, y);
        }
        public static Rect FixRect(Rect r)
        {
            float x = r.x;
            float y = r.y;
            float width = r.width;
            float height = r.height;
            if (width < 0)
            {
                x = r.x + width;
                width = -width;
            }
            if (height < 0)
            {
                y = r.y + height;
                height = -height;
            }
            return new Rect(x, y, width, height);
        }

        public static Material DefaultMaterial
        {
            get
            {
                if (mDefaultMaterial == null)
                {
                    mDefaultMaterial = new Material(Shader.Find("Standard"));
                    mDefaultMaterial.color = Color.blue;
                }
                return mDefaultMaterial;
            }
        }

        /// <summary>
        /// safely assigns a material to a renderer. if the material is null than the default material is set instead
        /// </summary>
        /// <param name="renderer">the renderer</param>
        /// <param name="material">the material</param>
        /// <returns>true if material is not null, false otherwise</returns>
        public static bool SafeAssignMaterial(Renderer renderer, Material material, Material defualt)
        {
            Material toSet = material;
            if (toSet == null)
            {
                toSet = defualt;
                if (toSet == null)
                    toSet = DefaultMaterial;
            }
            renderer.sharedMaterial = toSet;
            return material != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newMesh"></param>
        /// <param name="cleanMesh"></param>
        public static void CleanMesh(Mesh newMesh, ref Mesh cleanMesh)
        {
            if (cleanMesh == newMesh)
                return;
            if (cleanMesh != null)
                ChartCommon.SafeDestroy(cleanMesh);
            cleanMesh = newMesh;
        }

        public static bool IsInEditMode
        {
            get
            {
                return Application.isPlaying == false && Application.isEditor;
            }
        }

        public static void SafeDestroy(UnityEngine.Object obj)
        {
            if (obj == null)
                return;
            if (Application.isEditor && Application.isPlaying == false)
                UnityEngine.Object.DestroyImmediate(obj);
            else
                UnityEngine.Object.Destroy(obj);
        }

        public static UIVertex CreateVertex(Vector3 pos, Vector2 uv)
        {
            return CreateVertex(pos, uv, pos.z);
        }

        [ThreadStatic]
        static StringBuilder mInnerBuilder;

        static StringBuilder Builder
        {
            get
            {
                if (mInnerBuilder == null)
                    mInnerBuilder = new StringBuilder();
                return mInnerBuilder;
            }
        }

#if UNITY_EDITOR

        [System.Diagnostics.Conditional("ChartDevLogEnabled")]
        public static void DevLog(string tag, params object[] items)
        {
            // if in the editor this method should be empty unless loggin is enbaled. System.Diagnostics.Conditional does not support and opeerations 
            if (LogOptions.IsTagEnabled(tag))
            {
                Builder.Length = 0;
                Builder.Append(tag);
                Builder.Append(" - ");
                for (int i = 0; i < items.Length; i++)
                {
                    Builder.Append(items[i].ToString());
                    Builder.Append(' ');
                }
                Debug.Log(Builder.ToString());
            }
        }


#else


        [System.Diagnostics.Conditional("UNITY_EDITOR")] 
        public static void DevLog(string tag, params object[] items)
        {
            // if in the editor this method should be empty unless loggin is enbaled. System.Diagnostics.Conditional does not support and opeerations 
            if (LogOptions.IsTagEnabled(tag))
            {
                Builder.Length = 0;
                Builder.Append(tag);
                Builder.Append(" - ");
                for (int i = 0; i < items.Length; i++)
                {
                    Builder.Append(items[i].ToString());
                    Builder.Append(' ');
                }
                Debug.Log(Builder.ToString());
            }
        }

#endif

        //        [System.Diagnostics.Conditional("UNITY_EDITOR")]    // if not in the unity editor this method should be stripped completry for performance.
        //        public static void DevLog(string innerTag, object message,string tag = "")
        //        {
        //            // if in the editor this method should be empty unless loggin is enbaled. System.Diagnostics.Conditional does not support and opeerations
        //#if ChartDevLogEnabled
        //            if(LogOptions.IsTagEnabled(tag))
        //                Debug.Log(tag.ToString() +" - " + innerTag + ":" + message);           
        //#endif
        //        }
        [System.Diagnostics.Conditional("ChartDevWarningEnabled")]
        public static void DevWarning(object message)
        {
            Debug.LogWarning(message);
        }
        [System.Diagnostics.Conditional("ChartPerformanceWarningEnabled")]
        public static void PerofrmanceWarning(object message)
        {
            Debug.LogWarning(message);
        }

        [System.Diagnostics.Conditional("ChartDevCommentEnabled")]
        public static void DevComment(object message)
        {
#if ChartDevCommentEnabled
            Debug.LogWarning(message);
#endif
        }

        //        [System.Diagnostics.Conditional("ChartWarningEnabled")]
        public static void RuntimeWarning(object message)
        {
            if (Application.isEditor && Application.isPlaying)
                Debug.LogError(message);
        }

        [System.Diagnostics.Conditional("ChartLogEnabled")]
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        public static void CreatePremappedVertex(ref PreMappedVertex vert, DoubleVector3 pos, Vector2 uv, double z, Vector4 tangent, Color color, float extusionAmount, float extrusionAngleInterpolator = 1f)
        {
            vert = new PreMappedVertex()
            {
                preMapped = pos,
                uv = uv,
                color = color,
                tangent = tangent,
                ExtrusionFactor = extusionAmount,
                ExtrusionAngleInterpolator = extrusionAngleInterpolator
            };
            vert.preMapped.z = z;
        }

        public static void CreatePremappedVertex(ref PreMappedVertex vert, DoubleVector3 pos, Vector2 uv, double z, Vector4 tangent, float extusionAmount, float extrusionAngleInterpolator = 1f)
        {
            vert = new PreMappedVertex()
            {
                preMapped = pos,
                uv = uv,
                color = White,
                tangent = tangent,
                ExtrusionFactor = extusionAmount,
                ExtrusionAngleInterpolator = extrusionAngleInterpolator
            };
            vert.preMapped.z = z;
        }
        public static void CreatePremappedVertex(ref PreMappedVertex vert, DoubleVector3 pos, Vector4 tangent, float extusionAmount, float extrusionAngleInterpolator = 1f)
        {
            vert = new PreMappedVertex()
            {
                preMapped = pos,
                color = White,
                tangent = tangent,
                ExtrusionFactor = extusionAmount,
                ExtrusionAngleInterpolator = extrusionAngleInterpolator
            };
            vert.preMapped.z = 0.0;
        }

        public static void CreatePremappedVertex(ref PreMappedVertex vert, DoubleVector3 pos)
        {
            vert = new PreMappedVertex()
            {
                preMapped = pos,
                color = White,
                tangent = new Vector4(),
                ExtrusionFactor = 0.0f,
                ExtrusionAngleInterpolator = 0.0f
            };
        }

        public static void CreatePremappedVertex(ref PreMappedVertex vert, DoubleVector3 pos, Vector2 uv, double z)
        {
            vert = new PreMappedVertex()
            {
                preMapped = pos,
                uv = uv,
                color = White,
                tangent = new Vector4(),
                ExtrusionFactor = 0.0f,
                ExtrusionAngleInterpolator = 0.0f
            };
            vert.preMapped.z = z;
        }

        public static UIVertex CreateUIVertex()
        {
            UIVertex v = new UIVertex();
            v.color = new Color32(255, 255, 255, 255);
            return v;
        }

        public static UIVertex CreateVertex(Vector3 pos, Vector2 uv, float z)
        {
            UIVertex vert = new UIVertex();
            vert.color = new Color32(255, 255, 255, 255);
            vert.uv0 = uv;
            pos.z = z;
            vert.position = pos;
            return vert;
        }

        public static float GetTiling(MaterialTiling tiling)
        {
            if (tiling.EnableTiling == false)
                return -1f;
            return tiling.TileFactor;
        }


        public static T EnsureComponent<T>(GameObject obj) where T : Component
        {
            T comp = obj.GetComponent<T>();
            if (comp == null)
                comp = obj.AddComponent<T>();
            return comp;
        }

        static private double DotProduct(DoubleVector3 a, DoubleVector3 b, DoubleVector3 c)
        {
            DoubleVector3 ab = b - a;
            DoubleVector3 bc = c - b;
            return DoubleVector3.Dot(ab, bc);
        }

        static private double CrossProduct(DoubleVector3 a, DoubleVector3 b, DoubleVector3 c)
        {
            DoubleVector3 ab = b - a;
            DoubleVector3 ac = c - a;
            return ab.x * ac.y - ab.y * ac.x;
        }

        static private float DotProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 bc = c - b;
            return Vector2.Dot(ab, bc);
        }

        static private float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            return ab.x * ac.y - ab.y * ac.x;
        }

        static public double SegmentPointSqrDistance(DoubleVector3 a, DoubleVector3 b, DoubleVector3 point)
        {
            double dot = DotProduct(a, b, point);

            if (dot > 0)
                return (b - point).sqrMagnitude;

            dot = DotProduct(b, a, point);
            if (dot > 0)
                return (a - point).sqrMagnitude;

            double cross = CrossProduct(a, b, point);
            return (cross * cross) / (a - b).sqrMagnitude;
        }

        static public float SegmentPointSqrDistance(Vector2 a, Vector2 b, Vector2 point)
        {
            float dot = DotProduct(a, b, point);

            if (dot > 0)
                return (b - point).sqrMagnitude;

            dot = DotProduct(b, a, point);
            if (dot > 0)
                return (a - point).sqrMagnitude;

            float cross = CrossProduct(a, b, point);
            return (cross * cross) / (a - b).sqrMagnitude;
        }



        /// <summary>
        /// 
        /// </summary>
        public static IEqualityComparer<int> DefaultIntComparer { get; private set; }
    }
}
