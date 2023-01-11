using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketRecv.Main
{
    public enum State
    {
        CLOSE_WAIT,
        CLOSED,
        ESTABLISHED,
        FIN_WAIT_1,
        FIN_WAIT_2,
        LAST_ACK,
        LISTEN,
        SYN_RECEIVED,
        SYN_SEND,
        TIMED_WAIT,
        None, // UDP
    }

    public enum ConnectionType
    {
        TCP,
        UDP,
        Other,
    }
    public struct NetStat
    {
        public ConnectionType CType;
        public string lAddress;
        public string FAddress;
        public State State;
        public int Pid;
    }

    
}
