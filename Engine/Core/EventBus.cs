// Engine/Core/EventBus.cs
using System;
using System.Collections.Generic;

namespace OgreBattle.Engine.Core
{
    public interface IGameEvent { }

    public class EventBus
    {
        private readonly Dictionary<Type, List<Action<IGameEvent>>> _handlers =
            new Dictionary<Type, List<Action<IGameEvent>>>();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            var type = typeof(TEvent);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Action<IGameEvent>>();

            _handlers[type].Add(e => handler((TEvent)e));
        }

        public void Publish(IGameEvent gameEvent)
        {
            var type = gameEvent.GetType();
            if (!_handlers.TryGetValue(type, out var handlers))
                return;

            foreach (var handler in handlers)
                handler(gameEvent);
        }
    }
}
