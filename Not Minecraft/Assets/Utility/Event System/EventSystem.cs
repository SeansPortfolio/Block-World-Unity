
namespace MyGame.Events
{
    using System;
    using System.Collections.Generic;

    public class EventSystem
    {
        public delegate void EventFunction(object sender, EventArgs args);

        private Dictionary<EventChannel, EventFunction> EventCallbacks = new Dictionary<EventChannel, EventFunction>();
        
        public static EventSystem Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    _instance = new EventSystem();
                }
                return _instance;
            } 
        }

        private static EventSystem _instance;

        public void RegisterEvent(EventChannel channel, EventFunction callback)
        {
            if(EventCallbacks.ContainsKey(channel))
            {
                EventCallbacks[channel] += callback;
            }
            else
            {
                EventCallbacks.Add(channel, callback);
            }
        }

        public void UnregisterEvent(EventChannel channel, EventFunction callback)
        {
            if (EventCallbacks.ContainsKey(channel))
            {
                EventCallbacks[channel] -= callback;

                if(EventCallbacks[channel] == null)
                {
                    EventCallbacks.Remove(channel);
                }
            }
        }

        public void FireEvent(EventChannel channel, object sender, EventArgs args)
        {
            if(EventCallbacks.TryGetValue(channel, out EventFunction callback))
            {
                callback(sender, args);
            }
        }
    }
}
