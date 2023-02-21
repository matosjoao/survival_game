using System;
using System.Collections.Generic;

public class EventBus : Singleton<EventBus>
{
    private Dictionary<string, Action<object>> listeners = new Dictionary<string, Action<object>>();

    public void Publish(string message, object data = null)
    {
        if (listeners.ContainsKey(message))
        {
            listeners[message].Invoke(data);
        }
    }

    public void Subscribe(string message, Action<object> listener)
    {
        if (!listeners.ContainsKey(message))
        {
            listeners[message] = null;
        }

        listeners[message] += listener;
    }

    public void Unsubscribe(string message, Action<object> listener)
    {
        if (listeners.ContainsKey(message))
        {
            listeners[message] -= listener;
        }
    }
}
