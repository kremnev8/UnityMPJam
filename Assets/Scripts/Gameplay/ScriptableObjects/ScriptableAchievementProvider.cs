using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    /// <summary>
    /// Exception in the case called provider does not handle the achievement
    /// </summary>
    public class WrongProviderException : Exception{

        public WrongProviderException() : base("This provider can't return progress")
        {
            
        }
    }
    
    /// <summary>
    /// Base class for Achievement provider. This class can create achievements at runtime as needed. Useful for large amount of similar achievements.
    /// </summary>
    public abstract class ScriptableAchievementProvider : ScriptableObject
    {
        public string baseId;
        public Sprite baseIcon;
        
        public abstract List<Achievement> GetAchievements();
        public abstract Progress GetProgress(Achievement achievement);
        public abstract void CheckAchievementsState(AchievementDB achievements); 
    }

    /// <summary>
    /// Achievement progress struct
    /// </summary>
    public struct Progress
    {
        public int current;
        public int max;

        public Progress(int current, int max)
        {
            this.current = current;
            this.max = max;
        }
    }
}