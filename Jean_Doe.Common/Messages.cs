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
        public bool On { get; set; }
    }
}
