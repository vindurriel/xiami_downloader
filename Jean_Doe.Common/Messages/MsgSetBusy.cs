using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artwork.MessageBus.Interfaces;
namespace Jean_Doe.Common
{
    public class MsgSetBusy : IMessage
    {
        public MsgSetBusy(object sender, bool on)
        { On = on; Sender = sender; }
        public bool On { get; private set; }
        public object Sender { get; private set; }
    }
}
