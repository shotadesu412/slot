using System;
using System.Collections.Generic;

namespace SlotRogue.Core.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _subscribers[type] = list;
            }
            list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (_subscribers.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }

        public static void Publish<T>(T evt) where T : struct
        {
            if (_subscribers.TryGetValue(typeof(T), out var list))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                    ((Action<T>)list[i])(evt);
            }
        }

        public static void Clear()
        {
            _subscribers.Clear();
        }
    }
}
