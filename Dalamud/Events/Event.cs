using JetBrains.Annotations;

namespace Dalamud.Events
{
    public class Event
    {
        public static void Run(string name) => EventSystem.Instance.Run(name);

        public static void Run<T1>(string name, T1 arg0) => EventSystem.Instance.Run(name, arg0);

        public static void Run<T1, T2>(string name, T1 arg0, T2 arg1) => EventSystem.Instance.Run(name, arg0, arg1);

        public static void Run<T1, T2, T3>(string name, T1 arg0, T2 arg1, T3 arg2) => EventSystem.Instance.Run(name, arg0, arg1, arg2);

        public static void Run<T1, T2, T3, T4>(string name, T1 arg0, T2 arg1, T3 arg2, T4 arg3) => EventSystem.Instance.Run(name, arg0, arg1, arg2, arg3);

        public static void Run<T1, T2, T3, T4, T5>(string name, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4) => EventSystem.Instance.Run(name, arg0, arg1, arg2, arg3, arg4);

        public static void Run<T1, T2, T3, T4, T5, T6>(string name, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5) => EventSystem.Instance.Run(name, arg0, arg1, arg2, arg3, arg4, arg5);

        public class Chat
        {
            public class MessageAttribute : EventAttribute
            {
                public MessageAttribute() : base("chat.message") { }
            }
        }

        public class Framework
        {
            public class UpdateAttribute : EventAttribute
            {
                public UpdateAttribute() : base("framework.update") { }
            }
        }
    }
}
