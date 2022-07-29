using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptableObjects
{
    /// <summary>
    /// Defines an achievement
    /// </summary>
    [Serializable]
    public class Achievement : GenericItem
    {
        public string name;
        public string id;
        public string description;
        
        public Sprite icon;
        public Color spriteTint = Color.white;

        public int metadata;
        public bool hasProgress;
        public Achievement() { }

        public Achievement(string id, int metadata, string name, string description, bool hasProgress, Sprite icon, Color spriteTint)
        {
            this.id = id;
            this.metadata = metadata;
            this.name = name;
            this.description = description;
            this.hasProgress = hasProgress;
            this.icon = icon;
            this.spriteTint = spriteTint;
        }

        public string ItemId => id;
        public string index => id;
    }
    
    /// <summary>
    /// Data store for existing achievements
    /// </summary>
    [CreateAssetMenu(fileName = "Achievement DB", menuName = "SO/New Achievement DB", order = 0)]
    public class AchievementDB : GenericDB<Achievement>
    {
        public List<ScriptableAchievementProvider> providers;
        private Achievement[] dynamicItems;

        public ScriptableAchievementProvider GetProvider(Achievement achievement)
        {
            return providers.First(provider => achievement.id.Contains(provider.baseId));
        }
        
        public override Achievement[] GetAll()
        {
            if (itemsDictionary == null)
            {
                InitDictionary();
            }
            
            return dynamicItems;
        }

        protected override void InitDictionary()
        {
            itemsDictionary = new Dictionary<string, Achievement>();
            List<Achievement> achievements = new List<Achievement>(items);

            foreach (ScriptableAchievementProvider provider in providers)
            {
                achievements.AddRange(provider.GetAchievements());
            }

            dynamicItems = achievements.ToArray();
            
            foreach (Achievement item in dynamicItems)
            {
                itemsDictionary.Add(item.ItemId, item);
            }
        }
    }
}