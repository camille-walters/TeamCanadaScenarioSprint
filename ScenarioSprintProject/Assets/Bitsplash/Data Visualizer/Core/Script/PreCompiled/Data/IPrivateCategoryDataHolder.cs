using DataVisualizer;
using UnityEditor;
using UnityEngine;

namespace DataVisualizer
{
    public interface IPrivateCategoryDataHolder
    {
        StackedGenericDataHolder InnerData { get; }
    }
}