using System;
using Gameplay.Conrollers;
using UnityEngine;

namespace Gameplay.Logic
{
    [SelectionBase]
    public class TriggerPlayerSwapArea : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col != null)
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.allowSwap = true;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col != null)
            {
                PlayerController player = col.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.allowSwap = false;
                }
            }
        }
    }
}