using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Util
{
    /// <summary>
    /// Utility methods for collections
    /// </summary>
    public static class CollectionExtension
    {
        /// <summary>
        /// Utility method to try get a value from dictionary.
        /// If dictionary has no such key, a default value is returned
        /// </summary>
        public static TV TryGet<TK, TV>(this Dictionary<TK, TV> dictionary, TK key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return default;
        }
        
        /// <summary>
        /// Utility method to try set a value in dictionary.
        /// If dictionary has no such key, it is added
        /// </summary>
        public static void TrySet<TK, TV>(this Dictionary<TK, TV> dictionary, TK key, TV value)
        { 
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }


        /// <summary>
        /// Get an attribute of type T from a MemberInfo
        /// </summary>
        /// <param name="memberInfo">target object info</param>
        /// <param name="customAttribute">result attribute</param>
        /// <typeparam name="T">attribute type</typeparam>
        /// <returns>was an attribute found?</returns>
        public static bool TryGetAttribute<T>(this MemberInfo memberInfo, out T customAttribute) where T: Attribute {
            var attributes = memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            if (attributes == null) {
                customAttribute = null;
                return false;
            }
            customAttribute = (T)attributes;
            return true;
        }
    }
}