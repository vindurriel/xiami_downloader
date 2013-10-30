using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artwork.MessageBus.Interfaces;

namespace Jean_Doe.MusicControl
{
    public class MsgChangeWindowState
    {
        public EnumChangeWindowState State;
    }
    public enum EnumChangeWindowState { Close, Maximized, Minimized }
}
