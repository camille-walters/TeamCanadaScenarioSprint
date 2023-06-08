using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataVisualizer{
    interface ILabeledObject
    {
        TextDataHolder LabelData { get; }
    }
}
