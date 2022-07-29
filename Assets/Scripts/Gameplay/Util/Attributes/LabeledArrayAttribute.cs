/**
 * LabeledArrayAttribute.cs
 * Created by: Joao Borks [joao.borks@gmail.com]
 * Created on: 28/12/17 (dd/mm/yy)
 * Reference from John Avery: https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/
 */

using UnityEngine;

namespace Util
{

    /// <summary>
    /// Property drawer attribute to name all array elements, elements must implement <see cref="INamedArrayElement"/>
    /// </summary>
    public class LabeledArrayAttribute : PropertyAttribute
    {
        public LabeledArrayAttribute() { }
    }

    public interface INamedArrayElement
    {
        string editorName { get; }
        string displayName { get; }
    }
}