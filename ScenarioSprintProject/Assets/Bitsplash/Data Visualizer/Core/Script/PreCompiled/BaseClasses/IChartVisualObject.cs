using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DataVisualizer{
    /// <summary>
    /// this is an interface representation of any chart visual object that is used in a chart. This can be a Data Series, an Axis Visual feature , or any other object.
    /// 
    /// </summary>
    public interface IChartVisualObject
    {
        /// <summary>
        /// called when the object is initiated
        /// </summary>
        void OnInit();

        /// <summary>
        // notifies this object that the parent view has changed. This should usually trigger an invalidation of the underlying graphic objects
        /// </summary>
        /// <param name="data"></param>
        void OnSetView(ViewPortion view);

        /// <summary>
        /// positions and scales the object's gameobject so it fits into the specified rect
        /// </summary>
        /// <param name="localRect">the local rect in the parent transform in which the series gameobject should be fit</param>
        void FitInto(ViewPortion localRect);

        /// <summary>
        /// returns the unity gameobject for this visual object
        /// </summary>
        GameObject underlyingGameObject { get; }

        /// <summary>
        /// returns the current view that is set on this object. This is the view set using OnSetView
        /// </summary>
        ViewPortion CurrentView { get; }

        /// <summary>
        /// destorys the gameobject associated with this visual object
        /// </summary>
        void Destroy();

        /// <summary>
        /// set the active state of the visual object
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);

        /// <summary>
        /// sets the visibillty of the visual object
        /// </summary>
        /// <param name="visible"></param>
        void SetVisible(bool visible);

        /// <summary>
        ///  sets the parent chart of the visual object
        /// </summary>
        /// <param name="parent"></param>
        void SetParent(IPrivateDataSeriesChart parent);

        /// <summary>
        /// applie the given setting to this visual object.
        /// </summary>
        /// <param name="settings"></param>
        void ApplySettings(IDataSeriesSettings settings, string parentItemName, string visualFeatureName);

        /// <summary>
        /// this is an update function that is called from the main chart component. It replaces monobehviour.update and ensure that all the chart componentes update in the propert order
        /// </summary>
        void UniformUpdate();

    }
}
