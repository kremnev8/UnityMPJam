using Gameplay.Conrollers;
using UnityEngine;

namespace Gameplay.World
{
    public interface IInteractable
    {
        Vector2Int FacingDirection { get; }
        void Activate(PlayerController player);
    }
}