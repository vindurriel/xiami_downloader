using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
namespace Metro.IoC
{

    class Handler
    {
        readonly WeakReference reference;
        readonly Dictionary<Type, MethodInfo> supportedHandlers = new Dictionary<Type, MethodInfo>();

        public Handler(object handler)
        {
            reference = new WeakReference(handler);
            Type t=handler.GetType();
            var interfaces = handler.GetType().GetTypeInfo().ImplementedInterfaces
                .Where(x => typeof(IHandle).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo()) && x.GetTypeInfo().IsGenericType);

            foreach (var @interface in interfaces)
            {
                var type = @interface.GetTypeInfo().GenericTypeArguments[0];
                var method = @interface.GetTypeInfo().GetDeclaredMethod("Handle");
                supportedHandlers[type] = method;
            }
        }

        public bool Matches(object instance)
        {
            return reference.Target == instance;
        }

        public bool Handle(Type messageType, object message)
        {
            var target = reference.Target;
            if (target == null)
                return false;

            foreach (var pair in supportedHandlers)
            {
                if (pair.Key.GetTypeInfo().IsAssignableFrom(messageType.GetTypeInfo()))
                {
                    pair.Value.Invoke(target, new[] { message });
                    return true;
                }
            }

            return true;
        }
    }
}
