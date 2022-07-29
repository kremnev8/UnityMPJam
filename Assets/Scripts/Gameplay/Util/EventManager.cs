using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    /// <summary>
    /// Global event manager. Handles subscribing and sending events.
    /// </summary>
    public static class EventManager
    {
        public static void StartListening(string eventType, Action<object[]> listener)
        {
            if (eventDictionary.TryGetValue(eventType, out var action))
            {
                action = (Action<object[]>)Delegate.Combine(action, listener);
                eventDictionary[eventType] = action;
                return;
            }

            eventDictionary.Add(eventType, listener);
        }

        public static void StopListening(string eventType, Action<object[]> listener)
        {
            if (!eventDictionary.TryGetValue(eventType, out var action))
            {
                return;
            }

            action = (Action<object[]>)Delegate.Remove(action, listener);
            eventDictionary[eventType] = action;
        }

        public static void TriggerEvent(string eventType, params object[] arguments)
        {
            if (eventDictionary.TryGetValue(eventType, out var action) && action != null)
            {
                action(arguments);
            }
        }

        private static Dictionary<string, Action<object[]>> eventDictionary = new Dictionary<string, Action<object[]>>();
    }
}