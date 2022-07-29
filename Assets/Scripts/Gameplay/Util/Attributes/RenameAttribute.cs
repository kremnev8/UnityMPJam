using UnityEngine;

namespace Util
{

    /// <summary>
    /// Property drawer attribute to rename property
    /// </summary>
    public class RenameAttribute : PropertyAttribute
    {
        public string NewName { get ; private set; }    
        public RenameAttribute( string name )
        {
            NewName = name ;
        }
    }
}