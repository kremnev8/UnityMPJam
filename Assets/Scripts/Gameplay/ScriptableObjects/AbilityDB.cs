using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Controllers.Player.Ability;
using ScriptableObjects;
using UnityEngine;

namespace Gameplay.ScriptableObjects
{
    [Serializable]
    public class AbilityStack
    {
        public PlayerRole role;
        public List<BaseAbility> abilities;
    }
    
    
    [CreateAssetMenu(fileName = "Ability DB", menuName = "SO/New Ability DB", order = 0)]
    public class AbilityDB : GenericDB<string, BaseAbility>
    {
        public List<AbilityStack> abilityStacks;

        public AbilityStack GetAbilities(PlayerRole role)
        {
            return abilityStacks.First(stack => stack.role == role);
        }
    }
}