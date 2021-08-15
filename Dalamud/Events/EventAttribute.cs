using System;

namespace Dalamud.Events
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class EventAttribute : Attribute
    {
        public string EventName { get; set; }

        public int Priority { get; set; }

        public EventAttribute(string eventName)
        {
            this.EventName = eventName;
        }
    }
}
