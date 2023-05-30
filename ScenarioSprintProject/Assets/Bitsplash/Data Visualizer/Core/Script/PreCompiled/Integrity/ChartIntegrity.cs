using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    public class ChartIntegrity
    {
#if UNITY_EDITOR

#if ChartDevAssertPerformance
        static HashSet<Delegate> mCalledDelegtes = new HashSet<Delegate>();
        static HashSet<KeyValuePair<object, string>> mModifiedProperties = new HashSet<KeyValuePair<object, string>>();
        static HashSet<KeyValuePair<object, string>> mFlags = new HashSet<KeyValuePair<object, string>>();
                class CollectionInfo
        {
            public HashSet<object> Items = new HashSet<object>();
        }
        static HashSet<object> mTmpHashSet = new HashSet<object>();
        static HashSet<object> mTmpHashSetB = new HashSet<object>();
        static Dictionary<KeyValuePair<object, string>, CollectionInfo> mCollections = new Dictionary<KeyValuePair<object, string>, CollectionInfo>();


#endif

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void NotifyClearCollection(object instance, string collectionName)
        {
#if ChartDevAssertPerformance
            CollectionInfo info;
            if(mCollections.TryGetValue(new KeyValuePair<object, string>(instance, collectionName),out info) == false)
            {
                info = new CollectionInfo();
                mCollections.Add(new KeyValuePair<object, string>(instance, collectionName), info);
            }
            info.Items.Clear();
#endif
        }


        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void NotifyAddCollection(object instance, string collectionName,object add)
        {
#if ChartDevAssertPerformance
            CollectionInfo info;
            if(mCollections.TryGetValue(new KeyValuePair<object, string>(instance, collectionName),out info) == false)
            {
                info = new CollectionInfo();
                mCollections.Add(new KeyValuePair<object, string>(instance, collectionName), info);
            }
            info.Items.Add(add);
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void NotifyRemoveCollection(object instance, string collectionName, object remove)
        {
#if ChartDevAssertPerformance
            CollectionInfo info;
            if (mCollections.TryGetValue(new KeyValuePair<object, string>(instance, collectionName), out info))
            {
                info.Items.Remove(remove);
            }
#endif
        }
        public static void AsseetCollectionDistinct(IEnumerable<string> collectionA, IEnumerable<string> collectionB)
        {
#if ChartDevAssertPerformance
            mTmpHashSet.Clear();
            foreach (object o in collectionA)
                mTmpHashSet.Add(o);
            mTmpHashSetB.Clear();
            foreach (object o in collectionB)
                mTmpHashSetB.Add(o);

            foreach (object o in mTmpHashSetB)
            {
                if (mTmpHashSet.Contains(o) == false)
                {
                    Debug.Assert(false);
                    return;
                }
            }
            
            foreach (object o in mTmpHashSet)
            {
                if (mTmpHashSetB.Contains(o) == false)
                {
                    Debug.Assert(false);
                    return;
                }
            }
#endif
        }
        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AsseetCollectionDistinct(object instance, string collectionName,IEnumerable<object> collection)
        {
#if ChartDevAssertPerformance
            mTmpHashSet.Clear();
            foreach (object o in collection)
                mTmpHashSet.Add(o);
            CollectionInfo info;
            if (mCollections.TryGetValue(new KeyValuePair<object, string>(instance, collectionName), out info) == false)
            {
                info = new CollectionInfo();
                mCollections.Add(new KeyValuePair<object, string>(instance, collectionName), info);
            }
            
            foreach (object o in info.Items)
            {
                if (mTmpHashSet.Contains(o) == false)
                {
                    Debug.Assert(false);
                    return;
                }
            }
            foreach (object o in mTmpHashSet)
            {
                if(info.Items.Contains(o) == false)
                {
                    Debug.Assert(false);
                    return;
                }
            }
#endif
        }
        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AssertCollectionContains(object instance, string collectionName, object contains)
        {
#if ChartDevAssertPerformance
            CollectionInfo info;
            
            if (mCollections.TryGetValue(new KeyValuePair<object, string>(instance, collectionName), out info))
            {
                Debug.Assert(info.Items.Contains(contains));
            }
            else
            {
                Debug.Assert(false);
            }
#endif

        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AssertFlagRaised(object instance,string flag)
        {
#if ChartDevAssertPerformance
            Debug.Assert(mFlags.Contains(new KeyValuePair<object, string>(instance, flag)));
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AssertEitherFlagRaised(object instance, params string[] flags)
        {
#if ChartDevAssertPerformance
            bool res = false;
            for (int i = 0; i < flags.Length; i++)
            {
                if (mFlags.Contains(new KeyValuePair<object, string>(instance, flags[i])))
                {
                    res = true;
                    break;
                }
            }
            Debug.Assert(res);
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AssertFlagNotRaised(object instance, params string[] flags)
        {
#if ChartDevAssertPerformance
            for (int i = 0; i < flags.Length; i++)
            {
                Debug.Assert(!mFlags.Contains(new KeyValuePair<object, string>(instance, flags[i])));
            }
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AssertFlagNotRaised(object instance, string flag)
        {
#if ChartDevAssertPerformance
            Debug.Assert(!mFlags.Contains(new KeyValuePair<object, string>(instance, flag)));
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void RaiseFlag(object instance,string flag)
        {
#if ChartDevAssertPerformance
            mFlags.Add(new KeyValuePair<object, string>(instance, flag));
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void RemoveFlag(object instance, string flag)
        {
#if ChartDevAssertPerformance
            mFlags.Remove(new KeyValuePair<object, string>(instance, flag));
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void NotifyPropertySet(object instance, string property)
        {
#if ChartDevAssertPerformance
            mModifiedProperties.Add(new KeyValuePair<object, string>(instance, property));
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void NotifyMethodCall(Delegate method)
        {
#if ChartDevAssertPerformance
            mCalledDelegtes.Add(method);
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AssertPropertySet(object instance,string property)
        {
#if ChartDevAssertPerformance
            mModifiedProperties.Contains(new KeyValuePair<object, string>(instance, property));
#endif
        }
        /// <summary>
        /// asserts that a method was called prior to this assert
        /// </summary>
        /// <param name="method"></param>
        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AssertMethodCalled(Delegate method)
        {
#if ChartDevAssertPerformance
            Debug.Assert(mCalledDelegtes.Contains(method));
#endif
        }

        /// <summary>
        /// asserts that a method was called prior to this assert
        /// </summary>
        /// <param name="method"></param>
        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void AssertMethodNotCalled(Delegate method)
        {
#if ChartDevAssertPerformance
            Debug.Assert(!mCalledDelegtes.Contains(method));
#endif
        }

        [System.Diagnostics.Conditional("ChartDevAssert")]
        public static void Assert(bool condition)
        {
            Debug.Assert(condition);
        }

        [System.Diagnostics.Conditional("ChartDevAssert")]
        public static void Assert(bool condition,object message)
        {
            Debug.Assert(condition,message);
        }

        [System.Diagnostics.Conditional("ChartDevAssertPerformance")]
        public static void Assert(Func<bool> condition)
        {
            Debug.Assert(condition());
        }

#else

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void NotifyClearCollection(object instance, string collectionName)
        {

        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void NotifyAddCollection(object instance, string collectionName,object add)
        {
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void NotifyRemoveCollection(object instance, string collectionName, object remove)
        {
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AsseetCollectionDistinct(object instance, string collectionName,IEnumerable<object> collection)
        {
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AsseetCollectionDistinct (IEnumerable<string> collectionA, IEnumerable<string> collectionB)
        {
        }
       
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AssertCollectionContains(object instance, string collectionName, object contains)
        {

        }

            /// <summary>
            /// asserts that a method was called prior to this assert
            /// </summary>
            /// <param name="method"></param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AssertMethodCalled(Delegate method)
        {
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AssertEitherFlagRaised(object instance, params string[] flags)
        {

        }

                /// <summary>
        /// asserts that a method was called prior to this assert
        /// </summary>
        /// <param name="method"></param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AssertMethodNotCalled(Delegate method)
        {

        }
                [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AssertFlagNotRaised(object instance, params string[] flags)
        {
        }

                [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AssertFlagRaised(object instance,string flag)
        {

        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AssertFlagNotRaised(object instance, string flag)
        {

        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void RaiseFlag(object instance,string flag)
        {

        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void RemoveFlag(object instance, string flag)
        {

        }

                [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void NotifyPropertySet(object instance, string property)
        {
        }
                [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void AssertPropertySet(object instance,string property)
        {
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void NotifyMethodCall(Delegate method)
        {
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")] 
        public static void DevLog(string tag, params object[] items)
        {

        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition)
        {
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition,object message)
        {
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Assert(Func<bool> condition)
        {
        }
#endif
    }
}
