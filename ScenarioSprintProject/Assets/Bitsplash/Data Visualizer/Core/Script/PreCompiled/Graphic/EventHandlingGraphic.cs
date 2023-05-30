using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DataVisualizer{
    /// <summary>
    /// Base class for canvas series grsphic object. Contains functionallity that enable creating a hovering prefab where the mouse is located , and event handling
    /// The main idea behind this implementation is that it allows to create a mesh once. Then in order to create the hover effect , an object is placed on top of that mesh , those leaving the mesh unmodified.
    /// </summary>
    public abstract class EventHandlingGraphic : BaseCanvasGraphic
    {
    
    }
}
