using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using UnityEngine;

namespace Gameplay.ScriptableObjects
{
    public enum AbilityType
    {
        SHOOT,
        VISION
    }
    
    [Serializable]
    public class Ability
    {
        public string abilityName;
        public AbilityType abilityType;
        public ProjectileID projectileID;
        public float abilityCooldown;

        protected bool Equals(Ability other)
        {
            return abilityName.Equals(other.abilityName) && abilityType == other.abilityType && projectileID == other.projectileID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Ability)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(abilityName, (int)abilityType, (int)projectileID);
        }

        public static bool operator ==(Ability left, Ability right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Ability left, Ability right)
        {
            return !Equals(left, right);
        }
    }
    
    [Serializable]
    public class AbilityStack
    {
        public PlayerRole role;
        public List<Ability> abilities;
    }
    
    
    [CreateAssetMenu(fileName = "Ability DB", menuName = "SO/New Ability DB", order = 0)]
    public class AbilityDB : ScriptableObject
    {
        public List<AbilityStack> abilityStacks;

        public AbilityStack GetAbilities(PlayerRole role)
        {
            return abilityStacks.First(stack => stack.role == role);
        }
    }
}