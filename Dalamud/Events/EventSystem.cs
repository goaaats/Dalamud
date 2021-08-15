using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Logging.Internal;

namespace Dalamud.Events
{
    internal class EventSystem
    {
        private static ModuleLog Log = new("EVENTSYSTEM");

        private Dictionary<Type, EventClass> Classes = new();
        private Dictionary<string, List<EventCall>> Groups = new(StringComparer.OrdinalIgnoreCase);

        public delegate void EventDelegate(object target, object[]? args);

        // TODO Replace with a service
        public static EventSystem Instance { get; private set; }
        internal static void Init()
        {
            Instance = new EventSystem();
        }

        internal void RegisterType(Type toRegister)
        {
            if (!this.Classes.TryGetValue(toRegister, out var eventClass))
            {
                eventClass = new EventClass
                {
                    FromAssembly = toRegister.Assembly,
                    ClassType = toRegister,
                };
                this.Classes[toRegister] = eventClass;
            }

            foreach (var methodInfo in toRegister.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var attribute in methodInfo.GetCustomAttributes(true))
                {
                    if (attribute is not EventAttribute eventAttribute)
                        continue;

                    var group = this.GetGroup(eventAttribute.EventName) ?? this.AddGroup(eventAttribute.EventName);

                    var call = new EventCall
                    {
                        Priority = eventAttribute.Priority,
                        Class = eventClass,
                        Group = group,
                        Delegate = MakeDelegate(methodInfo),
                        IsStatic = methodInfo.IsStatic,
                    };

                    eventClass.Calls.Add(call);
                    group.Add(call);

                    Log.Verbose($"   => Registered {toRegister.FullName}::{methodInfo.Name} IsStatic: {call.IsStatic}");
                }
            }
        }

        internal void UnregisterType(Type toUnregister)
        {
            if (!this.Classes.TryGetValue(toUnregister, out var classEvent))
                return;
            this.Classes.Remove(toUnregister);

            foreach (var methodInfo in toUnregister.GetMethods())
            {
                foreach (var attribute in methodInfo.GetCustomAttributes(true))
                {
                    if (attribute is not EventAttribute eventAttribute)
                        continue;

                    var group = this.GetGroup(eventAttribute.EventName);
                    if (group == null)
                        continue;

                    group.Remove(group.FirstOrDefault(x => x.Class == classEvent));
                }
            }
        }

        internal void RegisterAssembly(Assembly assembly)
        {
            Log.Verbose($" => Registering {assembly.FullName}");

            foreach (var type in assembly.GetTypes())
            {
                this.RegisterType(type);
            }

            Log.Verbose($" => {assembly.FullName} registered!");
        }

        internal void UnregisterAssembly(Assembly assembly)
        {
            Log.Verbose($" => Unregistering {assembly.FullName}");

            foreach (var type in assembly.GetTypes())
            {
                this.UnregisterType(type);
            }

            Log.Verbose($" => {assembly.FullName} unregistered!");
        }

        internal void Run(string eventName, params object[] parameters)
        {
            var group = this.GetGroup(eventName);
            if (group == null)
            {
                return;
            }

            if (parameters.Length == 0)
                parameters = null;

            foreach (var call in group)
            {
                try
                {
                    if (call.IsStatic)
                    {
                        call.Delegate(null, parameters);
                    }
                    else
                    {
                        foreach (var instance in call.Class.TargetInstances)
                        {
                            call.Delegate(instance, parameters);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Call to '{eventName}' failed on '{call.Class.ClassType.FullName}'");
                }
            }
        }

        internal static EventDelegate MakeDelegate(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            object[] args = null;

            if (parameters.Length != 0)
                args = new object[parameters.Length];

            return (target, objects) => {
                if (args != null)
                {
                    if (objects.Length != args.Length)
                        return;

                    for (var i = 0; i < args.Length; i++)
                    {
                        args[i] = objects[i];
                    }
                }
                else if (objects != null && objects.Length != 0)
                {
                    return;
                }

                methodInfo.Invoke(target, args);
            };
        }

        internal List<EventCall>? GetGroup(string name) =>
            this.Groups.TryGetValue(name.ToLowerInvariant(), out var group) ? @group : null;

        internal List<EventCall> AddGroup(string name)
        {
            var list = new List<EventCall>();
            this.Groups.Add(name.ToLowerInvariant(), list);

            return list;
        }

        public void RegisterObject(object obj)
        {
            if (this.Classes.TryGetValue(obj.GetType(), out var eventClass))
            {
                eventClass.TargetInstances.Add(obj);
            }
        }

        public void UnregisterObject(object obj)
        {
            if (this.Classes.TryGetValue(obj.GetType(), out var eventClass))
            {
                eventClass.TargetInstances.Remove(obj);
            }
        }
    }
}
