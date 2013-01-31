using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metro.IoC
{
    /// <summary>
    ///   A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IHandle { }
    public interface IHandle<TMessage> : IHandle
    {
        /// <summary>
        ///   Handles the message.
        /// </summary>
        /// <param name = "message">The message.</param>
        void Handle(TMessage message);
    }
}
