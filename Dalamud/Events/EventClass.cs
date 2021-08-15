using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dalamud.Events
{
    internal class EventClass
    {
        public Assembly FromAssembly { get; set; }

        public Type ClassType { get; set; }

        public List<EventCall> Calls { get; set; } = new();

        public List<object> TargetInstances { get; set; } = new();
    }
}
