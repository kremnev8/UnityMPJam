using System;

namespace Util
{
    /// <summary>
    /// Attribute that allows to make a button for a method in the inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InspectorButtonAttribute : Attribute
    {
        
    }
}