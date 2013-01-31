using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Metro.IoC
{
    public class MessageBus
    {
        static MessageBus instance;
        public static MessageBus Instance
        {
            get
            {
                if (instance == null)
                    instance = new MessageBus();
                return instance;
            }
        }
        public static Action<System.Action> DefaultPublicationThreadMarshaller = action => action();
        readonly List<Handler> handlers = new List<Handler>();
        #region ITouchMessageBus
        /// <summary>
        ///   Subscribes an instance to all events declared through implementations of <see cref = "IHandle{T}" />
        /// </summary>
        /// <param name = "instance">The instance to subscribe for event publication.</param>
        public virtual void Register(object instance)
        {
            lock (handlers)
            {
                if (handlers.Any(x => x.Matches(instance)))
                    return;
                handlers.Add(new Handler(instance));
            }
        }

        /// <summary>
        ///   Unsubscribes the instance from all events.
        /// </summary>
        /// <param name = "instance">The instance to unsubscribe.</param>
        public virtual void Unregister(object instance)
        {
            lock (handlers)
            {
                var found = handlers.FirstOrDefault(x => x.Matches(instance));

                if (found != null)
                    handlers.Remove(found);
            }
        }

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <remarks>
        ///   Does not marshall the the publication to any special thread by default.
        /// </remarks>
        public virtual void Publish(object message)
        {
            Publish(message, DefaultPublicationThreadMarshaller);
        }

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
        public virtual void Publish(object message, Action<System.Action> marshal)
        {
            Handler[] toNotify;
            lock (handlers)
                toNotify = handlers.ToArray();

            marshal(() =>
            {
                var messageType = message.GetType();

                var dead = toNotify
                    .Where(handler => !handler.Handle(messageType, message))
                    .ToList();

                if (dead.Any())
                {
                    lock (handlers)
                    {
                        dead.Apply(x => handlers.Remove(x));
                    }
                }
            });
        }
        #endregion
        private MessageBus()
        {
        }
    }
}
