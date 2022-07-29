using UnityEngine;

namespace Util
{
    /// <summary>
    /// Hide game object this is attached to at the start
    /// </summary>
    public class HideOnStart : MonoBehaviour
    {
        private void Update()
        {
            gameObject.SetActive(false);
            Destroy(this);
        }
    }
}