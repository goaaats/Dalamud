using System.Collections.Generic;

namespace Dalamud.Events
{
    internal class EventCall
    {
        public int Priority { get; set; }

        public EventClass Class { get; set; }

        public List<EventCall> Group { get; set; }

        public EventSystem.EventDelegate Delegate { get; set; }

        public bool IsStatic { get; set; }
    }
}
