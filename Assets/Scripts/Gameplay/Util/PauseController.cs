using System;
using UnityEngine;

namespace Util
{
    /// <summary>
    /// Whenever this mono behavior is active, the game will be paused.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        private void OnEnable()
        {
            Time.timeScale = 0;
        }

        private void OnDisable()
        {
            Time.timeScale = 1;
        }
    }
}