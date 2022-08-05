using Gameplay.Controllers.Player.Ability;
using ScriptableObjects;
using UnityEngine;

namespace Gameplay.ScriptableObjects
{
    
    [CreateAssetMenu(fileName = "Ability DB", menuName = "SO/New Ability DB", order = 0)]
    public class AbilityDB : GenericDB<string, BaseAbility>
    {
    }
}