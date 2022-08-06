using System;
using Gameplay.Conrollers;
using UnityEngine;

namespace Gameplay.Logic
{
    public class TriggerPlayerSwapArea : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col != null)
            {
                PlayerController player = col.GetComponent<PlayerController>();
                player.allowSwap = true;
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col != null)
            {
                PlayerController player = col.GetComponent<PlayerController>();
                player.allowSwap = false;
            }
        }
    }
}