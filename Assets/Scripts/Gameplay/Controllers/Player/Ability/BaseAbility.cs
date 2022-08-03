using System;
using Gameplay.Conrollers;
using Gameplay.Util;
using ScriptableObjects;
using UnityEngine;

namespace Gameplay.Controllers.Player.Ability
{
    public abstract class BaseAbility : ScriptableObject, GenericItem<string>
    {
        public string abilityName;
        public float abilityCooldown;

        public string ItemId => abilityName;

        public abstract bool ActivateAbility(PlayerController player, Direction direction);


        protected bool Equals(BaseAbility other)
        {
            return base.Equals(other) && abilityName == other.abilityName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseAbility)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), abilityName);
        }
    }
}