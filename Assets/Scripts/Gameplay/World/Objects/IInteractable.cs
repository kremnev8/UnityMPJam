using Gameplay.Conrollers;
using UnityEngine;

namespace Gameplay.World
{
    public interface IInteractable
    {
        bool checkFacing { get; }
        Vector2Int FacingDirection { get; }
        void Activate(PlayerController player);
    }
}